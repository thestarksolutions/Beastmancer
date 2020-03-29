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
    public class Client : Script
    {
        private List<Ped> all_near_peds = new List<Ped>();
        private List<Ally> allies = new List<Ally>();
        private List<Enemy> enemies = new List<Enemy>();
        private Settings settings = new Settings();
        private Ally selected_ally = null;

        private int summon_ally_timer = 0;
        private int summon_ally_length = 2000;
        private int summon_ally_end = 0;

        private int ped_timer = 0;
        private int ped_length;
        private int ped_end = 0;

        private int summon_deadeye_cost;
        private int ressurect_deadeye_cost;
        private int ressurect_stamina_reward;
        private int kill_zombie_deadeye_reward;
        private int ally_health_reward;

        public Client()
        {
            ped_length = Int32.Parse(settings.settings.GetValue("global", "collect_peds", "80000"));
            summon_deadeye_cost = Int32.Parse(settings.settings.GetValue("global", "summon_allies_deadeye_cost", "60"));
            ressurect_deadeye_cost = Int32.Parse(settings.settings.GetValue("global", "ressurect_animal_deadeye_cost", "10"));
            ressurect_stamina_reward = Int32.Parse(settings.settings.GetValue("global", "ressurect_stamina_reward", "10"));
            ally_health_reward = Int32.Parse(settings.settings.GetValue("global", "undead_animal_death_health_reward", "20"));
            kill_zombie_deadeye_reward = Int32.Parse(settings.settings.GetValue("global", "kill_zombie_deadeye_reward", "20"));
            // KeyDown += OnKeyDown;
            // KeyUp += OnKeyUp;
            
            Tick += OnTick;
            Interval = 1;
        }
        
        private void OnTick(object sender, EventArgs evt)
        {
            CheckAllyStatus();
            CheckEnemyStatus();
            InvokeAlliesAction("Update");
            InvokeEnemiesAction("Update");

            all_near_peds = CollectPeds();
            CreateEnemies();
            if (Game.Player.IsDead)
            {
                InvokeAlliesAction("Kill");
            }

            CheckControls();
        }

        private void CheckControls()
        {
            if (Game.IsControlPressed(0, RDR2.Control.Whistle) || Game.IsControlPressed(0, RDR2.Control.WhistleHorseback))
            {

                if (summon_ally_timer != 0)
                {
                    if (summon_ally_timer > summon_ally_end)
                    {
                        SummonAllies();
                        summon_ally_end = Helpers.GetCurrentTime() + summon_ally_length;
                    }
                }
                else
                {
                    summon_ally_end = Helpers.GetCurrentTime() + summon_ally_length;
                }

                summon_ally_timer = Helpers.GetCurrentTime();
            }
            else
            {
                summon_ally_timer = 0;
            }

            CheckControlReleased();
        }

        private void CheckControlReleased()
        {

            Ped ped = Helpers.GetAimPed();
            Vector3 hit = Raycast.GetAimCoordinates();

            if (Game.IsControlJustReleased(0, RDR2.Control.Whistle) || Game.IsControlJustReleased(0, RDR2.Control.WhistleHorseback))
            {
                summon_ally_timer = 0;

                if (allies.Count != 0)
                {
                    InvokeAlliesAction("Follow");
                }
            }

            if (Game.IsControlJustReleased(0, RDR2.Control.Cover))
            {
                if (ped != null)
                {

                    if (PedIsAnAlly(ped))
                    {
                        selected_ally = GetAllyFromPed(ped);
                        Helpers.SelectSound();
                    }
                    else
                    {
                        int deadeye_available = Game.Player.Character.DeadEyeCore;
                        if (!ped.IsHuman && ped.IsDead)
                        {
                            if(deadeye_available < ressurect_deadeye_cost)
                            {
                                Debug.Subtitle("Can't resurrect, not enough deadeye...");
                            }
                            else
                            {
                                Helpers.SetWeatherTransition();
                                Wait(1000);
                                Game.Player.Character.DeadEyeCore = deadeye_available - ressurect_deadeye_cost;
                                Game.Player.Character.StaminaCore = Game.Player.Character.StaminaCore +  ressurect_stamina_reward;
                                string life_span = settings.settings.GetValue("undeadanimal", "life_span", "80000");
                                AllyUndead ally_animal = new AllyUndead(ped, undead: true, life_span: Int32.Parse(life_span));
                                allies.Add(ally_animal);
                                Helpers.ResumeWeather();
                            }
                        }
                        else if (selected_ally != null)
                        {
                            selected_ally.Attack(ped);
                        }
                    }
                }
                else if (selected_ally != null && hit != Vector3.Zero)
                {
                    selected_ally.MoveTo(hit);
                }

                if(! Game.IsControlPressed(0, RDR2.Control.Aim))
                {
                    InvokeAlliesAction("AttackInArea");
                }
            }
        }


        private void CreateEnemies()
        {
            bool enemy_created = false;
            foreach(Ped ped in all_near_peds)
            {
                if(ped.IsDead && PedIsNotAnEnemy(ped) && ped.IsHuman && Helpers.WasKilledByPlayer(ped) && ped != Game.Player.Character)
                {
                    enemy_created = true;
                    enemies.Add(new Enemy(ped));
                }
            }
            if (enemy_created)
            {
                Helpers.SetWeatherTransition();
                Wait(1000);
                Debug.Subtitle("The dead rise...");
                Helpers.ResumeWeather();
            }
        }

        private bool PedIsNotAnEnemy(Ped ped)
        {
            foreach(Enemy enemy in enemies)
            {
                if (enemy.enemy_ped == ped)
                {
                    return false;
                }
            }
            return true;
        }

        private List<Ped> CollectPeds()
        {
            if(ped_timer < ped_end)
            {
                ped_timer = Helpers.GetCurrentTime();
                return all_near_peds;
            }
            else
            {
                ped_timer = 0;
                ped_end = Helpers.GetCurrentTime() + ped_length;
                Ped[] peds = World.GetAllPeds();
                return peds.ToList<Ped>();
            }
        }

        private void InvokeAlliesAction(string action_name)
        {
            foreach(Ally ally in allies)
            {
                Type type = ally.GetType();
                MethodInfo ally_method = type.GetMethod(action_name);
                ally_method.Invoke(ally, null);
            }
        }

        private void InvokeEnemiesAction(string action_name)
        {
            foreach (Enemy enemy in enemies)
            {
                Type type = enemy.GetType();
                MethodInfo enemy_method = type.GetMethod(action_name);
                enemy_method.Invoke(enemy, null);
            }
        }

        private void CheckAllyStatus()
        {
            foreach (Ally ally in allies.ToList())
            {
                if (ally.ally_ped.IsDead)
                {
                    if (selected_ally == ally)
                    {
                        selected_ally = null;
                    }
                    Game.Player.Character.HealthCore = Game.Player.Character.HealthCore + ally_health_reward;
                    allies.Remove(ally);
                    ally.Destroy();
                }
            }
        }

        private void CheckEnemyStatus()
        {
            foreach (Enemy enemy in enemies.ToList())
            {
                if (enemy.enemy_ped.IsDead && enemy.has_risen && ! enemy.slain)
                {
                    Entity entity = Function.Call<Entity>(Hash.GET_PED_SOURCE_OF_DEATH, enemy.enemy_ped);
                    if(Function.Call<bool>(Hash.IS_ENTITY_A_PED, entity))
                    {
                        Ped killer = Function.Call<Ped>(Hash.GET_PED_INDEX_FROM_ENTITY_INDEX, entity);
                        if (PedIsAnAlly(killer))
                        {
                            Game.Player.Character.HealthCore = Game.Player.Character.HealthCore + 10;
                            Game.Player.Character.Health = Game.Player.Character.Health + 10;
                        }
                    }
                    Game.Player.Character.DeadEyeCore = Game.Player.Character.DeadEyeCore + kill_zombie_deadeye_reward;
                    enemy.Destroy();
                }
            }
        }

        private Ally GetAllyFromPed(Ped ped)
        {
            foreach(Ally ally in allies)
            {
                if (ally.ally_ped == ped)
                {
                    return ally;
                }
            }

            return null;
        }

        private void SummonAllies()
        {
            int deadeye_available = Game.Player.Character.DeadEyeCore;
            if(deadeye_available < summon_deadeye_cost)
            {
                Debug.Subtitle("Can't summon familiar, not enough deadeye...");
                return;
            }
            else
            {
                Game.Player.Character.DeadEyeCore = deadeye_available - summon_deadeye_cost;
            }

            Helpers.SetWeatherTransition();
            Wait(1000);
            foreach (string ally in settings.allies)
            {
                Function.Call(Hash.FORCE_LIGHTNING_FLASH);
                string ally_name = settings.settings.GetValue(ally, "name");
                string model_name = settings.settings.GetValue(ally, "model");
                string can_fly = settings.settings.GetValue(ally, "can_fly");
                string speed_multiplier = settings.settings.GetValue(ally, "speed_multiplier");
                AllyFamiliar new_ally = Summon(ally_name, model_name, can_fly, speed_multiplier);
                if(new_ally.ally_ped != null)
                {
                    allies.Add(new_ally);
                    new_ally.ally_ped.IsInvincible = true;
                }
            }
            Helpers.ResumeWeather();

        }

        private AllyFamiliar Summon(string ally_name, string model_name, string can_fly, string speed_multiplier)
        {
            return new AllyFamiliar(model_name, ally_name, can_fly: can_fly == string.Empty ? false : true, speed_multiplier: (float)Int32.Parse(speed_multiplier));
        }

        private bool PedIsAnAlly(Ped ped)
        {
            foreach (Ally ally in allies)
            {
                if (ally.ally_ped == ped)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
