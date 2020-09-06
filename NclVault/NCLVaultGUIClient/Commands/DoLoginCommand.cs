using NclVaultCLIClient.Controllers;
using NclVaultCLIClient.Models;
using NCLVaultGUIClient.Interfaces;
using NCLVaultGUIClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NCLVaultGUIClient.Commands
{
    public class DoLoginCommand : ICommand
    {

        #region Members
        private ILoginViewModel _loginViewModel;
        public event EventHandler CanExecuteChanged { add { } remove { } }
        #endregion

        public DoLoginCommand(ILoginViewModel loginViewModel)
        {
            _loginViewModel = loginViewModel;
        }

        /* Called where the login button is pressed */
        public bool CanExecute(object parameter)
        {
            if (null == parameter)
                return true;

            Credential receivedCredential = (Credential)parameter;
            return (receivedCredential.Username.Length > 0 && receivedCredential.Password.Length > 0);

        }

        /* Called when the Command is valid */
        public void Execute(object parameter)
        {
            /* Calls the ViewModel Function */
            _loginViewModel.DoLogin(parameter);
        }


    }
}
