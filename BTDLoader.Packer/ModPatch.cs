using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BTDBLoader.Packer
{
    public class ModPatch
    {
        public string file;
        public Dictionary<string, JToken> patch;
        public override string ToString()
        {
            string s = file + ": \n";

            foreach (KeyValuePair<string, JToken> entry in patch)
            {
                s += "=> " + entry.Key + " -> " + entry.Value + "\n";

            }

            return s;
        }

        public List<Patch> GetPatches() {
            var ret = new List<Patch>();
            foreach (KeyValuePair<string, JToken> p in patch)
            {
                var pch = new Patch();
                pch.File = this.file;
                pch.Path = p.Key;
                pch.Value = p.Value;
                ret.Add(pch);
            }
            return ret;
        }
    }
}
