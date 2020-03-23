using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDR2;
using RDR2.Native;
using RDR2.UI;
using RDR2.Math;

namespace Beastmancer
{
    public abstract class Ally
    {
        private Ped ally_ped;
        private string ally_name;

        public Ally(Ped ped)
        {
            this.ally_ped = ped;
            this.ally_name = GetName();
            CreateAlly();
        }

        public Ally(string model_name)
        {
            int hash_key = Int32.Parse(Enum.GetName(typeof(PedHash), model_name));
            CreateAlly();
        }

        private void CreateAlly()
        {
            ally_ped.SetPedPromptName(ally_name);
        }

        public abstract string GetName();
        public abstract void PostCreate();

    }
}
