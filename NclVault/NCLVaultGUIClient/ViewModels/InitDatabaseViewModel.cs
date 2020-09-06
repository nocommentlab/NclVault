using NclVaultFramework.Controllers;
using NclVaultFramework.Models;
using NCLVaultGUIClient.Commands;
using NCLVaultGUIClient.Interfaces;
using NCLVaultGUIClient.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NCLVaultGUIClient.ViewModels
{
    class InitDatabaseViewModel : IInitViewModel, INotifyPropertyChanged
    {
        #region Members
        private readonly BackendInterface _backendInterface;
        private string _STRING_InitId;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public ICommand InitCommand { get; set; }

        public Credential InitCredential { get; set; }

        public string STRING_InitId
        {
            get { return _STRING_InitId; }
            set { _STRING_InitId = value; OnPropertyChanged("STRING_InitId"); }
        }
        #endregion

        public InitDatabaseViewModel()
        {
            InitCredential = new Credential();
            InitCommand = new DoInitCommand(this);
            _backendInterface = BackendInterface.GetInstance();

            _STRING_InitId = "-";


        }

        public async void DoInit(object OBJECT_Parameter)
        {
            Credential receivedCredential = (Credential)OBJECT_Parameter;
            HTTPResponseResult initRestResponse = await _backendInterface.Init(receivedCredential);

            if (initRestResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                /* Updates the property to reflect the change to the UI */
                STRING_InitId = ((InitResponse)initRestResponse.OBJECT_RestResult).InitId;
                /* Creates the init_id.key file persistence */
                byte[] encryptedSecret = ProtectDataManager.Protect(Encoding.UTF8.GetBytes(((InitResponse)initRestResponse.OBJECT_RestResult).InitId));
                File.WriteAllText("init_id.key", $"{Convert.ToBase64String(encryptedSecret)}");
            }
            else
            {
                MessageBox.Show(initRestResponse.StatusCode.ToString());
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
