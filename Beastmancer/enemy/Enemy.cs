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
    public class Enemy
    {
        public Ped enemy_ped;

        private int resurrect_wait_time = 4000;
        private int res_time;
        public bool has_risen = false;
        public bool slain = false;

        public Enemy(Ped ped)
        {
            this.enemy_ped = ped;
            res_time = Helpers.GetCurrentTime() + resurrect_wait_time;
        }

        public void Attack()
        {
            Function.Call(Hash.REGISTER_HATED_TARGETS_AROUND_PED, enemy_ped, 150f);
            enemy_ped.Task.FightAgainstHatedTargets(150f);
        }

        public void Update()
        {
            if(!has_risen)
            {
                if(Helpers.GetCurrentTime() >= res_time)
                {
                    Function.Call(Hash.REVIVE_INJURED_PED, enemy_ped);
                    Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, enemy_ped, true);
                    Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, enemy_ped, 46, true);
                    Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, enemy_ped, 0, 0);
                    Function.Call(Hash.SET_PED_COMBAT_ABILITY, enemy_ped, 100);
                    enemy_ped.GiveWeapon(WeaponHash.MeleeHatchetMeleeonly, 1);
                    Function.Call(Hash.SET_CURRENT_PED_WEAPON, enemy_ped, WeaponHash.MeleeHatchetMeleeonly.ToString(), true, 0, false, false);
                    World.SetRelationshipBetweenGroups(Relationship.Hate, enemy_ped.RelationshipGroup, Game.Player.Character.RelationshipGroup);
                    enemy_ped.MaxHealth = 2000;
                    enemy_ped.Health = 2000;
                    enemy_ped.HealthCore = 100;
                    Attack();
                    has_risen = true;
                }
            }
            else
            {
                if (enemy_ped.IsOnFire || enemy_ped.IsInWater)
                {
                    enemy_ped.Health = 0;
                }
                //World.AddExplosion(enemy_ped.Position, 23, 0f, 0, false, false);
            }
        }

        public virtual void Destroy()
        {
            slain = true;
        }

    }
}
