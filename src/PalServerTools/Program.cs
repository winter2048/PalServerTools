
using BlazorPro.BlazorSize;
using CronQuery.Mvc.Jobs;
using CronQuery.Mvc.Options;
using LiteDB;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PalServerTools.Auth;
using PalServerTools.Components;
using PalServerTools.Data;
using PalServerTools.Job;
using PalServerTools.Middleware;
using PalServerTools.Models;
using PalServerTools.Utils;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace PalServerTools
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            AppUtil.HistoryProcessor();

            //args = new string[] { "-env","p1"};
            AppUtil.Env = string.Join(" ", args).GetArgumentValue("-Env") ?? "";
            await Console.Out.WriteLineAsync("PalServerTools：" + AppUtil.Env);
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile($"appsetting{(!string.IsNullOrWhiteSpace(AppUtil.Env) ? "." + AppUtil.Env : "")}.json", true, true);
           
            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddAntDesign();
            builder.Services.AddMediaQueryService();
            builder.Services.AddScoped<ConsoleService>();
            builder.Services.AddSingleton<LogService>();
            builder.Services.AddSingleton<PalProcessService>();
            builder.Services.AddSingleton<SystemInfoService>();
            builder.Services.AddSingleton<PalConfigService>();
            builder.Services.AddSingleton<ILogger, LiteDBLogger>();
            builder.Services.AddTransient<PalRconService>();
            builder.Services.AddTransient<BackupService>();
            builder.Services.AddScoped<ClientConfigService>();
            builder.Services.Configure<ToolsConfigModel>(builder.Configuration.GetSection("ToolsConfig"));
            builder.Services.AddSingleton<LiteDatabase>(sp =>
            {
                var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                if (!Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }
                return new LiteDatabase(Path.Combine(dataDir, "data.db".AddEnvPrefix()));
            });

            // ImitateAuthStateProvider
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICookieUtil, CookieUtil>();
            builder.Services.AddScoped<ImitateAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(implementationFactory => implementationFactory.GetRequiredService<ImitateAuthStateProvider>());

            builder.Services.Configure<JobRunnerOptions>(builder.Configuration.GetSection("ToolsConfig"));
            builder.Services.Configure<JobRunnerOptions>((option) =>
            {
                option.Running = true;
                // 自动备份Job
                var backupJob = option.Jobs.FirstOrDefault(p => p.Name == "BackupJob");
                if (backupJob == null)
                {
                    backupJob = new JobOptions() {
                        Name = "BackupJob"
                    };
                    option.Jobs.Add(backupJob);
                }
                backupJob.Cron = builder.Configuration.GetValue<string>("ToolsConfig:BackupCron", "0 0/30 * * * *");
                backupJob.Running = builder.Configuration.GetValue<bool>("ToolsConfig:AutoBackup", false);
                // 进程检测Job
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
                // 自动更新版本Job
                var autoUpgradeJob = option.Jobs.FirstOrDefault(p => p.Name == "AutoUpgradeJob");
                if (autoUpgradeJob == null)
                {
                    autoUpgradeJob = new JobOptions()
                    {
                        Cron = "0 0/5 * * * *",
                        Name = "AutoUpgradeJob"
                    };
                    option.Jobs.Add(autoUpgradeJob);
                }
                autoUpgradeJob.Running = builder.Configuration.GetValue<bool>("ToolsConfig:AutoUpgrade", false);
            });
            builder.Services.AddSingleton<JobRunner>();
            builder.Services.AddSingleton((IServiceProvider serviceProvider) => new JobCollection(builder.Services));
            builder.Services.AddHostedService<JobRunner>();

            builder.Services.AddTransient<BackupJob>();
            builder.Services.AddTransient<PalProcessJob>();
            builder.Services.AddTransient<AutoUpgradeJob>();
            var app = builder.Build();

            AppUtil.ServiceProvider = app.Services;
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            app.Urls.Add(builder.Configuration.GetValue<string>("ASPNETCORE_URLS"));
            await PalSavUtil.Init();
            app.Run();
        }
    }
}