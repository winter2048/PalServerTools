
using CronQuery.Mvc.Jobs;
using CronQuery.Mvc.Options;
using Microsoft.Extensions.Options;
using PalServerTools.Data;
using PalServerTools.Job;

namespace PalServerTools
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddSingleton<ConsoleService>();
            builder.Services.AddSingleton<PalProcessService>();
            builder.Services.AddTransient<ConfigService>();
            builder.Services.AddTransient<PalRconService>();

            builder.Services.AddTransient<BackupJob>();
            builder.Services.AddTransient<PalProcessJob>();
            builder.Services.Configure<JobRunnerOptions>(builder.Configuration.GetSection("ToolsConfig"));
            builder.Services.Configure<JobRunnerOptions>((option) =>
            {
                option.Running = true;
                option.Jobs.Add(new JobOptions()
                {
                    Cron = builder.Configuration.GetValue<string>("ToolsConfig:BackupCron", "0/5 * * * * *"),
                    Name = "BackupJob",
                    Running = builder.Configuration.GetValue<bool>("ToolsConfig:AutoBackup", true)
                });
                option.Jobs.Add(new JobOptions()
                {
                    Cron = "0/5 * * * * *",
                    Name = "PalProcessJob",
                    Running = true
                });
            });
            builder.Services.AddSingleton<JobRunner>();
            builder.Services.AddSingleton((IServiceProvider serviceProvider) => new JobCollection(builder.Services));
            builder.Services.AddHostedService<JobRunner>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            app.Urls.Add(builder.Configuration.GetValue<string>("ASPNETCORE_URLS"));

            app.Run();
        }
    }
}