using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;

namespace DatalotBundler
{
    public partial class Bundler : Form
    {
        public const string Version = "0.2.0";
        public const string BuildDate = "March 24, 2021";
        public const string UploadURL = "http://localhost:3000/data/api/";

        private bool uploaded = false;

        public Bundler()
        {
            InitializeComponent();
        }

        public void AddLine(string line)
        {
            progressLog.Invoke(new MethodInvoker(delegate { progressLog.Text += line + Environment.NewLine; }));
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void SetCompleted()
        {
            CanClose = true;
            CanUpload = true;
        }

        private bool HasApiKey()
        {
            return !String.IsNullOrEmpty(apiKeyBox.Text);
        }

        private async void uploadButton_Click(object sender, EventArgs e)
        {
            await UploadAsync();
        }

        public bool CanClose
        {
            get { return closeButton.Enabled; }
            set
            {
                closeButton.Invoke(new MethodInvoker(delegate { closeButton.Enabled = value; }));
            }
        }

        public bool CanUpload
        {
            get { return uploaded == false; }
            set {
                uploadButton.Invoke(new MethodInvoker(delegate {
                    if (value)
                    {
                        if (uploaded == false && HasApiKey())
                            uploadButton.Enabled = true;
                    }
                    else
                    {
                        uploadButton.Enabled = false;
                    }
                }));
            }
        }

        private async System.Threading.Tasks.Task UploadAsync()
        {
            // Disable buttons
            CanClose = false;
            CanUpload = false;

            // Get the archive name
            string[] args = Environment.GetCommandLineArgs();
            String archiveName = args[1];

            AddLine("Uploading archive " + archiveName);

            // Now create the file connection
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            // Fill out the form
            AddLine("Using API key " + apiKeyBox.Text);
            form.Add(new StringContent(apiKeyBox.Text), "apikey");

            var fileContent = new ByteArrayContent(File.ReadAllBytes(archiveName));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "bundle", Path.GetFileName(archiveName));

            // Send the request
            AddLine("Posting bundle to " + UploadURL);
            HttpResponseMessage response = await httpClient.PostAsync(UploadURL, form);

            // Await a response
            string stringContent = await response.Content.ReadAsStringAsync();
            AddLine("Response from server: '" + stringContent + "'");

            // Enable buttons
            uploaded = true;
            CanClose = true;
        }

        private void apiKeyBox_TextChanged(object sender, EventArgs e)
        {
            CanUpload = HasApiKey();
        }

        private void Bundler_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default.APIKey))
            {
                apiKeyBox.Text = Properties.Settings.Default.APIKey;
            }
        }

        private void Bundler_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.APIKey = apiKeyBox.Text;
            Properties.Settings.Default.Save();
        }

        private string[] FilterChatLog(string[] chatLog)
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
                    AddLine("Chat log recorded " + year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2") +
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

        public void Bundle()
        {
            try
            {
                AddLine("Welcome to DatalotBundler version " + Version + " (Build date " + BuildDate + ").");

                // Extract the bundle name
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length < 3)
                {
                    AddLine("No command line parameters, nothing to do. Goodbye.");
                    SetCompleted();
                    return;
                }
                String archiveName = args[1];

                // Better start by sleeping a little
                AddLine("Waiting a second to ensure the game chat log has been closed...");
                Thread.Sleep(1000);

                // If the archive exists, delete it first
                if (File.Exists(archiveName))
                {
                    AddLine("Archive " + archiveName + " already exists, deleting it.");
                    File.Delete(archiveName);
                }

                // Create the new archive
                AddLine("Creating archive " + archiveName + "...");
                using (var fileStream = new FileStream(archiveName, FileMode.CreateNew))
                {
                    using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                    {
                        // Step through the log files
                        string[] logs = args.Skip(2).ToArray();
                        foreach (var logFile in logs)
                        {
                            // Status message
                            AddLine("Compressing " + logFile + "...");

                            // Get the base name
                            string entryName = Path.GetFileName(logFile);

                            // Read this log file
                            string[] fileText = System.IO.File.ReadAllLines(logFile);
                            if (Path.GetExtension(logFile) == ".log")
                            {
                                // Filter the chat log
                                AddLine("Filtering " + logFile + " from communications...");
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
                AddLine("Bundling completed.");
            }
            catch (Exception e)
            {
                AddLine("Error: " + e);
                AddLine("Exiting...");
            }
            SetCompleted();
        }

    }
}
