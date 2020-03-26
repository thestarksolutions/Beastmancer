using RDR2;
using RDR2.Native;
using RDR2.UI;
using RDR2.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace Beastmancer
{
    class AllySpecial : Ally
    {
        private float speed_multiplier = 15f;
        private float stamina = 100f;

        public AllySpecial(string model_name, string ally_name, int max_health = 200, bool can_fly = false)
        {
            this.max_health = max_health;
            this.ally_name = Names.GetRandomName();
            this.ally_ped = Create(model_name);
            PostCreate();
            this.can_fly = can_fly;
        }

        public override void PostCreate()
        {
            AddPedAttributes(ally_ped, ally_name);
        }

        public override Ped Create(string model_name)
        {

            Model ped_model = new Model(model_name);
            Vector3 position = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 15, 0));

            bool model_loaded = ped_model.Request(5);

            if (!model_loaded)
            {
                return null;
            }

            Client.LightningStrike(position);
            var ped = Function.Call<Ped>(Hash.CREATE_PED, ped_model.Hash, position.X, position.Y, position.Z, 0, false, false, true);
            
            Debug.DebugThree($"Created! {model_name} ");
            
            return ped;
        }

        public override void AttackInArea()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {

            if (ally_ped.IsSprinting && ! can_fly)
            {
                ally_ped.ApplyForce(ally_ped.ForwardVector * speed_multiplier);
            }

            SetStamina(stamina);

            base.Update();
        }

    }
}
