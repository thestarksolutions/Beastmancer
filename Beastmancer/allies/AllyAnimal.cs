using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDR2;

namespace Beastmancer
{
    class AllyAnimal : Ally
    {
        public AllyAnimal(Ped ped)
        {
            this.ally_ped = ped;
            this.ally_name = Names.GetRandomName();
            PostCreate();
        }

        public override void AttackInArea()
        {
            throw new NotImplementedException();
        }

        public override Ped Create(string model_name)
        {
            return null;
        }

        public override void PostCreate()
        {
            AddPedAttributes(ally_ped, ally_name);
        }
    }
}
