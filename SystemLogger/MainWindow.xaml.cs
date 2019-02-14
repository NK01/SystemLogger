using System;
using System.Diagnostics;
using System.Windows;
using System.Globalization;
using System.ComponentModel;
using System.Threading;

namespace SystemLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int pid;
        string process;
        BackgroundWorker backgroundWorker1;
        PerformanceCounter appMemory;
        volatile bool isCancelRequested;
        int seconds;

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker1 = new BackgroundWorker();

            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);

            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isCancelRequested = false;

            if (System.IO.File.Exists(@"logs.txt"))
            {
                System.IO.File.Delete(@"logs.txt");
            }

            pid = int.Parse(PID.Text, CultureInfo.InvariantCulture.NumberFormat);
            seconds = int.Parse(interval.Text, CultureInfo.InvariantCulture.NumberFormat);
            process = GetProcessInstanceName(pid);

            MessageBox.Show(process);

            log.IsEnabled = false;
            stop.IsEnabled = true;

            appMemory = new PerformanceCounter("Process", "Working Set", process);

            appMemory.NextValue();

            backgroundWorker1.RunWorkerAsync();
        }

        private static string GetProcessInstanceName(int pid)
        {
            PerformanceCounterCategory cat = new PerformanceCounterCategory("Process");

            string[] instances = cat.GetInstanceNames();
            foreach (string instance in instances)
            {

                using (PerformanceCounter cnt = new PerformanceCounter("Process",
                     "ID Process", instance, true))
                {
                    int val = (int)cnt.RawValue;
                    if (val == pid)
                    {
                        return instance;
                    }
                }
            }
            throw new Exception("Could not find performance counter " +
                "instance name for current process. This is truly strange ...");
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            CancelWork();
            stop.IsEnabled = false;
            log.IsEnabled = true;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!isCancelRequested)
            {
                Thread.Sleep(seconds * 1000);

                float memory = appMemory.NextValue();

                memory = memory / (1024 * 1024);

                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@"logs.txt", true))
                {
                    file.WriteLine(memory.ToString() + "\t" + DateTime.Now.ToShortTimeString());
                }
            }
            
        }

        public void CancelWork()
        {
            isCancelRequested = true;
        }

    }
}
