﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkerService
{
    public interface IServiceA
    {
        void Run();
    }

    public class ServiceA : IServiceA
    {
        private readonly ILogger<ServiceA> _logger;
        private readonly IServiceB _serviceB;

        public ServiceA(ILogger<ServiceA> logger, IServiceB serviceB)
        {
            _logger = logger;
            _serviceB = serviceB;
        }

        public void Run()
        {
            //_logger.LogInformation("In Service A");
            _serviceB.Run();
        }
    }
}
