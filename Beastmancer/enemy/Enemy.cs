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
            res_time = Client.GetCurrentTime() + resurrect_wait_time;
            enemy_ped.GiveWeapon(WeaponHash.MeleeHatchetMeleeonly, 1);
        }

        public void Attack()
        {
            //Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, this.enemy_ped, false, false);
            Function.Call(Hash.REGISTER_HATED_TARGETS_AROUND_PED, enemy_ped, 150f);
            enemy_ped.Task.FightAgainstHatedTargets(150f);
            //enemy_ped.Task.Combat(Game.Player.Character);
        }

        public void Update()
        {
            if(!has_risen)
            {
                if(Client.GetCurrentTime() >= res_time)
                {
                    

                    Function.Call(Hash.REVIVE_INJURED_PED, enemy_ped);
                    //Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, this.enemy_ped, false, false);
                    Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, enemy_ped, true);
                    Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, enemy_ped, 46, true);
                    Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, enemy_ped, 0, 0);
                    Function.Call(Hash.SET_PED_COMBAT_ABILITY, enemy_ped, 100);
                    WeaponHash weapon = GetRandomMeleeWeapon();
                    //enemy_ped.GiveWeapon(weapon, 1);
                    //Function.Call(Hash.GIVE_DELAYED_WEAPON_TO_PED, enemy_ped, WeaponHash.MeleeHatchetMeleeonly.ToString(), true, 0);
                    Function.Call(Hash.SET_CURRENT_PED_WEAPON, enemy_ped, WeaponHash.MeleeHatchetMeleeonly.ToString(), true, 0, false, false);
                    World.SetRelationshipBetweenGroups(Relationship.Hate, enemy_ped.RelationshipGroup, Game.Player.Character.RelationshipGroup);
                    //enemy_ped.Task.FightAgainstHatedTargets(100f);
                    //enemy_ped.IsInvincible = true;
                    //enemy_ped.CanSufferCriticalHits = false;
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
            //World.AddExplosion(enemy_ped.Position, 3, 7f, 0, false, false);
            //World.AddExplosion(enemy_ped.Position, 5, .2f, 0, true, false);
            slain = true;
            //enemy_ped.MarkAsNoLongerNeeded();
        }

        private void WaitForRes()
        {
            int current_time = Client.GetCurrentTime();
            int ready_time = Client.GetCurrentTime() + resurrect_wait_time;
            while (current_time < ready_time)
            {
                Debug.DebugAllyStat("waiting...");
                current_time = Client.GetCurrentTime();
            }
        }

        private WeaponHash GetRandomMeleeWeapon()
        {
            List<WeaponHash> melee_weapons = new List<WeaponHash>()
            {
                WeaponHash.MeleeHatchetMeleeonly,
                WeaponHash.MeleeKnife,

            };
            var random = new Random();
            return melee_weapons[random.Next(melee_weapons.Count)];
        }

    }
}
