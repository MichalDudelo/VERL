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
using Common_Library;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Common_Library.Parts_of_map;
using System.Text.RegularExpressions;


namespace Map_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Cursor tempCursor;
        Map currentMap;
        bool isPainting = false;
        Image Img = new Image();
        String imagePath;
        Dictionary<string, int> imageDictionary = new Dictionary<string, int>();
        int imageTypeInt = 0;
        const int SIDE = 80;
        const int SIZE25 = 550;
        const int SIDE25 = 22;

        public MainWindow()
        {


            InitializeComponent();
            imageDictionary.Add("None", 0);
            imageDictionary.Add("Background", 1);
            imageDictionary.Add("Image", 2);
            imageDictionary.Add("Delete", 3);
            imageDictionary.Add("Color", 4);
            mapHeightTextBox.MaxLength = 3;
            mapWidthTextBox.MaxLength = 3;
            componentsListBox.Items.Add("Floor");
            componentsListBox.Items.Add("Wall");
            componentsListBox.Items.Add("Starting Position");
            componentsListBox.Items.Add("Small Item");
            componentsListBox.Items.Add("Big Item");
            componentsListBox.Items.Add("Delete");
            componentsListBox.Items.Add("Color");
            componentsListBox.Items.Add("None");
            colorPaletteComboBox.Items.Add("1");
            colorPaletteComboBox.Items.Add("2");
            colorPaletteComboBox.Items.Add("4");
            colorPaletteComboBox.Items.Add("8");
            colorPaletteComboBox.SelectedItem = "1";
            colorsListBox.Items.Add("Default");
            colorsListBox.SelectedItem = "Default";
            colorsListBox.Visibility = System.Windows.Visibility.Hidden;
            colorsLabel.Visibility = System.Windows.Visibility.Hidden;
            componentsListBox.SelectedItem = "None";
            buildMap(25, 25);
            //  currentMap.StartingPositionNumber = maxRobotNumber;
        }


        public MainWindow(Map loadedMap)
        {


            InitializeComponent();
            imageDictionary.Add("None", 0);
            imageDictionary.Add("Background", 1);
            imageDictionary.Add("Image", 2);
            imageDictionary.Add("Delete", 3);
            imageDictionary.Add("Color", 4);
            mapHeightTextBox.MaxLength = 3;
            mapWidthTextBox.MaxLength = 3;
            componentsListBox.Items.Add("Floor");
            componentsListBox.Items.Add("Wall");
            componentsListBox.Items.Add("Starting Position");
            componentsListBox.Items.Add("Small Item");
            componentsListBox.Items.Add("Big Item");
            componentsListBox.Items.Add("Delete");
            componentsListBox.Items.Add("Color");
            componentsListBox.Items.Add("None");
            colorPaletteComboBox.Items.Add("1");
            colorPaletteComboBox.Items.Add("2");
            colorPaletteComboBox.Items.Add("4");
            colorPaletteComboBox.Items.Add("8");
            colorPaletteComboBox.SelectedItem = "1";
            colorsListBox.Items.Add("Default");
            colorsListBox.SelectedItem = "Default";
            colorsListBox.Visibility = System.Windows.Visibility.Hidden;
            colorsLabel.Visibility = System.Windows.Visibility.Hidden;
            componentsListBox.SelectedItem = "None";


            // buildMap(loadedMap.MapWidth, loadedMap.MapHeight);

            currentMap = loadedMap;
            mapHeightTextBox.Text = loadedMap.MapHeight.ToString();
            mapWidthTextBox.Text = loadedMap.MapWidth.ToString();
            paintloadedMap();


        }

        /// <summary>
        /// Function responsible for building map of size of given parameters
        /// Built map is surrounded by a walls and filled by floor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// 
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

        /// <summary>
        /// Function responsible for repainting Map in Grid
        /// </summary>


        /// <summary>
        /// Function responsible for performing events selected in componentsListBox
        /// </summary>
        /// <param name="e"></param>
        private void Painting(object e)
        {
            /// Part responsible for changing colors of Segments - Rectangles
            if (imageTypeInt == imageDictionary["Color"])
            {
                var element = (UIElement)e;
                int c = Grid.GetColumn(element);
                int r = Grid.GetRow(element);

                if (element is Image)
                {
                    foreach (StackPanel sp in squareGrid.Children.OfType<StackPanel>())
                    {
                        foreach (UIElement ue in sp.Children)
                        {
                            if (ue == element)
                            {
                                c = Grid.GetColumn(sp);
                                r = Grid.GetRow(sp);
                            }
                        }
                    }
                }

                Rectangle tempRectangle = new Rectangle();

                foreach (Rectangle rect in squareGrid.Children.OfType<Rectangle>())
                {
                    if (Grid.GetColumn(rect) == c && Grid.GetRow(rect) == r)
                        tempRectangle = rect;
                }

                if (colorsListBox.SelectedItem.ToString() == "Gray")
                {
                    if (tempRectangle.Fill != Brushes.Transparent)
                    {
                        tempRectangle.Fill = Brushes.Transparent;
                        currentMap.GlobalMap[r, c].Color = "Default";
                    }
                }
                else
                {
                    BrushConverter conv = new BrushConverter();
                    String colorName = colorsListBox.SelectedItem.ToString();
                    SolidColorBrush brush = conv.ConvertFromString(colorName) as SolidColorBrush;
                    tempRectangle.Fill = brush;
                    currentMap.GlobalMap[r, c].Color = colorsListBox.SelectedItem.ToString();
                }


            }

            ///Part responsible for all other events

            if (imageTypeInt != imageDictionary["None"])
            {

                var element = (UIElement)e;
                int c = Grid.GetColumn(element);
                int r = Grid.GetRow(element);


                ///Part performed when action Delete is selected
                ///Detects which object is chosen (small item, big item, spawn point) and removes this object

                if (element is Image && imageTypeInt == imageDictionary["Delete"])
                {
                    foreach (StackPanel sp in squareGrid.Children.OfType<StackPanel>())
                    {
                        foreach (UIElement ue in sp.Children)
                        {
                            if (ue == element)
                            {
                                c = Grid.GetColumn(sp);
                                r = Grid.GetRow(sp);
                            }
                        }
                    }

                    foreach (StackPanel p in squareGrid.Children.OfType<StackPanel>())
                    {
                        if (p.Children.Count > 0 && p.Children[0] == element)
                        {
                            ImageSource imgSourceSpawnPoint = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/starting_position.png"));
                            ImageSource imgSourceSmallItem = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/small_item.png"));
                            ImageSource imgSourceBigItem = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "Images/big_item.png"));
                            ImageSource imgSource = (element as Image).Source.Clone();

                            if (imgSource.Width == imgSourceSpawnPoint.Width) // del spawn point
                            {
                                currentMap.StartingPositionList.Remove(currentMap.StartingPositionList.Find(st => st.X == Grid.GetColumn(p) && st.Y == Grid.GetRow(p)));
                                currentMap.StartingPositionNumber = currentMap.StartingPositionList.Count;
                                currentMap.GlobalMap[r, c].IsStartingPosition = false;
                                p.Children.Clear();
                            }

                            if (imgSource.Width == imgSourceBigItem.Width)
                            {
                                currentMap.GlobalMap[r, c].HasBigItem = false;
                                currentMap.BigItemList.Remove(currentMap.BigItemList.Find(st => st.X == Grid.GetColumn(p) && st.Y == Grid.GetRow(p)));
                                p.Children.Clear();
                            }

                            if (imgSource.Width == imgSourceSmallItem.Width)
                            {
                                if (currentMap.GlobalMap[r, c].SmallItemNumber > 1)
                                {
                                    currentMap.GlobalMap[r, c].SmallItemNumber--;
                                    currentMap.SmallItemList.Remove(currentMap.SmallItemList.Find(st => st.X == Grid.GetColumn(p) && st.Y == Grid.GetRow(p)));
                                }
                                else if (currentMap.GlobalMap[r, c].SmallItemNumber == 1)
                                {
                                    currentMap.GlobalMap[r, c].SmallItemNumber--;
                                    currentMap.SmallItemList.Remove(currentMap.SmallItemList.Find(st => st.X == Grid.GetColumn(p) && st.Y == Grid.GetRow(p)));
                                    p.Children.Clear();
                                }
                            }


                        }

                    }

                }

                ///Part performed when other action is selected:
                ///Add spawn point
                ///Add small item
                ///Add big item
                ///Change floor to wall and oppositely

                if (element is StackPanel && imageTypeInt != imageDictionary["Delete"] && imageTypeInt != imageDictionary["Color"])
                {
                    StackPanel panel = new StackPanel();
                    foreach (StackPanel sp in squareGrid.Children.OfType<StackPanel>())
                    {
                        if (Grid.GetColumn(sp) == c && Grid.GetRow(sp) == r)
                            panel = sp;

                    }


                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), imagePath));

                    if (imageTypeInt == imageDictionary["Image"] && currentMap.GlobalMap[r, c] is Floor)
                    {
                        if (imagePath == "Images/starting_position.png")
                        {
                            currentMap.GlobalMap[r, c].IsStartingPosition = true;
                            currentMap.StartingPositionList.Add(new Position(c, r));
                            currentMap.StartingPositionNumber = currentMap.StartingPositionList.Count;
                            panel.Children.Add(img);
                        }
                        else if (imagePath == "Images/small_item.png")
                        {
                            panel.Children.Add(img);
                            currentMap.GlobalMap[r, c].SmallItemNumber++;
                            currentMap.SmallItemList.Add(new SmallItem(c, r));
                        }
                        else if (imagePath == "Images/big_item.png")
                        {
                            panel.Children.Add(img);
                            currentMap.GlobalMap[r, c].HasBigItem = true;
                            currentMap.BigItemList.Add(new BigItem(c, r));
                        }
                    }
                    else if (imageTypeInt == imageDictionary["Background"])
                    {
                        Rectangle tempRectangle = new Rectangle();

                        if (imagePath == "Images/floor.png" && currentMap.GlobalMap[r, c] is Wall)
                        {
                            if (r == 0 || c == 0 || r == currentMap.MapHeight - 1 || c == currentMap.MapWidth - 1)
                                return;

                            currentMap.GlobalMap[r, c] = new Floor();
                            ImageBrush brush = new ImageBrush();
                            brush.Opacity = 0.7;
                            brush.ImageSource = img.Source;
                            panel.Background = brush;



                            foreach (Rectangle rect in squareGrid.Children.OfType<Rectangle>())
                            {
                                if (Grid.GetColumn(rect) == c && Grid.GetRow(rect) == r)
                                    tempRectangle = rect;
                            }

                            tempRectangle.Fill = Brushes.Transparent;
                            currentMap.GlobalMap[r, c].Color = "Default";

                        }

                        else if (imagePath == "Images/wall.png" && currentMap.GlobalMap[r, c] is Floor)
                        {
                            currentMap.GlobalMap[r, c] = new Wall();
                            ImageBrush brush = new ImageBrush();
                            brush.Opacity = 0.7;
                            brush.ImageSource = img.Source;
                            panel.Background = brush;

                            foreach (Rectangle rect in squareGrid.Children.OfType<Rectangle>())
                            {
                                if (Grid.GetColumn(rect) == c && Grid.GetRow(rect) == r)
                                    tempRectangle = rect;
                            }

                            tempRectangle.Fill = Brushes.Transparent;
                            currentMap.GlobalMap[r, c].Color = "Default";

                        }

                    }

                }

            }

        }


        /// <summary>
        /// Functions MapGrid_MouseEnter; MapGrid_MouseLeave
        /// Check if cursor is over MapGrid and change mouse cursor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void MapGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = tempCursor;

        }

        private void MapGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        /// <summary>
        /// Functions squareGrid_PreviewMouseDown, squareGrid_PreviewMouseMove, squareGrid_PreviewMouseUp
        /// Are responsible for Mouse Down, Move and Up actions, check of user want to draw and change isDrawing variable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void squareGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            isPainting = true;
            Painting(e.Source);

        }

        private void squareGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isPainting)
                Painting(e.Source);

        }

        private void squareGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            isPainting = false;

        }

        /// <summary>
        /// Function responsible for event SelectionChanged on componentListBox
        /// It changes mouse cursor and determines which action is performed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void componentsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String selectedValue = componentsListBox.SelectedValue.ToString();


            switch (selectedValue)
            {
                case "Floor":
                    tempCursor = new Cursor(new System.IO.MemoryStream(Map_Editor.Properties.Resources.cursor_floor));
                    imagePath = "Images/floor.png";
                    imageTypeInt = imageDictionary["Background"];
                    break;

                case "Wall":
                    tempCursor = this.Cursor = new Cursor(new System.IO.MemoryStream(Map_Editor.Properties.Resources.cursor_wall));
                    imagePath = "Images/wall.png";
                    imageTypeInt = imageDictionary["Background"];
                    break;

                case "Starting Position":
                    tempCursor = new Cursor(new System.IO.MemoryStream(Map_Editor.Properties.Resources.cursor_spawnPoint));
                    imagePath = "Images/starting_position.png";
                    imageTypeInt = imageDictionary["Image"];
                    break;

                case "Small Item":
                    tempCursor = new Cursor(new System.IO.MemoryStream(Map_Editor.Properties.Resources.cursor_small_item));
                    imagePath = "Images/small_item.png";
                    imageTypeInt = imageDictionary["Image"];
                    break;

                case "Big Item":
                    tempCursor = new Cursor(new System.IO.MemoryStream(Map_Editor.Properties.Resources.cursor_big_item));
                    imagePath = "Images/big_item.png";
                    imageTypeInt = imageDictionary["Image"];
                    break;

                case "None":
                    tempCursor = Cursors.Arrow;
                    imageTypeInt = imageDictionary["None"];
                    break;

                case "Delete":
                    tempCursor = Cursors.No;
                    imageTypeInt = imageDictionary["Delete"];
                    break;

                case "Color":
                    tempCursor = Cursors.Pen;
                    imageTypeInt = imageDictionary["Color"];
                    break;


            }


        }

        /// <summary>
        /// Function responsible for event SelectionChanged on colorPaletteComboBox
        /// It changes content of colorListBox and determines which color is chosen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colorPaletteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            String selectedValue = colorPaletteComboBox.SelectedValue.ToString();

            switch (selectedValue)
            {
                case "1":
                    colorsListBox.Visibility = System.Windows.Visibility.Hidden;
                    colorsLabel.Visibility = System.Windows.Visibility.Hidden;
                    if (currentMap != null)
                        currentMap.ColorPalette = 1;
                    break;

                case "2":
                    colorsListBox.Items.Clear();
                    colorsListBox.Items.Add("Gray");
                    colorsListBox.Items.Add("Red");
                    colorsListBox.SelectedItem = "Gray";
                    colorsListBox.Visibility = System.Windows.Visibility.Visible;
                    colorsLabel.Visibility = System.Windows.Visibility.Visible;
                    currentMap.ColorPalette = 2;
                    break;

                case "4":

                    colorsListBox.Items.Clear();
                    colorsListBox.Items.Add("Gray");
                    colorsListBox.Items.Add("Red");
                    colorsListBox.Items.Add("Blue");
                    colorsListBox.Items.Add("Green");
                    colorsListBox.SelectedItem = "Gray";
                    colorsListBox.Visibility = System.Windows.Visibility.Visible;
                    colorsLabel.Visibility = System.Windows.Visibility.Visible;
                    currentMap.ColorPalette = 4;
                    break;

                case "8":
                    colorsListBox.Items.Clear();
                    colorsListBox.Items.Add("Gray");
                    colorsListBox.Items.Add("Red");
                    colorsListBox.Items.Add("Blue");
                    colorsListBox.Items.Add("Green");
                    colorsListBox.Items.Add("Yellow");
                    colorsListBox.Items.Add("Orange");
                    colorsListBox.Items.Add("Violet");
                    colorsListBox.Items.Add("Black");
                    colorsListBox.SelectedItem = "Gray";
                    colorsListBox.Visibility = System.Windows.Visibility.Visible;
                    colorsLabel.Visibility = System.Windows.Visibility.Visible;
                    currentMap.ColorPalette = 8;
                    break;

            }


        }

        /// <summary>
        /// Function responsible for event SelectionChanged on colorsListBox
        /// It changes SelectedItem of componensListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            componentsListBox.SelectedItem = "Color";
        }

        /// <summary>
        /// Function responsible for event Click on setSizeButton
        /// It set size of map and build map of new size using buildMap function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setSizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(mapHeightTextBox.Text, @"^[1-9][0-9]?$|^100$") && Regex.IsMatch(mapWidthTextBox.Text, @"^[1-9][0-9]?$|^100$"))
            {

                if (int.Parse(mapHeightTextBox.Text) > 100 || (int.Parse(mapHeightTextBox.Text) < 10))
                {
                    MessageBoxResult errMsg = MessageBox.Show("Invalid Height: it must be an integer number from interval 10 - 100", "Invalid Height", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (int.Parse(mapWidthTextBox.Text) > 100 || int.Parse(mapWidthTextBox.Text) < 10)
                {

                    MessageBoxResult errMsg = MessageBox.Show("Invalid Width: it must be an integer number from interval 10 - 100", "Invalid Width", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                else if ((int.Parse(mapHeightTextBox.Text) > 100 && int.Parse(mapWidthTextBox.Text) > 100) || (int.Parse(mapHeightTextBox.Text) < 10 && int.Parse(mapWidthTextBox.Text) < 10))
                {
                    MessageBoxResult errMsg = MessageBox.Show("Invalid Width and height: must be an integer number from interval 10 - 100", "Invalid Width and Height", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBoxResult errMsg = MessageBox.Show("Invalid Width and height: must be an integer number from interval 10 - 100", "Invalid Width and Height", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            squareGrid.Children.Clear();

            double height = (double)(int.Parse(mapHeightTextBox.Text));
            double width = (double)(int.Parse(mapWidthTextBox.Text));
            double fraction;
            bool _isHeightChanged = false;
            bool _isWidthChanged = false;
            int rows = squareGrid.Rows;
            int columns = squareGrid.Columns;
            double old_side = squareGrid.SquareSideLength;

            if (height != currentMap.MapHeight)
                _isHeightChanged = true;
            if (width != currentMap.MapWidth)
                _isWidthChanged = true;
            
         
            if (_isHeightChanged && _isWidthChanged) 
            {
                if (height > currentMap.MapHeight && width > currentMap.MapWidth) 
                {
                    if (height == 25 && width == 25)
                    {
                        MapGrid.Height = SIZE25;
                        MapGrid.Width = SIZE25;
                        squareGrid.SquareSideLength = SIDE25;

                    }
                    else 
                    {
                        fraction = (double)height / currentMap.MapHeight;
                        MapGrid.Height = MapGrid.Height * fraction;
                  
                        fraction = (double)width / currentMap.MapWidth;
                        MapGrid.Width = MapGrid.Width * fraction;
                    }
                        

                  

                }
                else if (height > currentMap.MapHeight && width < currentMap.MapWidth)
                {

                    fraction = (double)height / currentMap.MapHeight;
                    MapGrid.Height = MapGrid.Height * fraction;

                }
                else if (height < currentMap.MapHeight && width > currentMap.MapWidth)
                {
                    fraction = (double)width / currentMap.MapWidth;
                    MapGrid.Width = MapGrid.Width * fraction;

                }
                else if (height < currentMap.MapHeight && width < currentMap.MapWidth)
                {
                    if (height == 25 && width == 25)
                    {
                        MapGrid.Height = SIZE25;
                        MapGrid.Width = SIZE25;
                        squareGrid.SquareSideLength = SIDE25;

                    }
                    else 
                    {
                        double old_height = (double)rows;
                        double old_width = (double)columns;

                        double fraction_height = (old_height / height);
                        double fraction_width = (old_width / width);
                        if (fraction_width > fraction_height) fraction = fraction_width;
                        else fraction = fraction_height;

                        double size = old_side * fraction;
                        int side = (int)size;
                        if (side > SIDE) side = SIDE;
                        squareGrid.SquareSideLength = side;

                        MapGrid.Width = width * side;
                        MapGrid.Height = height * side;
                    }
                                        
                }
                else if (height == 25 && width == 25) 
                {
                    MapGrid.Height = SIZE25;
                    MapGrid.Width = SIZE25;
                    squareGrid.SquareSideLength = SIDE25;

                }



            }
            else if (_isHeightChanged && !_isWidthChanged)
            {
                if (height > currentMap.MapHeight) 
                {
                    fraction = (double)height / currentMap.MapHeight;
                    MapGrid.Height = MapGrid.Height * fraction;
                }
                else
                {
                    double old_height = (double)rows;
                    
                    fraction = (old_height / height);
                   
                    double size = old_side * fraction;
                    int side = (int)size;
                    if (side > SIDE) side = SIDE;
                    squareGrid.SquareSideLength = side;

                    MapGrid.Width = columns * side;
                }
            }
            else if (!_isHeightChanged && _isWidthChanged)
            {
                if (width > currentMap.MapWidth)
                {
                    fraction = (double)width / currentMap.MapWidth;
                    MapGrid.Width = MapGrid.Width * fraction;
                }
                else
                {
                    double old_width = (double)columns;
                    fraction = (old_width / width);
                    double size = old_side * fraction;
                    int side = (int)size;
                    if (side > SIDE) side = SIDE;
                    squareGrid.SquareSideLength = side;

                    MapGrid.Height = rows * side;
                }
            }


            currentMap.StartingPositionNumber = 0;
            currentMap.MapHeight = int.Parse(mapHeightTextBox.Text);
            currentMap.MapWidth = int.Parse(mapWidthTextBox.Text);

            buildMap(int.Parse(mapWidthTextBox.Text), int.Parse(mapHeightTextBox.Text));



        }

        /// <summary>
        /// Function responsible for event Click on saveMapButton
        /// It saves current map to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveMapButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentMap.StartingPositionList.Count > 0)
            {

                // Configure save file dialog box
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Map"; // Default file name
                dlg.DefaultExt = ".map"; // Default file extension
                dlg.Filter = "Binary map format (.map)|*.map"; // Filter files by extension 

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results 
                if (result == true)
                {
                    // Save document 
                    Stream stream = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    IFormatter formatter = new BinaryFormatter();


                    formatter.Serialize(stream, currentMap);
                    stream.Close();

                }

            }

            else
            {
                MessageBox.Show("Map has to contain at least one Starting position!");
            }

        }

        /// <summary>
        /// Function responsible for event Click on loadMapButton
        /// It loads map from file and repaint current map using repaintMap function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadMapButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "Map"; // Default file name

            dlg.Filter = "Binary map format (.map)|*.map"; // Filter files by extension 

            // Display OpenFileDialog by calling ShowDialog method

            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox

            if (result == true)
            {

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                currentMap = (Map)formatter.Deserialize(stream);
                stream.Close();

                repaintMap(); // repaint the whole map

            }



        }

        private void PaintItems(Map currentMap)
        {
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

        private void repaintMap()
        {
            if (currentMap == null && (currentMap.MapWidth == 0 || currentMap.MapHeight == 0))
                return;

            squareGrid.Children.Clear();
            double height = (double)(int.Parse(mapHeightTextBox.Text));
            double width = (double)(int.Parse(mapWidthTextBox.Text));
            double fraction = 1;
            double old_height = height;
            double old_width = width;
            double old_side = squareGrid.SquareSideLength;
            double fraction_height = (old_height / currentMap.MapHeight);
            double fraction_width = (old_width / currentMap.MapWidth);

            if (currentMap.MapHeight > height || currentMap.MapWidth > width)
            {
                if (currentMap.MapHeight > currentMap.MapWidth)
                {
                    fraction = (double)currentMap.MapHeight / height;
                }
                else
                {
                    fraction = (double)currentMap.MapWidth / width;
                }

                double side = squareGrid.SquareSideLength;
                double size = side / fraction;
                if (size < 20) size = 20;
                double _height = MapGrid.Height * fraction;
                double _width = MapGrid.Width * fraction;
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

                MapGrid.Height = new_height;
                MapGrid.Width = new_width;
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
                MapGrid.Height = SIZE25;
                MapGrid.Width = SIZE25;
            }
            mapHeightTextBox.Text = currentMap.MapHeight.ToString();
            mapWidthTextBox.Text = currentMap.MapWidth.ToString();

            PaintItems(currentMap);

        }

        private void paintloadedMap()
        {
            if (currentMap == null && (currentMap.MapWidth == 0 || currentMap.MapHeight == 0))
                return;

            squareGrid.Children.Clear();
            double height = 25;
            double width = 25;
            double fraction = 1;
            if (currentMap.MapHeight > height || currentMap.MapWidth > width)
            {
                if (currentMap.MapHeight > currentMap.MapWidth)
                {
                    fraction = (double)currentMap.MapHeight / height;
                }
                else
                {
                    fraction = (double)currentMap.MapWidth / width;
                }
                MapGrid.Height = MapGrid.Height * fraction;
                MapGrid.Width = MapGrid.Width * fraction;

                double side = squareGrid.SquareSideLength;
                double size = side / fraction;
                if (size < 20) size = 20;
                squareGrid.SquareSideLength = size;
            }
            else if (currentMap.MapHeight == height || currentMap.MapWidth == height)
            {
                squareGrid.SquareSideLength = SIDE25;
                MapGrid.Height = SIZE25;
                MapGrid.Width = SIZE25;
            }
            else
            {
                double old_height = 25;
                double old_width = 25;
                double old_side = SIDE25;
                double fraction_height = (old_height / currentMap.MapHeight);
                double fraction_width = (old_width / currentMap.MapWidth);
                if (fraction_width > fraction_height) fraction = fraction_width;
                else fraction = fraction_height;

                double size = old_side * fraction;
                int side = (int)size;
                if (side > SIDE) side = SIDE;
                squareGrid.SquareSideLength = side;
            }

            mapHeightTextBox.Text = currentMap.MapHeight.ToString();
            mapWidthTextBox.Text = currentMap.MapWidth.ToString();

            PaintItems(currentMap);
        }

        /// <summary>
        /// Function handling clearMapButton_Click - clears map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearMapButton_Click(object sender, RoutedEventArgs e)
        {
            squareGrid.Children.Clear();
            currentMap.StartingPositionNumber = 0;
            buildMap(currentMap.MapWidth, currentMap.MapHeight);
        }
    }
}
