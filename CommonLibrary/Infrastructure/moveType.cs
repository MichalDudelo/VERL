using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Infrastructure
{
    public enum MoveType
    {
     
        
            MakeMove,
            PickBigItem,
            DropBigItem,
            PickSmallItem,
            Punch,
            Shoot,
            Rest,
            WrongAction,
            Burn,
            Disconnect,
            Timeout,
            DisconnectedPlayer
        
    }
}
