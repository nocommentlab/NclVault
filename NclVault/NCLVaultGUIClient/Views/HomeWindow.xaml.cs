using AutoMapper;
using NclVaultFramework.Models;
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
        private Mapper _mapper;
        public HomeWindow()
        {
            InitializeComponent();
            var config = new MapperConfiguration(cfg => cfg.CreateMap<PasswordEntryReadDto, PasswordEntryCreateDto>());
            _mapper = new Mapper(config);
        }

        private void uiPbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.DataContext)
            {
                ((dynamic)this.DataContext).PasswordEntryCreateDto.Password = ((PasswordBox)sender).Password;
            }
        }

        private void uiTwPassword_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            
            if( ((TreeView)sender).SelectedItem !=null && ((TreeView)sender).SelectedItem is PasswordEntryReadDto)
            {
                ((dynamic)this.DataContext).PasswordEntryCreateDto = _mapper.Map<PasswordEntryCreateDto>(((TreeView)sender).SelectedItem); 
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(((Button)sender).Tag as string);
        }
    }
}
