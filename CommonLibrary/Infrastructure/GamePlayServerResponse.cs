using Common_Library.Parts_of_map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Infrastructure
{
    public class GamePlayServerResponse: ServerResponse
    {
        [DataMember]
        private double _myCurrentPay;

        public double MyCurrentPay
        {
            get { return _myCurrentPay; }
            set { _myCurrentPay = value; }
        }
        [DataMember]
        private double _totalPay;

        public double TotalPay
        {
            get { return _totalPay; }
            set { _totalPay = value; }
        }
        [DataMember]
        private bool _hasBigItem;

        public bool HasBigItem
        {
            get { return _hasBigItem; }
            set { _hasBigItem = value; }
        }
        [DataMember]
        private int _smallItemNumber;

        public int SmallItemNumber
        {
            get { return _smallItemNumber; }
            set { _smallItemNumber = value; }
        }

       
        [DataMember]
        public ServerState ServerState;
        [DataMember]
        public string Message;
        [DataMember]
        public bool Result;
        [DataMember]
        private MoveConsequence _consequence;

        public MoveConsequence Consequence
        {
            get { return _consequence; }
            set { _consequence = value; }
        }


        public GamePlayServerResponse(int roundNumber, Position myPosition, double MyCurrentPay, double TotalPay, bool HasBigItem, int SmallItem,MoveConsequence Consequence, GamePlayServerResponse response)
        {
            RoundNumber = roundNumber;
            _myCurrentPay = MyCurrentPay;
            _totalPay = TotalPay;
            _hasBigItem = HasBigItem;
            _smallItemNumber = SmallItem;
            this.RoundNumber = roundNumber;
            this.MyPosition = myPosition;
            this._consequence = Consequence;
            if (response != null)
            {
                Result = response.Result;
                Message = response.Message;
                ServerState = response.ServerState;
            }
        }

        public GamePlayServerResponse(int roundNumber, Position myPosition, bool HasBigItem, int SmallItem, GamePlayServerResponse response)
        {
            RoundNumber = roundNumber;
            _hasBigItem = HasBigItem;
            SmallItemNumber = SmallItem;
            this.RoundNumber = roundNumber;
            this.MyPosition = myPosition;
            if (response != null)
            {
                Result = response.Result;
                Message = response.Message;
                ServerState = response.ServerState;
            }
        }

         public GamePlayServerResponse() {}

         public static GamePlayServerResponse InvalidMoveMessage(string message)
        {
            return new GamePlayServerResponse()
            {
                Result = false,
                Message = message,
                ServerState = ServerState.INVALID_MOVE
            };
        }

         public static GamePlayServerResponse MoveTimout(string message)
         {
             return new GamePlayServerResponse()
             {
                 Result = false,
                 Message = message,
                 ServerState = ServerState.MOVE_TIMOUT
             };
         }

         public static GamePlayServerResponse OK()
         {
             return new GamePlayServerResponse()
             {
                 Result = true,
                 Message = "OK",
                 ServerState = ServerState.OK
             };
         }

         public static GamePlayServerResponse Wounded()
         {
             return new GamePlayServerResponse()
             {
                 Result = true,
                 Message = "Wounded",
                 ServerState = ServerState.WOUNDED
             };
         }


    }
}
