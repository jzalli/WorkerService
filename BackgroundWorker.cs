using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
    public class BackgroundWorker : BackgroundService
    {
        private readonly ILogger<BackgroundWorker> _logger;
        private int executionCount = 0;
        private FileSystemWatcher _folderWatcher;
        private readonly string _inputFolder;
        private readonly IServiceProvider _services;

        public BackgroundWorker(IServiceProvider services,
            ILogger<BackgroundWorker> logger, IOptions<AppSettings> settings)
        {
            Services = services;
            _logger = logger;
            _inputFolder = settings.Value.InputFolder;
            _services = services;

        }

        public IServiceProvider Services { get; }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("BakgroundWorker Service started at : " + DateTimeOffset.Now);
            if (!Directory.Exists(_inputFolder))
            {
                _logger.LogWarning($"Please make sure the InputFolder {_inputFolder} exists, then restart the service.");
                return Task.CompletedTask;
            }

            _logger.LogInformation($"Binding Events from Input Folder: {_inputFolder}");
            _folderWatcher = new FileSystemWatcher(_inputFolder, "*.*")
            //("") or ("*.*") to watch for changes in all files 
            //("*.TXT*") changes only in text file
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName |
                                  NotifyFilters.DirectoryName | NotifyFilters.LastAccess
            };
            _folderWatcher.Created += Input_OnChanged;
            _folderWatcher.Changed += new FileSystemEventHandler(Input_OnChanged);
            _folderWatcher.Deleted += new FileSystemEventHandler(Input_OnChanged);
            //  Register a handler that gets called when a file is renamed.
            _folderWatcher.Renamed += new RenamedEventHandler(OnRenamed);

            //  FileSystemWatcher needs to report an error.
            _folderWatcher.Error += new ErrorEventHandler(OnError);
            _folderWatcher.EnableRaisingEvents = true;
            return base.StartAsync(cancellationToken);
        }
        //  This method is called when a file is renamed.
        protected void OnRenamed(object source, RenamedEventArgs e)
        {
            //  Show that a file has been renamed.
            WatcherChangeTypes wct = e.ChangeType;
            using (var scope = _services.CreateScope())
            {
                var serviceA = scope.ServiceProvider.GetRequiredService<IServiceA>();
                serviceA.Run();
            }

            _logger.LogInformation("File {0} was {1} to {2} . Old path: {3} . New Path : {4}.", e.OldName, wct.ToString(), e.Name, e.OldFullPath, e.FullPath);
            //Console.WriteLine("File {0} {2} to {1}", e.OldFullPath, e.FullPath, wct.ToString());
        }
        protected void Input_OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                _logger.LogInformation($"File {e.Name} was  created in {e.FullPath}");

                // do some work
                using (var scope = _services.CreateScope())
                {
                    var serviceA = scope.ServiceProvider.GetRequiredService<IServiceA>();
                    serviceA.Run();
                }
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Changed)
            {
                _logger.LogInformation($"File {e.Name} was  {e.ChangeType}");

                // do some work
                using (var scope = _services.CreateScope())
                {
                    var serviceA = scope.ServiceProvider.GetRequiredService<IServiceA>();
                    serviceA.Run();
                }
            }
        }
        //  This method is called when the FileSystemWatcher detects an error.
        private static void OnError(object source, ErrorEventArgs e)
        {
            //  Show that an error has been detected.
            Console.WriteLine("The FileSystemWatcher has detected an error");
            //  Give more information if the error is due to an internal buffer overflow.
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                //  This can happen if Windows is reporting many file system events quickly
                //  and internal buffer of the  FileSystemWatcher is not large enough to handle this
                //  rate of events. The InternalBufferOverflowException error informs the application
                //  that some of the file system events are being lost.
                Console.WriteLine(("The file system watcher experienced an internal buffer overflow: " + e.GetException().Message));
            }
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
            _logger.LogInformation(
            "BakgroundWorker Service running.");
            _logger.LogInformation("Starting time :" + DateTime.Now);

            await DoWork(stoppingToken);
           
		}
        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                executionCount++;

                _logger.LogInformation(
                    "Scoped Processing Service is working. Count: {Count}", executionCount);

                await Task.Delay(10000, stoppingToken);
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "BakgroundWorker Service  is stopping.");
            _folderWatcher.EnableRaisingEvents = false;
            await base.StopAsync(stoppingToken);
            _logger.LogInformation(
               "BakgroundWorker Service  stopped at :" + DateTime.Now);
        }
        public override void Dispose()
        {
            _logger.LogInformation("Disposing Service");
            _folderWatcher.Dispose();
            base.Dispose();
        }
    }
}
