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
        private Label lblInfo;
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
                this.lblInfo.Text = "Status: " + text;
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
                else if (str1 == "PlayerShotgunUpgradeGrenadeLauncherState")
                {
                    if (strArray.Length > 1)
                        this.engine.GrenadeLauncherStateChange(int.Parse(strArray[1].Trim()));
                }
            }
            this.WriteTextSafe(line);
            GC.Collect();
        }

        private void ParseConsole()
        {
            string path = "..\\game\\hlvr\\console.log";
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
            if (!File.Exists("..\\game\\bin\\win64\\hlvr.exe"))
            {
                int num1 = (int)MessageBox.Show("Mod was not extracted in the correct folder", "Error Starting", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!File.Exists("..\\game\\hlvr\\scripts\\vscripts\\tactsuit.lua"))
            {
                int num2 = (int)MessageBox.Show("Script file installation is not correct. Please read the instructions on the mod page and reinstall.", "Script Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!File.ReadAllText("..\\game\\hlvr\\cfg\\skill_manifest.cfg").Contains("script_reload_code tactsuit.lua"))
            {
                int num3 = (int)MessageBox.Show("skill_manifest.cfg file installation is not correct. Please read the instructions on the mod page and reinstall.", "Script Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                this.btnStart.Enabled = false;
                this.engine = new Engine();
                this.engine.initSyncAsync();
                this.WriteTextSafe("Initializing Provolver and starting...");
                this.parsingMode = true;
                new Thread(new ThreadStart(this.ParseConsole)).Start();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.btnStart.Enabled = true;
            this.parsingMode = false;
            this.WriteTextSafe("Stopping...");
        }

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
            this.lblInfo = new Label();
            this.label2 = new Label();
            ((ISupportInitialize)this.pictureBox1).BeginInit();
            this.SuspendLayout();
            this.pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.pictureBox1.Image = Resources.protube;
            this.pictureBox1.Location = new Point(145, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(160, 160);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.btnStart.Font = new Font("Microsoft Sans Serif", 20f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.btnStart.Location = new Point(135, 200);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new Size(188, 61);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new EventHandler(this.btnStart_Click);
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.lblInfo.Location = new Point(25, 335);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new Size(74, 20);
            this.lblInfo.TabIndex = 3;
            this.lblInfo.Text = "Status: Waiting...";
            this.label2.AutoSize = true;
            this.label2.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
            this.label2.Location = new Point(25, 270);
            this.label2.Name = "label2";
            this.label2.Size = new Size(421, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "Make sure you run the game with launch" + Environment.NewLine + " option -condebug";
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.AutoScaleDimensions = new SizeF(8f, 18f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.BackColor = SystemColors.ControlDark;
            this.ClientSize = new Size(463, 373);
            this.Controls.Add((Control)this.label2);
            this.Controls.Add((Control)this.lblInfo);
            this.Controls.Add((Control)this.btnStart);
            this.Controls.Add((Control)this.pictureBox1);
            this.Icon = Resources.favicon;
            this.MaximumSize = new Size(479, 412);
            this.MinimumSize = new Size(479, 412);
            this.Name = nameof(Form1);
            this.Text = "Provolver Alyx Interface";
            this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
            ((ISupportInitialize)this.pictureBox1).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private delegate void SafeCallDelegate(string text);
    }
}
