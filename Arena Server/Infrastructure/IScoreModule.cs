using Common_Library;
using Common_Library.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arena_Server.Infrastructure
{
    public interface IScoreModule
    {
        Dictionary<RobotAvatar,double> GetScore(Map map, List<Move> moveList, Dictionary<int, List<ActionHistory>> globalHistory);
    }
}
