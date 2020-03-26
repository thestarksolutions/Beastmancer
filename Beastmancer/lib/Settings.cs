using RDR2;
using RDR2.Native;
using RDR2.UI;
using RDR2.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Beastmancer
{
    class Settings
    {
        public ScriptSettings settings;
        public string[] allies;

        private string ini_path = "scripts\\Beastmancer.ini";

        public Settings()
        {
            this.settings = ScriptSettings.Load(ini_path);
            this.allies = settings.GetValue("global", "allies").Split(',');
        }

        public void LoadModels()
        {
            foreach (string ally_name in this.allies)
            {
                string model_name = settings.GetValue(ally_name, "model");
                if (model_name != string.Empty)
                {
                    Model model = new Model(model_name);
                    model.Request(5);
                }
            }
        }

        
    }
}
