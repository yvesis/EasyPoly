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

namespace MyPolyWPF.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private Action<Credential> callback;
        public Login()
        {
            InitializeComponent();
        }
        public Login(Action<Credential> callback):this()
        {
            this.callback = callback;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var credential = new Credential
            {
                Username = usernameText.Text,
                Password = passwordText.Password,
                Birthdate = birthdateText.Password,
            };
            callback(credential);
        }
    }
}
