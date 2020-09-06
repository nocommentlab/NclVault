using System;
using System.ComponentModel;
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
