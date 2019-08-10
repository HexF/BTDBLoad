using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BTDBLoader.Packer
{
    public class JetVersion
    {
        public string GameVersion = "6.3.2";
        public string Distributer = "SW"; //SW - Steam Windows

        public string ShortHand()
        {
            return Distributer + GameVersion.Replace(".", "");
        }
    }
}
