using Arena_Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaServer.Infrastructure.EventArgs
{
    public class PlayerLoginArgs:System.EventArgs
    {
        public RobotAvatar RobotAvatar;
        public PlayerLoginArgs(RobotAvatar RobotAvatar)
        {
            this.RobotAvatar = RobotAvatar;
        }
    }
}
