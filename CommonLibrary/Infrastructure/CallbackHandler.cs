using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Common_Library.Infrastructure;
using Common_Library;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Common_Library.Parts_of_map;


namespace Common_Library.Infrastructure
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    [ServiceBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public class CallbackHandler: Common_Library.IService_Contracts.IArenaCallback
    {
        private static Action<InitialServerResponse> _loginAction;
        public static void RegisterLoginAction(Action<InitialServerResponse> Action)
        {
            _loginAction = Action;
        }

        private static Action<Map, GamePlayServerResponse> _gamePlayAction;
        
        public static void RegisterGamePlayAction(Action<Map, GamePlayServerResponse> Action) 
        {
            _gamePlayAction = Action;
        }

        private static Action<int, List<PossibleAction>> _beginRoundAction;

        public static void RegisterBeginRoundAction(Action<int,List<PossibleAction>> Action)
        {
            _beginRoundAction = Action;
        }

        private static Action<int> _endRoundAction;

        public static void RegisterEndRoundAction(Action<int> Action)
        {
            _endRoundAction = Action;
        }

        private static Action<double> _endGameAction;

        public static void RegisterEndGameAction(Action<double> Action)
        {
            _endGameAction = Action;
        }

        private static Action<Map,int, string> _startGameAction;

        public static void RegisterStartGameAction(Action<Map,int, string> Action)
        {
            _startGameAction = Action;
        }

       
        
        public  void reciveInitialData( InitialServerResponse response)
        {
          
            if (_loginAction != null)
                _loginAction.BeginInvoke(response, a => _loginAction.EndInvoke(a),null);
        }


        public void reciveGamePlayData(byte[] currentMap, GamePlayServerResponse response)
        {
            if (_gamePlayAction != null)
                _gamePlayAction.BeginInvoke(Map.Deserialize(currentMap), response, a => _gamePlayAction.EndInvoke(a), null);
        }


        public void gameRoundStart(int roundNumber, List<PossibleAction> possibleActionsList)
        {
            if (_beginRoundAction != null)
                _beginRoundAction.BeginInvoke(roundNumber,possibleActionsList, a => _beginRoundAction.EndInvoke(a), null);
        }

        public void gameRoundEnd(int roundNumber)
        {
            if (_endRoundAction != null)
                _endRoundAction.BeginInvoke(roundNumber, a => _endRoundAction.EndInvoke(a), null);
        
        }

        public void gamePlayEnd(double finalScore)
        {
            if (_endGameAction != null)
                _endGameAction.BeginInvoke(finalScore, a => _endGameAction.EndInvoke(a), null);
        }

        public void gamePlayStarted(byte[] currentMap,int roundNumber, string team)
        {
            if (_startGameAction != null)
                _startGameAction.BeginInvoke(Map.Deserialize(currentMap), roundNumber, team, a => _startGameAction.EndInvoke(a), null);
        }
    }
}
