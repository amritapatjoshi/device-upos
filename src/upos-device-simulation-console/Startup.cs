using Microsoft.Extensions.DependencyInjection;
using System;
using upos_device_simulation.Services;
using upos_device_simulation.Helpers;
using upos_device_simulation.Interfaces;
using OposScanner_CCO;
using Serilog;
using ILogger = upos_device_simulation.Interfaces.ILogger;
using Microsoft.PointOfService;
using Logger = upos_device_simulation.Helpers.Logger;
using Serilog.Sinks.Graylog;

namespace UposDeviceSimulationConsole
{
    public static class Startup
    {
        public static IServiceProvider _serviceProvider;
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddSingleton<LoggerConfiguration, LoggerConfiguration>();
            services.AddSingleton<GraylogSinkOptions, GraylogSinkOptions>();
            services.AddSingleton<IFileHelper, FileHelper>();
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton(x =>
            {
                var socketclient = new SocketIOClient.SocketIO(System.Configuration.ConfigurationManager.AppSettings["SocketUrl"]);
                return socketclient;
            });
            services.AddSingleton<SocketIOClient.SocketIOOptions>();
            services.AddSingleton<SocketClient>();
            services.AddScoped<IBarcodeScanner, BarcodeScanner>();
            services.AddScoped<IPayMsr, PayMsr>();
            services.AddScoped<IPaypinpad, Paypinpad>();
            services.AddScoped<IReceiptPrinter, ReceiptPrinter>();
            services.AddScoped<OPOSScanner, OPOSScannerClass>();
            services.AddScoped<PosExplorer, PosExplorer>();
            services.AddScoped<PosExecutor, PosExecutor>();
            services.AddScoped<Utils>();

            _serviceProvider = services.BuildServiceProvider();
        }
    }
}
