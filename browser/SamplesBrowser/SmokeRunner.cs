// Scratch-only smoke runner: when SAMPLES_SMOKE=<report path> is set, App hands the launch over to
// this class, which instantiates every registered sample in turn, waits for it to load and settle,
// records exceptions (synchronous, dispatcher and unhandled), optionally captures a PNG, and exits.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace SamplesBrowser;

public static class SmokeRunner
{
    private static string _current = "(startup)";
    private static StreamWriter _log;

    public static bool TryStart(Application app)
    {
        var report = Environment.GetEnvironmentVariable("SAMPLES_SMOKE");
        if (string.IsNullOrEmpty(report)) return false;
        var shots = Environment.GetEnvironmentVariable("SAMPLES_SMOKE_SHOTS");
        var filter = Environment.GetEnvironmentVariable("SAMPLES_SMOKE_FILTER");
        var settleMs = int.TryParse(Environment.GetEnvironmentVariable("SAMPLES_SMOKE_SETTLE_MS"), out var ms) ? ms : 2500;

        _log = new StreamWriter(report, append: true) { AutoFlush = true };
        app.UnhandledException += (s, e) =>
        {
            _log.WriteLine($"UNHANDLED\t{_current}\t{Flatten(e.Exception)}");
            e.Handled = true;
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            _log.WriteLine($"UNOBSERVED\t{_current}\t{Flatten(e.Exception)}");
            e.SetObserved();
        };

        var window = new Window { Title = "Samples smoke" };
        var host = new Grid { Width = 1200, Height = 800 };
        window.Content = host;
        // Off-screen and never activated, so the run does not take focus from the desktop.
        window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(-6000, 0, 1260, 880));
        window.AppWindow.Show(false);
        _ = RunAsync(window, host, shots, filter, settleMs);
        return true;
    }

    private static async Task RunAsync(Window window, Grid host, string shots, string filter, int settleMs)
    {
        await Task.Delay(1000);
        // Resumable: a page that took the process down left a BEGIN with nothing after it. Record it as
        // a crash and carry on from the next page instead of retrying it.
        var report = Environment.GetEnvironmentVariable("SAMPLES_SMOKE");
        var attempted = new HashSet<string>();
        string lastBegun = null;
        var previous = File.Exists(report) ? File.ReadAllLines(report) : new string[0];
        foreach (var line in previous)
        {
            var parts = line.Split('\t');
            if (parts.Length < 2) continue;
            if (parts[0] == "BEGIN") { attempted.Add(parts[1]); lastBegun = parts[1]; }
            else if (parts[1] == lastBegun && (parts[0] == "OK" || parts[0] == "FAIL" || parts[0] == "CRASH")) { lastBegun = null; }
        }
        if (lastBegun != null) _log.WriteLine($"CRASH\t{lastBegun}");
        var routes = SampleRegistry.Samples.Keys.OrderBy(k => k, StringComparer.Ordinal)
            .Where(r => string.IsNullOrEmpty(filter) || r.Contains(filter))
            .Where(r => !attempted.Contains(r)).ToList();
        foreach (var route in routes)
        {
            _current = route;
            _log.WriteLine($"BEGIN\t{route}");
            var started = DateTime.UtcNow;
            try
            {
                var sample = (FrameworkElement)SampleRegistry.Samples[route]();
                var loaded = new TaskCompletionSource<bool>();
                sample.Loaded += (s, e) => loaded.TrySetResult(true);
                host.Children.Clear();
                host.Children.Add(sample);
                var finished = await Task.WhenAny(loaded.Task, Task.Delay(10000));
                if (finished != loaded.Task) _log.WriteLine($"NOLOAD\t{route}");
                await Task.Delay(settleMs);
                if (!string.IsNullOrEmpty(shots)) await CaptureAsync(host, Path.Combine(shots, route.Replace("/", "__") + ".png"));
                _log.WriteLine($"OK\t{route}\t{(DateTime.UtcNow - started).TotalMilliseconds:F0}ms");
            }
            catch (Exception ex)
            {
                _log.WriteLine($"FAIL\t{route}\t{Flatten(ex)}");
            }
        }
        host.Children.Clear();
        _current = "(done)";
        _log.WriteLine("DONE");
        _log.Flush();
        Application.Current.Exit();
    }

    private static async Task CaptureAsync(UIElement element, string path)
    {
        try
        {
            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(element);
            var pixels = await rtb.GetPixelsAsync();
            using var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, 96, 96, pixels.ToArray());
            await encoder.FlushAsync();
            stream.Seek(0);
            var bytes = new byte[stream.Size];
            await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
            File.WriteAllBytes(path, bytes);
        }
        catch (Exception ex)
        {
            _log.WriteLine($"SHOTFAIL\t{_current}\t{Flatten(ex)}");
        }
    }

    private static string Flatten(Exception ex) =>
        (ex?.GetType().Name + ": " + ex?.Message + " @ " + ex?.StackTrace?.Split('\n').FirstOrDefault()?.Trim())
            .Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
}
