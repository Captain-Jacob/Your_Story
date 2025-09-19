using System;
using System.Windows;
using System.Windows.Threading;

namespace Your_Story
{
    public partial class App : Application
    {
        public App()
        {
            // UI thread exceptions
            this.DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "DispatcherUnhandledException");
                e.Handled = true; // Uygulama kapanmasın; istersen false yapabilirsin
            };

            // Non-UI / background exceptions
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                MessageBox.Show(e.ExceptionObject?.ToString() ?? "Unknown error", "UnhandledException");
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                var w = new MainWindow();
                w.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "OnStartup error");
                Shutdown(-1);
            }
        }
    }
}
