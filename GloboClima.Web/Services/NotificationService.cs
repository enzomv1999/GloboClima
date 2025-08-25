using Microsoft.JSInterop;

namespace GloboClima.Web.Services
{
    public class NotificationService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly bool _isClientSide;

        public NotificationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _isClientSide = !(jsRuntime.GetType().Name == "UnsupportedJavaScriptRuntime");
        }

        public async Task ShowSuccessAsync(string message, string title = "")
        {
            await TryInvokeVoidAsync("showToast", "success", message, title);
        }

        public async Task ShowErrorAsync(string message, string title = "")
        {
            await TryInvokeVoidAsync("showToast", "error", message, title);
        }

        public async Task ShowWarningAsync(string message, string title = "")
        {
            await TryInvokeVoidAsync("showToast", "warning", message, title);
        }

        public async Task ShowInfoAsync(string message, string title = "")
        {
            await TryInvokeVoidAsync("showToast", "info", message, title);
        }

        private async Task TryInvokeVoidAsync(string identifier, params object[] args)
        {
            if (!_isClientSide) return;
            try
            {
                await _jsRuntime.InvokeVoidAsync(identifier, args);
            }
            catch (InvalidOperationException)
            {
            }
            catch (JSException)
            {
            }
        }
    }
}
