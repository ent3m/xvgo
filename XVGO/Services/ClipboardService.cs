using Microsoft.JSInterop;

namespace XVGO.Services;

internal interface IClipboardService
{
    Task CopyAsync(string text);
    Task<string> PasteAsync();
}

internal sealed class ClipboardService(IJSRuntime js) : IClipboardService
{
    public async Task CopyAsync(string text) =>
        await js.InvokeVoidAsync("navigator.clipboard.writeText", text);

    public async Task<string> PasteAsync()
    {
        try
        {
            return await js.InvokeAsync<string>("navigator.clipboard.readText");

        }
        catch
        {
            // Exception is thrown if the user denies permission to access the clipboard
            return string.Empty;
        }
    }
}