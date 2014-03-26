using Arena_Server.Infrastructure;
using Common_Library;
using Common_Library.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaServer.Infrastructure.EventArgs
{
    public class PlayerMoveArgs: System.EventArgs
    {
        public RobotAvatar robotAvatar;
        public Map map;
        public Move move;
        public PlayerMoveArgs(Map Map, RobotAvatar RobotAvatar, Move Mv)
        {
            this.robotAvatar = RobotAvatar;
            this.map = Map;
            this.move = Mv;
        }
    }
}
