using NclVaultCLIClient.Controllers;
using NclVaultCLIClient.Models;
using NCLVaultGUIClient.Commands;
using NCLVaultGUIClient.Interfaces;
using NCLVaultGUIClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NCLVaultGUIClient.ViewModels
{
    class LoginViewModel : ILoginViewModel
    {
        #region Members
        private Credential _Credential;
        private ICommand _COMMAND_Login;
        private BackendInterface _backendInterface;
        #endregion

        #region Properties
        public Credential LoginCredential
        {
            get { return _Credential; }
            set { _Credential = value; }
        }
        public ICommand LoginCommand
        {
            get
            {
                
                return _COMMAND_Login;
            }
            set
            {
                _COMMAND_Login = value;
            }
        }
        #endregion

        public LoginViewModel()
        {

            _Credential = new Credential();

            /* Declares the command to do the login */
            _COMMAND_Login = new DoLoginCommand(this);

            _backendInterface = BackendInterface.GetInstance();
            
        }

        public async void DoLogin(object OBJECT_Parameter)
        {
            Credential receivedCredential = (Credential)OBJECT_Parameter;
            HTTPResponseResult test = await _backendInterface.Login(receivedCredential, "");
                
        }
    }
}
