using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace I3DMapperUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog.FileName;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                MessageBox.Show("Must select a file before exporting");
                return;
            }
            try
            {
                XDocument doc = XDocument.Load(txtFilePath.Text);

                Task<XDocument> task = ExportMappings(doc);
                btnExport.Enabled = false;
                txtXml.Text = "Please wait, exporting mappings.";

                task.Wait();
                var mappings = task.Result;
                btnExport.Enabled = true;
                txtXml.Text = mappings.ToString().Replace("&gt;", ">");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        async Task<XDocument> ExportMappings(XDocument doc)
        {
            return await Task.Factory.StartNew(() =>
            {
                var mapper = new I3DMapper.Mapper(doc);
                mapper.Map();
                return mapper.Mappings;
            }).ConfigureAwait(continueOnCapturedContext: false);           
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtXml.Text) || txtXml.Text == "Please wait, exporting mappings.")
            {
                MessageBox.Show("Please export mappings first");
                return;
            }
            try
            {
                Clipboard.SetText(txtXml.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
