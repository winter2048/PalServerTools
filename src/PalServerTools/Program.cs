
using CronQuery.Mvc.Jobs;
using CronQuery.Mvc.Options;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using PalServerTools.Auth;
using PalServerTools.Components;
using PalServerTools.Data;
using PalServerTools.Job;
using PalServerTools.Utils;

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
            builder.Services.AddAntDesign();
            builder.Services.AddScoped<ConsoleService>();
            builder.Services.AddSingleton<PalProcessService>();
            builder.Services.AddSingleton<ServerInfo>();
            builder.Services.AddTransient<PalConfigService>();
            builder.Services.AddTransient<PalRconService>();
            builder.Services.AddTransient<BackupService>();
            builder.Services.AddScoped<ClientConfigService>();

            // ע��ImitateAuthStateProvider
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICookieUtil, CookieUtil>();
            builder.Services.AddScoped<ImitateAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(implementationFactory => implementationFactory.GetRequiredService<ImitateAuthStateProvider>());

            builder.Services.Configure<JobRunnerOptions>(builder.Configuration.GetSection("ToolsConfig"));
            builder.Services.Configure<JobRunnerOptions>((option) =>
            {
                option.Running = true;
                var backupJob = option.Jobs.FirstOrDefault(p => p.Name == "BackupJob");
                if (backupJob == null)
                {
                    backupJob = new JobOptions() {
                        Name = "BackupJob"
                    };
                    option.Jobs.Add(backupJob);
                }
                backupJob.Cron = builder.Configuration.GetValue<string>("ToolsConfig:BackupCron", "0 0/30 * * * *");
                backupJob.Running = builder.Configuration.GetValue<bool>("ToolsConfig:AutoBackup", true);

                var palProcessJob = option.Jobs.FirstOrDefault(p => p.Name == "PalProcessJob");
                if (palProcessJob == null)
                {
                    palProcessJob = new JobOptions()
                    {
                        Cron = "0/5 * * * * *",
                        Name = "PalProcessJob",
                        Running = true
                    };
                    option.Jobs.Add(palProcessJob);
                }
            });
            builder.Services.AddSingleton<JobRunner>();
            builder.Services.AddSingleton((IServiceProvider serviceProvider) => new JobCollection(builder.Services));
            builder.Services.AddHostedService<JobRunner>();

            builder.Services.AddTransient<BackupJob>();
            builder.Services.AddTransient<PalProcessJob>();

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