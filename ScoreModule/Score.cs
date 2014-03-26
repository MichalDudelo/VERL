using Arena_Server.Infrastructure;
using Common_Library.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreModule
{

    public class Score : IScoreModule
    {
        

        public Dictionary<RobotAvatar, double> GetScore(Common_Library.Map map, List<Move> moveList, Dictionary<int, List<ActionHistory>> globalHistory)
        {

            var scores = new Dictionary<RobotAvatar, double>();
            foreach (var move in moveList)
            {
                var action = move.MadeMove;
                var direction = move.DirectionOfMove;
                int res = 0;
                if (action == MoveType.MakeMove && direction == Common_Library.Parts_of_map.Directions.Down ) res = 10;
                else
                    if (action == MoveType.MakeMove && direction == Common_Library.Parts_of_map.Directions.Up) res = 0;
                    else
                        if (action == MoveType.MakeMove && direction == Common_Library.Parts_of_map.Directions.Left) res = -10;
                        else
                            if (action == MoveType.MakeMove && direction == Common_Library.Parts_of_map.Directions.Right) res = 4;
                            else
                                res = -4;
                                if (action == MoveType.Shoot && direction == Common_Library.Parts_of_map.Directions.Right) res = 4;
                                else
                                    res = -4;

                scores.Add(move.Robot, res);
            }

            return scores;
        }
    }

    
}
