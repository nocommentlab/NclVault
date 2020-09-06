using NCLVaultGUIClient.Commands;
using NCLVaultGUIClient.Interfaces;
using NCLVaultGUIClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NCLVaultGUIClient.ViewModels
{
    class InitDatabaseViewModel : IInitViewModel
    {
        #region Members
        private Credential _credential;
        private ICommand _doInitCommand;
        #endregion

        #region Properties
        public ICommand InitCommand
        {
            get { return _doInitCommand; }
            set { _doInitCommand = value; }
        }

        public Credential InitCredential
        {
            get { return _credential; }
            set { _credential = value; }
        }
        #endregion

        public InitDatabaseViewModel()
        {
            _credential = new Credential();
            _doInitCommand = new DoInitCommand(this);
        }

        public void DoInit(object OBJECT_Parameter)
        {
            throw new NotImplementedException();
        }
    }
}
