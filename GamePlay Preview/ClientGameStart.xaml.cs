using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GamePlay_Preview
{
    /// <summary>
    /// Interaction logic for ClientGameStart.xaml
    /// </summary>
    public partial class ClientGameStart : Window
    {
        public string ipAddress;
        public string login;
        ClientsGamePreview clientGP;

        public ClientGameStart()
        {
            InitializeComponent();
            this.MouseDown += delegate { DragMove(); };
            loginTextBox.MaxLength = 12;
        }


        private void serverIPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ipAddress = serverIPTextBox.Text.ToString();
        }

        private void loginTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            login = loginTextBox.Text.ToString();
        }


        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                startClientButton_Click(null, e);
            }
        }


        private void startClientButton_Click(object sender, RoutedEventArgs e)
        {
            Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

            if (ipAddress != null && login != null && ipAddress != "" && login != "")
            {

                if (!ip.IsMatch(ipAddress) && ipAddress != "localhost" && login.Length < 4)
                {
                    MessageBox.Show("Server IP Address is not valid and Login is shorter than 4 characters");
                    return;
                }
                else if (!ip.IsMatch(ipAddress) && ipAddress != "localhost")
                {
                    MessageBox.Show("Server IP Address is not valid");
                    return;
                }
                else if (login.Length < 4)
                {
                    MessageBox.Show("Login is shorter than 4 characters - valid login length (4 - 12)");
                    return;
                }
                else
                {
                    clientGP = new ClientsGamePreview(ipAddress, login);
                    this.Close();
                    clientGP.Show();
                }
            }
            else
                MessageBox.Show("Server IP Address and Login Text Boxes must be filled");
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
