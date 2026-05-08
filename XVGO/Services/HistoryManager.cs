using XVGO.Models;

namespace XVGO.Services;

public interface IHistoryManager
{
    IEnumerable<ConvertedItem> History { get; }
    void AddToHistory(string optimizedSvg, string xaml);
    void ClearHistory();
}

public sealed class HistoryManager(ConversionSettings settings) : IHistoryManager
{
    public IEnumerable<ConvertedItem> History => _history;

    private readonly List<ConvertedItem> _history = [];

    public void AddToHistory(string optimizedSvg, string xaml)
    {
        var item = new ConvertedItem(optimizedSvg, xaml, settings.Strategy, settings.Context, settings.AvaloniaCompatibility);
        _history.Insert(0, item);
    }

    public void ClearHistory()
    {
        _history.Clear();
    }
}