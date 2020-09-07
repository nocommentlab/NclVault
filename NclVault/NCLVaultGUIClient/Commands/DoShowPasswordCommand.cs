using NCLVaultGUIClient.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NCLVaultGUIClient.Commands
{
    class DoShowPasswordCommand : ICommand
    {
        private readonly IDoShowPassword _doShowPassword;

        public event EventHandler CanExecuteChanged;


        public DoShowPasswordCommand(IDoShowPassword doShowPassword)
        {
            _doShowPassword = doShowPassword;
        }
        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
