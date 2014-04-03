using Common_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clients_Library.Infrastructure
{
    public class GamePlayArgs: EventArgs
    {
        public double finalScore;
        public int initalRoundNumber;
        public string team;
        public Map map;
        public GamePlayArgs(Map map,double number, string team, bool final)
        {
            this.map = map;
            if (final)
                this.finalScore = number;
            else
                initalRoundNumber = (int)number;

            this.team = team;
        }

        
    }
}
