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
    class AllyUndead : Ally
    {
        private int life_span = 60000;
        private int life_start;
        private int time_to_live = 0;
        public AllyUndead(Ped ped, bool undead = false)
        {
            this.ally_ped = ped;
            this.ally_name = Names.GetRandomName();
            this.is_undead = undead;
            this.life_start = Client.GetCurrentTime();
            this.ally_ped.IsInvincible = true;
            if (undead)
            {
                Client.SetWeatherTransition();
                Client.LightningStrike(ped.Position);
                Function.Call(Hash.REVIVE_INJURED_PED, ped);
                Debug.Subtitle($"Rise, {ally_name}!");
                Client.ResumeWeather();
            }
            PostCreate();
        }
        
        public override void Update()
        {
            if (this.is_undead)
            {
                World.AddExplosion(ally_ped.Position, 23, 0f, 0, false, false);

                int current_time = Client.GetCurrentTime();
                time_to_live = life_start + life_span - current_time;

                if (BeingAimedAt())
                {
                    Debug.DebugAllyStat($"Time to live: {time_to_live}");
                }
                
                if ((current_time - life_start > life_span || current_time < life_start) && ally_ped.IsAlive)
                {
                    Kill();
                }
            }
            base.Update();
        }

        public override Ped Create(string model_name)
        {
            return null;
        }

        public override void PostCreate()
        {
            AddPedAttributes(ally_ped, ally_name);
            Follow();
        }

        public override void Kill()
        {
            base.Kill();
            World.AddExplosion(ally_ped.Position, 3, 7f, 0, false, false);
            World.AddExplosion(ally_ped.Position, 5, 1f, 0, true, false);
        }
    }
}
