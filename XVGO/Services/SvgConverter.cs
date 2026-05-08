using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using XVGO.Extensions;
using XVGO.Models;

namespace XVGO.Services;

internal interface ISvgConverter
{
    /// <summary>
    /// Converts SVG markup to XAML.
    /// Supports: paths, fills, strokes, opacity, fill-rule, inline and inherited styles and attributes.
    /// Limitations: No gradients, patterns, text, images, clip paths, shapes, or complex transforms.
    /// </summary>
    string Convert(string svgString);
}

internal partial class SvgConverter(ConversionSettings settings) : ISvgConverter
{
    private readonly ConversionSettings _settings = settings;

    public string Convert(string svgString)
    {
        if (string.IsNullOrWhiteSpace(svgString))
            throw new ArgumentException("SVG string cannot be empty", nameof(svgString));

        var doc = XDocument.Parse(svgString);
        var svgRoot = doc.Root;

        if (svgRoot?.Name.LocalName != "svg")
            throw new ArgumentException("Root element must be <svg>", nameof(svgString));

        var viewBox = TryParseViewBox(svgRoot.Attribute("viewBox")?.Value);
        var drawables = ExtractDrawables(svgRoot);

        return _settings.Strategy switch
        {
            OutputStrategy.Canvas => GenerateCanvas(drawables, viewBox),
            OutputStrategy.DrawingBrush => GenerateDrawingBrush(drawables, viewBox),
            OutputStrategy.DrawingImage => GenerateDrawingImage(drawables, viewBox),
            OutputStrategy.PathIcon => GeneratePathIcon(drawables, viewBox),
            _ => throw new InvalidOperationException($"Unsupported output strategy: {_settings.Strategy}")
        };
    }

    private static List<DrawableElement> ExtractDrawables(XElement svgRoot)
    {
        var drawables = new List<DrawableElement>();

        foreach (var element in svgRoot.Descendants())
        {
            var localName = element.Name.LocalName;

            string? pathData = localName == "path"
                ? element.Attribute("d")?.Value : null;

            if (string.IsNullOrWhiteSpace(pathData))
                continue;

            drawables.Add(new DrawableElement(
                pathData,
                ResolveFill(element),
                ResolveFillRule(element),
                ResolveStroke(element),
                ResolveStrokeLineCap(element),
                ResolveStrokeLineJoin(element),
                ResolveStrokeMiterLimit(element),
                ResolveStrokeDashArray(element),
                ResolveStrokeDashOffset(element),
                ResolveStrokeWidth(element),
                ResolveOpacity(element),
                ResolveFillOpacity(element),
                ResolveStrokeOpacity(element)
            ));
        }

        return drawables;
    }

