using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BTDBLoader.Packer
{
    public class Patch
    {
        public string File;
        public string Path;
        public JToken Value;

        public override string ToString()
        {
            return string.Format("Patch in {0} with JSON path of {1} => {2}", File, Path, Value);
        }
    }
}
