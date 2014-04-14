using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Clients_Library;
using Common_Library;
using Clients_Library.Infrastructure;
using System.Threading;
using Common_Library.Parts_of_map;
using Common_Library.Infrastructure;
using System.Reflection;
using Logger;


namespace GamePlay_Preview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ClientsGamePreview : Window
    {
        #region Private
        private Map _currentMap;
        private string imagePath;
        private string _login;
        private List<ActionHistory> _history;
        private Position _myPosition;
        private bool _myBigItem = false;
        private int _mySmallItems = 0;
        private int _currentRoundNumber;
        private int _allRoundsNumber;
        private string _myColor;
        private bool _canMakeMove = false;
        ActionHistory movesVisualizationHistory;
        private Position _startingPosition;
        private Map _initialMap;
        private ClientsLibrary _cl;
        private List<PossibleAction> _possibleMoveList;
        private int _healthPoints = 4;
        private object m_SyncObject = new object();
        private bool _isGameRunning = false;
        private double _myScore = 0;
        private MapSize _globalMapSize;
        private double _roundTime;
        private string _myTeam;
        private string logFilePath;
        #endregion

        /// <summary>
        /// Class properties with all valid information about the state of the game and user
        /// </summary>
        #region Properties



        public Map CurrentMap
        {
            get { return _currentMap; }
            set { _currentMap = value; }
        }

        public string Login
        {
            get { return _login; }
            set { _login = value; }
        }
        /// <summary>
        /// Global user history in which are logged:
        /// user action
        /// 
        /// </summary>
        public List<ActionHistory> History
        {
            get { return _history; }
            set { _history = value; }
        }

        public Position MyPosition
        {
            get { return _myPosition; }
            set { _myPosition = value; }
        }


        public bool MyBigItem
        {
            get { return _myBigItem; }
            set { _myBigItem = value; }
        }

        public int MySmallItems
        {
            get { return _mySmallItems; }
            set { _mySmallItems = value; }
        }

        public int CurrentRoundNumber
        {
            get { return _currentRoundNumber; }
            set { _currentRoundNumber = value; }
        }

        public int AllRoundsNumber
        {
            get { return _allRoundsNumber; }
            set { _allRoundsNumber = value; }
        }

        public ClientsLibrary Cl
        {
            get { return _cl; }
            set { _cl = value; }
        }

        public List<PossibleAction> PossibleMoveList
        {
            get { return _possibleMoveList; }
            set { _possibleMoveList = value; }
        }

        public int HealthPoints
        {
            get { return _healthPoints; }
            set { _healthPoints = value; }
        }

        public MapSize GlobalMapSize
        {
            get { return _globalMapSize; }
            set { _globalMapSize = value; }
        }

        public double RoundTime
        {
            get { return _roundTime; }
            set { _roundTime = value; }
        }

        public string MyTeam
        {
            get { return _myTeam; }
            set { _myTeam = value; }
        }

        #endregion

        /// <summary>
        /// Constructor which gets ip adress of the server and login name, creates ClientsLibrary class with ip address.
        /// Creates log file
        /// Hooks up all events from Clients Library
        /// Creates history
        /// Invoke Login method on Clients Library
        /// </summary>
        /// <param name="ipAddress"> Arena Server adress provided in the form XXX.XXX.XXX.XXX </param>
        /// <param name="login">Robot login name provided by the client</param>
        public ClientsGamePreview(string ipAddress, string login)
        {
            InitializeComponent();
            this.MouseDown += delegate { DragMove(); };
            var localPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var pos = localPath.LastIndexOf(@"\") + 1;
            logFilePath = localPath.Substring(0, pos) + "Client_" + login + "_Log.txt";


            EventLog.WriteMessageToLog(logFilePath, "User " + login + " trying to connect to server on " + ipAddress);
            _cl = new ClientsLibrary(ipAddress);

            _cl.InitialMessageReciveEvent += ClientsLibrary_InitialMessageReciveEvent;
            _cl.GamePlayMessageReciveEvent += cl_GamePlayMessageReciveEvent;
            _cl.RoundEndEvent += cl_RoundEndEvent;
            _cl.RoundStartEvent += cl_RoundStartEvent;
            _cl.GameEndEvent += cl_GameEndEvent;
            _cl.GameStartEvent += cl_GameStartEvent;

            _history = new List<ActionHistory>();
            _login = login;
            _cl.Login(_login);
            EventLog.WriteMessageToLog(logFilePath, "User " + login + " trying to log to game");


        }

        /// <summary>
        /// Overide this method to create own robot
        /// 
        /// Currently implemented "Dummy" robot which perform random action with priority to Punch and Shoot action
        /// </summary>
        public void PlayGame()
        {
            while (_isGameRunning && _currentRoundNumber != _allRoundsNumber)
            {
                if (_canMakeMove)
                {
                    Random r = new Random(Guid.NewGuid().GetHashCode());
                    var c = r.Next(0, _possibleMoveList.Count);
                    try
                    {
                        
                       
                            InvokeAction(_possibleMoveList[c], _currentRoundNumber);
                            EventLog.WriteMessageToLog(logFilePath, "User invokes action: " + _possibleMoveList[c].Action.ToString() + " with direction : " + _possibleMoveList[c].MoveDirection.ToString());
                        
                    }
                    catch (InvalidOperationException e)
                    {
                        InvokeAction(_possibleMoveList[c], _currentRoundNumber);

                    }

                }
            }
        }

        /// <summary>
        /// Invoke action method invokes actions on Clients Library object
        /// </summary>
        /// <param name="action"> Performed action - name of the action + direction of the move</param>
        /// <param name="currentRound"> Number of current round</param>
        void InvokeAction(PossibleAction action, int currentRound)
        {
            _history.Last().MadeMove = action.Action;
            MethodInfo method = _cl.GetType().GetMethod(action.Action.ToString()); // INVOKE ALL ACTIONS IN QUEUE
            if (method.GetParameters().Count() == 2)
                method.Invoke(_cl, new object[] { action.MoveDirection, _currentRoundNumber });
            else
                method.Invoke(_cl, new object[] { _currentRoundNumber });
            _canMakeMove = false;
        }

        #region Events

        /// <summary>
        /// Message send by server at the beging of each game.
        /// Client can start his logic in Play Game method
        /// </summary>
        /// <param name="oSender">Arena Server object</param>
        /// <param name="oEventArgs"> Initial round number</param>
        void cl_GameStartEvent(object oSender, GamePlayArgs oEventArgs)
        {
            _allRoundsNumber = oEventArgs.initalRoundNumber;
            _currentMap = oEventArgs.map;
            _initialMap = oEventArgs.map;
            _myScore = 0;
            _currentRoundNumber = 0;
            _myPosition = _startingPosition;
            _currentMap = _initialMap.Copy();
            _myTeam = oEventArgs.team;
            _isGameRunning = true;
            EventLog.WriteMessageToLog(logFilePath, "Game is starting!");
            Dispatcher.Invoke(() => repaintMap(_startingPosition));

            PlayGame();
        }

        /// <summary>
        /// Message send at the end of the game
        /// </summary>
        /// <param name="oSender"></param>
        /// <param name="oEventArgs"></param>
        void cl_GameEndEvent(object oSender, GamePlayArgs oEventArgs)
        {
            EventLog.WriteMessageToLog(logFilePath, "Game is ending!");
            _isGameRunning = false;
            _canMakeMove = false;
            MessageBox.Show("My final Score: " + oEventArgs.finalScore.ToString());
        }

        /// <summary>
        /// Message send at the begining of each round. Client recive current round number which has to be resend with his action.
        /// Client also recive possible move list in which are stored actions available for current robot position.
        /// Flag canMakeMove is set to true - user can perform action
        /// </summary>
        /// <param name="oSender"></param>
        /// <param name="oEventArgs"> Round number, possibleMoveList</param>
        void cl_RoundStartEvent(object oSender, GameRoundArgs oEventArgs)
        {

            _currentRoundNumber = oEventArgs.RoundNumber;
            EventLog.WriteMessageToLog(logFilePath, "ROUND nr " + _currentRoundNumber.ToString() + "  is starting!");
            _possibleMoveList = oEventArgs.possibleActionList;
            _history.Add(new ActionHistory(this.Login, _currentRoundNumber, _possibleMoveList));
            _canMakeMove = true;
        }

        /// <summary>
        /// Round end message. Flag canMakeMove is set to false
        /// </summary>
        /// <param name="oSender"></param>
        /// <param name="oEventArgs"></param>
        void cl_RoundEndEvent(object oSender, GameRoundArgs oEventArgs)
        {
            _currentRoundNumber = oEventArgs.RoundNumber;
            EventLog.WriteMessageToLog(logFilePath, "ROUND nr " + _currentRoundNumber.ToString() + "  is ending!");
            _canMakeMove = false;
        }

        /// <summary>
        /// Message sent as a response to clients move. It contains information about the robot- his current position, current state,
        /// health condition, consequence of the move etc.
        /// It carries also information about the current pay and total pay of all robots in this round.
        /// In case of invalid move or timeout, Message invalid is sent to the client 
        /// </summary>
        /// <param name="oSender"></param>
        /// <param name="oEventArgs">
        /// </param>
        void cl_GamePlayMessageReciveEvent(object oSender, MessageRecivedArgs oEventArgs)
        {
            Position _myLocalPosition = null;
            lock (m_SyncObject)
            {
                _currentMap = oEventArgs.Map;
                _myLocalPosition = oEventArgs.response.MyPosition;
            }
            _myBigItem = oEventArgs.response.HasBigItem;
            _mySmallItems = oEventArgs.response.SmallItemNumber;
            _history.Last().Consequence = oEventArgs.response.Consequence;
            _history.Last().MyCurrentPay = oEventArgs.response.MyCurrentPay;
            _history.Last().MyTotalPay = oEventArgs.response.TotalPay;
            _myScore += oEventArgs.response.MyCurrentPay;
            if (oEventArgs.response.ServerState == ServerState.WOUNDED)
                this.HealthPoints--;
            if (oEventArgs.response.ServerState == ServerState.HEALED)
                this.HealthPoints = 5; 
            EventLog.WriteMessageToLog(logFilePath, "Game Message recived with: current round: " + _currentRoundNumber.ToString() + "; local position: " + _myLocalPosition.ToString() +
                "; has big item: " + _myBigItem.ToString() + "; small item number " + _mySmallItems.ToString() + "; Consequence of move: " + oEventArgs.response.Consequence.ToString() +
                "; my current pay: " + oEventArgs.response.MyCurrentPay.ToString() + "; total pay: " + oEventArgs.response.TotalPay.ToString());


            Dispatcher.Invoke(() => roundTextBox.Text = (oEventArgs.response.RoundNumber + 1).ToString());
            Dispatcher.Invoke(() => scoreTextBox.Text = _myScore.ToString());
            Dispatcher.Invoke(() => MessageTextBox.Text = oEventArgs.response.Message);
            lock (m_SyncObject)
                Dispatcher.Invoke(() => repaintMap(_myLocalPosition));
            //Thread.Sleep(new TimeSpan(0,0,2));
        }

        /// <summary>
        /// Message send to the client after login to the server. Contains welcome package for client: map size, starting position
        /// initial color of the robot, round time.
        /// </summary>
        /// <param name="oSender"></param>
        /// <param name="Args"></param>
        void ClientsLibrary_InitialMessageReciveEvent(object oSender, InitialMessageRecivedArgs Args)
        {
            Position _myLocalPosition = null;

            _startingPosition = Args.response.MyPosition;
            _myLocalPosition = Args.response.MyPosition;
            _myColor = Args.response.Color;
            _roundTime = Args.response.RoundTime;
            _globalMapSize = Args.response.GlobalMapSize;
            if (_myLocalPosition != null)
                EventLog.WriteMessageToLog(logFilePath, "Initial Message recived with local position: " + _myLocalPosition.ToString());
            Dispatcher.Invoke(() => changeWindowClientColor(_myColor));
            Dispatcher.Invoke(() => roundTextBox.Text = Args.response.RoundNumber.ToString());
            Dispatcher.Invoke(() => scoreTextBox.Text = "0");
            Dispatcher.Invoke(() => MessageTextBox.Text = Args.response.Message);

        }
        #endregion

        #region Map methods

        /// <summary>
        /// Repaints map for the client each time client recives a new map.
        /// </summary>
        /// <param name="_myLocalPosition"></param>
        private void repaintMap(Position _myLocalPosition)
        {
            if (_currentMap == null && _currentMap.MapWidth == 0 || _currentMap.MapHeight == 0)
                return;
            squareGrid.Children.Clear();

            if (_history != null && _history.Count > 0 && _history.Last().MadeMove != MoveType.NULL)
                movesVisualizationHistory = _history.Last();


            for (int i = 0; i < _currentMap.MapHeight; i++)
                for (int j = 0; j < _currentMap.MapWidth; j++)
                {
                    if (_currentMap.GlobalMap[i, j] is Wall)
                    {
                        BrushConverter conv = new BrushConverter();
                        SolidColorBrush brush;
                        if (_currentMap.GlobalMap[i, j] != null && _currentMap.GlobalMap[i, j].Color != "Default")
                            brush = conv.ConvertFromString(_currentMap.GlobalMap[i, j].Color) as SolidColorBrush;
                        else
                            brush = Brushes.Transparent;
                        Rectangle rect = new Rectangle();
                        rect.Fill = brush;
                        squareGrid.Children.Add(rect);
                        Grid.SetColumn(rect, j);
                        Grid.SetRow(rect, i);
                        Grid.SetZIndex(rect, 0);

                        var wall = new StackPanel();
                        squareGrid.Children.Add(wall);
                        Grid.SetColumn(wall, j);
                        Grid.SetRow(wall, i);
                        Grid.SetZIndex(wall, 1);
                    }
                    else if (_currentMap.GlobalMap[i, j] is Floor)
                    {
                        string assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                        StackPanel wall = new StackPanel();
                        if (_currentMap.GlobalMap[i, j].Robot != null)
                        {
                            Image img = new Image();

                            # region MyColorSwitch
                            if (_currentMap.GlobalMap[i, j].Robot.RobotColor != null && _currentMap.GlobalMap[i, j].Robot.RobotColor != "default")
                            {
                                var color = _currentMap.GlobalMap[i, j].Robot.RobotColor;
                                if (_currentMap.GlobalMap[i, j].Robot.RobotPosition.X == _myLocalPosition.X && _currentMap.GlobalMap[i, j].Robot.RobotPosition.Y == _myLocalPosition.Y)
                                {
                                    if (movesVisualizationHistory != null)
                                    {
                                        switch (movesVisualizationHistory.MadeMove)
                                        {
                                            case MoveType.Burn:
                                                imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_stuck.png";
                                                break;
                                            case MoveType.WrongAction:
                                                imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_invalid.png";
                                                break;
                                            case MoveType.Rest:
                                                imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_rest.png";
                                                break;
                                            case MoveType.Shoot:
                                                imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_shoot.png";
                                                break;
                                            case MoveType.MakeMove:
                                                imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_standing.png";
                                                break;
                                            case MoveType.DropBigItem:
                                                imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_take.png";
                                                break;
                                            case MoveType.PickBigItem:
                                                imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_take.png";
                                                break;
                                            case MoveType.PickSmallItem:
                                                imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_take.png";
                                                break;
                                            //case MoveType.Punch:
                                            //    imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_standing.png";
                                            //    break;
                                        }
                                    }
                                    else
                                        imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_standing.png";
                                }
                                else
                                {
                                    imagePath = "pack://application:,,,/" + assembly + ";component/Resources/guard64_" + color + "_standing.png";
                                }
                            }
                            else
                            {
                                imagePath = "pack://application:,,,/" + assembly + ";component/Resources/SingleRobot.png";
                                img.Source = new BitmapImage(new Uri(imagePath));
                                wall.Children.Add(img);
                                StackPanel.SetZIndex(img, 0);
                            }



                            img.Source = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
                            wall.Children.Add(img);
                            StackPanel.SetZIndex(img, 0);
                            #endregion

                        }
                        else if (_currentMap.GlobalMap[i, j].HasBigItem)
                        {
                            imagePath = "Images/big_item.png";
                            Image img = new Image();
                            img.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));
                            wall.Children.Add(img);
                        }
                        else if (_currentMap.GlobalMap[i, j].SmallItemNumber > 0)
                        {
                            imagePath = "Images/small_item.png";
                            Image img = new Image();
                            img.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));
                            wall.Children.Add(img);
                        }

                        //else if (_currentMap.GlobalMap[i, j].IsStartingPosition)
                        //{
                        //    imagePath = "Images/starting_position.png";
                        //    Image img2 = new Image();
                        //    img2.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));
                        //    wall.Children.Add(img2);
                        //    StackPanel.SetZIndex(img2, 1);

                        //}

                        BrushConverter conv = new BrushConverter();
                        SolidColorBrush brush;
                        if (_currentMap.GlobalMap[i, j] != null && _currentMap.GlobalMap[i, j].Color != "Default")
                            brush = conv.ConvertFromString(_currentMap.GlobalMap[i, j].Color) as SolidColorBrush;
                        else
                            brush = Brushes.Transparent;
                        Rectangle rect = new Rectangle();
                        rect.Fill = brush;
                        squareGrid.Children.Add(rect);
                        Grid.SetColumn(rect, j);
                        Grid.SetRow(rect, i);
                        Grid.SetZIndex(rect, 0);

                        wall.Style = (Style)FindResource("FloorStyle");
                        squareGrid.Children.Add(wall);
                        Grid.SetColumn(wall, j);
                        Grid.SetRow(wall, i);
                        Grid.SetZIndex(wall, 1);
                    }
                    else if (_currentMap.GlobalMap[i, j] == null)
                    {
                        Rectangle rect = new Rectangle();
                        rect.Fill = Brushes.Black;
                        // resizeMap(currentMap);
                        squareGrid.Children.Add(rect);
                        Grid.SetColumn(rect, j);
                        Grid.SetRow(rect, i);
                        Grid.SetZIndex(rect, 0);
                    }


                }

        }
        #endregion

        public void changeWindowClientColor(string clientColor)
        {
            if (clientColor != null && clientColor != "")
            {
                if (clientColor != "default")
                {
                    switch (clientColor)
                    {
                        case "blue":
                            colorLabel.Background = Brushes.White;
                            colorLabel.Content = clientColor;
                            colorLabel.Foreground = Brushes.SteelBlue;
                            break;
                        case "brown":
                            colorLabel.Background = Brushes.White;
                            colorLabel.Content = clientColor;
                            colorLabel.Foreground = Brushes.Sienna;
                            break;
                        case "gold":
                            colorLabel.Background = Brushes.White;
                            colorLabel.Content = clientColor;
                            colorLabel.Foreground = Brushes.Goldenrod;
                            break;
                        case "green":
                            colorLabel.Background = Brushes.White;
                            colorLabel.Content = clientColor;
                            colorLabel.Foreground = Brushes.LimeGreen;
                            break;
                        case "pink":
                            colorLabel.Background = Brushes.White;
                            colorLabel.Content = clientColor;
                            colorLabel.Foreground = Brushes.HotPink;
                            break;
                        case "red":
                            colorLabel.Background = Brushes.White;
                            colorLabel.Content = clientColor;
                            colorLabel.Foreground = Brushes.Crimson;
                            break;
                        case "violet":
                            colorLabel.Background = Brushes.White;
                            colorLabel.Content = clientColor;
                            colorLabel.Foreground = Brushes.Indigo;
                            break;
                        case "white":
                            colorLabel.Content = clientColor;
                            colorLabel.Background = Brushes.DarkGray;
                            colorLabel.Foreground = Brushes.White;
                            break;
                        case "silver":
                            colorLabel.Background = Brushes.White;
                            colorLabel.Content = clientColor;
                            colorLabel.Foreground = Brushes.DarkGray;
                            break;
                    }

                }
                else
                {
                    colorLabel.Background = Brushes.White;
                    colorLabel.Content = "blue";
                    colorLabel.Foreground = Brushes.Blue;
                }
            }

        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }

            catch (Exception) { }
        }

    }
}
