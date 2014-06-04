using Arena_Server.Infrastructure;
using ArenaServer.Infrastructure.EventArgs;
using Common_Library.Infrastructure;
using Common_Library.IService_Contracts;
using Common_Library.Parts_of_map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Logger;

namespace Common_Library
{
    public class GamePlay
    {

        private Map _currentMap;
        private Dictionary<IArenaCallback, RobotAvatar> _avatarDictionary;
        private IScoreModule _scoreModule;
        private List<Move> _movesToScore;
        private string strLogPath;

        public delegate void PlayerActionDelegate(object oSender, PlayerMoveArgs playerMoveArgs);
        public static event PlayerActionDelegate PlayerActionEvent;

        public GamePlay(Map currentMap, Dictionary<IArenaCallback, RobotAvatar> avatarDictionary, IScoreModule ScoreModule, int gamePlayNumber)
        {
            var localPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var pos = localPath.LastIndexOf(@"\") + 1;
            strLogPath = localPath.Substring(0, pos) + "GamePlayLog("+gamePlayNumber.ToString()+").txt";

            _avatarDictionary = avatarDictionary;
            _currentMap = currentMap;
            _scoreModule = ScoreModule;

        }

        public Dictionary<RobotAvatar, double> PlayTurn(MovesQueue actionQueue, Dictionary<int, List<ActionHistory>> globalHistory)
        {
            Dictionary<RobotAvatar, double> ScoreDictionary = new Dictionary<RobotAvatar, double>();
            _movesToScore = new List<Move>();
            List<Move> MoveListToVisualization = new List<Move>();

            #region INVOKE MOVES
            while (actionQueue.Count != 0)
            {
                var currentMove = actionQueue.Dequeue();
                Move move;
                if (currentMove.MadeMove != MoveType.DisconnectedPlayer)
                {
                    MethodInfo method = currentMove.GetType().GetMethod(currentMove.MadeMove.ToString() + "Action"); // INVOKE ALL ACTIONS IN QUEUE
                    move = (Move)method.Invoke(currentMove, null);

                    if (move.MadeMove == MoveType.Burn)
                    {
                        globalHistory[move.CurrentRound].Find(hist => hist.RobotLogin.Equals(move.Robot.Login)).MadeMove = MoveType.Burn;
                        EventLog.WriteMessageToLog(strLogPath, "Client: " + move.Robot.Login + " is burning");
                    }

                    if (move.MadeMove == MoveType.WrongAction)
                    {
                        globalHistory[move.CurrentRound].Find(hist => hist.RobotLogin.Equals(move.Robot.Login)).MadeMove = MoveType.WrongAction;
                        EventLog.WriteMessageToLog(strLogPath, "Client: " + move.Robot.Login + " made wrong action");
                    }

                    if (globalHistory[move.CurrentRound].Exists(hist => hist.MadeMove.Equals(move.MadeMove)))
                        globalHistory[move.CurrentRound].Find(hist => hist.MadeMove.Equals(move.MadeMove)).Consequence = move.Consequence; // add move conseqence to global history for each move

                    EventLog.WriteMessageToLog(strLogPath, "Client: " + move.Robot.Login + "  " + move.MadeMove.ToString() + " with consequence " + move.Consequence.ToString());
                    MoveListToVisualization.Add(move);
                    _movesToScore.Add(move);
                }
                else if (!_movesToScore.Exists(m => m.Robot.Equals(currentMove.Robot)))
                {
                    ScoreDictionary.Add(currentMove.Robot, -10.0);
                    //_movesToScore.Add(currentMove);
                    currentMove.MyPay = -10.0;
                    MoveListToVisualization.Add(currentMove);
                }

                //if (PlayerActionEvent != null)
                //PlayerActionEvent(move,null);

            }
            #endregion

            #region SCORE MODULE
            if (_scoreModule != null)
            {
                Dictionary<RobotAvatar, double> ScoreDictionaryScoreModule = new Dictionary<RobotAvatar, double>();
                try
                {
                     ScoreDictionaryScoreModule = _scoreModule.GetScore(_currentMap, _movesToScore, globalHistory); // score moves
                }
                catch (Exception e)
                {
                    
                }
                foreach (var score in ScoreDictionaryScoreModule)
                    ScoreDictionary.Add(score.Key, score.Value);

                foreach (var move in _movesToScore.FindAll(m => m.MadeMove == MoveType.DisconnectedPlayer))
                    move.MyPay = ScoreDictionary[move.Robot];

                foreach (var score in ScoreDictionary)
                {
                    var robot = globalHistory.Values.ToList().Last().Find(a => a.RobotLogin.Equals(score.Key.Login));
                    if (robot != null)
                    {
                        robot.MyCurrentPay = score.Value;
                        robot.MyTotalPay += score.Value;
                    }
                }




                double totalPay = 0; // count total score
                foreach (var points in ScoreDictionary.Values)
                    totalPay += points;

                foreach (var history in globalHistory.Values.ToList().Last())
                    history.TotalPay = totalPay;

                try
                {
                    foreach (var client in _avatarDictionary) // send Message to client
                    {
                        var move = _movesToScore.Find(m => m.Robot.Equals(client.Value));
                        if (move.MadeMove != MoveType.Disconnect)
                        {
                            move.MyPay = ScoreDictionary[move.Robot];
                            var mapForRobot = _currentMap.getSmallerPartForRobot(move.Robot.RobotPosition).SerializeMap();
                            var roundNumber = move.CurrentRound;
                            var response = new GamePlayServerResponse(roundNumber, move.Robot.RobotPosition, move.MyPay,
                                totalPay, move.Robot.HasBigItem, move.Robot.SmallItem, move.Consequence, move.Response);
                            try { client.Key.reciveGamePlayData(mapForRobot, response); }
                            catch (Exception e) { EventLog.WriteErrorToLog(strLogPath, e); }
                            EventLog.WriteMessageToLog(strLogPath, "Client: " + client.Value.Login + " receive " + move.MyPay.ToString() + " points for move " + move.MadeMove.ToString());


                        }

                    }
                }
                catch (Exception e)
                {
                    EventLog.WriteErrorToLog(strLogPath, e);
                }

            }
            #endregion

            else
            {
                #region WITHOUT SCORE MODULE
                foreach (var client in _avatarDictionary) // send Message to client
                {
                    var move = _movesToScore.Find(m => m.Robot.Equals(client.Value));
                    if (move.MadeMove != MoveType.Disconnect)
                    {
                        move.MyPay = 0;
                        var mapForRobot = _currentMap.getSmallerPartForRobot(client.Value.RobotPosition).SerializeMap();
                        var roundNumber = _movesToScore.Find(m => m.Robot.Equals(client.Value)).CurrentRound;
                        var response = new GamePlayServerResponse(roundNumber, client.Value.RobotPosition, 0,
                            0, client.Value.HasBigItem, client.Value.SmallItem, _movesToScore.Find(m => m.Client.Equals(client.Key)).Consequence, _movesToScore.Find(m => m.Client.Equals(client.Key)).Response);

                        client.Key.reciveGamePlayData(mapForRobot, response);
                        EventLog.WriteMessageToLog(strLogPath, "Client: " + client.Value.Login + " - sending gameplay data");
                    }

                }

            }


            if (PlayerActionEvent != null)
                PlayerActionEvent(MoveListToVisualization, null);



            return ScoreDictionary;
                #endregion
        }


        public void PerformMove(Move m)
        {
            if (PlayerActionEvent != null)
                PlayerActionEvent(m, null);
        }


    }
}
