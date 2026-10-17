// Scratch-only smoke runner: when SAMPLES_SMOKE=<report path> is set, App hands the launch over to
// this class, which instantiates every registered sample in turn, waits for it to load and settle,
// records exceptions (synchronous, dispatcher and unhandled), optionally captures a PNG, and exits.
//
// After settling it also checks that the sample's main component actually drew something (CONTENT /
// BLANK), and, when the sample has a property editor panel, drives a few of its rows through
// SimulateInput -- the path a user takes -- and checks that the edited component's pixels change
// (PERM ... CHANGED / UNCHANGED / UNHANDLED). Set SAMPLES_SMOKE_PERMS=0 to skip the permutations.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Infragistics.Controls;
using Infragistics.Controls.Layouts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace SamplesBrowser;

public static class SmokeRunner
{
    private static string _current = "(startup)";
    private static StreamWriter _log;
    private static Window _window;
    private const int MaxPermsPerSample = 8;
    private const int PermSettleMs = 1200;

    public static bool TryStart(Application app)
    {
        var report = Environment.GetEnvironmentVariable("SAMPLES_SMOKE");
        if (string.IsNullOrEmpty(report)) return false;
        var shots = Environment.GetEnvironmentVariable("SAMPLES_SMOKE_SHOTS");
        var filter = Environment.GetEnvironmentVariable("SAMPLES_SMOKE_FILTER");
        var settleMs = int.TryParse(Environment.GetEnvironmentVariable("SAMPLES_SMOKE_SETTLE_MS"), out var ms) ? ms : 2500;
        var perms = Environment.GetEnvironmentVariable("SAMPLES_SMOKE_PERMS") != "0";

        _log = new StreamWriter(report, append: true) { AutoFlush = true };
        app.UnhandledException += (s, e) =>
        {
            _log.WriteLine($"UNHANDLED\t{_current}\t{Flatten(e.Exception)}");
            e.Handled = true;
        };
        if (Environment.GetEnvironmentVariable("SAMPLES_SMOKE_FIRSTCHANCE") == "1")
        {
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                try { _log.WriteLine($"FIRSTCHANCE\t{_current}\t{e.Exception.GetType().Name}: {e.Exception.Message} @ {Environment.StackTrace.Split('\n').Skip(4).Take(6).Select(l => l.Trim()).Aggregate((a, b) => a + " | " + b)}".Replace('\r', ' ').Replace('\n', ' ')); } catch { }
            };
            AppDomain.CurrentDomain.ProcessExit += (s, e) => { try { _log.WriteLine($"PROCESSEXIT\t{_current}\t{Environment.StackTrace.Replace('\r', ' ').Replace('\n', '|')}"); } catch { } };
        }
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            _log.WriteLine($"UNOBSERVED\t{_current}\t{Flatten(e.Exception)}");
            e.SetObserved();
        };

        if (Environment.GetEnvironmentVariable("SAMPLES_SMOKE_EXPLICIT") != "0")
        {
            app.DispatcherShutdownMode = DispatcherShutdownMode.OnExplicitShutdown;
        }
        var window = new Window { Title = "Samples smoke" };
        // Light theme on an opaque background, so the captures look like the samples do in the browser.
        var host = new Grid
        {
            Width = 1200,
            Height = 800,
            RequestedTheme = ElementTheme.Light,
            Background = new SolidColorBrush(Microsoft.UI.Colors.White)
        };
        window.Content = host;
        window.Closed += (s, e) => { try { _log.WriteLine($"WINDOWCLOSED\t{_current}\t{Environment.StackTrace.Replace('\r', ' ').Replace('\n', '|')}"); } catch { } };
        if (Environment.GetEnvironmentVariable("SAMPLES_SMOKE_VISIBLE") == "1")
        {
            // On screen and activated, to watch the run.
            window.Activate();
        }
        else try
        {
            // Off-screen and never activated, so the run does not take focus from the desktop.
            window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32 { X = -6000, Y = 0, Width = 1260, Height = 880 });
            window.AppWindow.Show(false);
        }
        catch
        {
            // Hosts without AppWindow placement (Uno desktop) just show the window.
            window.Activate();
        }
        _window = window;
        _ = RunAsync(host, shots, filter, settleMs, perms);
        return true;
    }

    private static async Task RunAsync(Grid host, string shots, string filter, int settleMs, bool perms)
    {
        await Task.Delay(1000);
        LoadMeta();
        // SAMPLES_SMOKE_PROFILE_TYPES=A,B: construct each named type twice and log how long it took.
        if (Environment.GetEnvironmentVariable("SAMPLES_SMOKE_PROFILE_MAP") == "1")
        {
            void Time(string what, Action act)
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                act();
                _log.WriteLine($"PROFILE\t{what}\t{sw.ElapsedMilliseconds}ms");
            }
            var flights = new WorldFlights();
            var airports = new WorldAirports();
            _log.WriteLine($"PROFILE\tflights={flights.Count}\tpaths={flights.Sum(f => f.Points.Count)}\tpoints={flights.Sum(f => f.Points.Sum(p => p.Count))}\tpointType={flights[0].Points[0][0].GetType().FullName}");
            var plain = flights.Select(f => new { Points = f.Points }).ToList();
            var map4 = new Infragistics.Controls.Maps.XamGeographicMap();
            var lines4 = new Infragistics.Controls.Maps.GeographicPolylineSeries { ShapeMemberPath = "Points" };
            map4.Series.Add(lines4);
            Time("polyline ItemsSource=anonymous wrappers (same points)", () => lines4.ItemsSource = plain);
            var map5 = new Infragistics.Controls.Maps.XamGeographicMap();
            var lines5 = new Infragistics.Controls.Maps.GeographicPolylineSeries { ShapeMemberPath = "Points" };
            map5.Series.Add(lines5);
            var half = flights.Take(flights.Count / 2).ToList();
            Time("polyline ItemsSource=half the flights", () => lines5.ItemsSource = half);
            var map = new Infragistics.Controls.Maps.XamGeographicMap();
            var lines = new Infragistics.Controls.Maps.GeographicPolylineSeries { ShapeMemberPath = "Points" };
            var symbols = new Infragistics.Controls.Maps.GeographicSymbolSeries { LatitudeMemberPath = "Lat", LongitudeMemberPath = "Lon" };
            Time("series add (lines)", () => map.Series.Add(lines));
            Time("series add (symbols)", () => map.Series.Add(symbols));
            Time("polyline ItemsSource=WorldFlights", () => lines.ItemsSource = flights);
            Time("symbol ItemsSource=WorldAirports", () => symbols.ItemsSource = airports);
            var lines2 = new Infragistics.Controls.Maps.GeographicPolylineSeries();
            Time("polyline ItemsSource before ShapeMemberPath", () => lines2.ItemsSource = flights);
            Time("then ShapeMemberPath", () => lines2.ShapeMemberPath = "Points");
            var map3 = new Infragistics.Controls.Maps.XamGeographicMap();
            var lines3 = new Infragistics.Controls.Maps.GeographicPolylineSeries { ShapeMemberPath = "Points" };
            Time("detached series ItemsSource", () => lines3.ItemsSource = flights);
            Time("then add to map", () => map3.Series.Add(lines3));
        }
        var profile = Environment.GetEnvironmentVariable("SAMPLES_SMOKE_PROFILE_TYPES");
        if (!string.IsNullOrEmpty(profile))
        {
            foreach (var name in profile.Split(','))
            {
                var type = typeof(SmokeRunner).Assembly.GetTypes().FirstOrDefault(t => t.Name == name);
                for (var pass = 1; pass <= 2 && type != null; pass++)
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var instance = Activator.CreateInstance(type);
                    _log.WriteLine($"PROFILE\t{name}\tpass={pass}\t{sw.ElapsedMilliseconds}ms\tcount={(instance as System.Collections.ICollection)?.Count}");
                }
            }
        }
        // Resumable: a page that took the process down left a BEGIN with nothing after it. Record it as
        // a crash and carry on from the next page instead of retrying it.
        var report = Environment.GetEnvironmentVariable("SAMPLES_SMOKE");
        var attempted = new HashSet<string>();
        string lastBegun = null;
        // The report is already open for appending, so read it through a shared stream.
        var previous = new List<string>();
        if (File.Exists(report))
        {
            using var stream = new FileStream(report, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            string read;
            while ((read = reader.ReadLine()) != null) previous.Add(read);
        }
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
        FrameworkElement previousSample = null;
        string previousRoute = null;
        foreach (var route in routes)
        {
            if (previousSample != null) await TeardownAsync(host, previousSample, previousRoute);
            previousSample = null;
            _current = route;
            _log.WriteLine($"BEGIN\t{route}");
            var started = DateTime.UtcNow;
            try
            {
                // Setup: construction, then until Loaded, then the first frame after it.
                var clock = System.Diagnostics.Stopwatch.StartNew();
                var sample = (FrameworkElement)SampleRegistry.Samples[route]();
                var ctorMs = clock.ElapsedMilliseconds;
                previousSample = sample;
                previousRoute = route;
                var loaded = new TaskCompletionSource<bool>();
                sample.Loaded += (s, e) => loaded.TrySetResult(true);
                host.Children.Add(sample);
                var addMs = clock.ElapsedMilliseconds;
                var finished = await Task.WhenAny(loaded.Task, Task.Delay(10000));
                if (finished != loaded.Task) _log.WriteLine($"NOLOAD\t{route}");
                var loadMs = clock.ElapsedMilliseconds;
                await NextFrameAsync();
                var frameMs = clock.ElapsedMilliseconds;
                _log.WriteLine($"{(frameMs > SlowMs ? "SLOWSETUP" : "SETUP")}\t{route}\tctor={ctorMs}\tadd={addMs}\tloaded={loadMs}\tframe={frameMs}");
                await Task.Delay(settleMs);
                if (Environment.GetEnvironmentVariable("SAMPLES_SMOKE_FIRSTCHANCE") == "1")
                    _log.WriteLine($"HOST	{route}	{host.ActualWidth}x{host.ActualHeight}	root={host.XamlRoot?.Size}	visible={_window.Visible}	contentIsHost={ReferenceEquals(_window.Content, host)}	sampleParent={VisualTreeHelper.GetParent(sample)?.GetType().Name}");
                if (!string.IsNullOrEmpty(shots)) await CaptureAsync(host, Path.Combine(shots, route.Replace("/", "__") + ".png"));

                var panels = Descendants(sample).OfType<XamPropertyEditorPanel>().ToList();
                var content = FindContent(sample, panels);
                var frame = await GrabAsync(content);
                var ink = frame == null ? -1 : Ink(frame);
                _log.WriteLine($"{(ink >= 0 && ink < 0.002 ? "BLANK" : "CONTENT")}\t{route}\t{Describe(content)}\tink={ink:F4}");

                if (perms)
                {
                    foreach (var panel in panels)
                    {
                        // Interaction rows reload the sample; carry on (and tear down) whatever is hosted now.
                        var replaced = await DrivePanelAsync(route, panel, content, host, shots);
                        if (replaced != null) previousSample = replaced;
                    }
                }
                _log.WriteLine($"OK\t{route}\t{(DateTime.UtcNow - started).TotalMilliseconds:F0}ms");
            }
            catch (Exception ex)
            {
                _log.WriteLine($"FAIL\t{route}\t{Flatten(ex)}");
            }
        }
        if (previousSample != null) await TeardownAsync(host, previousSample, previousRoute);
        _current = "(done)";
        _log.WriteLine("DONE");
        _log.Flush();
        Application.Current.Exit();
        // Uno's desktop host does not always leave on Exit(); the report is complete either way.
        await Task.Delay(2000);
        Environment.Exit(0);
    }

    private const int SlowMs = 3000;

    // Teardown: how long removing the sample blocks (sync), until its Unloaded fires, and until the
    // next frame. Popups still open at removal are counted, because an open WinUI popup whose host is
    // removed can make the removal very expensive.
    private static async Task TeardownAsync(Grid host, FrameworkElement sample, string route)
    {
        _current = route + " (teardown)";
        try
        {
            var popups = host.XamlRoot != null ? VisualTreeHelper.GetOpenPopupsForXamlRoot(host.XamlRoot).Count : -1;
            var unloaded = new TaskCompletionSource<bool>();
            sample.Unloaded += (s, e) => unloaded.TrySetResult(true);
            var clock = System.Diagnostics.Stopwatch.StartNew();
            host.Children.Clear();
            var syncMs = clock.ElapsedMilliseconds;
            var finished = await Task.WhenAny(unloaded.Task, Task.Delay(10000));
            var unloadMs = finished == unloaded.Task ? clock.ElapsedMilliseconds : -1;
            await NextFrameAsync();
            var frameMs = clock.ElapsedMilliseconds;
            var left = host.XamlRoot != null ? VisualTreeHelper.GetOpenPopupsForXamlRoot(host.XamlRoot).Count : -1;
            _log.WriteLine($"{(frameMs > SlowMs || unloadMs < 0 ? "SLOWTEARDOWN" : "TEARDOWN")}\t{route}\tsync={syncMs}\tunloaded={unloadMs}\tframe={frameMs}\topenPopups={popups}\tpopupsAfter={left}");
        }
        catch (Exception ex)
        {
            _log.WriteLine($"TEARDOWNFAIL\t{route}\t{Flatten(ex)}");
        }
    }

    // Completes on the next composition frame, or after a second if none comes.
    private static async Task NextFrameAsync()
    {
        var frame = new TaskCompletionSource<bool>();
        EventHandler<object> handler = (s, e) => frame.TrySetResult(true);
        CompositionTarget.Rendering += handler;
        await Task.WhenAny(frame.Task, Task.Delay(1000));
        CompositionTarget.Rendering -= handler;
    }

    // What the content check and the permutations look at: the panel's target when it is an element,
    // otherwise the sample's ContentRoot, otherwise the sample itself.
    private static FrameworkElement FindContent(FrameworkElement sample, List<XamPropertyEditorPanel> panels)
    {
        foreach (var panel in panels)
        {
            if (panel.Target is FrameworkElement target && target.ActualWidth > 0 && target.ActualHeight > 0) return target;
        }
        return Descendants(sample).OfType<FrameworkElement>().FirstOrDefault(e => e.Name == "ContentRoot") ?? sample;
    }

    // ---- Driving the property editor ---------------------------------------------------------
    //
    // Each row is moved to a few other values through the panel's SimulateInput and the target is
    // compared before and after. How the effect becomes visible is the row's strategy:
    //
    //   change           set the value, compare (the default)
    //   hover            hover the target at a point, compare hovered-before with hovered-after
    //   clickThenChange  click the target at a point (to select something), then change the value
    //   press:<row>      press that Button row after the change (and before the baseline)
    //   skip             do not drive the row (with a reason)
    //
    // SAMPLES_SMOKE_META names a JSON file of hand-written guidance (rules by property name, and
    // per-sample rows with a strategy, a point, values to try, or values equivalent to the initial
    // one). With inference on (the default), a row whose values all look inert is tried again with
    // hover, click, and each of the panel's Button rows, and what worked is written to
    // SAMPLES_SMOKE_LEARNED, ready to be folded into the guidance.
    private static System.Text.Json.Nodes.JsonObject _meta;
    private static System.Text.Json.Nodes.JsonObject _learned;
    private static string _learnedPath;
    private const int MaxValuesPerRow = 3;
    private static string _lastProbe;
    private static System.Text.Json.Nodes.JsonObject _requires;
    private static double[] _hostSize;
    private static Grid _host;
    private static List<RowDriver> _allDrivers = new();
    private static readonly double[][] DefaultPoints = { new[] { 0.5, 0.5 }, new[] { 0.35, 0.45 }, new[] { 0.65, 0.55 } };

    private static void LoadMeta()
    {
        var metaPath = Environment.GetEnvironmentVariable("SAMPLES_SMOKE_META");
        if (!string.IsNullOrEmpty(metaPath) && File.Exists(metaPath))
            _meta = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(metaPath)) as System.Text.Json.Nodes.JsonObject;
        _learnedPath = Environment.GetEnvironmentVariable("SAMPLES_SMOKE_LEARNED");
        _learned = new System.Text.Json.Nodes.JsonObject();
        if (!string.IsNullOrEmpty(_learnedPath) && File.Exists(_learnedPath))
            _learned = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(_learnedPath)) as System.Text.Json.Nodes.JsonObject ?? _learned;
    }

    private static void SaveLearned()
    {
        if (string.IsNullOrEmpty(_learnedPath)) return;
        File.WriteAllText(_learnedPath, _learned.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static System.Text.Json.Nodes.JsonObject RowMeta(string route, string label)
    {
        var rows = _meta?["samples"]?[route]?["rows"] as System.Text.Json.Nodes.JsonObject;
        if (rows != null && rows[label] is System.Text.Json.Nodes.JsonObject explicitRow) return explicitRow;
        if (_meta?["rules"] is System.Text.Json.Nodes.JsonArray rules)
        {
            foreach (var rule in rules.OfType<System.Text.Json.Nodes.JsonObject>())
            {
                var pattern = (string)rule["match"];
                var routePattern = (string)rule["routeMatch"];
                if (routePattern != null && !System.Text.RegularExpressions.Regex.IsMatch(route, routePattern)) continue;
                if (pattern != null && label != null && System.Text.RegularExpressions.Regex.IsMatch(label, pattern)) return rule;
            }
        }
        return null;
    }


    // The key a row is known by in the guidance: its property path, else its name, unless the name
    // is a generated GUID (buttons usually), in which case its label reads better.
    private static string RowKey(PropertyEditorPropertyDescription row) =>
        row.PropertyPath ?? (row.Name != null && !Guid.TryParse(row.Name, out _) ? row.Name : row.Label ?? row.Name);

    private sealed class RowDriver
    {
        public string Name, Label, Original;
        public PropertyEditorPropertyDescription Row;
        public List<string> Values = new();
        public Func<string, string[]> PathFor;
        public bool IsToggle, IsButton;
    }

    private static RowDriver MakeDriver(XamPropertyEditorPanel panel, PropertyEditorPropertyDescription row, System.Text.Json.Nodes.JsonObject meta)
    {
        var d = new RowDriver
        {
            Row = row,
            Name = panel.DescriptionType + "_" + PathToKey(row.PropertyPath ?? row.Name),
            Label = RowKey(row),
        };
        var equivalent = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (meta?["equivalentToInitial"] is System.Text.Json.Nodes.JsonArray eq)
            foreach (var v in eq) equivalent.Add((string)v);
        var explicitValues = (meta?["values"] as System.Text.Json.Nodes.JsonArray)?.Select(v => v?.ToString()).ToList();
        switch (row.ValueType)
        {
            case PropertyEditorValueType.EnumValue:
                d.Original = row.PrimitiveValue?.ToString();
                d.Values = (explicitValues ?? (row.DropDownValues ?? Array.Empty<string>()).ToList())
                    .Where(v => !string.Equals(v, d.Original, StringComparison.OrdinalIgnoreCase) && !equivalent.Contains(v))
                    .Take(MaxValuesPerRow).ToList();
                d.PathFor = v => new[] { d.Name, "Combo", "select", v };
                break;
            case PropertyEditorValueType.Slider:
                var min = double.IsNaN(row.Min) ? 0 : row.Min;
                var max = double.IsNaN(row.Max) ? 100 : row.Max;
                d.Original = row.PrimitiveValue != null
                    ? Convert.ToDouble(row.PrimitiveValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
                    : min.ToString(CultureInfo.InvariantCulture);
                d.Values = explicitValues ?? new[] { 0.15, 0.85 }.Select(f => (min + f * (max - min)).ToString(CultureInfo.InvariantCulture)).ToList();
                d.PathFor = v => new[] { d.Name, "Slider", "value", v };
                break;
            case PropertyEditorValueType.Boolean:
                d.IsToggle = true;
                d.Values = new() { "toggle" };
                d.PathFor = v => new[] { d.Name };
                break;
            case PropertyEditorValueType.Button:
                d.IsButton = true;
                d.Values = new() { "click" };
                d.PathFor = v => new[] { d.Name };
                break;
            default:
                return null;
        }
        return d;
    }

    private static bool Apply(XamPropertyEditorPanel panel, RowDriver d, string value)
    {
        var result = panel.SimulateInput(SimulatedInputType.Click, d.PathFor(value), 0, 0, null, null);
        return result != null && result.IsHandled;
    }

    // Puts the row back the way the sample started, so the next row is seen against the sample's
    // own state rather than whatever the last row left behind.
    private static void Restore(XamPropertyEditorPanel panel, RowDriver d)
    {
        if (d.IsButton) return;
        if (d.IsToggle) { Apply(panel, d, "toggle"); return; }
        if (d.Original != null) Apply(panel, d, d.Original);
    }

    private static bool Pointer(FrameworkElement target, string method, double[] at)
    {
        var m = target.GetType().GetMethod(method, new[] { typeof(IGPoint) });
        if (m == null) return false;
        m.Invoke(target, new object[] { new IGPoint(at[0] * target.ActualWidth, at[1] * target.ActualHeight) });
        return true;
    }

    private static void PointerLeave(FrameworkElement target)
    {
        var leave = target.GetType().GetMethod("SimulateMouseLeave", Type.EmptyTypes);
        if (leave != null) leave.Invoke(target, null);
        else Pointer(target, "SimulateHover", new[] { -1.0, -1.0 });
    }

    private static double[] PointFrom(System.Text.Json.Nodes.JsonObject meta) =>
        meta?["at"] is System.Text.Json.Nodes.JsonArray a && a.Count == 2 ? new[] { (double)a[0], (double)a[1] } : null;

    // One value under one strategy: returns (handled, diff, whole) where diff compares the target
    // in the strategy's baseline state with the target after the change.
    private static async Task<(bool Handled, double Diff, double Whole)> TryStepAsync(
        XamPropertyEditorPanel panel, FrameworkElement target, Grid host, RowDriver d, string value,
        string strategy, double[] at, Dictionary<string, RowDriver> buttons)
    {
        string press = strategy.StartsWith("press:") ? strategy.Substring(6) : null;
        // Some settings only matter when the component is cramped (label rotation, for one).
        var oldSize = (host.Width, host.Height);
        var resize = strategy == "resize" && _hostSize != null;
        if (_hostSize != null && !resize)
        {
            host.Width = _hostSize[0];
            host.Height = _hostSize[1];
            await Task.Delay(PermSettleMs);
        }
        var required = new List<RowDriver>();
        if (_requires != null)
        {
            foreach (var kv in _requires)
            {
                var other = _allDrivers.FirstOrDefault(o => o.Label == kv.Key);
                if (other == null) continue;
                Apply(panel, other, kv.Value?.ToString());
                required.Add(other);
            }
            if (required.Count > 0) await Task.Delay(PermSettleMs);
        }
        async Task Unprepare()
        {
            if (resize) { host.Width = oldSize.Width; host.Height = oldSize.Height; await Task.Delay(PermSettleMs); }
        }
        async Task Prepare()
        {
            if (resize) { host.Width = _hostSize[0]; host.Height = _hostSize[1]; await Task.Delay(PermSettleMs); }
            if (strategy == "hover") { Pointer(target, "SimulateHover", at); await Task.Delay(400); }
            if (press != null && buttons.TryGetValue(press, out var b)) { Apply(panel, b, "click"); await Task.Delay(PermSettleMs); }
        }
        if (strategy == "clickThenChange") { Pointer(target, "SimulateClick", at); await Task.Delay(400); }
        await Prepare();
        var before = await GrabAsync(target);
        var wholeBefore = await GrabAsync(host);
        await Unprepare();
        var probeBefore = ProbePath(target, d.Row.PropertyPath);
        var handled = Apply(panel, d, value);
        await Task.Delay(PermSettleMs);
        _lastProbe = probeBefore + "->" + ProbePath(target, d.Row.PropertyPath);
        if (Environment.GetEnvironmentVariable("SAMPLES_SMOKE_DUMP_DESC") == "1")
        {
            try
            {
                var impl = panel.GetType().GetField("_implementation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(panel);
                var json = impl?.GetType().GetMethod("GetDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(impl, null);
                _log.WriteLine("DESC	" + _current + "	" + d.Label + "=" + value + "	" + System.Text.RegularExpressions.Regex.Replace(json?.ToString() ?? "", @"\s+", " "));
            }
            catch (Exception ex) { _log.WriteLine($"DESC	{_current}	{Flatten(ex)}"); }
        }
        await Prepare();
        var after = await GrabAsync(target);
        var whole = Diff(wholeBefore, await GrabAsync(host));
        await Unprepare();
        var diff = Diff(before, after);
        Restore(panel, d);
        await Task.Delay(300);
        if (strategy == "hover") PointerLeave(target);
        if (strategy == "clickThenChange") { Pointer(target, "SimulateClick", at); await Task.Delay(300); }
        if (press != null && buttons.TryGetValue(press, out var b2)) { Apply(panel, b2, "click"); await Task.Delay(300); }
        foreach (var other in required) Restore(panel, other);
        if (required.Count > 0) await Task.Delay(300);
        if (_hostSize != null && !resize)
        {
            host.Width = oldSize.Width;
            host.Height = oldSize.Height;
            await Task.Delay(300);
        }
        return (handled, diff, whole);
    }


    // Reads a property editor path ("Series[0 as ScatterContourSeries].ValueResolver.ValueCount")
    // off the target by reflection, so a step can say whether the value actually landed.
    private static string ProbePath(object target, string path)
    {
        if (target == null || string.IsNullOrEmpty(path)) return "?";
        try
        {
            object current = target;
            foreach (var raw in path.Split('.'))
            {
                var part = raw;
                var asAt = part.IndexOf(" as ");
                if (asAt > 0 && part.IndexOf('[') < 0) part = part.Substring(0, asAt);
                int? index = null;
                var open = part.IndexOf('[');
                if (open >= 0)
                {
                    var inside = part.Substring(open + 1, part.IndexOf(']') - open - 1).Split(' ')[0];
                    index = int.Parse(inside, CultureInfo.InvariantCulture);
                    part = part.Substring(0, open);
                }
                var prop = current.GetType().GetProperty(part);
                if (prop == null) return "?no " + part;
                current = prop.GetValue(current);
                if (index != null) current = ((System.Collections.IList)current)[index.Value];
                if (current == null) return "null";
            }
            return Convert.ToString(current, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            return "?" + ex.GetType().Name;
        }
    }

    private static string Classify(bool handled, double diff, double whole) =>
        diff > 0.00003 ? "CHANGED" : !handled ? "UNHANDLED" : whole > 0.00003 ? "WHOLEONLY" : "UNCHANGED";

    // ---- Interaction rows: a fresh sample per value ---------------------------------------------
    //
    // strategy "interact" with "steps": [{ "type": "Click|DoubleClick|MouseEnter|KeyDown",
    // "cell": [visibleRow, columnIndex] | "header": columnIndex | "at": [fx, fy], "data": [...] }].
    // Interaction leaves state behind (active cell, selection, sort, an open editor), so the sample
    // is reloaded for the baseline and for each value, the value set, the steps run, and the
    // target compared with the baseline. Grid cells and headers are addressed by the automation
    // names the grid's visual model export reports, the way the XSharpTesting grid steps do.
    private static async Task<(FrameworkElement Sample, XamPropertyEditorPanel Panel, FrameworkElement Target)> ReloadAsync(Grid host, string route)
    {
        host.Children.Clear();
        var sample = (FrameworkElement)SampleRegistry.Samples[route]();
        var loaded = new TaskCompletionSource<bool>();
        sample.Loaded += (s, e) => loaded.TrySetResult(true);
        host.Children.Add(sample);
        await Task.WhenAny(loaded.Task, Task.Delay(10000));
        await Task.Delay(1500);
        var panel = Descendants(sample).OfType<XamPropertyEditorPanel>().FirstOrDefault();
        var target = FindContent(sample, panel == null ? new List<XamPropertyEditorPanel>() : new List<XamPropertyEditorPanel> { panel });
        return (sample, panel, target);
    }

    private static string GridAutomationName(Infragistics.Controls.Grids.XamDataGrid grid, System.Text.Json.Nodes.JsonObject step)
    {
        var model = grid.ExportVisualModel();
        var headers = model.HeaderLayer?.Cells?.Where(c => !string.IsNullOrEmpty(c.AutomationName)).OrderBy(c => c.X).ToList()
            ?? new List<Infragistics.Controls.Grids.ExportData.CellModelExport>();
        if (step["header"] != null)
        {
            var i = (int)step["header"];
            return i < headers.Count ? headers[i].AutomationName : null;
        }
        if (step["cell"] is System.Text.Json.Nodes.JsonArray rc)
        {
            var row = (int)rc[0];
            var col = (int)rc[1];
            var cells = model.ContentLayer?.Cells?.Where(c => !string.IsNullOrEmpty(c.AutomationName)).ToList()
                ?? new List<Infragistics.Controls.Grids.ExportData.CellModelExport>();
            var rows = cells.Select(c => Math.Round(c.Y)).Distinct().OrderBy(y => y).ToList();
            if (row >= rows.Count) return null;
            var inRow = cells.Where(c => Math.Round(c.Y) == rows[row]).OrderBy(c => c.X).ToList();
            if (col < headers.Count)
            {
                var x = headers[col].X;
                var match = inRow.FirstOrDefault(c => Math.Abs(c.X - x) < 1.0);
                if (match != null) return match.AutomationName;
            }
            return col < inRow.Count ? inRow[col].AutomationName : null;
        }
        return null;
    }

    private static async Task RunStepsAsync(FrameworkElement target, System.Text.Json.Nodes.JsonArray steps)
    {
        foreach (var step in steps.OfType<System.Text.Json.Nodes.JsonObject>())
        {
            var type = Enum.Parse<SimulatedInputType>((string)step["type"]);
            var data = (step["data"] as System.Text.Json.Nodes.JsonArray)?.Select(d => (string)d).ToArray();
            string[] path = null;
            double x = 5, y = 5;
            if (target is Infragistics.Controls.Grids.XamDataGrid grid && (step["cell"] != null || step["header"] != null))
            {
                var name = GridAutomationName(grid, step);
                if (name == null) { _log.WriteLine($"STEPFAIL\t{_current}\tno automation name for {step.ToJsonString()}"); continue; }
                path = new[] { name };
            }
            else if (step["at"] is System.Text.Json.Nodes.JsonArray a)
            {
                x = (double)a[0] * target.ActualWidth;
                y = (double)a[1] * target.ActualHeight;
            }
            var m = target.GetType().GetMethod("SimulateInput");
            if (m == null) { _log.WriteLine($"STEPFAIL\t{_current}\t{target.GetType().Name} has no SimulateInput"); return; }
            m.Invoke(target, new object[] { type, path, x, y, data, null });
            await Task.Delay(400);
        }
    }


    // Grid state rather than pixels: every exported cell's layer, type, automation name, selection,
    // activation, sort and background - but not its value, so random sample data does not register.
    // Comparing two of these counts the cells whose state differs, which is what a grid
    // interaction setting changes.
    private static HashSet<string> GridFingerprint(Infragistics.Controls.Grids.XamDataGrid grid)
    {
        var set = new HashSet<string>();
        var model = grid.ExportVisualModel();
        foreach (var prop in model.GetType().GetProperties())
        {
            if (prop.GetValue(model) is not Infragistics.Controls.Grids.ExportData.GridLayerExport layer || layer.Cells == null) continue;
            foreach (var c in layer.Cells)
                set.Add($"{prop.Name}|{c.GetType().Name}|{c.AutomationName}|{c.SelectedStatus}|{c.ActivationStatus}|{c.SortDirection}|{c.BackgroundBrushData?.Serialize()}");
        }
        return set;
    }

    private static int FingerprintDiff(HashSet<string> a, HashSet<string> b) =>
        a == null || b == null ? -1 : a.Count(x => !b.Contains(x)) + b.Count(x => !a.Contains(x));

    private static async Task<FrameworkElement> DriveInteractionRowsAsync(string route, Grid host, List<(string Label, System.Text.Json.Nodes.JsonObject Meta, PropertyEditorPropertyDescription Row)> rows)
    {
        FrameworkElement current = null;
        foreach (var (label, meta, row) in rows)
        {
            var steps = meta["steps"] as System.Text.Json.Nodes.JsonArray;
            var baseline = await ReloadAsync(host, route);
            current = baseline.Sample;
            await RunStepsAsync(baseline.Target, steps);
            await Task.Delay(PermSettleMs);
            var before = await GrabAsync(baseline.Target);
            var beforeState = baseline.Target is Infragistics.Controls.Grids.XamDataGrid bg ? GridFingerprint(bg) : null;
            // Control: a second fresh baseline with the same steps. Whatever differs between the two
            // is noise (animation, live data, timing), and a value only counts if it clearly beats it.
            var control = await ReloadAsync(host, route);
            current = control.Sample;
            await RunStepsAsync(control.Target, steps);
            await Task.Delay(PermSettleMs);
            var noise = Diff(before, await GrabAsync(control.Target));
            var stateNoise = control.Target is Infragistics.Controls.Grids.XamDataGrid cg ? FingerprintDiff(beforeState, GridFingerprint(cg)) : -1;
            _log.WriteLine($"CONTROL	{route}	{label}	noise={noise:F4}	stateNoise={stateNoise}");
            var d = baseline.Panel == null ? null : MakeDriver(baseline.Panel, row, meta);
            if (d == null) continue;
            foreach (var value in d.Values)
            {
                var fresh = await ReloadAsync(host, route);
                current = fresh.Sample;
                var fd = MakeDriver(fresh.Panel, fresh.Panel.Properties.Cast<PropertyEditorPropertyDescription>().First(r => RowKey(r) == label), meta);
                var handled = Apply(fresh.Panel, fd, value);
                await Task.Delay(PermSettleMs);
                var probe = ProbePath(fresh.Target, row.PropertyPath);
                await RunStepsAsync(fresh.Target, steps);
                await Task.Delay(PermSettleMs);
                var after = await GrabAsync(fresh.Target);
                var diff = Diff(before, after);
                var stateDiff = fresh.Target is Infragistics.Controls.Grids.XamDataGrid fg ? FingerprintDiff(beforeState, GridFingerprint(fg)) : -1;
                // Grids are judged on state (cells whose state differs beyond the control), anything else on pixels.
                var outcome = stateDiff >= 0
                    ? (stateDiff > Math.Max(0, stateNoise) ? "CHANGED" : "UNCHANGED")
                    : noise > 0.00003 && diff <= noise * 2 ? "NOISY" : Classify(handled, diff, diff);
                _log.WriteLine($"PERM	{route}	{label}={value}	{outcome}	probe={probe}	diff={diff:F4}	noise={noise:F4}	state={stateDiff}/{stateNoise}	strategy=interact");
            }
        }
        return current;
    }

    private static async Task<FrameworkElement> DrivePanelAsync(string route, XamPropertyEditorPanel panel, FrameworkElement fallback, Grid host, string shots)
    {
        var target = panel.Target is FrameworkElement t && t.ActualWidth > 0 && t.ActualHeight > 0 ? t : fallback;
        var rows = (panel.Properties ?? panel.ActualProperties)?.Cast<PropertyEditorPropertyDescription>().ToList()
            ?? new List<PropertyEditorPropertyDescription>();
        var infer = Environment.GetEnvironmentVariable("SAMPLES_SMOKE_INFER") != "0";
        var drivers = rows.Select(r => (Row: r, Meta: RowMeta(route, RowKey(r)))).ToList();
        _allDrivers = drivers.Select(x => MakeDriver(panel, x.Row, x.Meta)).Where(x => x != null).ToList();
        var buttons = new Dictionary<string, RowDriver>();
        foreach (var (row, meta) in drivers)
        {
            var d = row.ValueType == PropertyEditorValueType.Button ? MakeDriver(panel, row, meta) : null;
            if (d != null) buttons[d.Label] = d;
        }
        var done = 0;
        var interactions = new List<(string Label, System.Text.Json.Nodes.JsonObject Meta, PropertyEditorPropertyDescription Row)>();
        foreach (var (row, meta) in drivers)
        {
            if ((string)meta?["strategy"] == "interact")
            {
                interactions.Add((RowKey(row), meta, row));
                continue;
            }
            if (done >= MaxPermsPerSample) break;
            var label = RowKey(row);
            _requires = null;
            _hostSize = null;
            var strategy = (string)meta?["strategy"] ?? "change";
            if (strategy == "skip")
            {
                _log.WriteLine($"SKIP\t{route}\t{label}\t{(string)meta?["reason"]}");
                continue;
            }
            var d = MakeDriver(panel, row, meta);
            if (d == null)
            {
                _log.WriteLine($"NOACTION\t{route}\t{label}\t{row.ValueType}");
                continue;
            }
            var at = PointFrom(meta);
            _requires = meta?["requires"] as System.Text.Json.Nodes.JsonObject;
            _hostSize = meta?["hostSize"] is System.Text.Json.Nodes.JsonArray hs && hs.Count == 2 ? new[] { (double)hs[0], (double)hs[1] } : null;
            var outcomes = new List<(string Value, string Outcome, double Diff, double Whole)>();
            foreach (var value in d.Values)
            {
                if (done >= MaxPermsPerSample) break;
                done++;
                try
                {
                    var r = await TryStepAsync(panel, target, host, d, value, strategy, at ?? DefaultPoints[0], buttons);
                    outcomes.Add((value, Classify(r.Handled, r.Diff, r.Whole) + "	probe=" + _lastProbe, r.Diff, r.Whole));
                    if (!string.IsNullOrEmpty(shots) && done <= 2)
                        await CaptureAsync(host, Path.Combine(shots, route.Replace("/", "__") + $"__perm{done}.png"));
                }
                catch (Exception ex)
                {
                    outcomes.Add((value, "ERROR\t" + Flatten(ex), 0, 0));
                }
            }
            foreach (var o in outcomes)
                _log.WriteLine($"PERM\t{route}\t{label}={o.Value}\t{o.Outcome}\tdiff={o.Diff:F4}\twhole={o.Whole:F4}\tstrategy={strategy}");

            // Inference: every value looked inert under the given strategy (and the row is not a
            // button, whose own effect is the thing being pressed), so try the others.
            var inert = outcomes.Count > 0 && outcomes.All(o => !o.Outcome.StartsWith("CHANGED") && !o.Outcome.StartsWith("ERROR"));
            var known = (string)meta?["knownIssue"];
            if (inert && known != null) _log.WriteLine($"KNOWN	{route}	{label}	{known}");
            if (!infer || !inert || d.IsButton || meta?["strategy"] != null || known != null || meta?["requires"] != null) continue;
            var candidates = new List<(string Strategy, double[] At)>();
            if (target.GetType().GetMethod("SimulateHover", new[] { typeof(IGPoint) }) != null)
            {
                foreach (var p in at != null ? new[] { at } : DefaultPoints) candidates.Add(("hover", p));
                foreach (var p in at != null ? new[] { at } : DefaultPoints) candidates.Add(("clickThenChange", p));
            }
            foreach (var b in buttons.Keys) candidates.Add(("press:" + b, null));
            var probe = d.Values[0];
            string found = null;
            double[] foundAt = null;
            foreach (var c in candidates)
            {
                try
                {
                    var r = await TryStepAsync(panel, target, host, d, probe, c.Strategy, c.At ?? DefaultPoints[0], buttons);
                    var outcome = Classify(r.Handled, r.Diff, r.Whole);
                    _log.WriteLine($"INFER\t{route}\t{label}={probe}\t{outcome}\tdiff={r.Diff:F4}\tstrategy={c.Strategy}\tat={(c.At == null ? "" : string.Join(",", c.At))}");
                    if (outcome == "CHANGED") { found = c.Strategy; foundAt = c.At; break; }
                }
                catch (Exception ex)
                {
                    _log.WriteLine($"INFER\t{route}\t{label}={probe}\tERROR\t{Flatten(ex)}\tstrategy={c.Strategy}");
                }
            }
            var sample = _learned[route] as System.Text.Json.Nodes.JsonObject;
            if (sample == null) { sample = new System.Text.Json.Nodes.JsonObject { ["rows"] = new System.Text.Json.Nodes.JsonObject() }; _learned[route] = sample; }
            var entry = new System.Text.Json.Nodes.JsonObject
            {
                ["strategy"] = found ?? "unknown",
                ["inferred"] = true,
                ["valueType"] = row.ValueType.ToString(),
                ["inertUnderChange"] = new System.Text.Json.Nodes.JsonArray(outcomes.Select(o => (System.Text.Json.Nodes.JsonNode)o.Value).ToArray()),
            };
            if (foundAt != null) entry["at"] = new System.Text.Json.Nodes.JsonArray(foundAt[0], foundAt[1]);
            ((System.Text.Json.Nodes.JsonObject)sample["rows"])[label] = entry;
            SaveLearned();
            _log.WriteLine($"LEARNED\t{route}\t{label}\t{found ?? "unknown"}");
        }
        return interactions.Count > 0 ? await DriveInteractionRowsAsync(route, host, interactions) : null;
    }

    private static string PathToKey(string path) => path?
        .Replace(".", "__dot__").Replace("[", "__ob__").Replace("]", "__cb__")
        .Replace(" ", "__sp__").Replace("*", "__star__");

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }

    private static string Describe(FrameworkElement e) =>
        e.GetType().Name + (string.IsNullOrEmpty(e.Name) ? "" : "#" + e.Name) + $"({e.ActualWidth:F0}x{e.ActualHeight:F0})";

    private sealed class Frame
    {
        public int Width, Height;
        public byte[] Pixels;
    }

    private static async Task<Frame> GrabAsync(UIElement element)
    {
        try
        {
            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(element);
            var pixels = await rtb.GetPixelsAsync();
            return new Frame { Width = rtb.PixelWidth, Height = rtb.PixelHeight, Pixels = pixels.ToArray() };
        }
        catch (Exception ex)
        {
            _log.WriteLine($"GRABFAIL\t{_current}\t{Flatten(ex)}");
            return null;
        }
    }

    // Fraction of pixels that differ noticeably from the most common colour: near zero for an empty
    // or single-colour surface.
    private static double Ink(Frame f)
    {
        if (f.Pixels.Length < 4) return 0;
        var counts = new Dictionary<uint, int>();
        for (var i = 0; i + 3 < f.Pixels.Length; i += 16)
        {
            var key = BitConverter.ToUInt32(f.Pixels, i) & 0xF0F0F0F0;
            counts[key] = counts.TryGetValue(key, out var c) ? c + 1 : 1;
        }
        var bg = counts.OrderByDescending(kv => kv.Value).First().Key;
        int total = 0, ink = 0;
        for (var i = 0; i + 3 < f.Pixels.Length; i += 4)
        {
            total++;
            if ((BitConverter.ToUInt32(f.Pixels, i) & 0xF0F0F0F0) != bg) ink++;
        }
        return (double)ink / total;
    }

    private static double Diff(Frame a, Frame b)
    {
        if (a == null || b == null) return -1;
        if (a.Width != b.Width || a.Height != b.Height) return 1;
        int total = 0, changed = 0;
        for (var i = 0; i + 3 < a.Pixels.Length; i += 4)
        {
            total++;
            if (Math.Abs(a.Pixels[i] - b.Pixels[i]) > 12 || Math.Abs(a.Pixels[i + 1] - b.Pixels[i + 1]) > 12 ||
                Math.Abs(a.Pixels[i + 2] - b.Pixels[i + 2]) > 12 || Math.Abs(a.Pixels[i + 3] - b.Pixels[i + 3]) > 12)
                changed++;
        }
        return total == 0 ? 0 : (double)changed / total;
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
