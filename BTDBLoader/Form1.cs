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
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

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
            EnabledMods.Clear();
            LoadedMods.Clear();
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
            new Task(() =>
            {
                
                var r = PatchDeployer.PatchJet(JetLocation, jv, EnabledMods);
                
                if (r)
                    BeginInvoke(new MethodInvoker(() => {
                        IncrementProgress(30);
                        MessageBox.Show("Finished Patching v" + jv.ShortHand() + "\nInstalled " + EnabledMods.Count.ToString() + " mods");
                    }));
                else
                    BeginInvoke(new MethodInvoker(() => {
                        IncrementProgress(30);
                        MessageBox.Show("There was an error patching.");
                    })); 
            }).Start();
            
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
            BeginInvoke(new MethodInvoker(() =>
            {
                if (progress.Value == 100)
                    progress.Value = 0;
                progress.Value += 10;
                if (progress.Value >= Finish)
                    progress.Value = 100;
            }
            ));
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

        private void FileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {

        }
        
        private void Button7_Click(object sender, EventArgs e)
        {
            string filePath = "";
            string mName = "";
            string authr = "";
            string fName = "";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Data.jet Files (*.jet)|*.jet";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    //Get the path of specified file
                    return;
                }
                filePath = openFileDialog.FileName;
            }

            while(mName == "")
                mName = ShowDialog("Mod Name", "Input the name for the mod");
            while(authr == "")
                authr = ShowDialog("Author", "Input Author Name");
            while(fName == "")
                fName = ShowDialog("File Name", "Input the name for the mod to be saved as");

            new Task(() =>
            {

                var modzip = PatchDeployer.ExtractJet(filePath, Config.GAME_JETVERSION, Path.Combine(Config.JETE_PATH, "modded", fName));

                IncrementProgress();
                var JetLocation = Path.Combine(Config.GAME_PATH, "Assets", "data.jet");
                IncrementProgress();
                var normZip = PatchDeployer.ExtractJet(JetLocation, Config.GAME_JETVERSION, Config.JETB_PATH);
                IncrementProgress();

                var modInfo = new DirectoryInfo(modzip);
                JArray patchCollection = new JArray();
                foreach (FileInfo file in modInfo.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    var m = string.Join("\n", File.ReadAllLines(file.FullName));
                    string o = "";
                    try
                    {
                        o = string.Join("\n", File.ReadAllLines(file.FullName.Replace(modInfo.FullName, normZip)));
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(file.Name + " is a new file, and thus cannot be processed at this time");
                        continue;

                    }

                    if (m == o)
                        continue;
                    JObject thisPatch = new JObject();
                    var relativePath = file.FullName.Split(new string[] { @"Assets\JSON\" }, StringSplitOptions.None).Last();
                    thisPatch.Add("file", relativePath);

                    JObject patches = new JObject();

                    //There is a difference.
                    try
                    {
                        JObject modded = JObject.Parse(m);
                        JObject defjsn = JObject.Parse(o);

                        var diffs = GetDifferences(modded, defjsn);
                        if (diffs.Count > 0)
                            foreach (JValue diff in diffs)
                                patches.Add(diff.Path, diff);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("There was a parsing error in " + file.Name);
                        continue;
                    }

                    thisPatch.Add("patch", patches);
                    patchCollection.Add(thisPatch);
                }

                var mod = new JObject();
                mod["title"] = mName;
                mod["author"] = authr;
                mod["pool"] = true;
                mod["patches"] = patchCollection;
                //Loop all the files over and compare, saving all JToken and File Pathes that concern us.
                IncrementProgress();
                File.WriteAllText(Path.Combine(Config.MODS_PATH, fName + ".btdbmod"), mod.ToString());
                IncrementProgress(50);

            }).Start();
            

            //Ask for a jet
            //Extract it
            //Take game clean copy, and extract
            //Compare EVERY file
            //Generate patches from 
            //Ask user about Name/Author
            //Save mod into mod directory with filesafe name
        }


        //https://stackoverflow.com/questions/5427020/prompt-dialog-in-windows-forms
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private List<JToken> GetDifferences(JObject a, JObject b) {
            var ret = new List<JToken>();
            foreach (var x in a)
            {
                try {
                    var y = x.Value;
                    var z = b[x.Key];


                    if (y == z)
                        continue;

                    ret = ret.Concat(GetDifferences(x.Value, b[x.Key])).ToList();

                }
                catch (IndexOutOfRangeException)
                {
                    ret.Add(x.Value);
                }
            }

            return ret;
        }

        private List<JToken> GetDifferences(JToken y, JToken z) {
            var ret = new List<JToken>();

            if (y is JObject)
                ret = ret.Concat(GetDifferences(y as JObject, z as JObject)).ToList();
            else if (y is JArray)
            {
                int i = 0;
                foreach (JToken c in y)
                {
                    JToken d = z[i];
                    i++;
                    ret = ret.Concat(GetDifferences(c as JToken, d as JToken)).ToList();
                }
            }
            else
                ret.Add(y);


           

            return ret;
        }

        private void JetModWorker_DoWork(object sender, DoWorkEventArgs e)
        {

        }
    }
}
