using Arena_Server.Infrastructure;
using ArenaServer.Infrastructure.EventArgs;
using Common_Library;
using Common_Library.Infrastructure;
using Common_Library.IService_Contracts;
using Common_Library.Parts_of_map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arena_Server.Infrastructure
{
    public class Move
    {
        private RobotAvatar robot;
        private Map _currentMap;
        private GamePlayServerResponse response;
        private int healNumberRounds = 4;
        private bool _hostileMode = true;
        private bool _hasRobotBigItem = false;
        private int _numberOfSmallItems = 0;
        private MoveConsequence _consequence;
        private double _myPay;
        private bool _isWallShooted = false;
        private bool _isRobotShooted = false;

        public double MyPay
        {
            get { return _myPay; }
            set { _myPay = value; }
        }

        public MoveConsequence Consequence
        {
            get { return _consequence; }
            set { _consequence = value; }
        }
        public int NumberOfSmallItems
        {
            get { return _numberOfSmallItems; }
            set { _numberOfSmallItems = value; }
        }

        public bool HasRobotBigItem
        {
            get { return _hasRobotBigItem; }
            set { _hasRobotBigItem = value; }
        }

        public bool HostileMode
        {
            get { return _hostileMode; }
            set { _hostileMode = value; }
        }


        private Dictionary<IArenaCallback, RobotAvatar> _avatarDictionary;
        private int _currentRound;

        private IArenaCallback client;

        public IArenaCallback Client
        {
            get { return client; }
            set { client = value; }
        }


        public GamePlayServerResponse Response
        {
            get { return response; }
            set { response = value; }
        }

        public Map CurrentMap
        {
            get { return _currentMap; }
            set { _currentMap = value; }
        }

        public int CurrentRound
        {
            get { return _currentRound; }
            set { _currentRound = value; }
        }


        private MoveType madeMove;
        private Directions _directionOfMove;
        private RobotAvatar woundedRobot;

        public RobotAvatar WoundedRobot
        {
            get { return woundedRobot; }
            set { if  (this.madeMove == MoveType.Shoot) woundedRobot = value; }
        }

        public Directions DirectionOfMove
        {
            get { return _directionOfMove; }
            set { _directionOfMove = value; }
        }

        public MoveType MadeMove
        {
            get { return madeMove; }
            set { madeMove = value; }
        }

        public RobotAvatar Robot
        {
            get { return robot; }
            set { robot = value; }
        }


        public Move(MoveType move, Map Map, IArenaCallback client, Dictionary<IArenaCallback, RobotAvatar> AllRobotsDictionary, RobotAvatar robot, Directions directionOfMove, int curentRound)
        {
            this.robot = robot;
            this._avatarDictionary = AllRobotsDictionary;
            this.madeMove = move;
            this._currentMap = Map;
            this._directionOfMove = directionOfMove;
            this.client = client;
            this._currentRound = curentRound;
        }

        public Move(MoveType moveType, RobotAvatar robot)
        {
            this.robot = robot;
            this.madeMove = moveType;
            this.client = null;
        }


        public Move(MoveType move, Map Map, IArenaCallback client, Dictionary<IArenaCallback, RobotAvatar> AllRobotsDictionary, RobotAvatar robot, Directions directionOfMove, int curentRound, bool hostileMode)
        {
            this.robot = robot;
            this._avatarDictionary = AllRobotsDictionary;
            this.madeMove = move;
            this._currentMap = Map;
            this._directionOfMove = directionOfMove;
            this.client = client;
            this._currentRound = curentRound;
            _hostileMode = hostileMode;
        }


        #region MakeMoveAction
        public Move MakeMoveAction()
        {

            if (robot.IsHealed == false)
            {
                if (robot.HealRounds-- == 1) { robot.IsHealed = true; robot.HealthPoints = 5; };
                return this.BurnAction();
            }

            switch (DirectionOfMove)
            {
                case Directions.Up:
                    if (robot.RobotPosition.Y - 1 >= 1)
                    {
                        if (_currentMap.GlobalMap[robot.RobotPosition.Y - 1, robot.RobotPosition.X] is Floor &&
                           !_avatarDictionary.Values.ToList().Exists(r => r.RobotPosition.X == robot.RobotPosition.X && r.RobotPosition.Y == robot.RobotPosition.Y - 1))
                        {
                            _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].Robot = null;
                            robot.RobotPosition.Y = robot.RobotPosition.Y - 1;
                            _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].Robot = new Robot(robot.HealthPoints, robot.RobotPosition, robot.Color);
                            response = GamePlayServerResponse.OK();


                        }
                        else
                        {
                            robot.ErrorNumber++;
                            response = GamePlayServerResponse.InvalidMoveMessage("Robot is trying to move to unavailable segment");
                        }

                    }
                    else
                    {
                        robot.ErrorNumber++;
                        response = GamePlayServerResponse.InvalidMoveMessage("Robot is trying to move to segment outside the map");
                    }
                    break;

                case Directions.Right:
                    if (robot.RobotPosition.X + 1 < _currentMap.MapWidth - 1)
                    {
                        if (_currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X + 1] is Floor &&
                            !_avatarDictionary.Values.ToList().Exists(r => r.RobotPosition.X == robot.RobotPosition.X + 1 && r.RobotPosition.Y == robot.RobotPosition.Y))
                        {
                            _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].Robot = null;
                            robot.RobotPosition.X = robot.RobotPosition.X + 1;
                            _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].Robot = new Robot(robot.HealthPoints, robot.RobotPosition, robot.Color);
                            response = GamePlayServerResponse.OK();


                        }
                        else
                        {
                            robot.ErrorNumber++;
                            response = GamePlayServerResponse.InvalidMoveMessage("Robot is trying to move to unavailable segment");
                        }

                    }
                    else
                    {
                        robot.ErrorNumber++;
                        response = GamePlayServerResponse.InvalidMoveMessage("Robot is trying to move to segment outside the map");
                    }
                    break;

                case Directions.Down:
                    if (robot.RobotPosition.Y + 1 < _currentMap.MapHeight - 1)
                    {
                        if (_currentMap.GlobalMap[robot.RobotPosition.Y + 1, robot.RobotPosition.X] is Floor &&
                            !_avatarDictionary.Values.ToList().Exists(r => r.RobotPosition.X == robot.RobotPosition.X && r.RobotPosition.Y == robot.RobotPosition.Y + 1))
                        {
                            _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].Robot = null;
                            robot.RobotPosition.Y = robot.RobotPosition.Y + 1;
                            _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].Robot = new Robot(robot.HealthPoints, robot.RobotPosition, robot.Color);
                            response = GamePlayServerResponse.OK();

                        }
                        else
                        {
                            robot.ErrorNumber++;
                            response = GamePlayServerResponse.InvalidMoveMessage("Robot is trying to move to unavailable segment");
                        }

                    }
                    else
                    {
                        robot.ErrorNumber++;
                        response = GamePlayServerResponse.InvalidMoveMessage("Robot is trying to move to segment outside the map");
                    }
                    break;

                case Directions.Left:
                    if (robot.RobotPosition.X - 1 >= 1)
                    {
                        if (_currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X - 1] is Floor &&
                            !_avatarDictionary.Values.ToList().Exists(r => r.RobotPosition.X == robot.RobotPosition.X - 1 && r.RobotPosition.Y == robot.RobotPosition.Y))
                        {
                            _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].Robot = null;
                            robot.RobotPosition.X = robot.RobotPosition.X - 1;
                            _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].Robot = new Robot(robot.HealthPoints, robot.RobotPosition, robot.Color);
                            response = GamePlayServerResponse.OK();


                        }
                        else
                        {
                            robot.ErrorNumber++;
                            response = GamePlayServerResponse.InvalidMoveMessage("Robot is trying to move to unavailable segment");
                        }

                    }
                    else
                    {
                        robot.ErrorNumber++;
                        response = GamePlayServerResponse.InvalidMoveMessage("Robot is trying to move to segment outside the map");
                    }
                    break;
            }
            if (response.ServerState == ServerState.INVALID_MOVE)
            {
                Consequence = MoveConsequence.InvalidMove;
                MadeMove = MoveType.WrongAction;
                return this;
            }
            else
            {
                Consequence = MoveConsequence.MovedToAnotherPlace;
                return this;
            }
        }
        #endregion

        public Move PickBigItemAction()
        {
            if (robot.IsHealed == false)
            {
                if (robot.HealRounds-- == 1) { robot.IsHealed = true; robot.HealthPoints = 5; };
                return this.BurnAction();
            }
            else
            {
                if (robot.HasBigItem)
                {
                    robot.ErrorNumber++;
                    response = GamePlayServerResponse.InvalidMoveMessage("Robot is currently carrying a Big Item");
                }
                else
                {
                    if (_currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].HasBigItem)
                    {
                        robot.HasBigItem = true;
                        _hasRobotBigItem = true;
                        _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].HasBigItem = false;
                        response = GamePlayServerResponse.OK();

                    }
                    else
                    {
                        robot.ErrorNumber++;
                        response = GamePlayServerResponse.InvalidMoveMessage("There is no Big Item on field under the Robot");
                    }

                }
                if (response.ServerState == ServerState.INVALID_MOVE)
                {
                    Consequence = MoveConsequence.InvalidMove_PickingBigItem;
                    MadeMove = MoveType.WrongAction;
                    return this;
                }
                else
                {
                    Consequence = MoveConsequence.PickedBigItem;
                    return this;
                }
            }
        }
        public Move DropBigItemAction()
        {
            if (robot.IsHealed == false)
            {
                if (robot.HealRounds-- == 1) { robot.IsHealed = true; robot.HealthPoints = 5; };
                return this.BurnAction();
            }
            else
            {
                if (robot.HasBigItem == false)
                {
                    robot.ErrorNumber++;
                    response = GamePlayServerResponse.InvalidMoveMessage("Robot has no Big Items");
                }
                else
                {
                    if (_currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].HasBigItem == false &&
                        _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].SmallItemNumber == 0)
                    {
                        robot.HasBigItem = false;
                        _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].HasBigItem = false;
                        _hasRobotBigItem = false;
                        response = GamePlayServerResponse.OK();

                    }
                    else
                    {
                        robot.ErrorNumber++;
                        response = GamePlayServerResponse.InvalidMoveMessage("Field under the robot has an item already");

                    }

                }
                if (response.ServerState == ServerState.INVALID_MOVE)
                {
                    Consequence = MoveConsequence.InvalidMove_DroppingBigItem;
                    MadeMove = MoveType.WrongAction;
                    return this;
                }
                else
                {
                    Consequence = MoveConsequence.DroppedBigItem;
                    return this;
                }
            }

        }
        public Move PickSmallItemAction()
        {
            if (robot.IsHealed == false)
            {
                if (robot.HealRounds-- == 1) { robot.IsHealed = true; robot.HealthPoints = 5; };
                return this.BurnAction();
            }
            else
            {

                if (_currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].SmallItemNumber > 0)
                {
                    robot.SmallItem++;
                    _currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].SmallItemNumber--;
                    _numberOfSmallItems++;
                    response = GamePlayServerResponse.OK();

                }
                else
                {
                    robot.ErrorNumber++;
                    response = GamePlayServerResponse.InvalidMoveMessage("Field under the robot does not contain small item");
                }
                if (response.ServerState == ServerState.INVALID_MOVE)
                {
                    Consequence = MoveConsequence.InvalidMove_PickingSmallItem;
                    MadeMove = MoveType.WrongAction;
                    return this;
                }
                else
                {
                    Consequence = MoveConsequence.PickedSmallItem;
                    return this;
                }
            }
        }
        //public Move PunchAction()
        //{
        //    RobotAvatar hitRobot;

        //    if (robot.IsHealed == false)
        //    {
        //        if (robot.HealRounds-- == 1) { robot.IsHealed = true; robot.HealthPoints = 5; };
        //        return this.BurnAction();
        //    }
        //    switch (DirectionOfMove)
        //    {
        //        case Directions.Up:
        //            if (robot.RobotPosition.Y - 1 >= 1)
        //            {
        //                hitRobot = _avatarDictionary.Values.ToList().Find(hr => hr.RobotPosition.X == robot.RobotPosition.X && hr.RobotPosition.Y == robot.RobotPosition.Y - 1);
        //                if (hitRobot != null)
        //                {
        //                    if (hitRobot.HealthPoints >= 1)
        //                    {
        //                        if (_hostileMode)
        //                            hitRobot.HealthPoints = hitRobot.HealthPoints - 2;
        //                        response = GamePlayServerResponse.OK();
        //                        var woundedClient = _avatarDictionary.FirstOrDefault(wc => wc.Value == hitRobot).Key;
        //                        response = GamePlayServerResponse.Wounded();
        //                        woundedRobot = _avatarDictionary[woundedClient];
        //                        woundedClient.reciveGamePlayData(_currentMap.getSmallerPartForRobot(_avatarDictionary[woundedClient].RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, woundedRobot.RobotPosition, woundedRobot.HasBigItem, woundedRobot.SmallItem, response));

        //                        if (hitRobot.HealthPoints <= 0)
        //                        {
        //                            hitRobot.IsHealed = false;
        //                            hitRobot.HealRounds = healNumberRounds;
        //                        }
        //                        break;

        //                    }
        //                    else
        //                    {
        //                        response = GamePlayServerResponse.OK();
        //                        //robot.ErrorNumber++;
        //                        //response = GamePlayServerResponse.InvalidMoveMessage("Hit robot is already damaged");
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    robot.ErrorNumber++;
        //                    response = GamePlayServerResponse.InvalidMoveMessage("Field chosen to hit contains no robot");
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                robot.ErrorNumber++;
        //                response = GamePlayServerResponse.InvalidMoveMessage("Field chosen to hit contains no robot");
        //            }
        //            break;

        //        case Directions.Right:
        //            if (robot.RobotPosition.X + 1 < +_currentMap.MapWidth - 1)
        //            {
        //                hitRobot = _avatarDictionary.Values.ToList().Find(hr => hr.RobotPosition.X == robot.RobotPosition.X + 1 && hr.RobotPosition.Y == robot.RobotPosition.Y);
        //                if (hitRobot != null)
        //                {
        //                    if (hitRobot.HealthPoints >= 1)
        //                    {
        //                        if (_hostileMode)
        //                            hitRobot.HealthPoints = hitRobot.HealthPoints - 2;
        //                        response = GamePlayServerResponse.OK();
        //                        var woundedClient = _avatarDictionary.FirstOrDefault(wc => wc.Value == hitRobot).Key;
        //                        this.response = GamePlayServerResponse.Wounded();
        //                        woundedRobot = _avatarDictionary[woundedClient];
        //                        woundedClient.reciveGamePlayData(_currentMap.getSmallerPartForRobot(_avatarDictionary[woundedClient].RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, woundedRobot.RobotPosition, woundedRobot.HasBigItem, woundedRobot.SmallItem, response));

        //                        if (hitRobot.HealthPoints <= 0)
        //                        {
        //                            hitRobot.IsHealed = false;
        //                            hitRobot.HealRounds = healNumberRounds;
        //                        }
        //                        break;

        //                    }
        //                    else
        //                    {
        //                        response = GamePlayServerResponse.OK();
        //                        //robot.ErrorNumber++;
        //                        //response = GamePlayServerResponse.InvalidMoveMessage("Hit robot is already damaged");
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    robot.ErrorNumber++;
        //                    response = GamePlayServerResponse.InvalidMoveMessage("Field chosen to hit contains no robot");
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                robot.ErrorNumber++;
        //                response = GamePlayServerResponse.InvalidMoveMessage("Field chosen to hit contains no robot");
        //            }
        //            break;

        //        case Directions.Down:
        //            if (robot.RobotPosition.Y + 1 < _currentMap.MapHeight - 1)
        //            {
        //                hitRobot = _avatarDictionary.Values.ToList().Find(hr => hr.RobotPosition.X == robot.RobotPosition.X && hr.RobotPosition.Y == robot.RobotPosition.Y + 1);
        //                if (hitRobot != null)
        //                {
        //                    if (hitRobot.HealthPoints >= 1)
        //                    {
        //                        if (_hostileMode)
        //                            hitRobot.HealthPoints = hitRobot.HealthPoints - 2;
        //                        response = GamePlayServerResponse.OK();
        //                        var woundedClient = _avatarDictionary.FirstOrDefault(wc => wc.Value == hitRobot).Key;
        //                        this.response = GamePlayServerResponse.Wounded();
        //                        woundedRobot = _avatarDictionary[woundedClient];
        //                        woundedClient.reciveGamePlayData(_currentMap.getSmallerPartForRobot(_avatarDictionary[woundedClient].RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, woundedRobot.RobotPosition, woundedRobot.HasBigItem, woundedRobot.SmallItem, response));

        //                        if (hitRobot.HealthPoints <= 0)
        //                        {
        //                            hitRobot.IsHealed = false;
        //                            hitRobot.HealRounds = healNumberRounds;
        //                        }
        //                        break;

        //                    }
        //                    else
        //                    {
        //                        response = GamePlayServerResponse.OK();
        //                        //robot.ErrorNumber++;
        //                        //response = GamePlayServerResponse.InvalidMoveMessage("Hit robot is already damaged");
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    robot.ErrorNumber++;
        //                    response = GamePlayServerResponse.InvalidMoveMessage("Field chosen to hit contains no robot");
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                robot.ErrorNumber++;
        //                response = GamePlayServerResponse.InvalidMoveMessage("Field chosen to hit contains no robot");
        //            }

        //            break;

        //        case Directions.Left:
        //            if (robot.RobotPosition.X - 1 >= 1)
        //            {
        //                hitRobot = _avatarDictionary.Values.ToList().Find(hr => hr.RobotPosition.X == robot.RobotPosition.X - 1 && hr.RobotPosition.Y == robot.RobotPosition.Y);
        //                if (hitRobot != null)
        //                {
        //                    if (hitRobot.HealthPoints >= 1)
        //                    {
        //                        if (_hostileMode)
        //                            hitRobot.HealthPoints = hitRobot.HealthPoints - 2;
        //                        response = GamePlayServerResponse.OK();
        //                        var woundedClient = _avatarDictionary.FirstOrDefault(wc => wc.Value == hitRobot).Key;
        //                        this.response = GamePlayServerResponse.Wounded();
        //                        woundedRobot = _avatarDictionary[woundedClient];
        //                        woundedClient.reciveGamePlayData(_currentMap.getSmallerPartForRobot(_avatarDictionary[woundedClient].RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, woundedRobot.RobotPosition, woundedRobot.HasBigItem, woundedRobot.SmallItem, response));

        //                        if (hitRobot.HealthPoints <= 0)
        //                        {
        //                            hitRobot.IsHealed = false;
        //                            hitRobot.HealRounds = healNumberRounds;
        //                        }
        //                        break;

        //                    }
        //                    else
        //                    {
        //                        response = GamePlayServerResponse.OK();
        //                        //robot.ErrorNumber++;
        //                        //response = GamePlayServerResponse.InvalidMoveMessage("Hit robot is already damaged");
        //                        break;
        //                    }
        //                }
        //                else
        //                {
        //                    robot.ErrorNumber++;
        //                    response = GamePlayServerResponse.InvalidMoveMessage("Field chosen to hit contains no robot");
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                robot.ErrorNumber++;
        //                response = GamePlayServerResponse.InvalidMoveMessage("Field chosen to hit contains no robot");
        //            }
        //            break;
        //    }
        //    if (response.ServerState == ServerState.INVALID_MOVE)
        //    {
        //        if (response.Message == "Hit robot is already damaged")
        //            Consequence = MoveConsequence.InvalidMove_PunchedPlayerWithNoHealthPoints;
        //        else
        //            Consequence = MoveConsequence.InvalidMove_PunchedEmptySpace;
        //        MadeMove = MoveType.WrongAction;
        //        return this;
        //    }
        //    else
        //    {
        //        Consequence = MoveConsequence.PunchedPlayer;
        //        return this;
        //    }

        //}
        public Move ShootAction()
        {
            RobotAvatar hitRobot;

            if (robot.IsHealed == false)
            {
                if (robot.HealRounds-- == 1) { robot.IsHealed = true; robot.HealthPoints = 5; };
                return this.BurnAction();
            }
            switch (DirectionOfMove)
            {
                case Directions.Up:
                    for (int i = 1; i <= 4; i++)
                    {
                        if (robot.RobotPosition.Y - i >= 0)
                        {
                            if (_currentMap.GlobalMap[robot.RobotPosition.Y - i, robot.RobotPosition.X] is Wall)
                            {
                                response = GamePlayServerResponse.OK();
                                _isWallShooted = true;
                                break;
                            }
                            hitRobot = _avatarDictionary.Values.ToList().Find(hr => hr.RobotPosition.X == robot.RobotPosition.X && hr.RobotPosition.Y == robot.RobotPosition.Y - i);
                            if (hitRobot != null)
                            {
                                _isRobotShooted = true;
                                if (hitRobot.HealthPoints >= 1)
                                {
                                    if (_hostileMode)
                                        hitRobot.HealthPoints = hitRobot.HealthPoints - 1;
                                    response = GamePlayServerResponse.OK();
                                    var woundedClient = _avatarDictionary.FirstOrDefault(wc => wc.Value == hitRobot).Key;
                                    response = GamePlayServerResponse.Wounded();
                                    woundedRobot = _avatarDictionary[woundedClient];
                                    woundedClient.reciveGamePlayData(_currentMap.getSmallerPartForRobot(_avatarDictionary[woundedClient].RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, woundedRobot.RobotPosition, woundedRobot.HasBigItem, woundedRobot.SmallItem, response));

                                    if (hitRobot.HealthPoints == 0)
                                    {
                                        hitRobot.IsHealed = false;
                                        hitRobot.HealRounds = healNumberRounds;
                                    }
                                    break;

                                }
                                else
                                {
                                    response = GamePlayServerResponse.OK();
                                    //robot.ErrorNumber++;
                                    //response = GamePlayServerResponse.InvalidMoveMessage("Hit robot is already damaged");
                                    break;
                                }
                            }
                            else if (_currentMap.GlobalMap[robot.RobotPosition.Y - i, robot.RobotPosition.X] is Wall)
                            {
                                response = GamePlayServerResponse.OK();
                                break;

                            }
                            else
                                response = GamePlayServerResponse.OK();
                        }
                    }
                    break;

                case Directions.Right:
                    for (int i = 1; i <= 4; i++)
                    {
                        if (robot.RobotPosition.X + i <= _currentMap.MapWidth - 1)
                        {
                            if (_currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X + i] is Wall)
                            {
                                response = GamePlayServerResponse.OK();
                                _isWallShooted = true;
                                break;
                            }
                            hitRobot = _avatarDictionary.Values.ToList().Find(hr => hr.RobotPosition.X == robot.RobotPosition.X + i && hr.RobotPosition.Y == robot.RobotPosition.Y);
                            if (hitRobot != null)
                            {
                                _isRobotShooted = true;
                                if (hitRobot.HealthPoints >= 1)
                                {
                                    if (_hostileMode)
                                        hitRobot.HealthPoints = hitRobot.HealthPoints - 1;
                                    response = GamePlayServerResponse.OK();
                                    var woundedClient = _avatarDictionary.FirstOrDefault(wc => wc.Value == hitRobot).Key;
                                    response = GamePlayServerResponse.Wounded();
                                    woundedRobot = _avatarDictionary[woundedClient];
                                    woundedClient.reciveGamePlayData(_currentMap.getSmallerPartForRobot(_avatarDictionary[woundedClient].RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, woundedRobot.RobotPosition, woundedRobot.HasBigItem, woundedRobot.SmallItem, response));

                                    if (hitRobot.HealthPoints == 0)
                                    {
                                        hitRobot.IsHealed = false;
                                        hitRobot.HealRounds = healNumberRounds;
                                    }
                                    break;

                                }
                                else
                                {
                                    response = GamePlayServerResponse.OK();
                                    //robot.ErrorNumber++;
                                    //response = GamePlayServerResponse.InvalidMoveMessage("Hit robot is already damaged");
                                    break;
                                }
                            }
                            else if (_currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X + i] is Wall)
                            {
                                response = GamePlayServerResponse.OK();
                                break;

                            }
                            else
                                response = GamePlayServerResponse.OK();
                        }
                    }
                    break;

                case Directions.Down:
                    for (int i = 1; i <= 4; i++)
                    {
                        if (robot.RobotPosition.Y + i <= _currentMap.MapHeight - 1)
                        {
                            if (_currentMap.GlobalMap[robot.RobotPosition.Y + i, robot.RobotPosition.X] is Wall)
                            {
                                response = GamePlayServerResponse.OK();
                                _isWallShooted = true;
                                break;
                            }
                            hitRobot = _avatarDictionary.Values.ToList().Find(hr => hr.RobotPosition.X == robot.RobotPosition.X && hr.RobotPosition.Y == robot.RobotPosition.Y + i);
                            if (hitRobot != null)
                            {
                                _isRobotShooted = true;
                                if (hitRobot.HealthPoints >= 1)
                                {
                                    if (_hostileMode)
                                        hitRobot.HealthPoints = hitRobot.HealthPoints - 1;
                                    response = GamePlayServerResponse.OK();
                                    var woundedClient = _avatarDictionary.FirstOrDefault(wc => wc.Value == hitRobot).Key;
                                    this.response = GamePlayServerResponse.Wounded();
                                    woundedRobot = _avatarDictionary[woundedClient];
                                    woundedClient.reciveGamePlayData(_currentMap.getSmallerPartForRobot(_avatarDictionary[woundedClient].RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, woundedRobot.RobotPosition, woundedRobot.HasBigItem, woundedRobot.SmallItem, response));

                                    if (hitRobot.HealthPoints == 0)
                                    {
                                        hitRobot.IsHealed = false;
                                        hitRobot.HealRounds = healNumberRounds;
                                    }
                                    break;

                                }
                                else
                                {
                                    response = GamePlayServerResponse.OK();
                                    //robot.ErrorNumber++;
                                    //response = GamePlayServerResponse.InvalidMoveMessage("Hit robot is already damaged");
                                    break;
                                }
                            }
                            else if (_currentMap.GlobalMap[robot.RobotPosition.Y + i, robot.RobotPosition.X] is Wall)
                            {
                                response = GamePlayServerResponse.OK();
                                break;

                            }
                            else
                                response = GamePlayServerResponse.OK();
                        }
                    }
                    response = GamePlayServerResponse.OK();
                    break;

                case Directions.Left:
                    for (int i = 1; i <= 4; i++)
                    {
                        if (robot.RobotPosition.X - i >= 0)
                        {
                            if (_currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X - i] is Wall)
                            {
                                response = GamePlayServerResponse.OK();
                                _isWallShooted = true;
                                break;
                            }
                            hitRobot = _avatarDictionary.Values.ToList().Find(hr => hr.RobotPosition.X == robot.RobotPosition.X - i && hr.RobotPosition.Y == robot.RobotPosition.Y);
                            if (hitRobot != null)
                            {
                                _isRobotShooted = true;
                                if (hitRobot.HealthPoints >= 1)
                                {
                                    if (_hostileMode)
                                        hitRobot.HealthPoints = hitRobot.HealthPoints - 1;
                                    response = GamePlayServerResponse.OK();
                                    var woundedClient = _avatarDictionary.FirstOrDefault(wc => wc.Value == hitRobot).Key;
                                    this.response = GamePlayServerResponse.Wounded();
                                    woundedRobot = _avatarDictionary[woundedClient];
                                    woundedClient.reciveGamePlayData(_currentMap.getSmallerPartForRobot(_avatarDictionary[woundedClient].RobotPosition).SerializeMap(), new GamePlayServerResponse(_currentRound, woundedRobot.RobotPosition, woundedRobot.HasBigItem, woundedRobot.SmallItem, response));

                                    if (hitRobot.HealthPoints == 0)
                                    {
                                        hitRobot.IsHealed = false;
                                        hitRobot.HealRounds = healNumberRounds;
                                    }
                                    break;

                                }
                                else
                                {
                                    response = GamePlayServerResponse.OK();
                                    //robot.ErrorNumber++;
                                    //response = GamePlayServerResponse.InvalidMoveMessage("Hit robot is already damaged");
                                    break;
                                }
                            }
                            else if (_currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X - i] is Wall)
                            {
                                response = GamePlayServerResponse.OK();
                                break;

                            }
                            else
                                response = GamePlayServerResponse.OK();
                        }

                    }

                    break;
            }
            if (response.ServerState == ServerState.INVALID_MOVE)
            {
                Consequence = MoveConsequence.InvalidMove_ShotPlayerWithNoHealthPoints;
                MadeMove = MoveType.WrongAction;
                return this;
            }
            else
            {
                if (_isWallShooted)
                {
                    Consequence = MoveConsequence.ShotAndHitWall;
                    _isWallShooted = false;
                }
                else if (_isRobotShooted)
                {
                    Consequence = MoveConsequence.ShootAndNothingHappen;
                    _isRobotShooted = false;
                }
                else
                    Consequence = MoveConsequence.ShotAndHitPlayer;
                return this;
            }
        }
        public Move RestAction()
        {

            if (robot.IsHealed == false)
            {
                if (robot.HealRounds-- == 1) { robot.IsHealed = true; robot.HealthPoints = 5; };
                return this.BurnAction();
            }
            else
            {
                response = GamePlayServerResponse.OK();
                Consequence = MoveConsequence.Rest;
                if (response.ServerState == ServerState.INVALID_MOVE)
                {
                    MadeMove = MoveType.WrongAction;
                    return this;
                }
                else
                    return this;
            }
        }

        public Move WrongActionAction()
        {
            return this;
        }

        public Move DisconnectAction()
        {
            return new Move(MoveType.Disconnect, _currentMap, client, _avatarDictionary, robot, DirectionOfMove, _currentRound);
        }

        public Move BurnAction()
        {
            return new Move(MoveType.Burn, _currentMap, client, _avatarDictionary, robot, DirectionOfMove, _currentRound);
        }

        public static List<PossibleAction> GetPossibleActions(RobotAvatar robot, Map currentMap, List<RobotAvatar> LoggedRobots)
        {
            var RobotAction = new List<PossibleAction>();
            RobotAvatar hitRobot = null;
            bool canShoot = false;
            #region BURN
            //if (robot.IsHealed == false)
            //    return new List<PossibleAction>() { new PossibleAction(MoveType.Burn) };
            #endregion
            #region MOVE
            #region MOVE UP
            if (robot.RobotPosition.Y - 1 >= 1)
                if (currentMap.GlobalMap[robot.RobotPosition.Y - 1, robot.RobotPosition.X] is Floor &&
                   !LoggedRobots.Exists(r => r.RobotPosition.X == robot.RobotPosition.X && r.RobotPosition.Y == robot.RobotPosition.Y - 1))
                    RobotAction.Add(new PossibleAction(MoveType.MakeMove, Directions.Up));
            #endregion
            #region MOVE DOWN
            if (robot.RobotPosition.Y + 1 < currentMap.MapHeight - 1)
                if (currentMap.GlobalMap[robot.RobotPosition.Y + 1, robot.RobotPosition.X] is Floor &&
                    !LoggedRobots.Exists(r => r.RobotPosition.X == robot.RobotPosition.X && r.RobotPosition.Y == robot.RobotPosition.Y + 1))
                    RobotAction.Add(new PossibleAction(MoveType.MakeMove, Directions.Down));
            #endregion
            #region MOVE RIGHT
            if (robot.RobotPosition.X + 1 < currentMap.MapWidth - 1)
                if (currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X + 1] is Floor &&
                    !LoggedRobots.Exists(r => r.RobotPosition.X == robot.RobotPosition.X + 1 && r.RobotPosition.Y == robot.RobotPosition.Y))
                    RobotAction.Add(new PossibleAction(MoveType.MakeMove, Directions.Right));
            #endregion
            #region MOVE LEFT
            if (robot.RobotPosition.X - 1 >= 1)
                if (currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X - 1] is Floor &&
                    !LoggedRobots.Exists(r => r.RobotPosition.X == robot.RobotPosition.X - 1 && r.RobotPosition.Y == robot.RobotPosition.Y))
                    RobotAction.Add(new PossibleAction(MoveType.MakeMove, Directions.Left));
            #endregion
            #endregion
            #region PUNCH
            //#region PUNCH UP
            //if (robot.RobotPosition.Y - 1 >= 1)
            //{
            //    hitRobot = LoggedRobots.Find(hr => hr.RobotPosition.X == robot.RobotPosition.X && hr.RobotPosition.Y == robot.RobotPosition.Y - 1);
            //    if (hitRobot != null)
            //        //if (hitRobot.HealthPoints >= 1)
            //        RobotAction.Add(new PossibleAction(MoveType.Punch, Directions.Up));
            //}

            //#endregion
            //#region PUNCH RIGHT
            //if (robot.RobotPosition.X + 1 < +currentMap.MapWidth - 1)
            //{
            //    hitRobot = LoggedRobots.Find(hr => hr.RobotPosition.X == robot.RobotPosition.X + 1 && hr.RobotPosition.Y == robot.RobotPosition.Y);
            //    if (hitRobot != null)

            //        //if (hitRobot.HealthPoints >= 1)
            //        RobotAction.Add(new PossibleAction(MoveType.Punch, Directions.Right));
            //}
            //#endregion
            //#region PUNCH DOWN
            //if (robot.RobotPosition.Y + 1 < currentMap.MapHeight - 1)
            //{
            //    hitRobot = LoggedRobots.Find(hr => hr.RobotPosition.X == robot.RobotPosition.X && hr.RobotPosition.Y == robot.RobotPosition.Y + 1);
            //    if (hitRobot != null)

            //        //if (hitRobot.HealthPoints >= 1)
            //        RobotAction.Add(new PossibleAction(MoveType.Punch, Directions.Down));
            //}
            //#endregion
            //#region PUNCH LEFT
            //if (robot.RobotPosition.X - 1 >= 1)
            //{
            //    hitRobot = LoggedRobots.Find(hr => hr.RobotPosition.X == robot.RobotPosition.X - 1 && hr.RobotPosition.Y == robot.RobotPosition.Y);
            //    if (hitRobot != null)

            //        //if (hitRobot.HealthPoints >= 1)
            //        RobotAction.Add(new PossibleAction(MoveType.Punch, Directions.Left));
            //}
            //#endregion
            #endregion
            #region SHOOT
            #region SHOOT UP
            //for (int i = 1; i <= 4; i++)
            //{
            //    if (robot.RobotPosition.Y - i >= 0)
            //        if (currentMap.GlobalMap[robot.RobotPosition.Y - i, robot.RobotPosition.X] is Wall)
            //        {
            //            RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Up));
            //            break;
            //        }
            //        else
            //        {
            //            hitRobot = LoggedRobots.Find(hr => hr.RobotPosition.X == robot.RobotPosition.X && hr.RobotPosition.Y == robot.RobotPosition.Y - i);
            //            if (hitRobot != null)
            //            {
            //                RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Up));
            //                break;
            //            }
            //        }
            //    else
            //        break;


            //}
            RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Up));
            #endregion
            #region SHOOT RIGHT
            //for (int i = 1; i <= 4; i++)
            //{
            //    if (robot.RobotPosition.X + i >= currentMap.MapWidth)
            //        if (currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X + i] is Wall)
            //        {
            //            RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Right));
            //            break;
            //        }
            //        else
            //        {
            //            hitRobot = LoggedRobots.Find(hr => hr.RobotPosition.X == robot.RobotPosition.X + i && hr.RobotPosition.Y == robot.RobotPosition.Y);
            //            if (hitRobot != null)
            //            {
            //                RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Right));
            //                break;
            //            }
            //        }
            //    else
            //        break;
            //}
            RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Right));
            #endregion
            #region SHOOT DOWN
            //for (int i = 1; i <= 4; i++)
            //{
            //    if (robot.RobotPosition.Y + i >= 0)
            //        if (currentMap.GlobalMap[robot.RobotPosition.Y + i, robot.RobotPosition.X] is Wall)
            //        {
            //            RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Down));
            //            break;
            //        }
            //        else
            //        {
            //            hitRobot = LoggedRobots.Find(hr => hr.RobotPosition.X == robot.RobotPosition.X + i && hr.RobotPosition.Y == robot.RobotPosition.Y);
            //            if (hitRobot != null)
            //            {
            //                RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Down));
            //                break;
            //            }
            //        }
            //    else
            //        break;
            //}
            RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Down));
            #endregion
            #region SHOOT LEFT
            //for (int i = 1; i <= 4; i++)
            //{
            //    if (robot.RobotPosition.X - i >= 0)
            //        if (currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X - i] is Wall)
            //        {
            //            RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Left));
            //            break;
            //        }
            //        else
            //        {
            //            hitRobot = LoggedRobots.Find(hr => hr.RobotPosition.X == robot.RobotPosition.X - i && hr.RobotPosition.Y == robot.RobotPosition.Y);
            //            if (hitRobot != null)
            //            {
            //                RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Left));
            //                break;
            //            }
            //        }
            //    else
            //        break;
            //}
            RobotAction.Add(new PossibleAction(MoveType.Shoot, Directions.Left));
            #endregion
            #endregion
            #region PICK BIG ITEM
            if (!robot.HasBigItem && currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].HasBigItem)
                RobotAction.Add(new PossibleAction(MoveType.PickBigItem));
            #endregion
            #region DROP BIG ITEM
            if (robot.HasBigItem == true && currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].HasBigItem == false &&
                currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].SmallItemNumber == 0)
                RobotAction.Add(new PossibleAction(MoveType.DropBigItem));
            #endregion
            #region PICK SMALL ITEM
            if (currentMap.GlobalMap[robot.RobotPosition.Y, robot.RobotPosition.X].SmallItemNumber > 0)
                RobotAction.Add(new PossibleAction(MoveType.PickSmallItem));
            #endregion
            #region REST
            RobotAction.Add(new PossibleAction(MoveType.Rest));
            #endregion
            return RobotAction;
        }

    }
}