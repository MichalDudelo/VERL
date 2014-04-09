using Autofac;
using Common_Library.IService_Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Description;
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
using Map_Editor;
using Arena_Server.Infrastructure;
using Common_Library.Infrastructure;
using System.Xml;
using Autofac.Integration.Wcf;
using Arena_Server.Services;
using TileEngine;
using System.Threading;
using System.Reflection;
using ArenaServer.Infrastructure.EventArgs;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Globalization;
using Microsoft.Win32;
using Common_Library.Parts_of_map;
namespace Common_Library
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Map currentMap;

        public Map CurrentMap
        {
            get { return currentMap; }
            set { currentMap = value; }
        }
        String imagePath;
        int roundNumber = 100;
        int gameSecondTime;
        int gameMinuteTime;
        int numberOfTeams = 0;
        bool hostileMode = true;
        bool extendedRoundTime = false;
        bool endNRoundCriteria = true;
        bool _isGameRunning = false;
        bool _isGameStarted = false;
        bool _isPaymentModuleLoaded = false;
        bool _isMapLoaded = false;
        double _turnTime = 0.5;

        public ObservableCollection<string> teamList = new ObservableCollection<string>();


        public delegate void StartServerDelegate(object oSender, ServerStartArgs startServerArgs);
        public static event StartServerDelegate StartServerEvent;

        public delegate void EndGameDelegate(object oSender, EventArgs Args);
        public static event EndGameDelegate EndGameEvent;

        public delegate void StopServerDelegate(object oSender, EventArgs Args);
        public static event StopServerDelegate StopServerEvent;

        public delegate void ResumeServerDelegate(object oSender, EventArgs Args);
        public static event ResumeServerDelegate ResumeServerEvent;


        public delegate void MapChangeDelegate(object oSender, MapChangeArgs mapChangeArgs);
        public static event MapChangeDelegate MapChangeEvent;

        public delegate void ScoreModuleChangeDelegate(object oSender, ScoreModuleChangeArgs scoreModulChangeArgs);
        public static event ScoreModuleChangeDelegate ScoreModuleChangeEvent;

        private int _numberOfRobotsLogged = 0;
        private List<RobotAvatar> robotAvatarList = new List<RobotAvatar>();

        public List<RobotAvatar> RobotAvatarList
        {
            get { return robotAvatarList; }
            set { robotAvatarList = value; }
        }

        private Task GameXNAthread;

        private int _initialStartingPositionNumber;

        private Map initialMap;
        const int SIDE = 80;
        const int SIZE25 = 550;
        const int SIDE25 = 22;

        public MainWindow()
        {
            InitializeComponent();
            roundNumberTextBox.MaxLength = 4;
            totalTimeTextBox2.MaxLength = 2;
            totalTimeTextBox1.MaxLength = 2;

            teamList.Add("0");
            teamList.Add("2");
            teamList.Add("4");
            teamNumberComboBox.ItemsSource = teamList;
            teamNumberComboBox.SelectedIndex = 0;

            hostileModeRadioButton.IsChecked = true;
            InitializeServer();

            mainWindow.Title = "My IP address: " + LocalIPAddress().ToString();
            Arena_Server.Services.GamePlayService.PlayerLoginEvent += LoginService_PlayerLoginEvent;
            Arena_Server.Services.GamePlayService.PlayerDisconnectEvent += GamePlayService_PlayerDisconnectEvent;
            RoundTimeTextBox.Text = "0.5";
            Game1.EndGameEvent += Game1_EndGameEvent;

        }

        void Game1_EndGameEvent(object oSender, EventArgs Args)
        {
            if (robotAvatarList.Count == 0)
              Dispatcher.Invoke(() => startServerButton.IsEnabled = false);

            ResetGamePlayState();
        }

        void GamePlayService_PlayerDisconnectEvent(object oSender, PlayerLoginArgs playerLoginArgs)
        {
            robotAvatarList.Remove(playerLoginArgs.RobotAvatar);
            Dispatcher.Invoke(() => LoggedRobotsListView.Items.Remove(playerLoginArgs.RobotAvatar.Login));

            _numberOfRobotsLogged--;
        }



        private void LoginService_PlayerLoginEvent(object oSender, PlayerLoginArgs oEventArgs)
        {
            robotAvatarList.Add(oEventArgs.RobotAvatar);
            Dispatcher.Invoke(() => LoggedRobotsListView.Items.Add(oEventArgs.RobotAvatar.Login));
            _numberOfRobotsLogged++;
            if (_isMapLoaded && _isPaymentModuleLoaded)
                Dispatcher.Invoke(() => startServerButton.IsEnabled = true);

        }



        private IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        public void InitializeServer()
        {
            NetTcpBinding binding = new NetTcpBinding();
            EndpointAddress endpointGameplay = new EndpointAddress("net.tcp://localhost:8733/GamePlayService/");

            binding.Security.Mode = SecurityMode.None;

            binding.OpenTimeout = new TimeSpan(0, 0, 2);
            binding.CloseTimeout = new TimeSpan(0, 0, 1);
            binding.SendTimeout = new TimeSpan(0, 0, 1);
            binding.ReceiveTimeout = new TimeSpan(0, 5, 0);
            // binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
            var builder = new ContainerBuilder();

            builder.Register(c => new Dictionary<IArenaCallback, RobotAvatar>()).SingleInstance();
            builder.Register(c => new GamePlayService(c.Resolve<Dictionary<IArenaCallback, RobotAvatar>>())).As<IGamePlayService>().SingleInstance();
            var container = builder.Build();


            //Create a URI to serve as the base address
            Uri httpUrl = new Uri("net.tcp://localhost:8733/GamePlayService/");
            //Create ServiceHost
            ServiceHost host
            = new ServiceHost(container.Resolve<IGamePlayService>(), httpUrl);
            //Add a service endpoint

            host.AddServiceEndpoint(typeof(Common_Library.IService_Contracts.IGamePlayService)
            , binding, "");
            host.AddDependencyInjectionBehavior<IGamePlayService>(container);

            ////Enable metadata exchange
            //ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            //smb.HttpGetEnabled = true;

            //host.Description.Behaviors.Add(smb);
            //Start the Service
            host.Open();






        }


        public void loadMapButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "Map"; // Default file name

            dlg.Filter = "Binary map format (.map)|*.map"; // Filter files by extension 

            // Display OpenFileDialog by calling ShowDialog method

            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox

            if (result == true)
            {
                try
                {
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    currentMap = (Map)formatter.Deserialize(stream);
                    initialMap = currentMap.Copy();
                    previewMap();
                    stream.Close();
                    mapSizeTextBox1.Text = currentMap.MapWidth.ToString();
                    mapSizeTextBox2.Text = currentMap.MapHeight.ToString();
                    NumberOfRobotsTextBox.Text = currentMap.StartingPositionNumber.ToString();
                    _initialStartingPositionNumber = currentMap.StartingPositionNumber;
                    MapChangeEvent(this, new MapChangeArgs(currentMap));
                    _isMapLoaded = true;
                    LoadMapLabel.Content = System.IO.Path.GetFileName(dlg.FileName.ToString());
                    LoadMapLabel.Background = Brushes.YellowGreen;

                    editMapButton.IsEnabled = true;

                    if (_isMapLoaded && _isPaymentModuleLoaded && _numberOfRobotsLogged > 0)
                        startServerButton.IsEnabled = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Map loading error. Try another Map!");
                }
            }

        }

        private List<IScoreModule> getModules(string p)
        {
            var asm = //Assembly.LoadFrom(p);
            Assembly.LoadFile(p);
            try
            {
                var types = asm.GetTypes().Where(z => z.GetInterface("IScoreModule") != null);
                var modules = new List<IScoreModule>(types.Count());
                foreach (var t in types)
                {
                    var sm = Activator.CreateInstance(t) as IScoreModule;
                    if (sm != null)
                        modules.Add(sm);
                }
                return modules;
            }
            catch (Exception)
            { }

            return null;
        }

        private void previewMap()
        {
            if (currentMap == null && (currentMap.MapWidth == 0 || currentMap.MapHeight == 0))
                return;

            squareGrid.Children.Clear();
            double fraction = 1;
            double old_height = 25;
            double old_width = 25;
            double old_side = squareGrid.SquareSideLength;
            double fraction_height = (old_height / currentMap.MapHeight);
            double fraction_width = (old_width / currentMap.MapWidth);

            if (currentMap.MapHeight > 25 || currentMap.MapWidth > 25)
            {
                if (currentMap.MapHeight > currentMap.MapWidth)
                {
                    fraction = (double)currentMap.MapHeight / 25;
                }
                else
                {
                    fraction = (double)currentMap.MapWidth / 25;
                }

                double side = squareGrid.SquareSideLength;
                double size = side / fraction;
                if (size < 20) size = 20;
                double _height = loadedMapGrid.Height * fraction;
                double _width = loadedMapGrid.Width * fraction;
                double constant = 1;

                double new_height = currentMap.MapHeight * size;
                double new_width = currentMap.MapWidth * size;

                if (new_height > new_width)
                {
                    constant = _height - new_height;
                    new_height = _height - constant;

                }
                else if (new_height == new_width)
                {
                    constant = _height - new_height;
                    new_height = _height - constant;
                    new_width = _width - constant;
                }
                else
                {
                    constant = _width - new_width;
                    new_width = _width - constant;

                }

                loadedMapGrid.Height = new_height;
                loadedMapGrid.Width = new_width;
                squareGrid.SquareSideLength = size;
            }
            else
            {


                if (fraction_width > fraction_height) fraction = fraction_width;
                else fraction = fraction_height;

                double size = old_side * fraction;
                int side = (int)size;
                if (side > SIDE) side = SIDE;
                squareGrid.SquareSideLength = side;
            }
            if (currentMap.MapHeight == 25 || currentMap.MapWidth == 25)
            {
                squareGrid.SquareSideLength = SIDE25;
                loadedMapGrid.Height = SIZE25;
                loadedMapGrid.Width = SIZE25;
            }





            for (int i = 0; i < currentMap.MapHeight; i++)
                for (int j = 0; j < currentMap.MapWidth; j++)
                {
                    if (currentMap.GlobalMap[i, j] is Wall)
                    {
                        BrushConverter conv = new BrushConverter();
                        SolidColorBrush brush;
                        if (currentMap.GlobalMap[i, j].Color != "Default")
                            brush = conv.ConvertFromString(currentMap.GlobalMap[i, j].Color) as SolidColorBrush;
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
                        //currentMap.WallList.Add(new Wall());
                        Grid.SetColumn(wall, j);
                        Grid.SetRow(wall, i);
                        Grid.SetZIndex(wall, 1);
                    }
                    else if (currentMap.GlobalMap[i, j] is Floor)
                    {

                        StackPanel wall = new StackPanel();
                        if (currentMap.GlobalMap[i, j].HasBigItem)
                        {
                            imagePath = "Images/big_item.png";
                            Image img = new Image();
                            // currentMap.FloorList.Add(new Floor());
                            img.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));
                            wall.Children.Add(img);
                        }
                        else if (currentMap.GlobalMap[i, j].SmallItemNumber > 0)
                        {
                            imagePath = "Images/small_item.png";
                            Image img = new Image();
                            img.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));
                            wall.Children.Add(img);
                            // currentMap.SmallItemList.Add(new SmallItem());
                        }
                        else if (currentMap.GlobalMap[i, j].IsStartingPosition)
                        {
                            imagePath = "Images/starting_position.png";
                            Image img = new Image();
                            img.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));
                            //currentMap.StartingPositionNumber++;
                            // currentMap.StartingPositionList.Add(new Common_Library.Parts_of_map.StartingPosition(j, i));
                            wall.Children.Add(img);
                        }

                        BrushConverter conv = new BrushConverter();
                        SolidColorBrush brush;
                        if (currentMap.GlobalMap[i, j].Color != "Default")
                            brush = conv.ConvertFromString(currentMap.GlobalMap[i, j].Color) as SolidColorBrush;
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

                }
        }

        private void editMapButton_Click(object sender, RoutedEventArgs e)
        {
            Map_Editor.MainWindow editor = new Map_Editor.MainWindow(currentMap);
            editor.Show();

        }



        private void ResetGamePlayState()
        {

            foreach (var sp in currentMap.StartingPositionList)
                sp.Used = false;

            EndGameEvent(this, null);
            Dispatcher.Invoke(() => startServerButton.Content = "Start New Game");

            currentMap = initialMap.Copy();

            _isGameRunning = false;
            _isGameStarted = false;
            foreach (var robot in robotAvatarList)
                robot.Reset();


        }


        public void startServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isGameStarted == false)
            {

                if (currentMap.StartingPositionNumber >= _numberOfRobotsLogged)
                {

                    Game1 game;
                    var GamePlayThread = new Thread(new ThreadStart(() =>
                    {
                        try { game = new Game1(currentMap, robotAvatarList, _turnTime, roundNumber); game.Run(); }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Cannot start XNA! Error : \n" + ex.ToString());
                        }
                    }));

                    GamePlayThread.Start();

                    // GameXNAthread = Task.Factory.StartNew(() => ThreadStart());
                    Thread.Sleep(2000);
                    StartServerEvent(this, new ServerStartArgs(this.currentMap, this.roundNumber, this.hostileMode, this._turnTime, this.gameSecondTime, this.gameMinuteTime, this.endNRoundCriteria, this.numberOfTeams));
                    //  var contTask = GameXNAthread.ContinueWith((cont) => { ResetGamePlayState(); });


                    _isGameStarted = true;
                    _isGameRunning = true;
                    startServerButton.Content = "Pause Game";

                }
                else
                    MessageBox.Show("Too many robots logged for that map");
            }
            else if (_isGameRunning)
            {
                StopServerEvent(this, null);
                _isGameRunning = false;
                startServerButton.Content = "Resume Game";
            }
            else if (!_isGameRunning)
            {
                ResumeServerEvent(this, null);
                _isGameRunning = true;
                startServerButton.Content = "Pause Game";
            }
            //ServerVisualizationWindow arena = new ServerVisualizationWindow(this.currentMap);
            // arena.Show();



        }

        private void roundNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(roundNumberTextBox.Text, @"^([1-9][0-9]{0,2}|1000)$"))
            {
                if (roundNumberTextBox.Text != null && roundNumberTextBox.Text != "" && int.Parse(roundNumberTextBox.Text) >= 1 && int.Parse(roundNumberTextBox.Text) <= 1000)
                {
                    roundNumber = int.Parse(roundNumberTextBox.Text);


                    gameMinuteTime = (int)(roundNumber * _turnTime / 60);
                    gameSecondTime = (int)(roundNumber * _turnTime % 60);

                    totalTimeTextBox1.Text = gameMinuteTime.ToString();
                    totalTimeTextBox2.Text = gameSecondTime.ToString();



                }
                /*else if (int.Parse(roundNumberTextBox.Text) < 100 || int.Parse(roundNumberTextBox.Text)>1000)
                {
                    MessageBox.Show("Number of rounds should be from interval: [100,1000]", "Invalid number of rounds", MessageBoxButton.OK, MessageBoxImage.Error);
                   
                }*/

            }
        }






        private void hostileModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            hostileMode = true;
        }

        private void friendlyModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            hostileMode = false;
        }


        public void loadPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "ScoreModule"; // Default file name

            dlg.Filter = "Score module library (.dll)|*.dll| All files (*.*)|*.*"; // Filter files by extension 

            // Display OpenFileDialog by calling ShowDialog method

            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox

            if (result == true)
            {
                var modules = getModules(dlg.FileName);
                ScoreModuleSelectWindow scoreModuleWindow = new ScoreModuleSelectWindow(modules);
                scoreModuleWindow.ShowDialog();
                if (scoreModuleWindow.result == true)
                {
                    var module = modules.Find(m => m.ToString().Equals(scoreModuleWindow.SelectedModule));
                    ScoreModuleChangeEvent(this, new ScoreModuleChangeArgs(module));
                    LoadPaymentModuleLabel.Content = scoreModuleWindow.SelectedModule.ToString();
                    LoadPaymentModuleLabel.Background = Brushes.YellowGreen;
                    _isPaymentModuleLoaded = true;

                    if (_isMapLoaded && _isPaymentModuleLoaded && _numberOfRobotsLogged > 0)
                        startServerButton.IsEnabled = true;
                }

                // MapChangeEvent(this, new MapChangeArgs(currentMap));
            }
        }

        private void roundNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(roundNumberTextBox.Text, @"^([1-9][0-9]{0,2}|1000)$") == false)
            {
                MessageBoxResult errMsg = MessageBox.Show("Invalid Round number: must be an integer number from interval 1 - 1000", "Invalid Round number", MessageBoxButton.OK, MessageBoxImage.Error);

                if (roundNumberTextBox.Text != null && roundNumberTextBox.Text != "")
                {
                    if (int.Parse(roundNumberTextBox.Text) > 1000) roundNumberTextBox.Text = "1000";
                    else if (int.Parse(roundNumberTextBox.Text) < 1) roundNumberTextBox.Text = "1";
                }
                else
                    roundNumberTextBox.Text = "100";

                return;
            }
            else if (Regex.IsMatch(roundNumberTextBox.Text, @"^([1-9][0-9]{0,2}|1000)$"))
            {
                if (int.Parse(roundNumberTextBox.Text) < 1 || int.Parse(roundNumberTextBox.Text) > 1000)
                {
                    MessageBoxResult errMsg = MessageBox.Show("Invalid Round number: must be an integer number from interval 1 - 1000", "Invalid Round number", MessageBoxButton.OK, MessageBoxImage.Error);

                    if (roundNumberTextBox.Text != null && roundNumberTextBox.Text != "")
                    {
                        if (int.Parse(roundNumberTextBox.Text) > 1000) roundNumberTextBox.Text = "1000";
                        else if (int.Parse(roundNumberTextBox.Text) < 1) roundNumberTextBox.Text = "1";
                    }
                    else
                        roundNumberTextBox.Text = "100";


                    return;
                }
            }
        }



        private void teamNumberComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (teamNumberComboBox.SelectedItem.ToString() == teamList[0])
                numberOfTeams = 0;
            else if (teamNumberComboBox.SelectedItem.ToString() == teamList[1])
                numberOfTeams = 2;
            else if (teamNumberComboBox.SelectedItem.ToString() == teamList[2])
                numberOfTeams = 4;
        }

        private void RoundTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (Regex.IsMatch(RoundTimeTextBox.Text, @"^[0-9]([.,][0-9]{1,3})?$"))
            {
                if (double.Parse(RoundTimeTextBox.Text, CultureInfo.InvariantCulture) >= 0.5)
                {
                    _turnTime = double.Parse(RoundTimeTextBox.Text, CultureInfo.InvariantCulture);

                    gameMinuteTime = (int)(roundNumber * _turnTime / 60);
                    gameSecondTime = (int)(roundNumber * _turnTime % 60);

                    totalTimeTextBox1.Text = gameMinuteTime.ToString();
                    totalTimeTextBox2.Text = gameSecondTime.ToString();
                }

            }
        }

        private void RoundTimeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(RoundTimeTextBox.Text, @"^[0-9]([.,][0-9]{1,3})?$"))
            {
                if (double.Parse(RoundTimeTextBox.Text, CultureInfo.InvariantCulture) < 0.5)
                {
                    MessageBox.Show("Time interval from 0.5-9.99", "Invalid Round time", MessageBoxButton.OK, MessageBoxImage.Error);
                    RoundTimeTextBox.Text = "0.5";
                    _turnTime = 0.5;
                    gameMinuteTime = (int)(roundNumber / 60 * _turnTime);
                    gameSecondTime = (int)(roundNumber * _turnTime % 60);
                    totalTimeTextBox1.Text = gameMinuteTime.ToString();
                    totalTimeTextBox2.Text = gameSecondTime.ToString();
                }
                else
                    _turnTime = double.Parse(RoundTimeTextBox.Text, CultureInfo.InvariantCulture);
            }
            else
            {
                MessageBox.Show("Time interval from 0.5-9.99", "Invalid Round time", MessageBoxButton.OK, MessageBoxImage.Error);
                RoundTimeTextBox.Text = "0.5";
                _turnTime = 0.5;
                gameMinuteTime = (int)(roundNumber / 60 * _turnTime);
                gameSecondTime = (int)(roundNumber * _turnTime % 60);
                totalTimeTextBox1.Text = gameMinuteTime.ToString();
                totalTimeTextBox2.Text = gameSecondTime.ToString();
            }

        }


    }
}
