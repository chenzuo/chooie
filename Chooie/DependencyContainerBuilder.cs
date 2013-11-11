﻿using System.Collections.Generic;
using Chooie.Core.Logging;
using Chooie.Core.PackageManager;
using Chooie.Database;
using Chooie.Jobs;
using Chooie.Logging;
using Chooie.PackageManager;
using Chooie.SignalR;
using Nancy.TinyIoc;

namespace Chooie
{
    public class DependencyContainerBuilder
    {
        private PackageManagerSettings _packageManagerSettings;

        private PackageManagerSettings PackageManagerSettings
        {
            get
            {
                if (_packageManagerSettings == null)
                {
                    _packageManagerSettings = new PackageManagerSettings {PackageManagerType = PackageManagerProvider.GetInitialPackageManagerType() };
                }
                return _packageManagerSettings;
            }
        }

        private PackageManagerProvider _packageManagerProvider;

        private PackageManagerProvider PackageManagerProvider
        {
            get
            {
                if (_packageManagerProvider == null)
                {
                    var packageManagerProvider = new PackageManagerProvider(new ContainerFactory(), new AssemblyLoader());
                    packageManagerProvider.BuildContainers();
                    _packageManagerProvider = packageManagerProvider;
                }
                return _packageManagerProvider;
            }
        }

        private IMemoryLog _memoryLog;

        private IMemoryLog MemoryLog
        {
            get
            {
                if (_memoryLog == null)
                {
                    _memoryLog = new MemoryLog();
                }
                return _memoryLog;
            }
        }

        private ILogger _logger;

        public ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = new Logger(new Context("Chooie"), new CompositeLog(new List<ILog>
                        {
                            new FileLog("log.txt"), // TODO: Don't hardcode this here - it should be a part of settings
                            MemoryLog
                        }));
                }
                return _logger;
            }
        }

        public void ConfigureContainer(TinyIoCContainer container)
        {
            container.Register<ILogger>(Logger);
            container.Register<IMemoryLog>(MemoryLog);
            container.Register<IJobFactory, JobFactory>();
            container.Register<IJobListUpdater, JobListUpdater>();
            container.Register<IPackageList, PackageList>().AsSingleton();
            container.Register<IJobQueue, JobQueue>().AsSingleton();

            container.Register<IClientMessenger, ClientMessenger>().AsSingleton();
            container.Register(PackageManagerSettings);
            container.Register<IDatabaseManager, DatabaseManager>().AsSingleton();
            container.Register<IDatabaseAccessor, DatabaseAccessor>().AsSingleton();
            var packageManagerProxy = new PackageManagerProxy(PackageManagerProvider, PackageManagerSettings, container.Resolve<IClientMessenger>(), container.Resolve<IJobQueue>(), container.Resolve<IPackageList>());
            container.Register<IPackageManager>(packageManagerProxy);
            container.Register<IPackageManagerProxy>(packageManagerProxy);
        }
    }
}
