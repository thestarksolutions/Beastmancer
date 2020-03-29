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
        private static bool weather_locked = false;
        private static WeatherType current_weather;

        private int kill_ally_timer = 0;
        private int kill_ally_length = 2000;
        private int kill_ally_end = 0;

        private int summon_ally_timer = 0;
        private int summon_ally_length = 3000;
        private int summon_ally_end = 0;

        private int ped_timer = 0;
        private int ped_length = 80000;
        private int ped_end = 0;

        private int summon_deadeye_cost = 60;
        private int ressurect_deadeye_cost = 10;

        private int ally_health_reward = 20;

        public Client()
        {
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
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
            Debug.DebugPluginStat($"{Game.Player.Character.DeadEyeCore.ToString()} {Game.Player.Character.HealthCore.ToString()}");
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {

            Ped ped = GetAimPed();

            Vector3 hit = Raycast.GetAimCoordinates();

            if (Game.IsControlJustReleased(0, RDR2.Control.Whistle) || Game.IsControlJustReleased(0, RDR2.Control.WhistleHorseback))
            {
                summon_ally_timer = 0;

                if (allies.Count != 0)
                {
                    InvokeAlliesAction("Follow");
                    Debug.Subtitle("Hile! To me!");
                }
            }

            if (e.KeyCode == Keys.G)
            {
                kill_ally_timer = 0;
                if (ped != null)
                {

                    if (PedIsAnAlly(ped))
                    {
                        selected_ally = GetAllyFromPed(ped);
                        SelectSound();
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
                                Game.Player.Character.DeadEyeCore = deadeye_available - ressurect_deadeye_cost;
                                Game.Player.Character.StaminaCore = Game.Player.Character.StaminaCore +  ressurect_deadeye_cost;
                                AllyUndead ally_animal = new AllyUndead(ped, undead: true);
                                allies.Add(ally_animal);
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


        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Ped ped = GetAimPed();

            if (Game.IsControlPressed(0, RDR2.Control.Whistle) || Game.IsControlPressed(0, RDR2.Control.WhistleHorseback))
            {

                if (summon_ally_timer != 0)
                {
                    Debug.DebugPluginStat($"Summon timer {summon_ally_end - summon_ally_timer}");
                    if (summon_ally_timer > summon_ally_end)
                    {
                        SummonAllies();
                        summon_ally_end = GetCurrentTime() + summon_ally_length;
                    }
                }
                else
                {
                    summon_ally_end = GetCurrentTime() + summon_ally_length;
                }

                summon_ally_timer = GetCurrentTime();
            }
            else
            {
                summon_ally_timer = 0;
            }

            /*
            if (e.KeyCode == Keys.G)
            {
                if (ped != null && PedIsAnAlly(ped) && ped.IsAlive)
                {
                    if (kill_ally_timer != 0)
                    {
                        Debug.DebugPluginStat($"Kill timer {kill_ally_end - kill_ally_timer}");
                        if (kill_ally_timer > kill_ally_end)
                        {
                            Ally ally = GetAllyFromPed(ped);
                            ally.Kill();
                            kill_ally_end = GetCurrentTime() + kill_ally_length;
                        }
                    }
                    else
                    {
                        kill_ally_end = GetCurrentTime() + kill_ally_length;
                    }

                    kill_ally_timer = GetCurrentTime();
                }
                else
                {
                    kill_ally_timer = 0;
                }
            }
            else
            {
                kill_ally_timer = 0;
            }
            */
        }

        private void CreateEnemies()
        {
            bool enemy_created = false;
            foreach(Ped ped in all_near_peds)
            {
                if(ped.IsDead && PedIsNotAnEnemy(ped) && ped.IsHuman && WasKilledByPlayer(ped) && ped != Game.Player.Character)
                {
                    enemy_created = true;
                    enemies.Add(new Enemy(ped));
                }
            }
            if (enemy_created)
            {
                SetWeatherTransition();
                Debug.Subtitle("The dead rise...");
                ResumeWeather();
            }
        }

        private bool WasKilledByPlayer(Ped ped)
        {
            Entity entity = Function.Call<Entity>(Hash.GET_PED_SOURCE_OF_DEATH, ped);
            if(entity == Game.Player.Character)
            {
                return true;
            }
            return false;
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
                ped_timer = GetCurrentTime();
                Debug.DebugPluginStat("Returning no update");
                return all_near_peds;
            }
            else
            {
                ped_timer = 0;
                ped_end = GetCurrentTime() + ped_length;
                Debug.DebugAllyEvent("Collecting peds...");
                Ped[] peds = World.GetAllPeds();
                return peds.ToList<Ped>();
            }
        }

        private void InvokeAlliesAction(string action_name)
        {
            // can only call actions with no user params atm
            foreach(Ally ally in allies)
            {
                Type type = ally.GetType();
                MethodInfo ally_method = type.GetMethod(action_name);
                ally_method.Invoke(ally, null);
            }
        }

        private void InvokeEnemiesAction(string action_name)
        {
            // can only call actions with no user params atm
            foreach (Enemy enemy in enemies)
            {
                Type type = enemy.GetType();
                MethodInfo enemy_method = type.GetMethod(action_name);
                enemy_method.Invoke(enemy, null);
            }
        }

        private void SelectSound()
        {
            Function.Call(Hash.PLAY_SOUND_FRONTEND, "SELECT", "HUD_SHOP_SOUNDSET", true, 0);
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
                    //enemies.Remove(enemy);
                    Game.Player.Character.DeadEyeCore = Game.Player.Character.DeadEyeCore + ressurect_deadeye_cost;
                    enemy.Destroy();
                    //all_near_peds.Remove(enemy.enemy_ped);
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

        private Ped GetAimPed()
        {
            Entity tmp = GetAimEntity();
            if (tmp != null)
            {
                if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, tmp))
                {
                    return Function.Call<Ped>(Hash.GET_PED_INDEX_FROM_ENTITY_INDEX, tmp);
                }
            }

            return null;
        }

        private Entity GetAimEntity()
        {
            OutputArgument output = new OutputArgument();
            bool target_is_entity = Function.Call<bool>(Hash.GET_ENTITY_PLAYER_IS_FREE_AIMING_AT, Game.Player, output);
            if (target_is_entity)
            {
                return output.GetResult<Entity>();
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

            SetWeatherTransition();
            try
            {
                foreach (string ally in settings.allies)
                {
                    Function.Call(Hash.FORCE_LIGHTNING_FLASH);
                    string ally_name = settings.settings.GetValue(ally, "name");
                    string model_name = settings.settings.GetValue(ally, "model");
                    string can_fly = settings.settings.GetValue(ally, "can_fly");
                    AllyFamiliar new_ally = Summon(ally_name, model_name, can_fly);
                    if(new_ally.ally_ped != null)
                    {
                        allies.Add(new_ally);
                        new_ally.ally_ped.IsInvincible = true;
                    }
                    Debug.DebugAllyEvent($"Summoned {ally_name}!");
                }
            }
            catch
            {
                //Debug.DebugPluginStat($"Summoning failed...");
            }
            finally
            {
                ResumeWeather();
            }
            
        }

        public static void SetWeatherTransition()
        {
            if (weather_locked)
            {
                return;
            }
            weather_locked = true;
            current_weather = World.CurrentWeather;
            World.CurrentWeather = WeatherType.Sunny;
            Wait(500);

            World.CurrentWeather = WeatherType.Thunderstorm;
            Function.Call(Hash.FORCE_LIGHTNING_FLASH);
            Wait(1000);
        }

        public static int GetCurrentTime()
        {
            return Function.Call<int>(Hash.GET_GAME_TIMER);
        }

        public static void ResumeWeather()
        {
            World.CurrentWeather = current_weather;
            weather_locked = false;
        }

        private AllyFamiliar Summon(string ally_name, string model_name, string can_fly)
        {
            return new AllyFamiliar(model_name, ally_name, can_fly: can_fly == string.Empty ? false : true);
        }

        public static void LightningStrike(Vector3 position)
        {
            Function.Call((Hash)0x67943537D179597C, position.X, position.Y, position.Z);
            //Script.Wait(1000);
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
