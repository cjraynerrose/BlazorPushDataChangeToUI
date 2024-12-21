using Serilog;
using UpdatingTable.Components;
using UpdatingTable.Data;
using UpdatingTable.Services;

namespace UpdatingTable
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Host.UseSerilog();

            builder.Services.AddSerilog((services, lc) => lc
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {NewLine}{Exception}"));

            builder.Services.AddSingleton<DataService>();
            builder.Services.AddTransient<ItemViewService>();

            builder.Services.AddHttpContextAccessor();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
