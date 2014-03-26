using Arena_Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Infrastructure
{
    public class ScoreCounter
    {
        private Dictionary<int, Dictionary<RobotAvatar, double>> _totalScoreDictionary;

        private Dictionary<RobotAvatar, double> _rankingDictionary;

        public ScoreCounter()
        {
            _totalScoreDictionary = new Dictionary<int, Dictionary<RobotAvatar, double>>();
        }
        
        public void AddScore(int currentRound, Dictionary<RobotAvatar, double> totalTurnScore)
        {
            _totalScoreDictionary.Add(currentRound, totalTurnScore);
        }

        public void Clear()
        {
            _totalScoreDictionary.Clear();
        }
        public double GetTotalScoreForRobot(RobotAvatar robot)
        {
            double score = 0;
            foreach (var turnScore in _totalScoreDictionary.Values)
            {
                score += turnScore[robot];
            }
            return score;
        }

       
    }
}
