using NCLVaultGUIClient.ViewModels;
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

namespace NCLVaultGUIClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            new InitDatabaseView().ShowDialog();
        }

        /// <summary>
        /// https://stackoverflow.com/a/25001115
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiTxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            {
                 ((dynamic)this.DataContext).LoginCredential.SecurePassword = ((PasswordBox)sender).SecurePassword;
            }
        }
    }
}
