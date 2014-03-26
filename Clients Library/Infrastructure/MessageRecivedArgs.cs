using Common_Library;
using Common_Library.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clients_Library.Infrastructure
{
    public class MessageRecivedArgs : EventArgs
    {
        public Map Map;
        public GamePlayServerResponse response;

        public MessageRecivedArgs(Map map, GamePlayServerResponse response)
        {
            Map = map;
            this.response = response;
        }
    }
}
