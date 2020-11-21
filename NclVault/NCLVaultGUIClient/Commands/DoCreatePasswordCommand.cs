using NclVaultFramework.Models;
using NCLVaultGUIClient.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NCLVaultGUIClient.Commands
{
    class DoCreatePasswordCommand : ICommand
    {
        private readonly IDoCreatePassword _doCreatePassword;

        public event EventHandler CanExecuteChanged;

        public DoCreatePasswordCommand(IDoCreatePassword doCreatePassword)
        {
            _doCreatePassword = doCreatePassword;
        }
        public bool CanExecute(object parameter)
        {
            if (parameter == null)
                return true;

            /* Avoid the Designer error */
            /*if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return true;*/

            PasswordEntryCreateDto passwordEntryCreateDto = (PasswordEntryCreateDto)parameter;

            return (passwordEntryCreateDto.Name.Length > 0 &&
                    passwordEntryCreateDto.Username.Length > 0 && 
                    passwordEntryCreateDto.Password.Length >0);
            
        }

        public void Execute(object parameter)
        {
            _doCreatePassword.DoCreatePassword(parameter);
        }
    }
}
