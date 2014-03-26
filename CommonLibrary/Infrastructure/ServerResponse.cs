using Common_Library.Parts_of_map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Infrastructure
{
    [DataContract]
    public class ServerResponse
    {
        [DataMember]
        private int _roundNumber;

        public int RoundNumber
        {
            get { return _roundNumber; }
            set { _roundNumber = value; }
        }

        [DataMember]
        private Position _myPosition;

        public Position MyPosition
        {
            get { return _myPosition; }
            set { _myPosition = value; }
        }

        

    }
}
