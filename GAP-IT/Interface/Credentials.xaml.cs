using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GAP_IT.Code;

namespace GAP_IT.Interface
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Credentials : Window
    {

        private RegistryHandler registryHandler = new RegistryHandler();
        
        public Credentials()
        {
            InitializeComponent();

            registryHandler.LoadCredentials();

            if (ProgramSettings.Remember)
            {
                rememberCredentials.IsChecked = true;
            }
            else
            {
                rememberCredentials.IsChecked = false;
            }

            textboxUsername.Text = ProgramSettings.Username;
            textboxDomain.Text = ProgramSettings.Domain;
        }

        private void saveCredentials()
        {
            bool validForm = true;
            if (textboxUsername.Text == "")
            {
                textboxUsername.Background = Brushes.DarkRed;
                textboxUsername.Foreground = Brushes.White;
                validForm = false;
            }

            if (textboxPassword.Password == "")
            {
                textboxPassword.Background = Brushes.DarkRed;
                textboxPassword.Foreground = Brushes.White;
                validForm = false;
            }

            if (textboxDomain.Text == "")
            {
                textboxDomain.Background = Brushes.DarkRed;
                textboxDomain.Foreground = Brushes.White;
                validForm = false;
            }


            if (validForm)
            {

                if (rememberCredentials.IsChecked == true)
                {
                    ProgramSettings.Remember = true;
                }
                else
                {
                    ProgramSettings.Remember = false;
                }

                ProgramSettings.Username = textboxUsername.Text;
                ProgramSettings.Password = textboxPassword.SecurePassword;
                ProgramSettings.Domain = textboxDomain.Text;

                if (rememberCredentials.IsChecked == true)
                {
                    registryHandler.SaveCredentials();
                }

                this.Close();
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                saveCredentials();
            }
        }

        private void saveCredentials_Click(object sender, RoutedEventArgs e)
        {
            saveCredentials();
        }
    }
}