    #region XAML
    // Viewbox -> Canvas -> Path
    private string GenerateCanvas(IReadOnlyList<DrawableElement> drawables, ViewBox? viewBox)
    {
        if (drawables.Count < 1)
            throw new ArgumentException("SVG must contain at least one drawable element", nameof(drawables));

        var sb = new StringBuilder();
        int indentLevel = 0;

        bool requiresViewbox = _settings.Context == UsageContext.Standalone;
        // Viewbox opening
        if (requiresViewbox)
        {
            ReadOnlySpan<string> viewBoxAttrs = viewBox is null ? [] : [$"Width=\"{FormatDouble(viewBox.Width)}\"", $"Height=\"{FormatDouble(viewBox.Height)}\""];
            FormatControl(sb, "Viewbox", 0, false, viewBoxAttrs);
            indentLevel++;
        }

        // We don't need a canvas when there is only one path and no viewBox and standalone usage
        bool requiresCanvas = drawables.Count > 1 || viewBox is not null || _settings.Context == UsageContext.ResourceDictionary;
        // Canvas opening
        if (requiresCanvas)
        {
            var canvasAttrs = new List<string>();
            // Canvas is a UIElement with a visual parent. It cannot be a shared resource.
            if (_settings.Context == UsageContext.ResourceDictionary)
            {
                canvasAttrs.Add($"x:Key=\"{_settings.DefaultKey}\"");
                canvasAttrs.Add($"x:Shared=\"False\"");
            }
            // Canvas size for viewBox size
            if (viewBox is not null)
            {
                canvasAttrs.Add($"Width=\"{FormatDouble(viewBox.Width)}\"");
                canvasAttrs.Add($"Height=\"{FormatDouble(viewBox.Height)}\"");

                if (viewBox.HasOffsets && _settings.AvaloniaCompatibility)
                    canvasAttrs.Add($"RenderTransform=\"{viewBox.GetTransformString()}\"");
            }

            FormatControl(sb, "Canvas", indentLevel, false, CollectionsMarshal.AsSpan(canvasAttrs));
            indentLevel++;

            if (viewBox?.HasOffsets is true && !_settings.AvaloniaCompatibility)
            {
                sb.AppendLine($"{GetIndent(indentLevel)}<Canvas.RenderTransform>");
                indentLevel++;
                FormatControl(sb, "TranslateTransform", indentLevel, true, $"X=\"{FormatDouble(-1 * viewBox.X)}\"", $"Y=\"{FormatDouble(-1 * viewBox.Y)}\"");
                indentLevel--;
                sb.AppendLine($"{GetIndent(indentLevel)}</Canvas.RenderTransform>");
            }
        }

        // Paths
        foreach (var drawable in drawables)
        {
            List<string> attrs = [];

            // Bake fill-opacity into fill's alpha
            if (FormatColor(drawable.Fill, drawable.FillOpacity) is string fill)
                attrs.Add($"Fill=\"{fill}\"");

            if (drawable.Opacity < 1.0)
                attrs.Add($"Opacity=\"{FormatDouble(drawable.Opacity)}\"");

            // Bake stroke-opacity into stroke's alpha
            if (FormatColor(drawable.Stroke, drawable.StrokeOpacity) is string stroke)
            {
                attrs.Add($"Stroke=\"{stroke}\"");

                if (drawable.StrokeWidth != DefaultStrokeWidth)
                    attrs.Add($"StrokeThickness=\"{FormatDouble(drawable.StrokeWidth)}\"");

                if (drawable.StrokeLineJoin != DefaultStrokeLineJoin)
                {
                    if (_settings.AvaloniaCompatibility)
                        attrs.Add($"StrokeJoin=\"{drawable.StrokeLineJoin}\"");
                    else
                        attrs.Add($"StrokeLineJoin=\"{drawable.StrokeLineJoin}\"");
                }

                // Default XAML value is 10.0, but default SVG is 4.0
                if (drawable.StrokeLineJoin == StrokeLineJoin.Miter && drawable.StrokeMiterLimit != 10.0)
                    attrs.Add($"StrokeMiterLimit=\"{FormatDouble(drawable.StrokeMiterLimit)}\"");

                // SVG stroke-linecap applies to the start, end, and dash caps in XAML
                if (drawable.StrokeLineCap != DefaultStrokeLineCap)
                {
                    if (_settings.AvaloniaCompatibility)
                        attrs.Add($"StrokeLineCap=\"{drawable.StrokeLineCap}\"");
                    else
                    {
                        attrs.Add($"StrokeStartLineCap=\"{drawable.StrokeLineCap}\"");
                        attrs.Add($"StrokeEndLineCap=\"{drawable.StrokeLineCap}\"");
                        attrs.Add($"StrokeDashCap=\"{drawable.StrokeLineCap}\"");
                    }
                }

                if (drawable.StrokeDashArray.Length > 0)
                {
                    attrs.Add($"StrokeDashArray=\"{FormatDoubleArray(drawable.StrokeDashArray)}\"");

                    if (drawable.StrokeDashOffset != DefaultStrokeDashOffset)
                        attrs.Add($"StrokeDashOffset=\"{FormatDouble(drawable.StrokeDashOffset)}\"");
                }
            }

            attrs.Add($"Data=\"{FormatFillRule(drawable.FillRule)}{drawable.PathData}\"");
            FormatControl(sb, "Path", indentLevel, true, CollectionsMarshal.AsSpan(attrs));
        }
        indentLevel--;

        // Canvas closing
        if (requiresCanvas)
            sb.AppendLine($"{GetIndent(indentLevel)}</Canvas>");

        // Viewbox closing
        if (requiresViewbox)
            sb.AppendLine("</Viewbox>");

        return sb.TrimEnd().ToString();
    }

