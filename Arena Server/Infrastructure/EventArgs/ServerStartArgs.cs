using Common_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaServer.Infrastructure.EventArgs
{
    public class ServerStartArgs : System.EventArgs
    {
        
        public Map map;
        public  int roundNumber;
        public bool hostileMode;
        public double RoundTime;
        public int gameSecondTime;
        public int gameMinuteTime;
        public bool endNRoundCriteria;
        public int numberOfTeams;

        public ServerStartArgs(Map Map,int roundNumber, bool hostileMode, double RoundTime, int gameSecondTime, int gameMinuteTime, bool endNRoundCriteria, int numberOfTeams)
        {
            this.map = Map;
            this.roundNumber = roundNumber;
            this.hostileMode = hostileMode;
            this.RoundTime = RoundTime;
            this.gameSecondTime = gameSecondTime;
            this.gameMinuteTime = gameMinuteTime;
            this.endNRoundCriteria = endNRoundCriteria;
            this.numberOfTeams = numberOfTeams;
        }
    }
}
