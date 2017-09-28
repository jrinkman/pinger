using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace pinger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow window;

        private static MyViewModel viewModel = new MyViewModel();
        private static List<double> dataPoints = new List<double>();
        private static Process currentProc = new Process();
        private static bool reset = false;
        private static bool end = false;
        private static string pingAddr = "google.com";

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MyViewModel();
            DataContext = viewModel;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e) { window = this; }

        static void Ping(string address = "google.com")
        {
            Process Proc = new Process();
            Proc.StartInfo.FileName = "cmd.exe";
            Proc.StartInfo.Arguments = "/c ping " + address + " -t";
            Proc.StartInfo.UseShellExecute = false;
            Proc.StartInfo.RedirectStandardOutput = true;
            Proc.StartInfo.RedirectStandardError = true;
            Proc.StartInfo.CreateNoWindow = true;
            Proc.OutputDataReceived += new DataReceivedEventHandler(Out);
            Proc.ErrorDataReceived += new DataReceivedEventHandler(Out);
            Proc.Start();
            Proc.BeginOutputReadLine();
            Proc.BeginErrorReadLine();

            currentProc = Proc;
        }

        static void Out(object sender, DataReceivedEventArgs args)
        {
            if (end) { return; }

            string lastPing = null;
            try
            {
                lastPing = args.Data.Substring(args.Data.IndexOf("time=") + 5, args.Data.IndexOf("ms") - (args.Data.IndexOf("time=") + 5));
                dataPoints.Add(Convert.ToDouble(lastPing));

                if (lastPing != null)
                {
                    window.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(() => 
                    {
                        if ((dataPoints.Count() > (window.numUpDownPingLimit.Value + 1) && window.checkBoxAutoRefresh.IsChecked == true) || reset)
                        {
                            reset = false;
                            Console.WriteLine("Limit reached, resetting.");
                            dataPoints.Clear();
                            viewModel.Data.Collection.Clear();
                            return;
                        }

                        viewModel.Data.Collection.Add(new Point(dataPoints.Count() - 1, dataPoints[dataPoints.Count() - 1]));
                        window.Title = "Pinger | " + lastPing + "ms";
                        window.transitionStatus.Content = args.Data;
                    }));
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message + " at " + ex.Source); }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            end = false;

            Ping(pingAddr);
            buttonStart.IsEnabled = false;
            buttonStop.IsEnabled = true;
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            end = true;
            reset = true;

            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;
            currentProc.CancelErrorRead();
            currentProc.CancelOutputRead();
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e) { reset = true; buttonRefresh.IsEnabled = false; }

        private void checkBoxAutoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            var cBox = (CheckBox)sender;
            if ((bool)cBox.IsChecked) { numUpDownPingLimit.IsEnabled = true; }
            else { numUpDownPingLimit.IsEnabled = false; }
        }

        private void pingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pingAddr = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
        }
    }
}