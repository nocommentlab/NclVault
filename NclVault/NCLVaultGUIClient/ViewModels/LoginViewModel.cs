using NclVaultFramework.Controllers;
using NclVaultFramework.Models;
using NCLVaultGUIClient.Commands;
using NCLVaultGUIClient.Interfaces;
using NCLVaultGUIClient.Models;
using NCLVaultGUIClient.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NCLVaultGUIClient.ViewModels
{
    class LoginViewModel : ILoginViewModel, INotifyPropertyChanged
    {
        #region Members
        private readonly BackendInterface _backendInterface;
        private bool _IsWindowVisible = true;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public Credential LoginCredential { get; set; }
        public ICommand LoginCommand { get; set; }
        public bool IsWindowVisible { get => _IsWindowVisible; set{ _IsWindowVisible = value; OnPropertyChanged("IsWindowVisible"); } }
        #endregion

        public LoginViewModel()
        {

            LoginCredential = new Credential();

            /* Declares the command to do the login */
            LoginCommand = new DoLoginCommand(this);

            _backendInterface = BackendInterface.GetInstance();

                        
        }

        public async void DoLogin(object OBJECT_Parameter)
        {
            Credential receivedCredential = (Credential)OBJECT_Parameter;

            HTTPResponseResult httpResponseResult = await _backendInterface.Login(receivedCredential, ProtectDataManager.Unprotect("init_id.key"));
            
            if(httpResponseResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                /* Hides the login window */
                IsWindowVisible = false;
                
                new Home().Show();
            }
                
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
