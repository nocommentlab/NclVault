using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCLVaultGUIClient.Models
{
    public class Credential : INotifyPropertyChanged
    {
        #region Members
        public event PropertyChangedEventHandler PropertyChanged;
        private string _STRING_Username = String.Empty;
        private string _STRING_Password = String.Empty;
        #endregion

        #region Properties
        public string Password
        {
            get { return _STRING_Password; }
            set { _STRING_Password = value; OnPropertyChanged("Password"); }
        }

        public string Username
        {
            get { return _STRING_Username; }
            set { _STRING_Username = value; OnPropertyChanged("Username"); }
        }
        #endregion

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
