using Common_Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArenaServer.Infrastructure.EventArgs
{
    public class MapChangeArgs:System.EventArgs
    {
        public Map Map;
        public MapChangeArgs(Map map)
        {
            this.Map = map;
        }
    }
}
