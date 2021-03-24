using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.IO.Compression;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Http;

namespace DatalotBundler
{

    static class Program
    {
        public const string Version = "0.1.1";
        public const string BuildDate = "March 17, 2020";

        private static Bundler app;

        private static string[] FilterChatLog(string[] chatLog)
        {
            var output = new List<string>();

            // Get the time zone information
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            TimeSpan tzOffset = localZone.BaseUtcOffset;

            // Add a header to the chat log
            output.Add("time,message");

            // Regular expressions
            Regex dateLine = new Regex(@"^\*\*\* Chat Log Opened: (\w+) (?<month>\w\w\w) (?<day>\d+) (\d\d):(\d\d):(\d\d) (?<year>\d+)$", RegexOptions.Compiled);
            Regex entryLine = new Regex(@"^\[(?<hour>\d\d):(?<min>\d\d):(?<sec>\d\d)\] (?<msg>.*)$", RegexOptions.Compiled);

            int month = 1, day = 1, year = 2021;

            // Step through each line in the chatlog
            foreach (var line in chatLog)
            {

                // Trim the line
                string trimmedLine = line.Trim();

                // Match to a chat log opening
                MatchCollection matches = dateLine.Matches(trimmedLine);
                if (matches.Count > 0)
                {
                    month = DateTime.ParseExact(matches[0].Groups["month"].Value, "MMM", CultureInfo.CurrentCulture).Month;
                    day = Int32.Parse(matches[0].Groups["day"].Value);
                    year = Int32.Parse(matches[0].Groups["year"].Value);
                    app.addLine("Chat log recorded " + year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2") +
                        " (time zone: UTC" + (tzOffset < TimeSpan.Zero ? "-" : "+") + tzOffset.ToString(@"hh\:mm") + ").");
                    continue;
                }

                // Match to a timestamp
                matches = entryLine.Matches(trimmedLine);
                if (matches.Count > 0)
                {
                    // Filter out written communication
                    var msg = matches[0].Groups["msg"].Value;
                    if (msg.Contains("'") || msg.Contains("\"") || msg.Contains("@@"))
                    {
                        continue;
                    }

                    // Build the string 
                    string logEntry = String.Format("{0}-{1}-{2}T{3}:{4}:{5}.000{6}{7},\"{8}\"", year.ToString("D4"), month.ToString("D2"), day.ToString("D2"),
                        matches[0].Groups["hour"].Value, matches[0].Groups["min"].Value, matches[0].Groups["sec"],
                        tzOffset < TimeSpan.Zero ? "-" : "+", tzOffset.ToString(@"hh\:mm"), msg);
                    output.Add(logEntry);
                }
            }
            return output.ToArray();
        }

        private static void Bundle()
        {
            try
            {
                app.addLine("Welcome to DatalotBundler version " + Version + " (Build date " + BuildDate + ").");

                // Extract the bundle name
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length < 3)
                {
                    app.addLine("No command line parameters, nothing to do. Goodbye.");
                    app.setCompleted();
                    return;
                }
                String archiveName = args[1];

                // Better start by sleeping a little
                app.addLine("Waiting a second to ensure the game chat log has been closed...");
                Thread.Sleep(1000);

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

                            // Get the base name
                            string entryName = Path.GetFileName(logFile);

                            // Read this log file
                            string[] fileText = System.IO.File.ReadAllLines(logFile);
                            if (Path.GetExtension(logFile) == ".log")
                            {
                                // Filter the chat log
                                app.addLine("Filtering " + logFile + " from communications...");
                                fileText = FilterChatLog(fileText);

                                // Change the file extension
                                entryName = Path.ChangeExtension(entryName, ".csv");
                            }

                            // Write it into the archive
                            var zipArchiveEntry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                            using (var zipStream = new StreamWriter(zipArchiveEntry.Open()))
                            {
                                zipStream.Write(string.Join(Environment.NewLine, fileText));
                            }
                        }
                    }
                }

                // Enable closing the app
                app.addLine("Bundling completed.");
            }
            catch (Exception e)
            {
                app.addLine("Error: " + e);
                app.addLine("Exiting...");
            }
            app.setCompleted();
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
            var thread = new Thread(Bundle);
            thread.Start();

            // Now run the application
            Application.Run(app);
        }
    }
}
