using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Provolver_HalfLifeAlyx.Properties;

namespace Provolver_HalfLifeAlyx
{
    public class Form1 : Form
    {
        private bool parsingMode;
        public Engine engine;
        private IContainer components;
        private PictureBox pictureBox1;
        private Button btnStart;
        private Button btnStop;
        private Label lblInfo;
        private TextBox txtAlyxDirectory;
        private Button btnBrowse;
        private Label label1;
        private Label label2;

        public Form1() => this.InitializeComponent();

        public static IEnumerable<string> ReadLines(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
            {
                using (StreamReader sr = new StreamReader((Stream)fs, Encoding.UTF8))
                {
                    string str;
                    while ((str = sr.ReadLine()) != null)
                        yield return str;
                }
            }
        }

        private void WriteTextSafe(string text)
        {
            if (this.lblInfo.InvokeRequired)
                this.lblInfo.Invoke((Delegate)new Form1.SafeCallDelegate(this.WriteTextSafe), (object)text);
            else
                this.lblInfo.Text = text;
        }

        private void ParseLine(string line)
        {
            string[] strArray = line.Replace('{', ' ').Replace('}', ' ').Trim().Split('|');
            if (strArray.Length != 0)
            {
                string str1 = strArray[0].Trim();
                if (str1 == "PlayerShootWeapon")
                {
                    if (strArray.Length > 1)
                        this.engine.PlayerShoot(strArray[1].Trim());
                }
                else if (str1 == "PlayerPistolClipInserted" || str1 == "PlayerShotgunShellLoaded" || str1 == "PlayerRapidfireInsertedCapsuleInChamber" || str1 == "PlayerRapidfireInsertedCapsuleInMagazine")
                    this.engine.ClipInserted();
                else if (str1 == "PlayerPistolChamberedRound" || str1 == "PlayerShotgunLoadedShells" || str1 == "PlayerRapidfireClosedCasing" || str1 == "PlayerRapidfireOpenedCasing")
                    this.engine.ChamberedRound();
            }
            this.WriteTextSafe(line);
            GC.Collect();
        }

