﻿using System;
using Chooie.Core.Logging;

namespace Chooie.Jobs
{
    public class JobFactory : IJobFactory
    {
        private readonly IJobListUpdater _jobListUpdater;
        private readonly ILogger _logger;

        public JobFactory(IJobListUpdater jobListUpdater, ILogger logger)
        {
            _jobListUpdater = jobListUpdater;
            _logger = logger;
        }

        public IJob CreateJob(string name, Action action)
        {
            return new Job(name, action, _jobListUpdater, _logger);
        }
    }
}
