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
    public abstract class Ally
    {
        public Ped ally_ped { get; set; }
        public string ally_name { get; set; }
        public int max_health = 100;
        public bool can_fly = false;
        public bool is_undead = false;
        public abstract void PostCreate();
        public abstract Ped Create(string model_name);

        public float hated_target_radius = 30f;
        public float follow_speed = 100f;
        public float follow_stopping_range = 15f;

        public void AttackInArea()
        {
            // NOTE: Different animal types will not attack the same enemy
            Debug.Subtitle("Kill them all!");
            Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, this.ally_ped, false, false);
            Function.Call(Hash.REGISTER_HATED_TARGETS_AROUND_PED, ally_ped, hated_target_radius);
            Function.Call(Hash.TASK_COMBAT_HATED_TARGETS_IN_AREA, ally_ped, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, hated_target_radius, 0, 0);
        }

        public virtual void Kill()
        {
            ally_ped.IsInvincible = false;
            ally_ped.Health = 0;
        }

        public virtual void Destroy()
        {
            //ally_ped.MarkAsNoLongerNeeded();
        }

        public virtual void Follow()
        {
            Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, this.ally_ped, false, false);
            this.ally_ped.Task.FollowToEntity(Game.Player.Character, follow_speed, stoppingRange: follow_stopping_range);
        }

        public virtual void Update()
        {
            if (BeingAimedAt()) // blocking permanent events prevents info box from showing in lower right corner
            {
                this.ally_ped.BlockPermanentEvents = false;
            }
            else
            {
                this.ally_ped.BlockPermanentEvents = true;
            }
        }

        public virtual void Attack(Ped ped)
        {
            ally_ped.Task.Combat(ped);
        }

        public void AddPedAttributes(Ped ped, string ped_name)
        {
            ped.MaxHealth = max_health;
            Function.Call((Hash)0x283978A15512B2FE, ped, true); // sets ped visibility
            Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ped, false, false);
            Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped.Handle, 0);
            ped.RelationshipGroup = Game.Player.Character.RelationshipGroup;
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true); // this disables the popup info for an animal :(
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped, 0, 0);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 46, true);
            Function.Call(Hash.SET_PED_COMBAT_ABILITY, ped, 100);
            ped.SetPedPromptName(ped_name);
        }

        public void MoveTo(Vector3 coords)
        {
            if (can_fly)
            {
                FlyTo(coords);
            }
            else
            {
                RunTo(coords);
            }
        }

        private void FlyTo(Vector3 coords)
        {
            // This doesn't work :/
            ally_ped.Task.LookAt(coords);
            float angle = Function.Call<float>(Hash.GET_HEADING_FROM_VECTOR_2D, coords.X - ally_ped.Position.X, coords.Y - ally_ped.Position.Y);
            Function.Call(Hash.SET_ENTITY_HEADING, ally_ped, angle);
            float actual = Function.Call<float>(Hash.GET_ENTITY_HEADING, ally_ped);

            Function.Call(Hash.TASK_FLY_TO_COORD, ally_ped.Handle, coords.X, 700f, coords.Z, 100f, 100f, actual, 100f);
        }

        private void RunTo(Vector3 coords)
        {
            //ally_ped.Task.GoTo(coords, true);
            Function.Call(Hash.TASK_GO_TO_COORD_ANY_MEANS, ally_ped.Handle, coords.X, coords.Y, coords.Z, follow_speed, false, false, 0, 0.0f);
        }

        public bool BeingAimedAt()
        {
            return Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING_AT_ENTITY, Game.Player, ally_ped);
        }

        public void SetStamina(float stamina)
        {
            ally_ped.StaminaCore = 100;
        }

    }
}
