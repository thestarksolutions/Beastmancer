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
        private List<Ped> allies = new List<Ped>();
        public Client()
        {
            KeyDown += OnKeyDown;
            Tick += OnTick;
            Interval = 1;
        }

        private void OnTick(object sender, EventArgs evt)
        {

        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            if (Game.IsControlPressed(0, RDR2.Control.Whistle) || Game.IsControlPressed(1, RDR2.Control.Whistle) || Game.IsControlPressed(2, RDR2.Control.Whistle))
            {
                Debug.Subtitle("To me, minions!");
                foreach (Ped ally in allies)
                {
                    Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ally, false, false);
                    ally.Task.FollowToEntity(Game.Player.Character, 10f, stoppingRange:15f);
                }
            }

            if (e.KeyCode == Keys.G)
            {
                Debug.Subtitle($"Trying to Target!");

                OutputArgument output = new OutputArgument();
                bool target_is_entity = Function.Call<bool>(Hash.GET_ENTITY_PLAYER_IS_FREE_AIMING_AT, Game.Player, output);
                if (target_is_entity)
                {
                    Entity tmp = output.GetResult<Entity>();
                    Debug.Subtitle($"Target! {tmp} ");
                    if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, tmp))
                    {
                        Debug.Subtitle($"Target is a ped! {tmp} ");
                        Ped tmp_ped = Function.Call<Ped>(Hash.GET_PED_INDEX_FROM_ENTITY_INDEX, tmp);
                        Function.Call(Hash.KNOCK_PED_OFF_VEHICLE, tmp_ped);
                    }
                }
            }

            if (e.KeyCode == Keys.K)
            {
                Debug.Subtitle("Summoning ally...");
                //PedHash.A_C_Horse_Andalusian_DarkBay
                Ped my_new_ped = CreatePed("A_C_Cougar_01", Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 15, 0)), isVisible: true);
                allies.Add(my_new_ped);
                //PedCore.
            }
            if (e.KeyCode == Keys.L)
            {
                Debug.Subtitle("Summoning ally...");
                Ped my_new_ped = CreatePed("A_C_BearBlack_01", Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 15, 0)), isVisible: true);
                allies.Add(my_new_ped);
            }

            if (e.KeyCode == Keys.J)
            {
                Debug.Subtitle($"Trying to Target!");

                OutputArgument output = new OutputArgument();
                bool target_is_entity = Function.Call<bool>(Hash.GET_ENTITY_PLAYER_IS_FREE_AIMING_AT, Game.Player, output);
                if (target_is_entity)
                {
                    Entity tmp = output.GetResult<Entity>();
                    Debug.Subtitle($"Target! {tmp} ");
                    if(Function.Call<bool>(Hash.IS_ENTITY_A_PED, tmp))
                    {
                        Debug.Subtitle($"Target is a ped! {tmp} ");
                        Ped tmp_ped = Function.Call<Ped>(Hash.GET_PED_INDEX_FROM_ENTITY_INDEX, tmp);
                        if (allies.Contains(tmp_ped))
                        {
                            Debug.Subtitle($"Ped Is An Ally! {tmp_ped} ");

                            Game.Player.Character.Task.MountAnimal(tmp_ped);
                        }
                        else if (tmp_ped.IsAlive)
                        {
                            foreach (Ped ally in allies)
                            {
                                Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ally, false, false);
                                Function.Call(Hash.REGISTER_HATED_TARGETS_AROUND_PED, ally, 400f);

                                ally.Task.Combat(tmp_ped);
                                //Function.Call(Hash.TASK_SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ally, true);
                            }
                        }
                    }
                }
                else
                {
                    Debug.Subtitle($"No Target, attacking any in area!");
                    RaycastResult result = World.CrosshairRaycast(400f, IntersectOptions.Everything);
                    
                    Vector3 cam_coords = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_COORD);
                    Debug.Subtitle(result.HitPosition.ToString());                    
                    foreach(Ped ally in allies)
                    {
                        //ally.Task.GoTo(result.HitPosition, true);
                        Function.Call(Hash.REGISTER_HATED_TARGETS_AROUND_PED, ally, 400f);
                        Function.Call(Hash.TASK_COMBAT_HATED_TARGETS_IN_AREA, ally, result.HitPosition.X, result.HitPosition.Y, result.HitPosition.Z, 400f, 0, 0);
                    }
                }
            }
        }

        public static int GetHashKeyFromModelName(string modelName)
        {
            return Function.Call<int>(Hash.GET_HASH_KEY, modelName);
        }

        public Ped CreatePed(string modelName, Vector3 position, bool isVisible = true, bool is_friendly = true)
        {
            int hash_key = GetHashKeyFromModelName(modelName);
            Function.Call(Hash.REQUEST_MODEL, hash_key, false);
            WeatherType weather = World.CurrentWeather;
            World.CurrentWeather = WeatherType.Thunderstorm;
            Function.Call(Hash.FORCE_LIGHTNING_FLASH);
            Wait(4000);

            while (! Function.Call<bool>(Hash.HAS_MODEL_LOADED, hash_key)) {
                Debug.DebugTwo($"Waiting... ");
                Wait(1000);
            }
            Game.Player.IsInvincible = true;
            Game.Player.Character.CanRagdoll = false;
            Function.Call((Hash)0x67943537D179597C, position.X, position.Y, position.Z);
            Wait(1000);

            var ped = Function.Call<Ped>(Hash.CREATE_PED, hash_key, position.X, position.Y, position.Z, 0, false, false, 0);

            Function.Call((Hash)0x283978A15512B2FE, ped, isVisible);
            Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ped, false, false);
            Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped.Handle, 0);
            ped.RelationshipGroup = Game.Player.Character.RelationshipGroup;
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped, 0, 0);
            Function.Call(Hash._SET_PED_SCALE, ped, 2f);
            /*
             * https://gtaforums.com/topic/833391-researchguide-combat-behaviour-flags
             * {
                BF_CanUseCover = 0,
                BF_CanUseVehicles = 1,
                BF_CanDoDrivebys = 2,
                BF_CanLeaveVehicle = 3,
                BF_CanFightArmedPedsWhenNotArmed = 5,
                BF_CanTauntInVehicle = 20,
                BF_AlwaysFight = 46,
                BF_IgnoreTrafficWhenDriving = 52
                };
             * */
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 46, true);
            World.CurrentWeather = weather;
            ped.SetPedPromptName("Harpo");
            Debug.DebugThree($"Created! {modelName} ");
            Game.Player.IsInvincible = false;
            Game.Player.Character.CanRagdoll = true;


            return ped;
        }

    }
}
