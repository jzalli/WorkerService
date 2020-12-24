using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerService
{
    public interface IServiceB
    {
        void Run();
    }

    public class ServiceB : IServiceB
    {
        private readonly ILogger<ServiceB> _logger;

        public ServiceB(ILogger<ServiceB> logger)
        {
            _logger = logger;
        }

        public void Run()
        {
            //_logger.LogInformation("In Service B");
        }
    }
}
