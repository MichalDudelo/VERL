using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Library.Infrastructure
{
    public class ActionHistory
    {
        private double _myCurrentPay;

        public double MyCurrentPay
        {
            get { return _myCurrentPay; }
            set { _myCurrentPay = value; }
        }
        private double _myTotalPay;

        public double MyTotalPay
        {
            get { return _myTotalPay; }
            set { _myTotalPay = value; }
        }
        private double _totalPay;

        public double TotalPay
        {
            get { return _totalPay; }
            set { _totalPay = value; }
        }

        private MoveType _madeMove;

        public MoveType MadeMove
        {
            get { return _madeMove; }
            set { _madeMove = value; }
        }

        private int _currentRound;

        public int CurrentRound
        {
            get { return _currentRound; }
            set { _currentRound = value; }
        }

        private List<PossibleAction> _possibleActions;

        public List<PossibleAction> PossibleActions
        {
            get { return _possibleActions; }
            set { _possibleActions = value; }
        }
        private MoveConsequence _consequence;

        public MoveConsequence Consequence
        {
            get { return _consequence; }
            set { _consequence = value; }
        }

        private string _robotLogin;

        public string RobotLogin
        {
            get { return _robotLogin; }
            set { _robotLogin = value; }
        }

        public ActionHistory(int currentRound, MoveType madeMove, double myCurrentPay, double myTotalPay, double totalPay)
        {
            this._currentRound = currentRound;
            this._madeMove = madeMove;
            this._myCurrentPay = myCurrentPay;
            this._myTotalPay = myTotalPay;
            this._totalPay = totalPay;
            this._consequence = MoveConsequence.NULL;
            
            
        }
        public ActionHistory(string robotLogin,int currentRound, List<PossibleAction> possibleActions)
        {
            this._robotLogin = robotLogin;
            this._currentRound = currentRound;
            this._possibleActions = possibleActions;
            this._madeMove = MoveType.NULL;
            this._consequence = MoveConsequence.NULL;
            this._myCurrentPay = 0;
            this._myTotalPay = 0;
        }
    }

}
