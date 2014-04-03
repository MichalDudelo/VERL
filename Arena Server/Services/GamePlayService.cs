using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common_Library.IService_Contracts;
using Common_Library.Parts_of_map;
using System.ServiceModel;
using Arena_Server.Infrastructure;
using Common_Library;
using Common_Library.Infrastructure;
using System.ServiceModel.Activation;
using System.Threading;
using ArenaServer.Infrastructure.EventArgs;
using Logger;
namespace Arena_Server.Services
{

    [ServiceBehavior(UseSynchronizationContext = false, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class GamePlayService : IGamePlayService
    {

        private Dictionary<IArenaCallback, RobotAvatar> avatarDictionary;
        private Map _currentMap;
        private Map _initialMap;
        private List<Move> _movesList = new List<Move>();
        private int _roundNumber;
        private bool _hostileMode;
        private bool _extendedRoundTime; // if true then round time == 1s, else round time == 0.5;
        private int _gameSecondTime;
        private int _gameMinuteTime;
        private bool _endNRoundCriteria;
        private int _currentRound;
        private int _numberOfLoggedRobots = 0;
        private int _numberOfTeams = 0;
        private GamePlay GamePlay;
        private IScoreModule _scoreModule;
        private MovesQueue _currentMovesQueue = new MovesQueue();
        private List<string> colorList = new List<string> { "blue", "brown", "gold", "green", "pink", "red", "violet", "white", "silver" };
        private List<string> teamList = new List<string> { "Team 1", "Team 2", "Team 3", "Team 4" };
        private Thread GamePlayThread;
        private bool _pauseGame = false;
        private int _roundTime = 500;
        private bool _isGameStarted = false;
        private Dictionary<int, List<ActionHistory>> _globalHistory = new Dictionary<int, List<ActionHistory>>();
        private ScoreCounter _scoreCounter = new ScoreCounter();
        private int _numberOfPlayedGames = 0;
        private string strLogPath ;
        private string strScoreLogPath;
        private List<RobotAvatar> _disconnectedRobotAvatarList = new List<RobotAvatar>();
        private bool _endGameInformationSend = false;


        public delegate void PlayerLoginDelegate(object oSender, PlayerLoginArgs playerLoginArgs);
        public static event PlayerLoginDelegate PlayerLoginEvent;

        public delegate void PlayerDisconnectDelegate(object oSender, PlayerLoginArgs playerLoginArgs);
        public static event PlayerDisconnectDelegate PlayerDisconnectEvent;

        private Object thisLock = new Object();

        public GamePlayService() { }
        public GamePlayService(Dictionary<IArenaCallback, RobotAvatar> AvatarDictionary)
        {

            avatarDictionary = AvatarDictionary;
            MainWindow.StartServerEvent += MainWindow_StartServerEvent;
            MainWindow.MapChangeEvent += MainWindow_MapChangeEvent;
            MainWindow.ScoreModuleChangeEvent += MainWindow_ScoreModuleChangeEvent;
            MainWindow.ResumeServerEvent += MainWindow_ResumeServerEvent;
            MainWindow.StopServerEvent += MainWindow_StopServerEvent;
            MainWindow.EndGameEvent += MainWindow_EndGameEvent;
            var localPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var pos = localPath.LastIndexOf(@"\")+1;
            strLogPath = localPath.Substring(0, pos) + "GamePlayLog.txt";

        }


        void CreateHistoryLogFile(int gamePlayNumber)
        {


            var localPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var pos = localPath.LastIndexOf(@"\") + 1;
            strScoreLogPath = localPath.Substring(0, pos) + "ScoreLog(" + gamePlayNumber.ToString() + ").txt";

            EventLog.WriteMessageToLog(strScoreLogPath, "Score Log number: " + gamePlayNumber.ToString());

            EventLog.WriteMessageToLog(strScoreLogPath, "Game play parameters:  Number of logged players: " + _numberOfLoggedRobots.ToString() + "; Number of rounds: " + _roundNumber.ToString() +
            "; Number of teams: " + _numberOfTeams.ToString());
            EventLog.WriteMessageToLog(strScoreLogPath, "SCORE:");

            try
            {
                //_globalHistory.Values.ToList().Last().Sort(delegate(ActionHistory o1, ActionHistory o2) { if (o1.MyTotalPay > o2.MyTotalPay) return o1.MyTotalPay; else return o2.MyTotalPay; });
                var scoreRobotList = new Dictionary<string,double>();
                foreach (var robot in avatarDictionary)
                    scoreRobotList.Add(robot.Value.Login, _scoreCounter.GetTotalScoreForRobot(robot.Value));
                var sortedRobotsActionHistory = scoreRobotList.OrderBy(x => x.Value).ToList();
                foreach (var action in sortedRobotsActionHistory)
                    EventLog.WriteMessageToLog(strScoreLogPath, "Robot: " + action.Key + " score: " + action.Value.ToString());
            }
            catch (Exception e)
            {
                EventLog.WriteErrorToLog(strScoreLogPath, e);
            }


        }

        void MainWindow_EndGameEvent(object oSender, EventArgs Args)
        {
            GamePlayThread.Abort();

            if (!_endGameInformationSend)
            {
                _endGameInformationSend = true;
                if (_scoreModule != null)
                {
                    foreach (IArenaCallback client in avatarDictionary.Keys) // GamePlay is ending;
                        try
                        {
                            client.gamePlayEnd(_scoreCounter.GetTotalScoreForRobot(avatarDictionary[client]));
                        }
                        catch (Exception e)
                        {
                            EventLog.WriteErrorToLog(strLogPath, e);
                        }
                    CreateHistoryLogFile(_numberOfPlayedGames);
                    _globalHistory.Clear(); // clear all history for that game
                }
                else
                    foreach (IArenaCallback client in avatarDictionary.Keys)
                    {// GamePlay is ending;
                        try
                        {
                            client.gamePlayEnd(0);
                        }
                        catch (Exception e)
                        {
                            EventLog.WriteErrorToLog(strLogPath, e);
                        }
                    }
            }
            _currentRound = 0;
            _currentMap = _initialMap.Copy();
            _isGameStarted = false;
            _currentMovesQueue.Clear();
            _globalHistory.Clear();
            _scoreCounter.Clear();
            _currentMap = (oSender as MainWindow).CurrentMap;


            foreach (RobotAvatar robotA in avatarDictionary.Values)
            {
                removeRobotFromMap(robotA.Color);
            }


            foreach (var client in avatarDictionary)
            {
                client.Value.CurrentMapForRobot = CreateStartingPack(client.Key, client.Value);
            }

            EventLog.WriteMessageToLog(strLogPath, "End of the game");
            

        }

        void MainWindow_StopServerEvent(object oSender, EventArgs Args)
        {
            _pauseGame = true;
            EventLog.WriteMessageToLog(strLogPath, "Game is paused");
        }

        void MainWindow_ResumeServerEvent(object oSender, EventArgs Args)
        {
            GamePlayThread.Interrupt();
            _pauseGame = false;
            EventLog.WriteMessageToLog(strLogPath, "Game is resumed");
        }



        void MainWindow_ScoreModuleChangeEvent(object oSender, ScoreModuleChangeArgs scoreModulChangeArgs)
        {
            this._scoreModule = scoreModulChangeArgs.Score;
        }

        void MainWindow_MapChangeEvent(object oSender, MapChangeArgs mapChangeArgs)
        {
            this._currentMap = mapChangeArgs.Map;
            this._initialMap = mapChangeArgs.Map;
            foreach (var client in avatarDictionary.Keys)
            {
                var robotAvatar = avatarDictionary[client];
                robotAvatar.CurrentMapForRobot = CreateStartingPack(client, robotAvatar);
                
     
            }
            EventLog.WriteMessageToLog(strLogPath, "Map has been changed");

        }

        /// <summary>
        /// Event invoked after StartServerButton Clicked in MainWindow Form
        /// </summary>
        /// <param name="oSender"></param>
        /// <param name="startServerArgs"> Start server arguments like: roundNumber, hostileMode, extendedRoundTime</param>
        void MainWindow_StartServerEvent(object oSender, ServerStartArgs startServerArgs)
        {
            _endGameInformationSend = false;
            _numberOfPlayedGames++;
            _roundNumber = startServerArgs.roundNumber;
            _currentRound = 0;
            _hostileMode = startServerArgs.hostileMode;
            _roundTime = (int)(startServerArgs.RoundTime * 1000);
            _gameMinuteTime = startServerArgs.gameMinuteTime;
            _gameSecondTime = startServerArgs.gameSecondTime;
            _endNRoundCriteria = startServerArgs.endNRoundCriteria;
            _numberOfTeams = startServerArgs.numberOfTeams;


            if (_scoreModule != null)
                GamePlay = new GamePlay(_currentMap, avatarDictionary, _scoreModule,_numberOfPlayedGames);
            else
                GamePlay = new GamePlay(_currentMap, avatarDictionary, null, _numberOfPlayedGames);
            GamePlayThread = new Thread(new ThreadStart(() => StartGamePlay()));
            GamePlayThread.Start();
            string listOfPlayers = null;
            foreach (RobotAvatar ra in avatarDictionary.Values)
            {
                listOfPlayers = listOfPlayers + ra.Login + " ";
            }
            EventLog.WriteMessageToLog(strLogPath, "Server Started with following parameters: Round Number: " + _roundNumber.ToString() + "; Hostile Mode: " + _hostileMode.ToString() +
                "; Extended Round Time: " + _extendedRoundTime.ToString() + "; Number of Teams: " + _numberOfTeams.ToString() + "; Logged Players: " + listOfPlayers);
            //StartGamePlay();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public void Login(string user)
        {
            var client = OperationContext.Current.GetCallbackChannel<IArenaCallback>();

            EventLog.WriteMessageToLog(strLogPath, "Client: " + user + " want to log in");

            if (!avatarDictionary.Values.ToList().Exists(c => c.Login.Equals(user)) && !_isGameStarted && avatarDictionary.Count <= 8)
            {
                if (_currentMap != null)
                    if (_currentMap.StartingPositionList.Count < _numberOfLoggedRobots)
                    {
                        client.reciveInitialData(InitialServerResponse.Exception("Too many players, no place for you"));
                        EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + user + " cannot log in because there are too many players");
                        return;
                    }

                string color = "default";
                if (colorList.Count > 0)
                {
                    color = colorList.First();
                    colorList.Remove(color);
                }

                var robotAvatar = new RobotAvatar(user, color, null);
                PlayerLoginEvent(this, new PlayerLoginArgs(robotAvatar));
                Map mapForUser = null;

                avatarDictionary.Add(client, robotAvatar);
                _numberOfLoggedRobots++;
                OperationContext.Current.Channel.Faulted += new EventHandler(Channel_Faulted);
                OperationContext.Current.Channel.Closed += new EventHandler(Channel_Faulted);

                if (_currentMap != null)
                {
                    if (_currentMap.StartingPositionNumber < _numberOfLoggedRobots)
                    {
                        client.reciveInitialData(InitialServerResponse.Exception("Too many players, no place for you"));
                        EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + user + " cannot log in because there are too many players");

                        return;
                    }

                    robotAvatar.CurrentMapForRobot = CreateStartingPack(client, robotAvatar);
                    client.reciveInitialData( InitialServerResponse.PlayerRegistered(robotAvatar.Color, (int)_roundTime, robotAvatar.RobotPosition, new MapSize(_currentMap.MapWidth,_currentMap.MapHeight)));
                    EventLog.WriteMessageToLog(strLogPath, "Client: " + user + " logged in and recieved Color " + color + " and Starting Position: " + robotAvatar.RobotPosition.ToString());
                }
                else
                {
                    client.reciveInitialData(InitialServerResponse.Exception("Wait for map"));
                    EventLog.WriteMessageToLog(strLogPath, "Client: " + user + " has to wait for map");
                }

            }
            else if (avatarDictionary.Values.ToList().Exists(c => c.Login.Equals(user)))
            {
                client.reciveInitialData(InitialServerResponse.LoginAlreadyExists());
                EventLog.WriteMessageToLog(strLogPath, "ERROR: Login of Client: " + user + " already exists");
            }
            else if (_isGameStarted)
            {
                client.reciveInitialData(InitialServerResponse.Exception("Game already started, wait for the end!"));
                EventLog.WriteMessageToLog(strLogPath, "ERROR: Game Started!");
            }
            else
            {
                client.reciveInitialData(InitialServerResponse.Exception("Maximal number of players 9. Sorry no place for you!"));
                EventLog.WriteMessageToLog(strLogPath, "ERROR: Too many players");
            }

        }

        void Channel_Faulted(object sender, EventArgs e)
        {

            Disconnect(sender as IArenaCallback);

        }

        protected void Disconnect(IArenaCallback client)
        {
            if (avatarDictionary.Keys.ToList<IArenaCallback>().Exists(cl => cl.Equals(client)))
            {
                if (_isGameStarted)
                {
                    RobotAvatar robot;
                    avatarDictionary.TryGetValue(client, out robot);
                    removeRobotFromMap(robot.Color);

                    _disconnectedRobotAvatarList.Add(robot);
                    try
                    {
                        if (_currentMovesQueue.Exists(m => m.Client.Equals(client)))
                            _currentMovesQueue.Remove(_currentMovesQueue.Find(m => m.Client.Equals(client)));
                    }
                    catch (Exception e)
                    {
                        EventLog.WriteErrorToLog(strLogPath, e);
                    }
                    _currentMovesQueue.Add(new Move(MoveType.Disconnect, _currentMap, client, avatarDictionary, avatarDictionary[client], Directions.Down, _currentRound));
                }

                try
                {
                    _currentMap.StartingPositionList.Find(pos => pos.X == avatarDictionary[client].InitialPosition.X && pos.Y == avatarDictionary[client].InitialPosition.Y).Used = false;
                }
                catch (Exception e)
                {
                    EventLog.WriteErrorToLog(strLogPath, e);
                }
                colorList.Add(avatarDictionary[client].Color);
                PlayerDisconnectEvent(this, new PlayerLoginArgs(avatarDictionary[client]));
                EventLog.WriteMessageToLog(strLogPath, "Login of Client: " + avatarDictionary[client].Login + " DISCONNECTED");
                avatarDictionary.Remove(client);
                _numberOfLoggedRobots--;

            }

        }

        public Map CreateStartingPack(IArenaCallback client, RobotAvatar robotAvatar)
        {
            Map mapForUser;
            var rnd = new Random();
            _currentMap.StartingPositionList = _currentMap.StartingPositionList.OrderBy(item => rnd.Next()).Select(c => c).ToList(); ;
          //  this.OrderBy(x => Guid.NewGuid());

            try
            {
                Position st = this._currentMap.StartingPositionList.First(start => start.Used == false);
                robotAvatar.RobotPosition = st;
                robotAvatar.InitialPosition = st.Copy();

                _currentMap.GlobalMap[st.Y, st.X].Robot = new Robot(robotAvatar.HealthPoints, robotAvatar.RobotPosition, robotAvatar.Color); ;
                mapForUser = _currentMap.getSmallerPartForRobot(new Position(st.X, st.Y));
                //mapForUser.GlobalMap[st.Y, st.X].IsRobot = true;
                st.Used = true;

                return mapForUser;
            }
            catch (Exception e)
            {
                EventLog.WriteErrorToLog(strLogPath, e);
            }
            return new Map(0, 0);
            
        }



        public void MakeMove(Directions direction, int round)
        {
            var client = OperationContext.Current.GetCallbackChannel<IArenaCallback>();
            var robot = avatarDictionary[client];

            if (direction == null || direction == Directions.NULL)
            {
                if (robot.ErrorNumber++ > 10)
                    Disconnect(client);
                client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("Direction of Move action cannot be null")));
                EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " null Direction");
                _currentMovesQueue.Enqueue(new Move(MoveType.WrongAction, _currentMap, client, avatarDictionary, robot, Directions.NULL, _currentRound, _hostileMode));
            }
            else
            {
            if (round == _currentRound)
            {
                if (!_currentMovesQueue.Enqueue(new Move(MoveType.MakeMove, _currentMap, client, avatarDictionary, robot, direction, _currentRound)))
                {
                    if (robot.ErrorNumber++ > 10)
                        Disconnect(client);
                    client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("You cannot move two times in one round")));
                    EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " moved two times in one round");
                }
                else
                    EventLog.WriteMessageToLog(strLogPath, "Client: " + robot.Login + " Made move in direction " + direction.ToString() + " in round " + round.ToString());
            }
            else
            {
                client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("Move not to this round")));
                EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " moved not in current round");

            }

            }

        }



        public void PickBigItem(int round)
        {

            Directions direction = new Directions();

            var client = OperationContext.Current.GetCallbackChannel<IArenaCallback>();
            var robot = avatarDictionary[client];
            if (round == _currentRound)
                if (!_currentMovesQueue.Enqueue(new Move(MoveType.PickBigItem, _currentMap, client, avatarDictionary, robot, direction, _currentRound)))
                {
                    if (robot.ErrorNumber++ > 10)
                        Disconnect(client);
                    client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("You cannot move two times in one round")));
                    EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " moved two times in one round");
                }
                else
                    EventLog.WriteMessageToLog(strLogPath, "Client: " + robot.Login + " Picked Big item " + " in round " + round.ToString());


        }




        public void DropBigItem(int round)
        {

            Directions direction = new Directions();
            var client = OperationContext.Current.GetCallbackChannel<IArenaCallback>();
            var robot = avatarDictionary[client];
            if (round == _currentRound)
                if (!_currentMovesQueue.Enqueue(new Move(MoveType.DropBigItem, _currentMap, client, avatarDictionary, robot, direction, _currentRound)))
                {
                    if (robot.ErrorNumber++ > 10)
                        Disconnect(client);
                    client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("You cannot move two times in one round")));
                    EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " moved two times in one round");
                }
                else
                    EventLog.WriteMessageToLog(strLogPath, "Client: " + robot.Login + " Dropped Big item " + " in round " + round.ToString());

        }





        public void PickSmallItem(int round)
        {

            Directions direction = new Directions();
            var client = OperationContext.Current.GetCallbackChannel<IArenaCallback>();
            var robot = avatarDictionary[client];
            if (round == _currentRound)

                if (!_currentMovesQueue.Enqueue(new Move(MoveType.PickSmallItem, _currentMap, client, avatarDictionary, robot, direction, _currentRound)))
                {
                    if (robot.ErrorNumber++ > 10)
                        Disconnect(client);
                    client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("You cannot move two times in one round")));
                    EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " moved two times in one round");
                }
                else
                    EventLog.WriteMessageToLog(strLogPath, "Client: " + robot.Login + " Picked Big item " + " in round " + round.ToString());

        }





        //public void Punch(Directions direction, int round)
        //{

        //    var client = OperationContext.Current.GetCallbackChannel<IArenaCallback>();
        //    var robot = avatarDictionary[client];
        //    if (round == _currentRound)
        //        if (!_currentMovesQueue.Enqueue(new Move(MoveType.Punch, _currentMap, client, avatarDictionary, robot, direction, _currentRound, _hostileMode)))
        //        {
        //            if (robot.ErrorNumber++ > 10)
        //                Disconnect(client);
        //            client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("You cannot move two times in one round")));
        //            EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " moved two times in one round");
        //        }
        //    EventLog.WriteMessageToLog(strLogPath, "Client: " + robot.Login + " Punched in direction " + direction.ToString() + " in round " + round.ToString());

        //}





        public void Shoot(Directions direction, int round)
        {

            var client = OperationContext.Current.GetCallbackChannel<IArenaCallback>();
            var robot = avatarDictionary[client];

            if (direction == null || direction == Directions.NULL)
            {
                if (robot.ErrorNumber++ > 10)
                    Disconnect(client);
                client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("Shoot Action cannot have null direction")));
                EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " Direction of shoot cannot be null");
                _currentMovesQueue.Enqueue(new Move(MoveType.WrongAction, _currentMap, client, avatarDictionary, robot, Directions.NULL, _currentRound, _hostileMode));
            }
            else
            {

            if (round == _currentRound)
                if (!_currentMovesQueue.Enqueue(new Move(MoveType.Shoot, _currentMap, client, avatarDictionary, robot, direction, _currentRound, _hostileMode)))
                {
                    if (robot.ErrorNumber++ > 10)
                        Disconnect(client);
                    client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("You cannot move two times in one round")));
                    EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " moved two times in one round");
                }
                else
                    EventLog.WriteMessageToLog(strLogPath, "Client: " + robot.Login + " Shot in direction " + direction.ToString() + " in round " + round.ToString());
            }
        }




        public void Rest(int round)
        {

            Directions direction = new Directions();
            var client = OperationContext.Current.GetCallbackChannel<IArenaCallback>();
            var robot = avatarDictionary[client];
            if (round == _currentRound)
                if (!_currentMovesQueue.Enqueue(new Move(MoveType.Rest, _currentMap, client, avatarDictionary, robot, direction, _currentRound)))
                {

                    if (robot.ErrorNumber++ > 10)
                        Disconnect(client);
                    //DISCONECT
                    client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(robot.RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.InvalidMoveMessage("You cannot move two times in one round")));
                    EventLog.WriteMessageToLog(strLogPath, "ERROR: Client: " + robot.Login + " moved two times in one round");

                }
                else
                    EventLog.WriteMessageToLog(strLogPath, "Client: " + robot.Login + " Rest " + "in round " + round.ToString());

        }



        public void StartGamePlay()
        {

            #region Send Teams
            foreach (var client in avatarDictionary)
            {
                // GamePlay is starting - get ready !!!

                string team = null;
                int teamCounter = avatarDictionary.Values.ToList().FindIndex(c => c.Equals(client.Value)) + 1;
                if (_numberOfTeams > 0)
                {
                    if (_numberOfTeams == 2)
                    {
                        if (teamCounter % 2 == 0)
                            team = teamList[1];
                        else team = teamList[0];
                    }
                    else if (_numberOfTeams == 4)
                    {
                        if (teamCounter % 4 == 0)
                            team = teamList[3];
                        else if (teamCounter % 4 == 3)
                            team = teamList[2];
                        else if (teamCounter % 4 == 2)
                            team = teamList[1];
                        else team = teamList[0];
                    }
                    client.Value.Team = team;
                }

                client.Key.gamePlayStarted(client.Value.CurrentMapForRobot.SerializeMap(),_roundNumber, team); // send team with initialMap
                EventLog.WriteMessageToLog(strLogPath, "Client: " + client.Value.Login + " is in TEAM: " + team);
            }
            #endregion

           // #region Send visible map
            //foreach (var client in avatarDictionary)
           // client.Key.reciveInitialData(InitialServerResponse.PlayerRegistered(avatarDictionary[client.Key].Color, (int)_roundTime, client.Value.RobotPosition,new MapSize(_currentMap.MapWidth, _currentMap.MapHeight)));
           // #endregion

            _isGameStarted = true;
            _globalHistory = new Dictionary<int, List<ActionHistory>>();

            EventLog.WriteMessageToLog(strLogPath, "GAME IS STARTING!");

            while (_currentRound != _roundNumber)
            {
                EventLog.WriteMessageToLog(strLogPath, "ROUND nr: " + _currentRound + " IS STARTING!");

                if (avatarDictionary.Values.ToList().Exists(rob => rob.ErrorNumber >= 10))
                    Disconnect(avatarDictionary.FirstOrDefault(cl => cl.Value == avatarDictionary.Values.ToList().Find(rob => rob.ErrorNumber >= 10)).Key);


                _globalHistory.Add(_currentRound, new List<ActionHistory>()); // add history card for this round
                
                try
                {
                    foreach (IArenaCallback client in avatarDictionary.Keys)
                    {// Round is starting - get ready !!!

                        var possibleMovesSet = Move.GetPossibleActions(avatarDictionary[client], _currentMap, avatarDictionary.Values.ToList());
                        _globalHistory[_currentRound].Add(new ActionHistory(avatarDictionary[client].Login, _currentRound, possibleMovesSet));
                        client.gameRoundStart(_currentRound, possibleMovesSet);

                    }
                }
                catch (Exception e)
                {
                    EventLog.WriteErrorToLog(strLogPath, e);
                }

                EventLog.WriteMessageToLog(strLogPath, "ROUND nr: " + _currentRound + " SEND gameRoundStart message");

                try
                {
#if !DEBUG
                    Thread.Sleep((int)_roundTime);
#else
                    while (_currentMovesQueue.Count != _numberOfLoggedRobots)
                        Thread.Sleep(1000);
#endif
                }
                catch (ThreadInterruptedException e) // wake up!
                {

                }


                foreach (var history in _globalHistory[_currentRound])
                    if (_currentMovesQueue.Exists(m => m.Robot.Login.Equals(history.RobotLogin)))
                        history.MadeMove = _currentMovesQueue.Find(m => m.Robot.Login.Equals(history.RobotLogin)).MadeMove;
              
                var TimeoutPlayers = avatarDictionary.Keys.ToList<IArenaCallback>().FindAll(client => !_currentMovesQueue.Exists(m => m.Client.Equals(client)));



                foreach (var client in TimeoutPlayers)
                {// send Timout to those who do not send move
                    _globalHistory[_currentRound].Find(history => history.RobotLogin.Equals(avatarDictionary[client].Login)).MadeMove = MoveType.Timeout; // set madeMove = Timeout for those who make timeout
                    EventLog.WriteMessageToLog(strLogPath, "TIMEOUT: Client: " + avatarDictionary[client].Login);
                    avatarDictionary[client].ErrorNumber++;
                    client.reciveGamePlayData(_currentMap.getSmallerPartForRobot(avatarDictionary[client].RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, avatarDictionary[client].RobotPosition, 0, 0, avatarDictionary[client].HasBigItem, avatarDictionary[client].SmallItem, MoveConsequence.TimeOut, GamePlayServerResponse.MoveTimout("Time out. Be quick!")));
                    _currentMovesQueue.Enqueue(new Move(MoveType.WrongAction, _currentMap, client, avatarDictionary, avatarDictionary[client], Directions.Down, _currentRound));
                }

                if (_disconnectedRobotAvatarList.Count > 0)
                    foreach (var robot in _disconnectedRobotAvatarList)
                        _currentMovesQueue.Add(new Move(MoveType.DisconnectedPlayer, robot));
             
                
                var roundScore = GamePlay.PlayTurn(_currentMovesQueue, _globalHistory);

                _scoreCounter.AddScore(_currentRound, roundScore);

               
                    try
                    {
                      
                        foreach (IArenaCallback client in avatarDictionary.Keys) // Round is starting - get ready !!!
                        {
                            try
                            {
                                client.gameRoundEnd(_currentRound);
                                EventLog.WriteMessageToLog(strLogPath, "SEND: gameRoundEnd");
                            }
                            catch (Exception e)
                            {
                                EventLog.WriteErrorToLog(strLogPath, e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        EventLog.WriteErrorToLog(strLogPath, e);
                    }
                _currentRound++;

                if (_pauseGame)
                    try
                    {
                        Thread.Sleep(Timeout.Infinite);
                    }
                    catch (ThreadInterruptedException e) // wake up!
                    {

                    }

            }

            if (!_endGameInformationSend)
            {
                _endGameInformationSend = true;
                if (_scoreModule != null)
                    foreach (IArenaCallback client in avatarDictionary.Keys)
                    {// GamePlay is ending;
                        EventLog.WriteMessageToLog(strLogPath, "SEND: gamePlayEnd");
                        client.gamePlayEnd(_scoreCounter.GetTotalScoreForRobot(avatarDictionary[client]));
                    }
                else
                    foreach (IArenaCallback client in avatarDictionary.Keys)
                    {// GamePlay is ending;
                        client.gamePlayEnd(0);
                        EventLog.WriteMessageToLog(strLogPath, "SEND: gamePlayEnd");
                    }
            }

        }


        public void removeRobotFromMap(string robotColor)
        {
            for (int i = 0; i < _currentMap.MapHeight; i++)
            {
                for (int j = 0; j < _currentMap.MapWidth; j++)
                {
                    if (_currentMap.GlobalMap[i, j].Robot != null && _currentMap.GlobalMap[i, j].Robot.RobotColor == robotColor)
                        _currentMap.GlobalMap[i, j].Robot = null;
                }
            }
        }





        public void SendInitialData(byte[] initialMap, int roundNumber, bool hostileMode, bool extendedRoundTime, int gameSecondTime, int gameMinuteTime, bool endNRoundCriteria)
        {
            throw new NotImplementedException();
        }
    }
}
