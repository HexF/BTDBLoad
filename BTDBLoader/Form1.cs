using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BTDBLoader.Packer;

namespace BTDBLoader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private List<Mod> EnabledMods = new List<Mod>();
        private List<Mod> LoadedMods = new List<Mod>();
        private void Form1_Load(object sender, EventArgs e)
        {
            versionDisplay.Text = versionDisplay.Text.Replace("{version}", Config.VERSION_STRING);
            Directory.CreateDirectory(Config.APP_PATH);
            Directory.CreateDirectory(Config.MODS_PATH);
            Directory.CreateDirectory(Config.JETE_PATH);
            Directory.CreateDirectory(Config.JETB_PATH);


            RefreshBTDPath();


            ReloadMods();

        }

        private void ReloadMods() {
            var mods = LoadMods();
            dataGridView1.Rows.Clear();
            foreach (Mod m in mods)
            {
                dataGridView1.Rows.Add(true, m.Name, m.Author);
                EnabledMods.Add(m);
                LoadedMods.Add(m);
            }
            //Load mods into the display list
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            // Open mods folder
            Process.Start("explorer.exe", Config.MODS_PATH);
        }

        private List<Mod> LoadMods() {
            var modFiles = Directory.GetFiles(Config.MODS_PATH);
            var mods = new List<Mod>();
            foreach (var mod in modFiles) {
                var ext = Path.GetExtension(mod);
                
                if (ext != ".btdbmod")
                    continue;
                var json = string.Join("\n", File.ReadAllLines(mod));
                var m = JsonConvert.DeserializeObject<Mod>(json);

                mods.Add(m);

                
                    //Parse file as a mod
            }

            return mods;
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            ReloadMods();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            //DeployPatches();
            var JetLocation = Path.Combine(Config.GAME_PATH, "Assets", "data.jet");
            var jv = Config.GAME_JETVERSION;
            var backupName = "org" + jv.ShortHand() + ".jet";
            var backupPath = Path.Combine(Config.JETB_PATH, backupName);
            IncrementProgress();
            if (File.Exists(backupPath))
            {
                File.Delete(JetLocation);
                File.Copy(backupPath, JetLocation);
                IncrementProgress();
            }
            else {
                PatchDeployer.BackupDataJet(JetLocation, jv, Config.JETB_PATH, "org");
                IncrementProgress();
            }

            IncrementProgress();
            var r = PatchDeployer.PatchJet(JetLocation, jv, EnabledMods);
            IncrementProgress(30);
            MessageBox.Show("Finished Patching v" + jv.ShortHand() + "\nInstalled " + EnabledMods.Count.ToString() + " mods");
        }


        private bool TestGameDirectory(string path) {
            Config.GAME_VERSION = "UNKNOWN";
            foreach (string file in Directory.GetFiles(path))
                if (file.Contains("Battles-Win"))
                {
                    Config.GAME_VERSION = FileVersionInfo.GetVersionInfo(Path.Combine(Config.GAME_PATH, "Battles-Win.exe")).ProductVersion;
                    Config.GAME_JETVERSION = new JetVersion() { GameVersion = Config.GAME_VERSION, Distributer = "SW" };
                    return true;
                }
            return false;

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please select the root directory of your steam game instance.");
            folderBrowserDialog1.ShowDialog();
            Config.GAME_PATH = folderBrowserDialog1.SelectedPath;
            RefreshBTDPath();
        }

        private void RefreshBTDPath() {
            label1.Text = "Not Found";
            if (TestGameDirectory(Config.GAME_PATH))
                label1.Text = "Battles-Win v" + Config.GAME_VERSION;

        }

        private void Button6_Click(object sender, EventArgs e)
        {
            var jv = Config.GAME_JETVERSION;

            var JetLocation = Path.Combine(Config.GAME_PATH, "Assets", "data.jet");
            
            var backupName = "org" + jv.ShortHand() + ".jet";
            var backupPath = Path.Combine(Config.JETB_PATH, backupName);

            File.Delete(JetLocation);
            File.Copy(backupPath, JetLocation);
            MessageBox.Show("Restored data.jet for " + jv.ShortHand());
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(Config.GAME_PATH, "Battles-Win.exe"));
        }

        private void Progress_Click(object sender, EventArgs e)
        {

        }

        public void IncrementProgress(int Finish = 100000) {
            if (progress.Value == 100)
                progress.Value = 0;
            progress.Value += 10;
            if (progress.Value >= Finish)
                progress.Value = 100;
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (LoadedMods.Count < e.RowIndex)
                return;
            var mod = LoadedMods[e.RowIndex];
            if (EnabledMods.Contains(mod))
                EnabledMods.Remove(mod);
            else
                EnabledMods.Add(mod);

            
        }
    }
}
