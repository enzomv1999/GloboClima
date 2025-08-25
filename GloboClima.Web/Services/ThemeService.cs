using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace GloboClima.Web.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isDarkMode;
        private bool _isInitialized = false;
        private bool _isClientSide = false;

        public event Action? OnThemeChanged;

        public bool IsDarkMode => _isDarkMode;

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _isClientSide = !(jsRuntime.GetType().Name == "UnsupportedJavaScriptRuntime");
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized || !_isClientSide) return;
            
            try 
            {
                var theme = await _jsRuntime.InvokeAsync<string>("applySavedTheme");
                _isDarkMode = string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase);
                _isInitialized = true;
                OnThemeChanged?.Invoke();
            }
            catch (JSException)
            {
                _isInitialized = true;
            }
            catch (InvalidOperationException)
            {
                _isInitialized = true;
            }
        }

        public async Task ToggleThemeAsync()
        {
            if (!_isClientSide) return;
            
            _isDarkMode = !_isDarkMode;
            
            try
            {
                await _jsRuntime.InvokeVoidAsync("setTheme", _isDarkMode ? "dark" : "light");
                OnThemeChanged?.Invoke();
            }
            catch (JSException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void Dispose()
        {
        }
    }
}
