using NclVaultFramework.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCLVaultGUIClient.Models
{
    /// <summary>
    /// Identify a simple container of passwords per group
    /// </summary>
    class PasswordGroup
    {
        #region Members
        private string _STRING_GroupName;
        private ObservableCollection<PasswordEntryReadDto> _passwords;
        #endregion
        
        #region Properties
        public ObservableCollection<PasswordEntryReadDto> Passwords
        {
            get { return _passwords; }
            set { _passwords = value; }
        }

        public string GroupName
        {
            get { return _STRING_GroupName; }
        }
        #endregion





        public PasswordGroup(string groupName)
        {
            Passwords = new ObservableCollection<PasswordEntryReadDto>();
            _STRING_GroupName = groupName;
        }
    }
}
