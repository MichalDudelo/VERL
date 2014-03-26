using Arena_Server.Infrastructure;
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
using System.Windows.Shapes;

namespace Common_Library
{
    /// <summary>
    /// Interaction logic for ScoreModuleSelectWindow.xaml
    /// </summary>
    public partial class ScoreModuleSelectWindow : Window
    {
        public string SelectedModule;
        public bool result = false;
        public ScoreModuleSelectWindow()
        {
            InitializeComponent();
        }

        public ScoreModuleSelectWindow(List<IScoreModule> scoreModuleList)
        {
            InitializeComponent();
            this.MouseDown += delegate { DragMove(); };
            try
            {
                foreach (IScoreModule score in scoreModuleList)
                    ScoreModuleListBox.Items.Add(score.ToString());
            }
            catch (Exception e) { MessageBox.Show("Score module loading error. Try another Score Module!"); };
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScoreModuleListBox.SelectedItem != null)
            {
                this.SelectedModule = ScoreModuleListBox.SelectedItem.ToString();
                result = true;
                this.Close();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            result = false;
            this.Close();
        }
    }
}
