using NclVaultFramework.Controllers;
using NclVaultFramework.Models;
using NCLVaultGUIClient.Commands;
using NCLVaultGUIClient.Interfaces;
using NCLVaultGUIClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NCLVaultGUIClient.ViewModels
{
    class HomeViewModel : IDoCreatePassword, IDoShowPassword, INotifyPropertyChanged
    {

        #region Members
        private readonly BackendInterface _backendInterface;
        public ICommand CreatePasswordCommand { get; set; }
        public ICommand ShowPasswordCommand { get; set; }
        private ObservableCollection<PasswordGroup> _groupsPassword;
        private PasswordEntryCreateDto _passwordEntryCreateDto;

        public event PropertyChangedEventHandler PropertyChanged;







        #endregion

        #region Properties
        public PasswordEntryCreateDto PasswordEntryCreateDto { get => _passwordEntryCreateDto; set { _passwordEntryCreateDto = value; OnPropertyChanged("PasswordEntryCreateDto"); } }

        public ObservableCollection<PasswordGroup> GroupsPassword
        {
            get { return _groupsPassword; }
            set { _groupsPassword = value; }
        }
        #endregion

        public HomeViewModel()
        {
            _backendInterface = BackendInterface.GetInstance(true);

            CreatePasswordCommand = new DoCreatePasswordCommand(this);
            ShowPasswordCommand = new DoShowPasswordCommand(this);

            _passwordEntryCreateDto = new PasswordEntryCreateDto();

            _groupsPassword = new ObservableCollection<PasswordGroup>();


        }
        public async void DoCreatePassword(object OBJECT_Element)
        {
            HTTPResponseResult httpResponseResult = await _backendInterface.CreatePassword((PasswordEntryCreateDto)OBJECT_Element);
            if (httpResponseResult.StatusCode == System.Net.HttpStatusCode.Created)
            {
                MessageBox.Show("ok!");
            }


        }

        public async void DoShowPassword(object OBJECT_Element)
        {
            HTTPResponseResult httpResponseResult = await _backendInterface.ReadPasswords();

            if (httpResponseResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                /* Removes the objects from the bindend list */
                _groupsPassword.Clear();
                /* Converts the retrieve password list to List<PasswordEntryReadDto> object */
                List<PasswordEntryReadDto> passwordList = (List<PasswordEntryReadDto>)httpResponseResult.OBJECT_RestResult;
                /* Group the passwordList by Group Name */
                IEnumerable<IGrouping<string, PasswordEntryReadDto>> test = passwordList.GroupBy(element => element.Group);

                PasswordGroup passwordGroup = null;
                foreach (var item in test)
                {
                    /* Instantiate a new password group by passing the Group Name */
                    passwordGroup = new PasswordGroup(item.Key);

                    foreach (var element in item)
                    {
                        /* Adds a new Password to the password list */
                        passwordGroup.Passwords.Add(element);
                    }

                    /* Adds the password to the binded object */
                    _groupsPassword.Add(passwordGroup);
                }


            }



        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
