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
       
        public InitialServerResponse response;

        public InitialMessageRecivedArgs(InitialServerResponse response)
        {
            
            this.response = response;
        }
    }
}
