using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common_Library.Parts_of_map;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Common_Library.Infrastructure
{
    [DataContract]  
    public class InitialServerResponse:ServerResponse
    {
        [DataMember]
        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        } 
      
        [DataMember]
        private int _roundTime;

        public int RoundTime
        {
          get { return _roundTime; }
          set { _roundTime = value; }
        }
        
        [DataMember]
        private string _color;

        public string Color
        {
          get { return _color; }
          set { _color = value; }
        }

        [DataMember]
        public ServerState ServerState;

        [DataMember]
        public bool Result;

        [DataMember]
        private MapSize _globalMapSize;

        public MapSize GlobalMapSize
        {
            get { return _globalMapSize; }
            set { _globalMapSize = value; }
        }

       
        public static InitialServerResponse PlayerRegistered(string color, int roundTime, Position positionOnMap, MapSize globalMapSize)
        {
            return new InitialServerResponse()
            {
                Result = true,
                Color = color,
                ServerState = ServerState.REGISTERED,
                Message = "Welcome to the game",
                RoundTime = roundTime,
                MyPosition = positionOnMap,
                _globalMapSize = globalMapSize
 
            };
        }

       

        public static InitialServerResponse LoginAlreadyExists()
        {
            return new InitialServerResponse()
            {
                Result = false,
                Message = "Such login exists",
                ServerState = ServerState.LOGIN_ALREADY_EXISTS
            };
        }

       

      


        public static InitialServerResponse Exception(string msg)
        {
            return new InitialServerResponse()
            {
                Result = false,
                Message = msg,
                ServerState = ServerState.EXCEPTION
            };
        }

        
    }
}
