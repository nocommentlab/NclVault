using NCLVaultGUIClient.Interfaces;
using NCLVaultGUIClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NCLVaultGUIClient.Commands
{
    class DoInitCommand : ICommand
    {
        #region Members
        public event EventHandler CanExecuteChanged { add { } remove { } }
        private IInitViewModel _initViewModel;
        #endregion

        #region Properties

        #endregion

        public DoInitCommand(IInitViewModel initViewModel)
        {
            _initViewModel = initViewModel;
        }
        public bool CanExecute(object parameter)
        {
            if (null == parameter)
                return true;

            Credential receivedCredential = (Credential)parameter;
            return (receivedCredential.Username.Length > 0 && receivedCredential.Password.Length > 0);
        }

        public void Execute(object parameter)
        {
            _initViewModel.DoInit(parameter);
        }
    }
}
