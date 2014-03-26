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
        public GamePlayArgs(double number, string team, bool final)
        {
            if (final)
                this.finalScore = number;
            else
                initalRoundNumber = (int)number;

            this.team = team;
        }

        
    }
}
