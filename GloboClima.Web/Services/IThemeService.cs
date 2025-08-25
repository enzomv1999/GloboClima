using System;
using System.Threading.Tasks;

namespace GloboClima.Web.Services
{
    public interface IThemeService
    {
        event Action OnThemeChanged;
        bool IsDarkMode { get; }
        Task InitializeAsync();
        Task ToggleThemeAsync();
    }
}
