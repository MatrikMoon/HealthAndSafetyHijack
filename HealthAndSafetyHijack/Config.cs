using HealthAndSafetyHijack.SimpleJSON;
using System;
using System.IO;

namespace HealthAndSafetyHijack
{
    class Config
    {
        private static bool _showDisclaimer = true;
        public static bool ShowDisclaimer
        {
            get { return _showDisclaimer; }
            set
            {
                _showDisclaimer = value;
                SaveConfig();
            }
        }

        private static string ConfigLocation = $"{Environment.CurrentDirectory}/UserData/Disclaimers.json";

        public static void LoadConfig()
        {
            if (File.Exists(ConfigLocation))
            {
                JSONNode node = JSON.Parse(File.ReadAllText(ConfigLocation));
                ShowDisclaimer = Convert.ToBoolean(node["ShowDisclaimer"].Value);
            }
        }

        public static void SaveConfig()
        {
            JSONNode node = new JSONObject();
            node["ShowDisclaimer"] = ShowDisclaimer;
            File.WriteAllText(ConfigLocation, node.ToString());
        }
    }
}