        private void ParseConsole()
        {
            string path = this.txtAlyxDirectory.Text + "\\game\\hlvr\\console.log";
            bool flag = true;
            int count = 0;
            while (this.parsingMode)
            {
                if (File.Exists(path))
                {
                    if (flag)
                    {
                        flag = false;
                        this.WriteTextSafe("Interface active");
                        count = Form1.ReadLines(path).Count<string>();
                    }
                    int num = Form1.ReadLines(path).Count<string>();
                    if (count < num && num > 0)
                    {
                        List<string> list = Form1.ReadLines(path).Skip<string>(count).Take<string>(num - count).ToList<string>();
                        for (int index = 0; index < list.Count; ++index)
                        {
                            if (list[index].Contains("[Tactsuit]"))
                            {
                                string line = list[index].Substring(list[index].LastIndexOf(']') + 1);
                                new Thread((ThreadStart)(() => this.ParseLine(line))).Start();
                            }
                            else if (list[index].Contains("unpaused the game"))
                                this.engine.menuOpen = false;
                            else if (list[index].Contains("paused the game"))
                                this.engine.menuOpen = true;
                        }
                        count += list.Count;
                    }
                    else if (count == num && num > 0)
                        Thread.Sleep(50);
                    else
                        count = 0;
                }
                else
                {
                    this.WriteTextSafe("Cannot file console.log. Waiting.");
                    Thread.Sleep(2000);
                }
            }
            this.WriteTextSafe("Waiting...");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!File.Exists(this.txtAlyxDirectory.Text + "\\game\\bin\\win64\\hlvr.exe"))
            {
                int num1 = (int)MessageBox.Show("Please select your Half-Life Alyx installation folder correctly first.", "Error Starting", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!File.Exists(this.txtAlyxDirectory.Text + "\\game\\hlvr\\scripts\\vscripts\\tactsuit.lua"))
            {
                int num2 = (int)MessageBox.Show("Script file installation is not correct. Please read the instructions on the mod page and reinstall.", "Script Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!File.ReadAllText(this.txtAlyxDirectory.Text + "\\game\\hlvr\\cfg\\skill_manifest.cfg").Contains("script_reload_code tactsuit.lua"))
            {
                int num3 = (int)MessageBox.Show("skill_manifest.cfg file installation is not correct. Please read the instructions on the mod page and reinstall.", "Script Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                this.btnStart.Enabled = false;
                this.btnStop.Enabled = true;
                this.btnBrowse.Enabled = false;
                this.engine = new Engine();
                this.WriteTextSafe("Starting...");
                this.parsingMode = true;
                new Thread(new ThreadStart(this.ParseConsole)).Start();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
            this.btnBrowse.Enabled = true;
            this.parsingMode = false;
            this.WriteTextSafe("Stopping...");
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog();
            commonOpenFileDialog.InitialDirectory = "C:\\";
            commonOpenFileDialog.RestoreDirectory = true;
            commonOpenFileDialog.IsFolderPicker = true;
            if (commonOpenFileDialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;
            this.txtAlyxDirectory.Text = commonOpenFileDialog.FileName;
            Settings.Default.AlyxDirectory = this.txtAlyxDirectory.Text;
            Settings.Default.Save();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
            this.btnBrowse.Enabled = true;
            this.parsingMode = false;
            this.WriteTextSafe("Stopping...");
        }

        private void Form1_Load(object sender, EventArgs e) => this.txtAlyxDirectory.Text = Settings.Default.AlyxDirectory;

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Form1));
            this.pictureBox1 = new PictureBox();
            this.btnStart = new Button();
            this.btnStop = new Button();
            this.lblInfo = new Label();
            this.txtAlyxDirectory = new TextBox();
            this.btnBrowse = new Button();
            this.label1 = new Label();
            this.label2 = new Label();
            ((ISupportInitialize)this.pictureBox1).BeginInit();
            this.SuspendLayout();
            this.pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            //this.pictureBox1.Image = (Image)componentResourceManager.GetObject("protube");
            this.pictureBox1.Location = new Point(125, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(191, 50);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.btnStart.Font = new Font("Microsoft Sans Serif", 20f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.btnStart.Location = new Point(29, 88);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new Size(188, 61);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new EventHandler(this.btnStart_Click);
            this.btnStop.Enabled = false;
            this.btnStop.Font = new Font("Microsoft Sans Serif", 20f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.btnStop.Location = new Point(245, 88);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new Size(188, 61);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new EventHandler(this.btnStop_Click);
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lblInfo.Location = new Point(25, 342);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new Size(74, 20);
            this.lblInfo.TabIndex = 3;
            this.lblInfo.Text = "Waiting...";
            this.txtAlyxDirectory.Enabled = false;
            this.txtAlyxDirectory.Font = new Font("Microsoft Sans Serif", 10f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.txtAlyxDirectory.Location = new Point(29, (int)byte.MaxValue);
            this.txtAlyxDirectory.Name = "txtAlyxDirectory";
            this.txtAlyxDirectory.Size = new Size(323, 23);
            this.txtAlyxDirectory.TabIndex = 4;
            this.txtAlyxDirectory.Text = "C:\\Steam\\steamapps\\common\\Half-Life Alyx";
            this.btnBrowse.Location = new Point(358, (int)byte.MaxValue);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new Size(75, 23);
            this.btnBrowse.TabIndex = 5;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
            this.label1.AutoSize = true;
            this.label1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label1.Location = new Point(25, 232);
            this.label1.Name = "label1";
            this.label1.Size = new Size(276, 20);
            this.label1.TabIndex = 6;
            this.label1.Text = "Select Your Half-Life Alyx install folder";
            this.label2.AutoSize = true;
            this.label2.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label2.Location = new Point(25, 304);
            this.label2.Name = "label2";
            this.label2.Size = new Size(421, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Make sure you run the game with launch option -condebug";
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.BackColor = SystemColors.ControlDarkDark;
            this.ClientSize = new Size(463, 373);
            this.Controls.Add((Control)this.label2);
            this.Controls.Add((Control)this.label1);
            this.Controls.Add((Control)this.btnBrowse);
            this.Controls.Add((Control)this.txtAlyxDirectory);
            this.Controls.Add((Control)this.lblInfo);
            this.Controls.Add((Control)this.btnStop);
            this.Controls.Add((Control)this.btnStart);
            this.Controls.Add((Control)this.pictureBox1);
            //this.Icon = (Icon)componentResourceManager.GetObject("favicon");
            this.MaximumSize = new Size(479, 412);
            this.MinimumSize = new Size(479, 412);
            this.Name = nameof(Form1);
            this.Text = "Provolver Tactsuit Alyx Interface";
            this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new EventHandler(this.Form1_Load);
            ((ISupportInitialize)this.pictureBox1).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private delegate void SafeCallDelegate(string text);
    }
}
