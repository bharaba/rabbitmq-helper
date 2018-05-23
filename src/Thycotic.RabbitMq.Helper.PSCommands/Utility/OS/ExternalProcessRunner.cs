﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Thycotic.RabbitMq.Helper.PSCommands.Utility.OS
{
    /// <summary>
    /// External process runner
    /// </summary>
    public class ExternalProcessRunner
    {
        /// <summary>
        /// Gets or sets the duration of the estimated process.
        /// </summary>
        /// <value>
        /// The duration of the estimated process.
        /// </value>
        public TimeSpan EstimatedProcessDuration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalProcessRunner"/> class.
        /// </summary>
        public ExternalProcessRunner()
        {
            EstimatedProcessDuration = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Runs the specified process information.
        /// </summary>
        /// <param name="processInfo">The process information.</param>
        /// <exception cref="System.ApplicationException">
        /// Process failed
        /// or
        /// Process appears to have failed
        /// </exception>
        /// <exception cref="System.Exception">
        /// </exception>
        public void Run(ProcessStartInfo processInfo)
        {
            Process process = null;

            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    process = Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(string.Format("Could not start process from {0}", processInfo.FileName), ex);
                }


                if (process == null)
                {
                    throw new ApplicationException("Process could not start");
                }

                process.WaitForExit();

            });

            //wait for process to complete
            task.Wait(EstimatedProcessDuration);

            //there was an exception, rethrow it
            if (task.Exception != null)
            {
                throw task.Exception;
            }

            if (process != null)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }

                var output = process.StandardOutput.ReadToEnd();

                //process didn't exit correctly, extract output and throw
                if (process.ExitCode != 0)
                {
                    throw new ApplicationException("Process failed", new Exception(output));
                }

                if (output.ToLower().Contains("error"))
                {
                    throw new ApplicationException("Process appears to have failed", new Exception(output));
                }
            }

        }

        /// <summary>
        /// Runs the specified executable path.
        /// </summary>
        /// <param name="executablePath">The executable path.</param>
        /// <param name="workingPath">The working path.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.ApplicationException">
        /// Process failed
        /// or
        /// Process appears to have failed
        /// </exception>
        /// <exception cref="System.Exception">
        /// </exception>
        public void Run(string executablePath, string workingPath, string parameters = null)
        {
            var processInfo = new ProcessStartInfo(executablePath, parameters)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = workingPath
            };

            Run(processInfo);
        }

    }
}