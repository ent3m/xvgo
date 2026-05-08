# XVGO

A browser-based SVG → XAML converter built with Blazor WebAssembly.  
Paste or open an SVG, pick your output strategy, and copy production-ready XAML — no install, no server, no data leaves your machine.

🔗 **[Try it live](https://ent3m.github.io/xvgo/)**

---

## Features

- **SVG optimization** via [SVGO](https://github.com/svg/svgo) before conversion, reducing noise in the output
- **Four output strategies** — `Canvas`, `DrawingBrush`, `DrawingImage`, and `PathIcon` — covering WPF, WinUI, UWP, Uno Platform, and Avalonia
- **Standalone or Resource Dictionary** output context
- **Avalonia compatibility** toggle for Avalonia-specific property names
- **Full SVG presentation attribute support** — fills, strokes, opacity, fill-rule, stroke-dasharray, and more, with correct CSS cascade and inheritance
- **`currentColor` support** with a configurable fallback value
- **Conversion history** with per-item copy and one-click clear
- **File upload** and **clipboard paste** as SVG input methods
- Runs entirely **client-side** — AOT-compiled, trimmed, and SIMD-enabled in production

## Supported SVG Features

| Feature | Supported |
|---|---|
| `<path>` elements | ✅ |
| `fill`, `stroke`, `opacity` | ✅ |
| `fill-opacity`, `stroke-opacity` | ✅ |
| `fill-rule` (`nonzero` / `evenodd`) | ✅ |
| `stroke-width`, `stroke-linecap`, `stroke-linejoin` | ✅ |
| `stroke-miterlimit` | ✅ |
| `stroke-dasharray`, `stroke-dashoffset` | ✅ |
| `currentColor` | ✅ |
| Inline styles and inherited attributes | ✅ |
| Named, hex (`#RGB`, `#RRGGBB`, `#RGBA`, `#RRGGBBAA`) colors | ✅ |
| `rgb()` / `rgba()` colors | ✅ |
| `viewBox` with offsets | ✅ |
| Gradients, patterns, text, images, clip paths | ❌ |
| Shapes (`<rect>`, `<circle>`, etc.) | ❌ |
| Complex transforms | ❌ |

## Output Strategies

<!-- Add your Mermaid decision graph here -->

### Canvas
Wraps paths in a `<Canvas>` (optionally inside a `<Viewbox>`). Best for pixel-perfect layout where explicit coordinates matter.

### DrawingBrush
Produces a `<DrawingBrush>` backed by `<GeometryDrawing>` elements. Ideal for use as a brush on any shape or background.

### DrawingImage
Produces a `<DrawingImage>` for use as an `<Image>` source. A good fit for icons in `<Image>` controls.

### PathIcon
Produces a single `<PathIcon>` (standalone) or `<PathIconSource>` / `<StreamGeometry>` (resource dictionary). Limited to one path; ignores viewBox.

## Tech Stack

| Layer | Technology |
|---|---|
| UI framework | [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) (.NET 10) |
| Component library | [Microsoft Fluent UI Blazor](https://www.fluentui-blazor.net/) |
| SVG optimization | [SVGO](https://github.com/svg/svgo) (bundled JS) |
| Hosting | GitHub Pages |

## Building Locally

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/ent3m/xvgo.git
cd xvgo/XVGO
dotnet run
```

Open `http://localhost:5000` (or the URL printed by the dev server).

For a production build:

```bash
dotnet publish -c Release
```

Output is in `bin/Release/net10.0/publish/wwwroot`.

## License

[MIT](LICENSE)