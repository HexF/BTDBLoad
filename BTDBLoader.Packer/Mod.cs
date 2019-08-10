using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BTDBLoader.Packer
{
    public class Mod
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "title")]
        public string Name;

        public string Author;

        public bool Pool = true;

        public List<ModPatch> Patches;

        public override string ToString()
        {
            string s = string.Format("{0} by {1}\n", Name, Author);
            foreach (ModPatch m in Patches)
                s += m.ToString();
            return s;
        }

        public List<Patch> GetAllPatches()
        {
            var ret = new List<Patch>();
            foreach (ModPatch m in Patches)
                foreach(Patch p in m.GetPatches())
                    ret.Add(p);
                return ret;
        }
    }
}
