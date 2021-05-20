using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;
using System.IO;

namespace CustomService
{
    public partial class CustomService : ServiceBase
    {
        enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        System.Diagnostics.EventLog eventLog1 = new System.Diagnostics.EventLog();
        Timer timer = new Timer();
        int eventId = 1;

        public CustomService()
        {
            InitializeComponent();
            String serviceName = Properties.Settings.Default.ServiceName;
            if (!EventLog.SourceExists(serviceName))
            {
                EventLog.CreateEventSource(serviceName, "Application");
            }
            eventLog1.Source = serviceName;
            eventLog1.Log = "Application";
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            timer.Start();
            String workingDirectory = Properties.Settings.Default.WorkingDir;
            String startupBat = Properties.Settings.Default.StartupPath;
            String serviceName = Properties.Settings.Default.ServiceName;

            // CHECK IF STARTUP BAT EXISTS
            if (!File.Exists(startupBat))
            {
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                String msg = "ERROR:  Cannot find startup file located at: " + startupBat + "\rPlease Ensure your file exists.";
                eventLog1.WriteEntry(msg, EventLogEntryType.Error);
                throw new Exception(msg);

            } else if (!Directory.Exists(workingDirectory)) {
                serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
                String msg = "ERROR:  Cannot find working directory located at: " + workingDirectory + "\rPlease Ensure your folder exists.";
                eventLog1.WriteEntry(msg, EventLogEntryType.Error);
                throw new Exception(msg);
            } else
            {
                eventLog1.WriteEntry("Starting " + serviceName + " located at: " + startupBat);
                try
                {
                    Process myProcess = new Process();
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = startupBat;
                    myProcess.StartInfo.CreateNoWindow = true;
                    myProcess.StartInfo.WorkingDirectory = workingDirectory;
                    myProcess.Start();
                }
                catch (Exception e)
                {
                    eventLog1.WriteEntry(e.ToString());
                    throw;
                }


                // Set up a timer that triggers every minute.
                timer.Interval = 60000; // 60 seconds
                timer.Elapsed += new ElapsedEventHandler(this.OnTimer);

                // Update the service state to Running.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }

            
        }

        protected override void OnStop()
        {
 
            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            String workingDirectory = Properties.Settings.Default.WorkingDir;
            String shutdownBat = Properties.Settings.Default.ShutdownPath;
            String serviceName = Properties.Settings.Default.ServiceName;

            try
            {
                eventLog1.WriteEntry("Stopping " + serviceName);

                Console.WriteLine(shutdownBat);

                Process myProcess = new Process();
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = shutdownBat;
                myProcess.StartInfo.CreateNoWindow = true;
                myProcess.StartInfo.WorkingDirectory = workingDirectory;
                myProcess.Start();
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry(e.ToString());
                throw;
            }

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            //eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventId++);
        }
    }
}
