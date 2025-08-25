using GloboClima.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Blazored.LocalStorage;

namespace GloboClima.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddScoped<ApiService>();
            builder.Services.AddScoped<IThemeService, ThemeService>();
            builder.Services.AddScoped<ThemeService>();
            builder.Services.AddScoped<NotificationService>();

            builder.Services.AddHttpClient();

            var apiUrl = builder.Configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("ApiBaseUrl não está configurado no appsettings.json");
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri(apiUrl);
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
