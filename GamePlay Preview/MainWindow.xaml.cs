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

namespace GamePlay_Preview
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ClientsGamePreview : Window
    {

        Map currentMap;
        String imagePath;
        Position myPosition;
        bool myBigItem = false;
        int mySmallItems = 0;
        string team;

        int currentRoundNumber;
        int allRoundsNumber;
        bool canMakeMove = false;
        ClientsLibrary cl;
        List<PossibleAction> possibleMoveList;
        double myPoints = 0;
        public ClientsGamePreview()
        {
            InitializeComponent();
            cl = new ClientsLibrary();
            //buildMap(17, 17);
            cl.InitialMessageReciveEvent += ClientsLibrary_MessageReciveEvent;
            cl.GamePlayMessageReciveEvent += cl_GamePlayMessageReciveEvent;
            cl.RoundEndEvent += cl_RoundEndEvent;
            cl.RoundStartEvent += cl_RoundStartEvent;
            cl.GameEndEvent += cl_GameEndEvent;
            cl.GameStartEvent += cl_GameStartEvent;

            cl.Login("Test" + new Random().Next(0, 100).ToString());
            //cl.MakeMove();

        }



        public void PlayGame()
        {
            while (currentRoundNumber != allRoundsNumber)
            {
                if (canMakeMove)
                {
                    //if (currentMap.GlobalMap[myPosition.Y, myPosition.X].SmallItemNumber > 0)
                    //{
                    //    cl.PickSmallItem(currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if (currentMap.GlobalMap[myPosition.Y, myPosition.X].HasBigItem)
                    //{
                    //    cl.PickBigItem(currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if (myBigItem == true && currentMap.GlobalMap[myPosition.Y, myPosition.X].HasBigItem == false &&
                    //    currentMap.GlobalMap[myPosition.Y, myPosition.X].SmallItemNumber == 0 && currentMap.GlobalMap[myPosition.Y, myPosition.X].IsStartingPosition == false)
                    //{
                    //    cl.DropBigItem(currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if (currentMap.GlobalMap[myPosition.Y - 1, myPosition.X].Robot != null)
                    //{
                    //    cl.Punch(Directions.Up, currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if (currentMap.GlobalMap[myPosition.Y + 1, myPosition.X].Robot != null)
                    //{
                    //    cl.Punch(Directions.Down, currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if (currentMap.GlobalMap[myPosition.Y, myPosition.X + 1].Robot != null)
                    //{
                    //    cl.Punch(Directions.Right, currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if (currentMap.GlobalMap[myPosition.Y, myPosition.X - 1].Robot != null)
                    //{
                    //    cl.Punch(Directions.Left, currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if ((myPosition.X + 2 < currentMap.MapWidth - 1  && currentMap.GlobalMap[myPosition.Y, myPosition.X + 2].Robot != null) ||
                    //    (myPosition.X + 3 < currentMap.MapWidth - 1 && currentMap.GlobalMap[myPosition.Y, myPosition.X + 3].Robot != null) ||
                    //    (myPosition.X + 4 < currentMap.MapWidth - 1 && currentMap.GlobalMap[myPosition.Y, myPosition.X + 4].Robot != null))
                    //{
                    //    cl.Shoot(Directions.Right, currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if ((myPosition.X - 2 > 0 && currentMap.GlobalMap[myPosition.Y, myPosition.X - 2].Robot != null) ||
                    //    (myPosition.X - 3 > 0 && currentMap.GlobalMap[myPosition.Y, myPosition.X - 3].Robot != null) ||
                    //    (myPosition.X - 4 > 0 && currentMap.GlobalMap[myPosition.Y, myPosition.X - 4].Robot != null))
                    //{
                    //    cl.Shoot(Directions.Left, currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if ((myPosition.Y + 2 < currentMap.MapHeight - 1 && currentMap.GlobalMap[myPosition.Y + 2, myPosition.X].Robot != null) ||
                    //    (myPosition.Y + 3 < currentMap.MapHeight - 1 && currentMap.GlobalMap[myPosition.Y + 3, myPosition.X].Robot != null) ||
                    //    (myPosition.Y + 4 < currentMap.MapHeight - 1 && currentMap.GlobalMap[myPosition.Y + 4, myPosition.X].Robot != null))
                    //{
                    //    cl.Shoot(Directions.Down, currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}
                    //else if ((myPosition.Y - 2 > 0 && currentMap.GlobalMap[myPosition.Y - 2, myPosition.X].Robot != null) ||
                    //    (myPosition.Y - 2 > 0 && currentMap.GlobalMap[myPosition.Y - 3, myPosition.X].Robot != null) ||
                    //    (myPosition.Y - 4 > 0 && currentMap.GlobalMap[myPosition.Y - 4, myPosition.X].Robot != null))
                    //{
                    //    cl.Shoot(Directions.Up, currentRoundNumber);
                    //    canMakeMove = false;
                    //    //continue;
                    //}                   
                    //else
                    //{
                    //    var random = new Random();

                    //    switch (random.Next(0, 3))
                    //    {
                    //        case 0:
                    //            cl.MakeMove(Common_Library.Parts_of_map.Directions.Right, currentRoundNumber);
                    //            canMakeMove = false;
                    //            break;
                    //        case 1:
                    //            cl.MakeMove(Common_Library.Parts_of_map.Directions.Left, currentRoundNumber);
                    //            canMakeMove = false;
                    //            break;
                    //        case 2:
                    //            cl.MakeMove(Common_Library.Parts_of_map.Directions.Down, currentRoundNumber);
                    //            canMakeMove = false;
                    //            break;
                    //        case 3:
                    //            cl.MakeMove(Common_Library.Parts_of_map.Directions.Up, currentRoundNumber);
                    //            canMakeMove = false;
                    //            break;
                    //    }
                    //}


                    Random r = new Random();
                    var c = r.Next(0, possibleMoveList.Count);
                    MethodInfo method = cl.GetType().GetMethod(possibleMoveList[c].Action.ToString()); // INVOKE ALL ACTIONS IN QUEUE
                    if (method.GetParameters().Count() == 2)
                        method.Invoke(cl, new object[] { possibleMoveList[0].MoveDirection, currentRoundNumber});
                    else
                        method.Invoke(cl, new object[] { currentRoundNumber });
                                    canMakeMove = false;
                    

                    //var random = new Random();
                    //switch (random.Next(0, 6))
                    //{
                    //    case 0:
                    //        switch (random.Next(0, 3))
                    //        {
                    //            case 0:
                    //                cl.MakeMove(Common_Library.Parts_of_map.Directions.Right, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //            case 1:
                    //                cl.MakeMove(Common_Library.Parts_of_map.Directions.Left, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //            case 2:
                    //                cl.MakeMove(Common_Library.Parts_of_map.Directions.Down, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //            case 3:
                    //                cl.MakeMove(Common_Library.Parts_of_map.Directions.Up, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //        }
                    //        break;
                    //    case 1:
                    //        switch (random.Next(0, 3))
                    //        {
                    //            case 0:
                    //                cl.Shoot(Common_Library.Parts_of_map.Directions.Right, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //            case 1:
                    //                cl.Shoot(Common_Library.Parts_of_map.Directions.Left, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //            case 2:
                    //                cl.Shoot(Common_Library.Parts_of_map.Directions.Down, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //            case 3:
                    //                cl.Shoot(Common_Library.Parts_of_map.Directions.Up, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //        }
                    //        break;
                    //    case 2:
                    //        switch (random.Next(0, 3))
                    //        {
                    //            case 0:
                    //                cl.Punch(Common_Library.Parts_of_map.Directions.Right, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //            case 1:
                    //                cl.Punch(Common_Library.Parts_of_map.Directions.Left, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //            case 2:
                    //                cl.Punch(Common_Library.Parts_of_map.Directions.Down, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //            case 3:
                    //                cl.Punch(Common_Library.Parts_of_map.Directions.Up, currentRoundNumber);
                    //                canMakeMove = false;
                    //                break;
                    //        }
                    //        break;


                    //    case 3:
                    //        cl.PickBigItem(currentRoundNumber);
                    //        canMakeMove = false;
                    //        break;
                    //    case 4:
                    //        cl.PickSmallItem(currentRoundNumber);
                    //        canMakeMove = false;
                    //        break;
                    //    case 5:
                    //        cl.DropBigItem(currentRoundNumber);
                    //        canMakeMove = false;
                    //        break;
                    //    case 6:
                    //        cl.Rest(currentRoundNumber);
                    //        canMakeMove = false;
                    //        break;
                    //}
                }
            }
        }


        void cl_GameStartEvent(object oSender, GamePlayArgs oEventArgs)
        {
            team = oEventArgs.team;
            allRoundsNumber = oEventArgs.initalRoundNumber;
            PlayGame();
        }

        void cl_GameEndEvent(object oSender, GamePlayArgs oEventArgs)
        {
            MessageBox.Show("My final Score: " + myPoints.ToString());
        }

        void cl_RoundStartEvent(object oSender, GameRoundArgs oEventArgs)
        {
            currentRoundNumber = oEventArgs.RoundNumber;
            possibleMoveList = oEventArgs.possibleActionList;
            canMakeMove = true;
        }

        void cl_RoundEndEvent(object oSender, GameRoundArgs oEventArgs)
        {
            currentRoundNumber = oEventArgs.RoundNumber;
            canMakeMove = false;
        }

        void cl_GamePlayMessageReciveEvent(object oSender, MessageRecivedArgs oEventArgs)
        {
            currentMap = oEventArgs.Map;
            myPosition = oEventArgs.response.MyPosition;
            myBigItem = oEventArgs.response.HasBigItem;
            mySmallItems = oEventArgs.response.SmallItemNumber;
            myPoints += oEventArgs.response.MyCurrentPay;
            Dispatcher.Invoke(() => roundTextBox.Text = oEventArgs.response.RoundNumber.ToString());
            Dispatcher.Invoke(() => scoreTextBox.Text = myPoints.ToString());

            Dispatcher.Invoke(() => repaintMap());
            //Thread.Sleep(new TimeSpan(0,0,2));
        }

        void ClientsLibrary_MessageReciveEvent(object oSender, InitialMessageRecivedArgs Args)
        {
            currentMap = Args.Map;
            myPosition = Args.response.MyPosition;
            Dispatcher.Invoke(() => roundTextBox.Text = "0");
            Dispatcher.Invoke(() => scoreTextBox.Text = "0");
            Dispatcher.Invoke(() => repaintMap());
        }



        private void buildMap(int width, int height)
        {

            currentMap = new Map(width, height);
            Grid segmentGrid = new Grid();
            squareGrid.Rows = height;
            squareGrid.Columns = width;


            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (i == 0 || i == height - 1)
                    {
                        currentMap.GlobalMap[i, j] = new Wall();
                        currentMap.GlobalMap[i, j].Color = "Default";

                        Rectangle rect = new Rectangle();
                        rect.Fill = Brushes.Transparent;
                        squareGrid.Children.Add(rect);
                        Grid.SetColumn(rect, j);
                        Grid.SetRow(rect, i);
                        Grid.SetZIndex(rect, 0);

                        StackPanel wall = new StackPanel();
                        squareGrid.Children.Add(wall);
                        Grid.SetColumn(wall, j);
                        Grid.SetRow(wall, i);
                        Grid.SetZIndex(wall, 1);

                    }
                    else if (j == 0 || j == width - 1)
                    {
                        Rectangle rect = new Rectangle();
                        rect.Fill = Brushes.Transparent;
                        squareGrid.Children.Add(rect);
                        Grid.SetColumn(rect, j);
                        Grid.SetRow(rect, i);
                        Grid.SetZIndex(rect, 0);


                        currentMap.GlobalMap[i, j] = new Wall();
                        currentMap.GlobalMap[i, j].Color = "Default";
                        StackPanel wall = new StackPanel();
                        squareGrid.Children.Add(wall);
                        Grid.SetColumn(wall, j);
                        Grid.SetRow(wall, i);
                        Grid.SetZIndex(wall, 1);


                    }
                    else
                    {
                        currentMap.GlobalMap[i, j] = new Floor();
                        currentMap.GlobalMap[i, j].Color = "Default";

                        Rectangle rect = new Rectangle();
                        rect.Fill = Brushes.Transparent;
                        squareGrid.Children.Add(rect);
                        Grid.SetColumn(rect, j);
                        Grid.SetRow(rect, i);
                        Grid.SetZIndex(rect, 0);

                        StackPanel floor = new StackPanel();
                        floor.Style = (Style)FindResource("FloorStyle");
                        squareGrid.Children.Add(floor);
                        Grid.SetColumn(floor, j);
                        Grid.SetRow(floor, i);
                        Grid.SetZIndex(floor, 1);
                    }
                }

            }


        }


        private void repaintMap()
        {
            if (currentMap == null && currentMap.MapWidth == 0 || currentMap.MapHeight == 0)
                return;

            squareGrid.Children.Clear();


            for (int i = 0; i < currentMap.MapHeight; i++)
                for (int j = 0; j < currentMap.MapWidth; j++)
                {
                    if (currentMap.GlobalMap[i, j] is Wall)
                    {
                        BrushConverter conv = new BrushConverter();
                        SolidColorBrush brush;
                        if (currentMap.GlobalMap[i, j] != null && currentMap.GlobalMap[i, j].Color != "Default")
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
                        if (currentMap.GlobalMap[i, j].Robot != null)
                        {
                            imagePath = "Images/SingleRobot.png";
                            Image img = new Image();
                            img.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));
                            // currentMap.StartingPositionList.Add(new Common_Library.Parts_of_map.StartingPosition(j, i));
                            wall.Children.Add(img);
                            StackPanel.SetZIndex(img, 0);
                        }
                        else if (currentMap.GlobalMap[i, j].HasBigItem)
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
                            //if (currentMap.GlobalMap[i, j].Robot != null)
                            //{
                            //    imagePath = "Images/SingleRobot.png";
                            //    Image img = new Image();
                            //    img.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));
                            //    // currentMap.StartingPositionList.Add(new Common_Library.Parts_of_map.StartingPosition(j, i));
                            //    wall.Children.Add(img);
                            //    StackPanel.SetZIndex(img, 0);
                            //}

                            imagePath = "Images/starting_position.png";
                            Image img2 = new Image();
                            img2.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));
                            // currentMap.StartingPositionList.Add(new Common_Library.Parts_of_map.StartingPosition(j, i));
                            wall.Children.Add(img2);
                            StackPanel.SetZIndex(img2, 1);

                        }

                        BrushConverter conv = new BrushConverter();
                        SolidColorBrush brush;
                        if (currentMap.GlobalMap[i, j] != null && currentMap.GlobalMap[i, j].Color != "Default")
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
                    else if (currentMap.GlobalMap[i, j] == null)
                    {
                        Rectangle rect = new Rectangle();
                        rect.Fill = Brushes.Transparent;
                        // resizeMap(currentMap);
                        squareGrid.Children.Add(rect);
                        Grid.SetColumn(rect, j);
                        Grid.SetRow(rect, i);
                        Grid.SetZIndex(rect, 0);
                    }

                }
        }
        /*
        private void resizeMap(Map currentMap)
        {
           int height= currentMap.MapHeight;
           int width = currentMap.MapWidth;
           double side = squareGrid.SquareSideLength;

           if (height < 17 && width==17)
           {
               MapGrid.Height =height * side;
           }
           else if (height == 17 && width < 17) 
           {
               MapGrid.Width = width * side;

           }
           else if (height < 17 && width < 17) 
           {
               MapGrid.Height = height * side;
               MapGrid.Width = width * side;


           }




        }
       */
    }
}
