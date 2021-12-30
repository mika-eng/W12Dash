using System.Diagnostics;
using System.Windows;

namespace W12Dash
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            string processName = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcessesByName(processName).Length > 1)
                Current.Shutdown();
        }
    }
}