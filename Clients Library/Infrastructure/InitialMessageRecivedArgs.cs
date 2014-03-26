using Common_Library;
using Common_Library.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clients_Library.Infrastructure
{
    public class InitialMessageRecivedArgs : EventArgs
    {
        public Map Map;
        public InitialServerResponse response;

        public InitialMessageRecivedArgs(Map map, InitialServerResponse response)
        {
            Map = map;
            this.response = response;
        }
    }
}
