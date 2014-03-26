using Common_Library.Parts_of_map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Infrastructure
{
    [DataContract]
    public class PossibleAction
    {
        [DataMember]
        public MoveType Action;
        [DataMember]
        public Directions MoveDirection;
        public PossibleAction(MoveType Action, Directions MoveDirection = Directions.NULL)
        {
            this.Action = Action;
            this.MoveDirection = MoveDirection;
        }
       

    }
}
