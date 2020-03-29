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
        private int life_span;
        private int life_start;
        private int time_to_live = 0;
        private Ped attack_target = null;
        public AllyUndead(Ped ped, bool undead = false, int life_span = 60000)
        {
            this.ally_ped = ped;
            this.ally_name = Names.GetRandomName();
            this.is_undead = undead;
            this.life_start = Helpers.GetCurrentTime();
            this.ally_ped.IsInvincible = true;
            this.life_span = life_span;
            if (undead)
            {
                Helpers.LightningStrike(ped.Position);
                Function.Call(Hash.REVIVE_INJURED_PED, ped);
                Debug.Subtitle($"Rise, {ally_name}!");
            }
            PostCreate();
        }

        public override void Attack(Ped ped)
        {
            attack_target = ped;
            Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, this.ally_ped, false, false);
            this.ally_ped.Task.FollowToEntity(ped, follow_speed, stoppingRange: 1f);
        }

        public override void Follow()
        {
            this.attack_target = null;
            base.Follow();
        }

        public override void Update()
        {
            if (this.is_undead)
            {
                World.AddExplosion(ally_ped.Position, 23, 0f, 0, false, false); // helps visually identify what animals are undead

                int current_time = Helpers.GetCurrentTime();
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

            if(attack_target != null && ally_ped.Position.DistanceTo(attack_target.Position) <= 3f && ally_ped.IsAlive)
            {
                // If near attack target, blow yourself up
                Kill();
            }

            base.Update();
        }

        public override Ped Create(string model_name)
        {
            return null;
        }

        public override void PostCreate()
        {
            ally_ped.AddBlip(BlipType.WhiteDot);

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
