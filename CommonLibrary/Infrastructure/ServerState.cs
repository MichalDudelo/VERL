using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common_Library.Infrastructure
{
    [DataContract]
    public enum ServerState
    {
        [EnumMember]
        REGISTERED = 0,
        [EnumMember]
        GAME_STARTED = 1,
        [EnumMember]
        LOGIN_ALREADY_EXISTS = 2,
        [EnumMember]
        INVALID_MOVE = 3,
        [EnumMember]
        EXCEPTION = 4,
        [EnumMember]
        RESPONSE = 5,
        [EnumMember]
        OK = 6,
        [EnumMember]
        WOUNDED = 7,
        [EnumMember]
        MOVE_TIMOUT

    }
}
