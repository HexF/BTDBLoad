using BTDBLoader.Packer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BTDBLoader
{
    class Config
    {
        public static string VERSION_STRING = "1.0.2a";

        public static string APP_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"btdml");
        public static string MODS_PATH = Path.Combine(APP_PATH, "mods");
        public static string JETE_PATH = Path.Combine(APP_PATH, "jetextracts");
        public static string JETB_PATH = Path.Combine(APP_PATH, "jetbackups");

        public static string GAME_PATH = @"C:\Program Files (x86)\Steam\steamapps\common\Bloons TD Battles";
        public static string GAME_VERSION = "UNKNOWN";

        public static JetVersion GAME_JETVERSION = null;
    }
}
