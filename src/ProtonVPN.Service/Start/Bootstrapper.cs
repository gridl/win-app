/*
 * Copyright (c) 2020 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading.Tasks;
using Autofac;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.CrashReporting;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Native.PInvoke;
using ProtonVPN.Service.Config;
using ProtonVPN.Service.Settings;
using ProtonVPN.Service.Vpn;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.OpenVpn;

namespace ProtonVPN.Service.Start
{
    internal class Bootstrapper
    {
        private IContainer _container;

        private T Resolve<T>() => _container.Resolve<T>();

        public void Initialize()
        {
            SetDllDirectories();
            Configure();
            Start();
        }

        private void Configure()
        {
            var config = new ConfigFactory().Config();
            new ConfigDirectories(config).Prepare();

            var builder = new ContainerBuilder();
            builder.RegisterModule<ServiceModule>();
            _container = builder.Build();
        }

        private void Start()
        {
            var config = Resolve<Common.Configuration.Config>();
            var logger = Resolve<ILogger>();

            logger.Info($"= Booting ProtonVPN Service version: {config.AppVersion} os: {Environment.OSVersion.VersionString} {config.OsBits} bit =");

            TaskScheduler.UnobservedTaskException += Task_UnobservedException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            InitCrashReporting();
            RegisterEvents();

            Resolve<LogCleaner>().Clean(config.ServiceLogFolder, 30);
            ServiceBase.Run(Resolve<VpnService>());

            logger.Info("= ProtonVPN Service has exited =");
        }

        private void InitCrashReporting()
        {
            CrashReports.Init(Resolve<Common.Configuration.Config>(), Resolve<ILogger>());
        }

        private void RegisterEvents()
        {
            Resolve<ObservableConnection>().AfterStateChanged += (s, e) =>
            {
                var state = e.Data;
                var instances = Resolve<IEnumerable<IVpnStateAware>>();
                foreach (var instance in instances)
                {
                    switch (state.Status)
                    {
                        case VpnStatus.Connecting:
                        case VpnStatus.Reconnecting:
                            instance.OnVpnConnecting(state);
                            break;
                        case VpnStatus.Connected:
                            instance.OnVpnConnected(state);
                            break;
                        case VpnStatus.Disconnecting:
                        case VpnStatus.Disconnected:
                            instance.OnVpnDisconnected(state);
                            break;
                    }
                }
            };

            Resolve<IServiceSettings>().SettingsChanged += (s, e) =>
            {
                var instances = Resolve<IEnumerable<IServiceSettingsAware>>();
                foreach (var instance in instances)
                {
                    instance.OnServiceSettingsChanged(e);
                }
            };
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var config = Resolve<Common.Configuration.Config>();
            var processes = Resolve<IOsProcesses>();
            LogException((Exception)e.ExceptionObject);
            Resolve<IVpnConnection>().Disconnect();
            Resolve<OpenVpnProcess>().Stop();
            processes.KillProcesses(config.AppName);
        }

        private void Task_UnobservedException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var logger = Resolve<ILogger>();
            logger.Error($"Unobserved exception occured: {e.Exception.Message}");

            foreach (var ex in e.Exception.Flatten().InnerExceptions)
                logger.Error(ex);
        }

        private void LogException(Exception exception)
        {
            var logger = Resolve<ILogger>();

            if (exception is AggregateException aggregateException)
            {
                logger.Fatal($"Aggregate exception occured: {aggregateException.Message}");
                foreach (var ex in aggregateException.Flatten().InnerExceptions)
                    logger.Fatal(ex.ToString());
            }
            else
            {
                logger.Fatal(exception.ToString());
            }
        }

        private static void SetDllDirectories()
        {
            Kernel32.SetDefaultDllDirectories(Kernel32.SetDefaultDllDirectoriesFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
        }
    }
}
