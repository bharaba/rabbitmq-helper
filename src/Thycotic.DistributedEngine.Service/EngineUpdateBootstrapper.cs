//#define BREAKINTOVS
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using SBSSHForwarding;
using Thycotic.DistributedEngine.Service.Update;
using Thycotic.Logging;
using Thycotic.WindowsService.Bootstraper;

namespace Thycotic.DistributedEngine.Service
{
    /// <summary>
    /// Engine updater. This class is run by <see cref="Program"/> when the bootstrap external process starts. <seealso cref="UpdateInitializer"/>
    /// </summary>
    public class EngineUpdateBootstrapper
    {
        private readonly ILogWriter _log = Log.Get(typeof (EngineUpdateBootstrapper));

        [Conditional("BREAKINTOVS")]
        private static void InterceptChildProcess()
        {
            Debugger.Launch();
            Debugger.Break();
        }

        /// <summary>
        /// Bootstraps the specified msi path and run it as an external process.
        /// </summary>
        /// <param name="msiPath">The msi path.</param>
        public void Bootstrap(string msiPath)
        {

            InterceptChildProcess();

            Trace.TraceInformation("Configuring bootstrap logging...");
            Log.Configure();

            using (LogContext.Create("Child process"))
            {

                _log.Info("Running bootstrapper");

                try
                {
                    var cts = new CancellationTokenSource();

                    //TODO: Maybe not hardcoded -dkk
                    const string serviceName = "Thycotic.DistributedEngine.Service";

                    var serviceUpdater = new ServiceUpdater(cts, GetServiceInstallationPath(),
                        serviceName, msiPath);

                    serviceUpdater.Update();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Engine update bootstrapper failed", ex);
                }
                finally
                {
                    //delete the update file regardless of update outcome
                    File.Delete(msiPath);
                }
            }
        }

        

        private static string GetServiceInstallationPath()
        {
            //return @"C:\Program Files (x86)\Thycotic Software Ltd\Distributed Engine";

            var backupEntryPoint = Path.GetDirectoryName(Assembly.GetAssembly(typeof(EngineService)).Location);

            // ReSharper disable once PossibleNullReferenceException
            return backupEntryPoint.Replace(ServiceUpdater.BackupDirectoryName, string.Empty);
        }
        
    }
}