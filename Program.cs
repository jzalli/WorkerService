using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WorkerService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var baseDir = AppDomain.CurrentDomain.BaseDirectory;
			var logfile = Path.Combine(baseDir, "WorkerService", "Log.txt");//if I want the file to be written in project solution
			Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.
				Warning).Enrich.FromLogContext().WriteTo.File(@"C:\WorkerService\Log.txt",
			rollingInterval: RollingInterval.Day, retainedFileCountLimit: 90).CreateLogger();
			try
			{
				Log.Information("Starting up service");
				CreateHostBuilder(args).Build().Run();
			}
			catch(Exception ex)
			{
				Log.Fatal(ex, "Error Occured in service");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<Worker>();
					services.AddHostedService<BackgroundWorker>();
					services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));
					services.AddScoped<IServiceA, ServiceA>();
					services.AddScoped<IServiceB, ServiceB>();
				}).UseWindowsService().UseSerilog();
	}
}
