using System;
using System.Windows.Forms;
using System.Threading;

namespace DatalotBundler
{

    static class Program
    {

        private static Bundler app;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Start the application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            app = new Bundler();

            // Start the bundling thread
            var thread = new Thread(app.Bundle);
            thread.Start();

            // Now run the application
            Application.Run(app);
        }
    }
}
