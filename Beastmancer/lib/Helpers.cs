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
    public class Helpers
    {
        private static bool weather_locked = false;
        private static WeatherType current_weather;

        public static bool WasKilledByPlayer(Ped ped)
        {
            Entity entity = Function.Call<Entity>(Hash.GET_PED_SOURCE_OF_DEATH, ped);
            if (entity == Game.Player.Character)
            {
                return true;
            }
            return false;
        }

        public static void SelectSound()
        {
            Function.Call(Hash.PLAY_SOUND_FRONTEND, "SELECT", "HUD_SHOP_SOUNDSET", true, 0);
        }

        public static Ped GetAimPed()
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

        public static Entity GetAimEntity()
        {
            OutputArgument output = new OutputArgument();
            bool target_is_entity = Function.Call<bool>(Hash.GET_ENTITY_PLAYER_IS_FREE_AIMING_AT, Game.Player, output);
            if (target_is_entity)
            {
                return output.GetResult<Entity>();
            }

            return null;
        }

        public static void SetWeatherTransition()
        {
            if (weather_locked)
            {
                return;
            }
            weather_locked = true;
            current_weather = World.CurrentWeather;
            World.CurrentWeather = WeatherType.Thunderstorm;
            Function.Call(Hash.FORCE_LIGHTNING_FLASH);
        }

        public static void ResumeWeather()
        {
            World.CurrentWeather = current_weather;
            weather_locked = false;
        }

        public static int GetCurrentTime()
        {
            return Function.Call<int>(Hash.GET_GAME_TIMER);
        }

        public static void LightningStrike(Vector3 position)
        {
            Function.Call((Hash)0x67943537D179597C, position.X, position.Y, position.Z);
            //Script.Wait(1000);
        }

    }
}
