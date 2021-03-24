using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;

namespace DatalotBundler
{
    public partial class Bundler : Form
    {

        public Bundler()
        {
            InitializeComponent();
        }

        public void addLine(string line)
        {
            progressLog.Invoke(new MethodInvoker(delegate { progressLog.Text += line + Environment.NewLine; }));
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void setCompleted()
        {
            closeButton.Invoke(new MethodInvoker(delegate { closeButton.Enabled = true; }));
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            UploadAsync();
        }

        private async System.Threading.Tasks.Task UploadAsync()
        {
            // Get the archive name
            string[] args = Environment.GetCommandLineArgs();
            String archiveName = args[1];

            addLine("Uploading archive " + archiveName);

            // Now create the file connection
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            // Fill out the form
            addLine("Using API key " + apiKeyBox.Text);
            form.Add(new StringContent(apiKeyBox.Text), "apikey");

            var fileContent = new ByteArrayContent(File.ReadAllBytes(archiveName));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "bundle", Path.GetFileName(archiveName));

            // Send the request
            String url = "http://localhost:3000/data/api/";
            addLine("Posting bundle to " + url);
            HttpResponseMessage response = await httpClient.PostAsync(url, form);

            // Await a response
            string stringContent = await response.Content.ReadAsStringAsync();
            addLine("Status message: '" + stringContent + "'");
        }
    }
}
