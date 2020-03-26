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
        //private Vector3 last_hit_pos = Vector3.Zero;
        private List<Ped> targets = new List<Ped>();
        private List<Ally> allies = new List<Ally>();
        private Settings settings = new Settings();
        private Ally selected_ally = null;
        
        public Client()
        {
            KeyDown += OnKeyDown;
            Tick += OnTick;
            Interval = 1;
        }
        
        private void OnTick(object sender, EventArgs evt)
        {
            CheckAllyStatus();
            //CheckForNewAlly();
            InvokeAlliesAction("Update");
        }

        private void CheckForNewAlly()
        {
            throw new NotImplementedException();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // probably should consolidate these
            Ped ped = GetAimPed();
            
            Vector3 hit = Raycast.GetAimCoordinates();

            if (Game.IsControlPressed(0, RDR2.Control.Whistle) || Game.IsControlPressed(0, RDR2.Control.WhistleHorseback))
            {
                InvokeAlliesAction("Follow");
                Debug.Subtitle("To me, minions!");
            }
            
            if (e.KeyCode == Keys.G)
            {
                if(ped != null)
                {
                    if ( PedIsAnAlly(ped))
                    {
                        selected_ally = GetAllyFromPed(ped);
                        SelectSound();
                    }
                    else
                    {
                        if(! ped.IsHuman && ped.IsDead)
                        {
                            Function.Call(Hash.REVIVE_INJURED_PED, ped);
                            // saddle? lead horse?
                            AllyAnimal ally_animal = new AllyAnimal(ped);
                            allies.Add(ally_animal);
                            Debug.Subtitle($"Capturing {ally_animal.ally_name}");
                        }
                        else if(selected_ally != null)
                        {
                            selected_ally.Attack(ped);
                            //Debug.Subtitle($"Attack, {selected_ally.ally_name}!");
                        }
                    }
                }
                else if(selected_ally != null)
                {
                    if(hit != Vector3.Zero)
                    {
                        selected_ally.MoveTo(hit);
                    }
                }
            }

            if (e.KeyCode == Keys.I)
            {
                SummonAllies();
            }

        }

        private bool WasDamagedByBow(Ped ped)
        {
            var weapon_hash = new InputArgument(WeaponHash.Bow);
            return Function.Call<bool>(Hash._HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON, ped, weapon_hash, 0);
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
                    if(selected_ally == ally)
                    {
                        selected_ally = null;
                    }
                    allies.Remove(ally);
                    ally.ally_ped.MarkAsNoLongerNeeded();
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
            WeatherType weather = World.CurrentWeather;
            World.CurrentWeather = WeatherType.Thunderstorm;
            try
            {
                foreach (string ally in settings.allies)
                {
                    Function.Call(Hash.FORCE_LIGHTNING_FLASH);
                    string ally_name = settings.settings.GetValue(ally, "name");
                    string model_name = settings.settings.GetValue(ally, "model");
                    string can_fly = settings.settings.GetValue(ally, "can_fly");
                    AllySpecial new_ally = Summon(ally_name, model_name, can_fly);
                    if(new_ally.ally_ped != null)
                    {
                        allies.Add(new_ally);
                        new_ally.ally_ped.IsInvincible = true;
                    }
                    Debug.Subtitle($"Summoned {ally_name}!");
                    Wait(2000);
                }
            }
            catch
            {
                Debug.DebugThree($"Summoning failed...");
            }
            finally
            {
                World.CurrentWeather = weather;
                foreach(AllySpecial ally in allies)
                {
                    ally.ally_ped.IsInvincible = false;
                }
            }
            
        }

        private AllySpecial Summon(string ally_name, string model_name, string can_fly)
        {
            return new AllySpecial(model_name, ally_name, can_fly: can_fly == string.Empty ? false : true);
        }

        public static void LightningStrike(Vector3 position)
        {
            Function.Call((Hash)0x67943537D179597C, position.X, position.Y, position.Z);
            Script.Wait(1000);
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
