using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Squirrel;

namespace I3DMapperUI
{
    public partial class MainForm : Form
    {
        private bool checkingForUpdates = false;

        public MainForm()
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

        private async void btnExport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                MessageBox.Show("Must select a file before exporting", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                XDocument doc = XDocument.Load(txtFilePath.Text);

                Task<XDocument> task = ExportMappings(doc);
                btnExport.Enabled = false;
                txtXml.Text = "Please wait, exporting mappings.";

                var mappings = await task;
                btnExport.Enabled = true;
                txtXml.Text = mappings.ToString().Replace("&gt;", ">");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        async Task<XDocument> ExportMappings(XDocument doc)
        {
            return await Task.Factory.StartNew(() =>
            {
                var mapper = new I3DMapper.Mapper(doc);
                mapper.Map(chkShortenID.Checked);
                return mapper.Mappings;
            }).ConfigureAwait(continueOnCapturedContext: false);           
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtXml.Text) || txtXml.Text == "Please wait, exporting mappings.")
            {
                MessageBox.Show("Please export mappings first", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                Clipboard.SetText(txtXml.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            
            this.Text = string.Format("{0} - v{1}.{2}.{3}", this.Text, version.Major, version.Minor, version.Build);
            btnPreReleaseUpdates.Checked = Properties.Settings.Default.updatePrerelease;
            chkShortenID.Checked = Properties.Settings.Default.shortenIDs;
            Task task = updateApp(Properties.Settings.Default.updatePrerelease);
            await task;
        }

        private async Task updateApp(bool preRelease = false)
        {
            if (checkingForUpdates) return;

            checkingForUpdates = true;
            ReleaseEntry release = null;
            try
            {
                using (var mgr = new UpdateManager("https://slreleases.ams3.cdn.digitaloceanspaces.com/I3DMappingExporter/stable"))
                {
                    lblUpdateStatus.Text = "Checking for updates";
                    pbUpdateProg.Value = 0;
                    pbUpdateProg.Visible = true;
                    var updateInfo = await mgr.CheckForUpdate(false, (prg) => UpdateProgress(prg));
                    if (updateInfo.ReleasesToApply.Any())
                    {
                        lblUpdateStatus.Text = "Downloading update";
                        pbUpdateProg.Value = 0;
                        release = await mgr.UpdateApp((prg) => UpdateProgress(prg));
                    }
                    else
                    {
                        await Task.Delay(250);
                        lblUpdateStatus.Text = "Up to date";
                        pbUpdateProg.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                pbUpdateProg.Visible = false;
                lblUpdateStatus.Text = "Update failed";
                MessageBox.Show(ex.Message, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                checkingForUpdates = false;
            }

            if(release != null)
            {
                pbUpdateProg.Visible = false;
                lblUpdateStatus.Text = "Restart to finish update";
                var result = MessageBox.Show("A new update downloaded, would you like to restart now to complete the process?", "Update", MessageBoxButtons.YesNo);
                if(result == DialogResult.Yes)
                {
                    await UpdateManager.RestartAppWhenExited();
                    Environment.Exit(0);
                }
            }
        }

        private async void btnCheckForUpdates_Click(object sender, EventArgs e)
        {
            Task task = updateApp(Properties.Settings.Default.updatePrerelease);
            await task;
        }

        private void btnPreReleaseUpdates_Click(object sender, EventArgs e)
        {
            bool flag = !Properties.Settings.Default.updatePrerelease;
            Properties.Settings.Default.updatePrerelease = flag;
            btnPreReleaseUpdates.Checked = flag;
            Properties.Settings.Default.Save();
        }

        private void UpdateProgress(int prog)
        {
            if (pbUpdateProg.GetCurrentParent().InvokeRequired)
            {
                pbUpdateProg.GetCurrentParent().Invoke(new MethodInvoker(delegate { pbUpdateProg.Value = prog; }));
            }
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            var aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void btnReportABug_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/SeanLatimer/I3DMappingExporter/issues");
        }

        private void chkShortenID_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.shortenIDs = chkShortenID.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
