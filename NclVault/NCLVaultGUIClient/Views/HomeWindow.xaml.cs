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

namespace NCLVaultGUIClient.Views
{
    /// <summary>
    /// Interaction logic for HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
        }

        private void uiPbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.DataContext)
            {
                ((dynamic)this.DataContext).PasswordEntryCreateDto.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}
