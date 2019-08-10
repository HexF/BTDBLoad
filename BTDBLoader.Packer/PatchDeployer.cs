using Ionic.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BTDBLoader.Packer
{
    class PatchDeployer
    {
       
        public static string DeployPatch(string json, Patch patch)
        {
            var j = JObject.Parse(json);
            var tokens = j.SelectTokens(patch.Path);
            foreach (var token in tokens)
            {
                var par = token.Parent;
                if (par is JProperty)
                {
                    var p = par as JProperty;
                    p.Value = patch.Value;
                }
            }
            return j.ToString();
        }


        public static bool ModConflicts(Mod a, Mod b)
        {
            if (a == b)
                return false;
            var ap = a.GetAllPatches();
            var bp = b.GetAllPatches();

            foreach (Patch pa in ap)
            {
                foreach (Patch pb in bp)
                {
                    if (pa.File != pb.File)
                        continue;
                    if (pb.Path == pa.Path)
                    {
                        if (pb.Value != pa.Value)
                            return true;
                    }
                }
            }


            return false;
        }

        public static List<Mod> GetModConflicts(List<Mod> mods)
        {
            var cm = new List<Mod>();
            foreach (var m1 in mods)
            {
                foreach (var m2 in mods)
                {
                    if (m1 == m2)
                        continue;
                    if (ModConflicts(m1, m2))
                    {
                        if (!cm.Contains(m1))
                            cm.Add(m1);
                        if (!cm.Contains(m2))
                            cm.Add(m2);
                    }
                }

            }
            return cm;
        }

        public static List<Patch> GetPatchesForFile(List<Mod> mods, string file)
        {
            var ret = new List<Patch>();
            foreach (Mod m in mods)
            {
                var p = m.GetAllPatches();
                foreach (Patch c in p)
                {
                    if (file == c.File)
                        ret.Add(c);
                }
            }

            return ret;
        }

        public static Dictionary<string, List<Patch>> GetPatchesForFiles(List<Mod> mods, JetVersion jv)
        {
            var ret = new Dictionary<string, List<Patch>>();
            foreach (Mod m in mods)
            {
                foreach (Patch p in m.GetAllPatches())
                {
                    var f = p.File;
                    if (!ret.ContainsKey(f))
                        ret.Add(f, new List<Patch>());

                    ret.TryGetValue(f, out var patches);
                    patches.Add(p);
                }
            }

            return ret;
        }



        public static void BackupDataJet(string DataJetLocation, JetVersion jv, string StorageLocation, string Name = "")
        {
            File.Copy(DataJetLocation, Path.Combine(StorageLocation, Name + jv.ShortHand() + ".jet"));
        }

        public static string ExtractJet(string DataJetLocation, JetVersion jv, string StorageLocation, string Password = "RETR")
        {
            ZipFile zf = ZipFile.Read(DataJetLocation);
            string loc = Path.Combine(StorageLocation, jv.ShortHand());

            DJ_PASSWORDS.TryGetValue(jv.ShortHand(), out var pw);
            zf.Password = Password;


            if (Password == "RETR")
                zf.Password = pw;
            try
            {
                zf.ExtractAll(loc);
            }
            catch (Exception){}
            zf.Dispose();

            return loc;
        }

        public static string GetFileFromJet(string file, string DataJetLocation, JetVersion jv, string Password = "RETR") {
            ZipFile zf = ZipFile.Read(DataJetLocation);

            DJ_PASSWORDS.TryGetValue(jv.ShortHand(), out var pw);
            var pass = Password;
            if (Password == "RETR")
                pass = pw;
            if (!zf.ContainsEntry(file))
                return "error";

            ZipEntry ze = null;
            foreach (ZipEntry e in zf) {
                if (e.FileName == file)
                    ze = e;                    
            }

            var ms = new MemoryStream();
            ze.ExtractWithPassword(ms, pass);
            var bytes = ms.ToArray();
            var content = Encoding.UTF8.GetString(bytes);

            zf.Dispose();
            ms.Dispose();
            return content;
        }
        public static void UpdateFileInJet(string file, string content, string DataJetLocation, JetVersion jv, string Password = "RETR")
        {
            ZipFile zf = ZipFile.Read(DataJetLocation);

            DJ_PASSWORDS.TryGetValue(jv.ShortHand(), out var pw);
            zf.Password = Password;
            if (Password == "RETR")
                zf.Password = pw;

            zf.UpdateEntry(file, content);

            zf.Save();
            zf.Dispose();
        }

        public static bool PatchFile(string DataJetLocation, JetVersion jv, List<Patch> patches, string fileName, string JetPassword = "RETR") {
            
            var file = Path.Combine("Assets", "JSON", fileName);
            file = file.Replace(@"\", @"/");
            foreach (Patch p in patches)
                if (fileName != p.File)
                    return false;


            var ctn = GetFileFromJet(file, DataJetLocation, jv, JetPassword);
            foreach (Patch p in patches)
            {
                ctn = DeployPatch(ctn, p);
            }
            
            UpdateFileInJet(file, ctn, DataJetLocation, jv, JetPassword);
          

            return true;
        }

        public static bool PatchJet(string DataJetLocation, JetVersion jv, List<Mod> mods, string JetPassword = "RETR") {

            var conflits = GetModConflicts(mods);
            if (conflits.Count != 0)
                return false;

            var retVal = true;

            var pf = GetPatchesForFiles(mods,jv);

            foreach (KeyValuePair<string, List<Patch>> f in pf) {
                retVal = retVal && PatchFile(DataJetLocation, jv, f.Value, f.Key, JetPassword);
            }
            
            return retVal;
        }
    }
}
