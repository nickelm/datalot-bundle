using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.Threading;

namespace DatalotBundler
{

    static class Program
    {

        private static Bundler app;

        public static void bundle()
        {
            // Extract the bundle name
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 3) return;
            String archiveName = args[1];

            // If the archive exists, delete it first
            if (File.Exists(archiveName))
            {
                app.addLine("Archive " + archiveName + " already exists, deleting it.");
                File.Delete(archiveName);
            }

            // Create the new archive
            app.addLine("Creating archive " + archiveName + "...");
            using (var fileStream = new FileStream(archiveName, FileMode.CreateNew))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    // Step through the log files
                    string[] logs = args.Skip(2).ToArray();
                    foreach (var logFile in logs)
                    {
                        // Status message
                        app.addLine("Compressing " + logFile + "...");

                        // Read this log file
                        string fileText = System.IO.File.ReadAllText(logFile);

                        // Write it into the archive
                        string entryName = Path.GetFileName(logFile);
                        var zipArchiveEntry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                        using (var zipStream = new StreamWriter(zipArchiveEntry.Open()))
                        {
                            zipStream.Write(fileText);
                        }
                    }
                }
            }
        }

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
            var thread = new Thread(bundle);
            thread.Start();

            // Now run it
            Application.Run(app);
        }
    }
}
