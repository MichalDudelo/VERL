using Common_Library.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clients_Library.Infrastructure
{
    public class GameRoundArgs : EventArgs
    {
        public int RoundNumber;
        public List<PossibleAction> possibleActionList;
        public GameRoundArgs(int roundNumber, List<PossibleAction> possibleActionList = null)
        {
            RoundNumber = roundNumber;
            this.possibleActionList = possibleActionList;
        }
    }
}
