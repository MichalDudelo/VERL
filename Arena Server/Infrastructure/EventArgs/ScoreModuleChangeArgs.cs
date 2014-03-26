using Arena_Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaServer.Infrastructure.EventArgs
{
    public class ScoreModuleChangeArgs:  System.EventArgs
    {
        public IScoreModule Score;
        public ScoreModuleChangeArgs(IScoreModule Score)
        {
            this.Score = Score;
        }

    }
}
