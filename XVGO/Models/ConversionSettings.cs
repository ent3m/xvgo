namespace XVGO.Models;

public sealed class ConversionSettings
{
    /// <summary>
    /// The XAML control to be generated.
    /// </summary>
    public OutputStrategy Strategy { get; set; } = OutputStrategy.Canvas;

    /// <summary>
    /// Where the generated controls will be placed.
    /// </summary>
    public UsageContext Context { get; set; } = UsageContext.Standalone;

    /// <summary>
    /// Whether to generate Avalonia-compatible XAML. Specifically, StrokeJoin and StrokeLineCap properties.
    /// </summary>
    public bool AvaloniaCompatibility { get; set; } = false;

    /// <summary>
    /// Number of spaces per indentation.
    /// </summary>
    public int IndentSpaces { get; set; } = 4;

    /// <summary>
    /// Whether to use tab character for indentation instead of spaces.
    /// </summary>
    public bool TabIndent { get; set; } = false;

    /// <summary>
    /// Whether to place properties on multiple lines for better readability. Disable for more compact outputs.
    /// </summary>
    public bool MultiLine { get; set; } = false;

    /// <summary>
    /// Color value used when converting 'currentColor' to XAML.
    /// </summary>
    public string CurrentColorValue { get; set; } = "#000000";

    /// <summary>
    /// Default value for 'x:Key' when <see cref="Context"/> is <see cref="UsageContext.ResourceDictionary"/>.
    /// </summary>
    public string DefaultKey { get; set; } = "Icon";

    /// <summary>
    /// Whether to merge paths that spatially overlap.
    /// </summary>
    public bool ForceMerge { get; set; } = false;
}
