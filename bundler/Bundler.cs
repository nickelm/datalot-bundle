using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            progressLog.Text += line + Environment.NewLine;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
