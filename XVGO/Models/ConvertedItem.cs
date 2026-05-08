namespace XVGO.Models;

public record ConvertedItem(string OptimizedSvg, string Xaml, OutputStrategy Strategy, UsageContext Context, bool AvaloniaCompatibility);
