using NclVaultFramework.Controllers;
using NclVaultFramework.Models;
using NCLVaultGUIClient.Commands;
using NCLVaultGUIClient.Interfaces;
using System;
using System.Collections.Generic;
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

        private PasswordEntryCreateDto _passwordEntryCreateDto;

        public event PropertyChangedEventHandler PropertyChanged;
        private List<PasswordEntryReadDto> _lPassword;

        public List<PasswordEntryReadDto> PasswordList
        {
            get { return _lPassword; }
            set { _lPassword = value; }
        }

        #endregion

        #region Properties
        public PasswordEntryCreateDto PasswordEntryCreateDto { get => _passwordEntryCreateDto; set { _passwordEntryCreateDto = value; OnPropertyChanged("PasswordEntryCreateDto"); } }
        #endregion

        public HomeViewModel()
        {
            _backendInterface = BackendInterface.GetInstance();

            CreatePasswordCommand = new DoCreatePasswordCommand(this);
            ShowPasswordCommand = new DoShowPasswordCommand(this);

            _passwordEntryCreateDto = new PasswordEntryCreateDto();

            HTTPResponseResult httpResponseResult = _backendInterface.ReadPasswords().GetAwaiter().GetResult();
            _lPassword = (List<PasswordEntryReadDto>)httpResponseResult.OBJECT_RestResult;
        }
        public async void DoCreatePassword(object OBJECT_Element)
        {
            HTTPResponseResult httpResponseResult = await _backendInterface.CreatePassword((PasswordEntryCreateDto)OBJECT_Element);
            if(httpResponseResult.StatusCode == System.Net.HttpStatusCode.Created)
            {
                MessageBox.Show("ok!");
            }
        }

        public void DoShowPassword(object OBJECT_Element)
        {
            throw new NotImplementedException();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
