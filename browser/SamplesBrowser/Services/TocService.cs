using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SamplesBrowser.Services;

// ── TOC data model ─────────────────────────────────────────────────────────

public class Toc
{
    [JsonPropertyName("groups")]
    public List<TocGroup> Groups { get; set; } = new();
}

public class TocGroup
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("components")]
    public List<TocComponent> Components { get; set; } = new();
}

public class TocComponent
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("folder")]
    public string Folder { get; set; } = "";

    [JsonPropertyName("samples")]
    public List<TocSample> Samples { get; set; } = new();
}

public class TocSample
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("route")]
    public string Route { get; set; } = "";

    [JsonPropertyName("showLink")]
    public bool ShowLink { get; set; } = true;
}

// ── TocService ──────────────────────────────────────────────────────────────

public class TocService
{
    public Toc? Toc { get; private set; }

    /// <summary>
    /// Loads toc.json from the Assets folder next to the executable.
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            var exeDir = AppContext.BaseDirectory;
            var tocPath = Path.Combine(exeDir, "Assets", "toc.json");

            if (!File.Exists(tocPath))
            {
                System.Diagnostics.Debug.WriteLine($"[TocService] toc.json not found at: {tocPath}");
                Toc = new Toc();
                return;
            }

            await using var stream = File.OpenRead(tocPath);
            Toc = await JsonSerializer.DeserializeAsync<Toc>(stream) ?? new Toc();
            System.Diagnostics.Debug.WriteLine(
                $"[TocService] Loaded {Toc.Groups.Count} group(s) from toc.json");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TocService] Failed to load toc.json: {ex.Message}");
            Toc = new Toc();
        }
    }
}