    // Rectangle -> DrawingBrush -> DrawingGroup -> GeometryDrawing
    private string GenerateDrawingBrush(IReadOnlyList<DrawableElement> drawables, ViewBox? viewBox)
    {
        if (drawables.Count < 1)
            throw new ArgumentException("SVG must contain at least one drawable element", nameof(drawables));

        var sb = new StringBuilder();
        int indentLevel = 0;

        var requiresContainer = _settings.Context == UsageContext.Standalone;
        // Container opening
        if (requiresContainer)
        {
            ReadOnlySpan<string> rectAttrs = viewBox is null ? [] : [$"Width=\"{FormatDouble(viewBox.Width)}\"", $"Height=\"{FormatDouble(viewBox.Height)}\""];
            FormatControl(sb, "Rectangle", 0, false, rectAttrs);
            indentLevel++;
            sb.AppendLine($"{GetIndent(indentLevel)}<Rectangle.Fill>");
            indentLevel++;
        }

        // DrawingBrush opening
        var brushAttrs = new List<string>();
        if (_settings.Context == UsageContext.ResourceDictionary)
            brushAttrs.Add($"x:Key=\"{_settings.DefaultKey}\"");
        if (viewBox != null)
        {
            if (_settings.AvaloniaCompatibility)
            {
                // Default Stretch is already Uniform in Avalonia
                brushAttrs.Add($"SourceRect=\"{FormatDoubleArray(viewBox.ToArray())}\"");
            }
            else
            {
                brushAttrs.Add($"Viewbox=\"{FormatDoubleArray(viewBox.ToArray())}\"");
                brushAttrs.Add("ViewboxUnits=\"Absolute\"");
                brushAttrs.Add("Stretch=\"Uniform\"");
            }
        }
        FormatControl(sb, "DrawingBrush", indentLevel, false, CollectionsMarshal.AsSpan(brushAttrs));
        indentLevel++;
        sb.AppendLine($"{GetIndent(indentLevel)}<DrawingBrush.Drawing>");
        indentLevel++;

        var requiresGroup = drawables.Count > 1;
        // DrawingGroup opening
        if (requiresGroup)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}<DrawingGroup>");
            indentLevel++;
        }

        // Geometries
        foreach (var drawable in drawables)
        {
            FormatGeometryDrawing(sb, drawable, indentLevel);
        }
        indentLevel--;

        // DrawingGroup closing
        if (requiresGroup)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}</DrawingGroup>");
            indentLevel--;
        }

        // DrawingBrush closing
        sb.AppendLine($"{GetIndent(indentLevel)}</DrawingBrush.Drawing>");
        indentLevel--;
        sb.AppendLine($"{GetIndent(indentLevel)}</DrawingBrush>");
        indentLevel--;

        // Container closing
        if (requiresContainer)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}</Rectangle.Fill>");
            sb.AppendLine("</Rectangle>");
        }

        return sb.TrimEnd().ToString();
    }

    // Image -> DrawingImage -> DrawingGroup -> GeometryDrawing
    private string GenerateDrawingImage(IReadOnlyList<DrawableElement> drawables, ViewBox? viewBox)
    {
        if (drawables.Count < 1)
            throw new ArgumentException("SVG must contain at least one drawable element", nameof(drawables));

        var sb = new StringBuilder();
        int indentLevel = 0;

        var requiresImage = _settings.Context == UsageContext.Standalone;
        // Image opening
        if (requiresImage)
        {
            // DrawingImage has no intrinsic size. Width & Height must match the SVG viewBox dimensions to preserve 1:1 coordinate mapping.
            ReadOnlySpan<string> imageAttrs = viewBox is null ? [] : [$"Width=\"{FormatDouble(viewBox.Width)}\"", $"Height=\"{FormatDouble(viewBox.Height)}\""];
            FormatControl(sb, "Image", indentLevel, false, imageAttrs);
            indentLevel++;
            // WPF syntax requires specifying Image.Source
            if (!_settings.AvaloniaCompatibility)
            {
                sb.AppendLine($"{GetIndent(indentLevel)}<Image.Source>");
                indentLevel++;
            }
        }

        // DrawingImage opening
        sb.Append($"{GetIndent(indentLevel)}<DrawingImage");
        if (_settings.Context == UsageContext.ResourceDictionary)
            sb.Append($" x:Key=\"{_settings.DefaultKey}\"");
        sb.AppendLine(">");
        indentLevel++;
        // WPF syntax requires specifying DrawingImage.Drawing
        if (!_settings.AvaloniaCompatibility)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}<DrawingImage.Drawing>");
            indentLevel++;
        }

        var requiresGroup = drawables.Count > 1;
        // DrawingGroup opening
        if (requiresGroup)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}<DrawingGroup>");
            indentLevel++;
        }

        // Geometries
        foreach (var drawable in drawables)
        {
            FormatGeometryDrawing(sb, drawable, indentLevel);
        }
        indentLevel--;

        // DrawingGroup closing
        if (requiresGroup)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}</DrawingGroup>");
            indentLevel--;
        }

        // DrawingImage closing
        if (!_settings.AvaloniaCompatibility)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}</DrawingImage.Drawing>");
            indentLevel--;
        }
        sb.AppendLine($"{GetIndent(indentLevel)}</DrawingImage>");
        indentLevel--;

        // Image closing
        if (requiresImage)
        {
            if (!_settings.AvaloniaCompatibility)
            {
                sb.AppendLine($"{GetIndent(indentLevel)}</Image.Source>");
            }
            sb.AppendLine("</Image>");
        }

        return sb.TrimEnd().ToString();
    }

    // PathIcon -> StreamGeometry
    private string GeneratePathIcon(IReadOnlyList<DrawableElement> drawables, ViewBox? viewBox)
    {
        if (drawables.Count != 1)
            throw new ArgumentException("SVG must contain exactly one drawable element for PathIcon output strategy", nameof(drawables));

        var sb = new StringBuilder();
        var drawable = drawables[0];

        if (_settings.Context == UsageContext.Standalone)
        {
            var attrs = new List<string>();

            // PathIcon in WinUI and Uno doesn't care about Width and Height
            // PathIcon in general has no meaningful way of specifying viewBox offsets
            if (_settings.AvaloniaCompatibility && viewBox is not null)
            {
                attrs.Add($"Width=\"{FormatDouble(viewBox.Width)}\"");
                attrs.Add($"Height=\"{FormatDouble(viewBox.Height)}\"");
            }

            // PathIcon can inherit foreground color, so we don't need to set Foreground if fill is currentColor
            if (drawable.Fill != Color.CurrentColor && FormatColor(drawable.Fill, drawable.FillOpacity) is string fillColor)
                attrs.Add($"Foreground=\"{fillColor}\"");

            if (drawable.Opacity != DefaultOpacity)
                attrs.Add($"Opacity=\"{FormatDouble(drawable.Opacity)}\"");

            attrs.Add($"Data=\"{FormatFillRule(drawable.FillRule)}{drawable.PathData}\"");

            FormatControl(sb, "PathIcon", 0, true, CollectionsMarshal.AsSpan(attrs));
        }
        else if (_settings.Context == UsageContext.ResourceDictionary)
        {
            // Ignore viewBox and fill entirely because StreamGeometry does not support them
            if (_settings.AvaloniaCompatibility)
                sb.AppendLine($"<StreamGeometry x:Key=\"{_settings.DefaultKey}\">{FormatFillRule(drawable.FillRule)}{drawable.PathData}</StreamGeometry>");
            else
            {
                List<string> attrs = [$"x:Key=\"{_settings.DefaultKey}\""];
                if (drawable.Fill != Color.CurrentColor && FormatColor(drawable.Fill, drawable.FillOpacity) is string fillColor)
                    attrs.Add($"Foreground=\"{fillColor}\"");
                attrs.Add($"Data=\"{FormatFillRule(drawable.FillRule)}{drawable.PathData}\"");
                FormatControl(sb, "PathIconSource", 0, true, CollectionsMarshal.AsSpan(attrs));
            }
        }
        return sb.TrimEnd().ToString();
    }

    private string GetIndent(int indentLevel)
    {
        if (indentLevel < 1)
            return string.Empty;

        return _settings.TabIndent ? new string('\t', indentLevel) : new string(' ', Math.Max(0, _settings.IndentSpaces) * indentLevel);
    }

    // F0 is EvenOdd and F1 is NonZero. When unspecified, F0 is assumed.
    private static string FormatFillRule(FillRule fillRule)
        => fillRule == FillRule.NonZero ? "F1 " : string.Empty;

    /// <summary>
    /// XAML supports #RGB, #RRGGBB, #AARRGGBB, and SolidColorBrush.
    /// </summary>
    /// <returns>Return #RRGGBB if fully opaque, null if transparent, otherwise #AARRGGBB.</returns>
    private string? FormatColor(Color color, params IEnumerable<double> opacities)
    {
        // Specially handle currentColor
        if (color == Color.CurrentColor)
            return _settings.CurrentColorValue;

        double alpha = color.A / 255.0;
        foreach (var opacity in opacities)
        {
            alpha *= opacity;
        }
        alpha = Math.Clamp(alpha, 0, 1.0);

        if (alpha <= 0) return null;

        if (alpha < 1.0)
        {
            int alphaByte = (int)Math.Round(alpha * 255);
            return $"#{alphaByte:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    // Use floating precision = 3 to be consistent with svgo
    private static string FormatDouble(double v)
        => Math.Round(v, 3).ToString(CultureInfo.InvariantCulture);

    private static string FormatDoubleArray(params ReadOnlySpan<double> array)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < array.Length; i++)
        {
            sb.Append(FormatDouble(array[i]));
            if (i < array.Length - 1)
                sb.Append(',');
        }
        return sb.ToString();
    }

    private void FormatControl(StringBuilder sb, string controlName, int indentLevel, bool selfContained, params ReadOnlySpan<string> attributes)
    {
        var cap = selfContained ? " />" : ">";

        if (attributes.Length < 1)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}<{controlName}{cap}");
            return;
        }

        if (_settings.MultiLine)
        {
            // Put the first attribute on the same line as the control
            sb.Append($"{GetIndent(indentLevel)}<{controlName} {attributes[0]}");
            string attrIndent = GetIndent(indentLevel) + new string(' ', $"<{controlName} ".Length);
            // Append the rest of the attributes on new lines, aligned under the first attribute
            for (int i = 1; i < attributes.Length; i++)
                sb.AppendLine().Append($"{attrIndent}{attributes[i]}");
            sb.AppendLine(cap);
        }
        else
        {
            // Put all attributes on the same line
            sb.AppendLine($"{GetIndent(indentLevel)}<{controlName} {string.Join(' ', attributes)}{cap}");
            return;
        }
    }

    private void FormatGeometryDrawing(StringBuilder sb, DrawableElement drawable, int indentLevel)
    {
        var geometryAttrs = new List<string>();
        // GeometryDrawing does not have an Opacity property
        // Bake fill-opacity and opacity into color's alpha
        if (FormatColor(drawable.Fill, drawable.FillOpacity, drawable.Opacity) is string fill)
            geometryAttrs.Add($"Brush=\"{fill}\"");

        geometryAttrs.Add($"Geometry=\"{FormatFillRule(drawable.FillRule)}{drawable.PathData}\"");

        // Bake stroke-opacity and opacity into stroke's alpha
        if (FormatColor(drawable.Stroke, drawable.StrokeOpacity, drawable.Opacity) is string stroke)
        {
            FormatControl(sb, "GeometryDrawing", indentLevel, false, CollectionsMarshal.AsSpan(geometryAttrs));

            indentLevel++;
            sb.AppendLine($"{GetIndent(indentLevel)}<GeometryDrawing.Pen>");
            indentLevel++;

            List<string> attrs = [];
            attrs.Add($"Brush=\"{stroke}\"");

            if (drawable.StrokeWidth != DefaultStrokeWidth)
                attrs.Add($"Thickness=\"{FormatDouble(drawable.StrokeWidth)}\"");

            // SVG & XAML both default to Miter, but rendering differs between Path and GeometryDrawing.
            // Always set explicitly for consistent SVG conversion, even when using default value.
            attrs.Add($"LineJoin=\"{drawable.StrokeLineJoin}\"");

            // Default XAML value 10.0, but SVG default is 4.0
            if (drawable.StrokeLineJoin == StrokeLineJoin.Miter && drawable.StrokeMiterLimit != 10.0)
                attrs.Add($"MiterLimit=\"{FormatDouble(drawable.StrokeMiterLimit)}\"");

            // SVG stroke-linecap applies to the start, end, and dash caps in XAML
            if (drawable.StrokeLineCap != DefaultStrokeLineCap)
            {
                if (_settings.AvaloniaCompatibility)
                    attrs.Add($"LineCap=\"{drawable.StrokeLineCap}\"");
                else
                {
                    attrs.Add($"StartLineCap=\"{drawable.StrokeLineCap}\"");
                    attrs.Add($"EndLineCap=\"{drawable.StrokeLineCap}\"");
                    attrs.Add($"DashCap=\"{drawable.StrokeLineCap}\"");
                }
            }

            var hasDashStyle = drawable.StrokeDashArray.Length > 0;
            FormatControl(sb, "Pen", indentLevel, !hasDashStyle, CollectionsMarshal.AsSpan(attrs));

            if (hasDashStyle)
            {
                indentLevel++;
                sb.AppendLine($"{GetIndent(indentLevel)}<Pen.DashStyle>");
                indentLevel++;

                List<string> dashStyleAttrs = [$"Dashes=\"{FormatDoubleArray(drawable.StrokeDashArray)}\""];
                if (drawable.StrokeDashOffset != DefaultStrokeDashOffset)
                    dashStyleAttrs.Add($"Offset=\"{FormatDouble(drawable.StrokeDashOffset)}\"");
                FormatControl(sb, "DashStyle", indentLevel, true, CollectionsMarshal.AsSpan(dashStyleAttrs));
                
                indentLevel--;
                sb.AppendLine($"{GetIndent(indentLevel)}</Pen.DashStyle>");
                indentLevel--;

                sb.AppendLine($"{GetIndent(indentLevel)}</Pen>");
            }

            indentLevel--;
            sb.AppendLine($"{GetIndent(indentLevel)}</GeometryDrawing.Pen>");
            indentLevel--;

            sb.AppendLine($"{GetIndent(indentLevel)}</GeometryDrawing>");
        }
        else
        {
            FormatControl(sb, "GeometryDrawing", indentLevel, true, CollectionsMarshal.AsSpan(geometryAttrs));
        }
    }
    #endregion

    #region CSS
    /// <summary>
    /// Returns the effective value of a CSS presentation property for <paramref name="element"/> by walking the cascade in priority order:
    ///   1. Inline style on the element itself
    ///   2. Presentation attribute on the element itself
    ///   3. Recursively inherited from the nearest ancestor that supplies a value
    /// </summary>
    /// <param name="isValidValue">Predicate should check for empty string and whitespace values. Null values will never be passed to this predicate.</param>
    /// <returns>
    /// Returns a valid resolved value or null if the property is not set anywhere.
    /// </returns>
    private static string? GetEffectiveValue(XElement element, string property, Predicate<string> isValidValue)
    {
        // 1. Inline style wins over everything
        if (GetStyleProperty(element.Attribute("style")?.Value, property) is string fromStyle && isValidValue(fromStyle))
            return fromStyle;

        // 2. Presentation attribute on this element
        if (element.Attribute(property)?.Value is string fromAttr && isValidValue(fromAttr))
            return fromAttr;

        // 3. Inherit from parent
        if (element.Parent is XElement parent)
            return GetEffectiveValue(parent, property, isValidValue);

        return null;
    }

    /// <summary>
    /// Parses a single CSS property value from an inline style string such as <c>"fill:#333; stroke:none; opacity:0.5"</c>.
    /// </summary>
    /// <returns>
    /// Returns null if the property is not present.
    /// </returns>
    private static string? GetStyleProperty(string? style, string property)
    {
        if (string.IsNullOrWhiteSpace(style))
            return null;

        foreach (var declaration in style.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var colonIdx = declaration.IndexOf(':');
            if (colonIdx < 0) continue;

            var name = declaration[..colonIdx].Trim();
            var value = declaration[(colonIdx + 1)..].Trim();

            if (name.Equals(property, StringComparison.OrdinalIgnoreCase))
                return value;
        }

        return null;
    }
    #endregion

    #region Parsers
    // Matches all rgb and rgba syntax, including padding, space, slash, negative, decimal, and percentage variations
    [GeneratedRegex(@"rgba?\(\s*(?<r>[-\d.%]+)[,\s]+(?<g>[-\d.%]+)[,\s]+(?<b>[-\d.%]+)(?:\s*[,\s\/]\s*(?<a>[-\d.%]+))?\s*\)")]
    private static partial Regex RgbFunctionRegex();

    // Matches comma and/or whitespace separated numbers in a string, such as the contents of a viewBox or stroke-dasharray attribute
    [GeneratedRegex(@"[+-]?(?:\d*\.\d+|\d+)")]
    private static partial Regex JSArrayRegex();

    /// <summary>
    /// Converts a 'viewBox' string to a ViewBox object.
    /// </summary>
    /// <returns>
    /// <see cref="ViewBox"/> if parsing is successful; otherwise null.
    /// </returns>
    private static ViewBox? TryParseViewBox(string? viewBox)
    {
        if (string.IsNullOrWhiteSpace(viewBox)) return null;

        var matches = JSArrayRegex().Matches(viewBox);

        if (matches.Count < 4) return null;

        return new(ParseDouble(matches[0].Value),
            ParseDouble(matches[1].Value),
            ParseDouble(matches[2].Value),
            ParseDouble(matches[3].Value));
    }

    /// <summary>
    /// Converts a `stroke-dasharray` string to an array of doubles.
    /// </summary>
    /// <returns>
    /// double[] of length > 0 if parsing is successful; otherwise an empty array.
    /// </returns>
    private static double[] TryParseDashArray(string? array)
    {
        if (string.IsNullOrWhiteSpace(array)) return [];

        var matches = JSArrayRegex().Matches(array);

        if (matches.Count == 0) return [];

        var results = new double[matches.Count];
        for (int i = 0; i < matches.Count; i++)
        {
            results[i] = ParseDouble(matches[i].Value);
        }
        return results;
    }

    /// <summary>
    /// Converts an SVG color string to System.Drawing.Color.
    /// </summary>
    /// <returns>
    /// System.Drawing.Color if conversion is successful; otherwise null.
    /// </returns>
    private static Color? TryParseColor(string? colorValue)
    {
        if (string.IsNullOrWhiteSpace(colorValue))
            return null;

        colorValue = colorValue.Trim();

        // In SVG, 'transparent' and 'none' are functionally equivalent
        if (colorValue.Equals("none", StringComparison.OrdinalIgnoreCase))
            return Color.Transparent;

        if (colorValue.Equals("currentColor", StringComparison.OrdinalIgnoreCase))
            return Color.CurrentColor;

        // Handle rgb() and rgba()
        if (RgbFunctionRegex().Match(colorValue) is var match && match.Success)
        {
            try
            {
                return FromRgb();
            }
            catch
            {
                // Failing here means invalid numbers for r, g, b, or a
                return null;
            }
        }

        // Handle named and hex colors
        try
        {
            return FromHtml();
        }
        catch
        {
            // All other cases cannot be parsed, including:
            // "inherit", "currentColor", invalid strings, and unsupported CSS functions like hsl() or url()
            return null;
        }

        Color FromRgb()
        {
            int r = ParseColorChannel(match.Groups["r"].Value);
            int g = ParseColorChannel(match.Groups["g"].Value);
            int b = ParseColorChannel(match.Groups["b"].Value);

            int alpha = 255;
            if (match.Groups["a"].Success)
                alpha = ParseAlphaChannel(match.Groups["a"].Value);

            return Color.FromArgb(alpha, r, g, b);
        }

        // An upgraded version of ColorTranslator.FromHtml that supports #RRGGBBAA and #RGBA
        Color FromHtml()
        {
            // Handle #RRGGBBAA explicitly since ColorTranslator doesn't support it
            if (colorValue[0] == '#' && colorValue.Length == 9)
            {
                string colorPart = colorValue[0..7]; // #RRGGBB
                string alphaPart = colorValue[7..9]; // AA
                Color baseColor = ColorTranslator.FromHtml(colorPart);
                var alpha = ParseAlphaHex(alphaPart);
                return Color.FromArgb(alpha, baseColor);
            }
            // Handle #RGBA
            if (colorValue[0] == '#' && colorValue.Length == 5)
            {
                string colorPart = colorValue[0..4]; // #RGB
                string alphaPart = colorValue[4..]; // A
                Color baseColor = ColorTranslator.FromHtml(colorPart);
                var alpha = ParseAlphaHex(alphaPart + alphaPart);
                return Color.FromArgb(alpha, baseColor);
            }
            // #RGB, #RRGGBB, and named colors are handled by ColorTranslator
            return ColorTranslator.FromHtml(colorValue);
        }
    }

    /// <summary>
    /// Parse color channels that are captured by RgbFunctionRegex.
    /// </summary>
    /// <returns>
    /// An integer between 0 and 255, inclusive.
    /// </returns>
    private static int ParseColorChannel(ReadOnlySpan<char> value)
    {
        if (value.IsWhiteSpace())
            throw new ArgumentException("Color channel value cannot be null, empty, or whitespace.");

        value = value.Trim();
        double result;
        // A percentage between 0% and 100%
        if (value[^1] == '%')
        {
            result = double.Parse(value[..^1], NumberStyles.Any, CultureInfo.InvariantCulture);
            result = Math.Clamp(result, 0, 100);
            result = result * 255 / 100;
        }
        // An integer between 0 and 255
        else
        {
            result = double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            result = Math.Clamp(result, 0, 255);
        }
        return (int)Math.Round(result);
    }

    /// <summary>
    /// Parse alpha channel that is captured by RgbFunctionRegex.
    /// </summary>
    /// <returns>
    /// An integer between 0 and 255, inclusive, which corresponds to 0.0 and 1.0 alpha values, respectively.
    /// </returns>
    private static int ParseAlphaChannel(ReadOnlySpan<char> value)
    {
        if (value.IsWhiteSpace())
            throw new ArgumentException("Alpha channel value cannot be null, empty, or whitespace.");

        value = value.Trim();
        double result;
        // A percentage between 0% and 100%
        if (value[^1] == '%')
        {
            result = double.Parse(value[..^1], NumberStyles.Any, CultureInfo.InvariantCulture);
            result = Math.Clamp(result, 0, 100);
            result = result / 100;
        }
        // A double between 0.0 and 1.0
        else
        {
            result = double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
            result = Math.Clamp(result, 0, 1.0);
        }
        result *= 255;
        return (int)Math.Round(result);
    }

    private static int ParseAlphaHex(ReadOnlySpan<char> code)
    {
        if (code.IsWhiteSpace())
            throw new ArgumentException("Hexadecimal alpha value cannot be null, empty, or whitespace.");

        int alpha = int.Parse(code, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return Math.Clamp(alpha, 0, 255);
    }

    private static double ParseDouble(string value)
    => double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
    private static double? TryParseDouble(string? value)
        => double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
    #endregion

    #region Resolvers
    // Resolvers attempt to resolve a specific property/attribute from an element.
    // If the property/attribute is not present, then the default value is returned.

    private static Color ResolveFill(XElement element)
        => ResolveColor(element, "fill") ?? DefaultFill;

    private static Color ResolveStroke(XElement element)
        => ResolveColor(element, "stroke") ?? DefaultStroke;

    private static Color? ResolveColor(XElement element, string property)
    {
        Color? result = null;
        _ = GetEffectiveValue(element, property, s =>
        {
            var color = TryParseColor(s);
            if (color is not null)
            {
                result = color;
                return true;
            }
            return false;
        });
        return result;
    }

    private static FillRule ResolveFillRule(XElement element)
    {
        FillRule result = DefaultFillRule;
        _ = GetEffectiveValue(element, "fill-rule", s =>
        {
            switch (s.ToLowerInvariant())
            {
                case "nonzero":
                    result = FillRule.NonZero;
                    return true;
                case "evenodd":
                    result = FillRule.EvenOdd;
                    return true;
                default:
                    return false;
            }
        });
        return result;
    }

    private static StrokeLineCap ResolveStrokeLineCap(XElement element)
    {
        StrokeLineCap result = DefaultStrokeLineCap;
        _ = GetEffectiveValue(element, "stroke-linecap", s =>
        {
            switch (s.ToLowerInvariant())
            {
                case "butt":
                    result = StrokeLineCap.Flat;
                    return true;
                case "round":
                    result = StrokeLineCap.Round;
                    return true;
                case "square":
                    result = StrokeLineCap.Square;
                    return true;
                default:
                    return false;
            }
        });
        return result;
    }

    private static StrokeLineJoin ResolveStrokeLineJoin(XElement element)
    {
        StrokeLineJoin result = DefaultStrokeLineJoin;
        _ = GetEffectiveValue(element, "stroke-linejoin", s =>
        {
            switch (s.ToLowerInvariant())
            {
                case "miter":
                    result = StrokeLineJoin.Miter;
                    return true;
                case "round":
                    result = StrokeLineJoin.Round;
                    return true;
                case "bevel":
                    result = StrokeLineJoin.Bevel;
                    return true;
                default:
                    return false;
            }
        });
        return result;
    }

    private static double ResolveStrokeMiterLimit(XElement element)
    {
        // SVG spec requires stroke-miterlimit >= 1; values below 1 are discarded and default value will be used
        var value = ResolveDouble(element, "stroke-miterlimit") ?? DefaultStrokeMiterLimit;
        return value < 1.0 ? DefaultStrokeMiterLimit : value;
    }

    private static double ResolveStrokeDashOffset(XElement element)
        => ResolveDouble(element, "stroke-dashoffset") ?? DefaultStrokeDashOffset;

    private static double ResolveStrokeWidth(XElement element)
    {
        var value = ResolveDouble(element, "stroke-width") ?? DefaultStrokeWidth;
        // SVG spec treats negative stroke-width as an error; zero produces no stroke — use default in both cases
        return value > 0 ? value : DefaultStrokeWidth;
    }

    private static double ResolveFillOpacity(XElement element)
        => ResolveOpacity(element, "fill-opacity") ?? DefaultFillOpacity;

    private static double ResolveStrokeOpacity(XElement element)
        => ResolveOpacity(element, "stroke-opacity") ?? DefaultStrokeOpacity;

    private static double? ResolveOpacity(XElement element, string property)
    {
        var result = ResolveDouble(element, property);
        return result is null ? null : Math.Clamp((double)result, 0.0, 1.0);
    }

    private static double? ResolveDouble(XElement element, string property)
    {
        double? result = null;
        _ = GetEffectiveValue(element, property, s =>
        {
            var d = TryParseDouble(s);
            if (d is not null)
            {
                result = d.Value;
                return true;
            }
            return false;
        });
        return result;
    }

    private static double ResolveOpacity(XElement element)
    {
        // Opacity is cumulative, meaning all opacities (direct and inherited) must be multiplied together to get the final effective opacity.
        // In other words, we must resolve opacity for all elements from the current up to the root, and multiply them together.
        string property = "opacity";
        double opacity = DefaultOpacity;
        var current = element;
        while (current != null)
        {
            // Borrow logic from GetEffectiveValue
            if (GetStyleProperty(current.Attribute("style")?.Value, property) is string fromStyle && TryParseDouble(fromStyle) is double styleOpacity)
            {
                opacity *= Math.Clamp(styleOpacity, 0.0, 1.0);
            }
            else if (current.Attribute(property)?.Value is string fromAttr && TryParseDouble(fromAttr) is double attrOpacity)
            {
                opacity *= Math.Clamp(attrOpacity, 0.0, 1.0);
            }
            current = current.Parent;
        }
        return opacity;
    }

    private static double[] ResolveStrokeDashArray(XElement element)
    {
        double[] result = DefaultStrokeDashArray;
        _ = GetEffectiveValue(element, "stroke-dasharray", s =>
        {
            var array = TryParseDashArray(s);
            if (array.Length > 0)
            {
                result = array;
                return true;
            }
            return false;
        });
        return result;
    }
    #endregion

    #region Data Structures
    private record ViewBox(double X, double Y, double Width, double Height)
    {
        /// <summary>
        /// Returns <see cref="true"/> when the viewBox has non-zero X or Y offsets.
        /// </summary>
        public bool HasOffsets => X != 0 || Y != 0;

        /// <summary>
        /// Returns an array containing the X, Y, Width, and Height values of the rectangle in that order.
        /// </summary>
        public double[] ToArray() => [X, Y, Width, Height];

        /// <summary>
        /// Generates a compact Matrix Transform string representing a pure translation of (-X, -Y) in Avalonia.
        /// </summary>
        public string GetTransformString() => $"matrix(1,0,0,1,{FormatDouble(-X)},{FormatDouble(-Y)})";
    }

    private class DrawableElement
    {
        public string PathData { get; }
        public Color Fill { get; }
        public FillRule FillRule { get; }
        public Color Stroke { get; }
        public StrokeLineCap StrokeLineCap { get; }
        public StrokeLineJoin StrokeLineJoin { get; }
        public double StrokeMiterLimit { get; }
        public double[] StrokeDashArray { get; }
        public double StrokeDashOffset { get; }
        public double StrokeWidth { get; }
        public double Opacity { get; }
        public double FillOpacity { get; }
        public double StrokeOpacity { get; }

        public DrawableElement(string pathData, Color fill, FillRule fillRule, Color stroke, StrokeLineCap strokeLineCap, StrokeLineJoin strokeLineJoin,
            double strokeMiterLimit, double[] strokeDashArray, double strokeDashOffset, double strokeWidth, double opacity, double fillOpacity, double strokeOpacity)
        {
            PathData = pathData;
            Fill = fill;
            FillRule = fillRule;
            Stroke = stroke;
            StrokeLineCap = strokeLineCap;
            StrokeLineJoin = strokeLineJoin;
            StrokeMiterLimit = strokeMiterLimit;
            StrokeDashArray = strokeDashArray;
            StrokeDashOffset = strokeDashOffset;
            StrokeWidth = strokeWidth;
            Opacity = opacity;
            FillOpacity = fillOpacity;
            StrokeOpacity = strokeOpacity;

            // stroke-dasharray and stroke-dashoffset has to be divided by stroke-width because they are thickness-dependent in XAML but not in SVG
            // Only apply this division when there is a visible stroke; also guard against division by zero, by one, or by negatives (invalid stroke-width)
            // currentColor strokes (A=0 sentinel) are also visible at runtime, so they must be normalized too
            bool strokeIsVisible = Stroke.A > 0 || Stroke == Color.CurrentColor;
            if (strokeIsVisible && StrokeDashArray.Length > 0 && StrokeWidth > 0 && StrokeWidth != 1.0)
            {
                for (int i = 0; i < StrokeDashArray.Length; i++)
                {
                    StrokeDashArray[i] = StrokeDashArray[i] / StrokeWidth;
                }
                StrokeDashOffset = StrokeDashOffset / StrokeWidth;
            }
        }
    }

    // SVG "butt" is equivalent to XAML "Flat". We use Flat for easy XAML generation.
    private enum StrokeLineCap
    {
        Flat,
        Round,
        Square,
    }

    private enum StrokeLineJoin
    {
        Miter,
        Round,
        Bevel,
    }

    private enum FillRule
    {
        NonZero,
        EvenOdd,
    }

    // These are SVG defaults
    static readonly Color DefaultFill = Color.Black;
    const FillRule DefaultFillRule = FillRule.NonZero;
    static readonly Color DefaultStroke = Color.Transparent;
    const StrokeLineCap DefaultStrokeLineCap = StrokeLineCap.Flat;
    const StrokeLineJoin DefaultStrokeLineJoin = StrokeLineJoin.Miter;
    const double DefaultStrokeMiterLimit = 4.0;
    static readonly double[] DefaultStrokeDashArray = [];
    const double DefaultStrokeDashOffset = 0.0;
    const double DefaultStrokeWidth = 1.0;
    const double DefaultOpacity = 1.0;
    const double DefaultFillOpacity = 1.0;
    const double DefaultStrokeOpacity = 1.0;
    #endregion
}
