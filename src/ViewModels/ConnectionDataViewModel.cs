/*************************************************************************************
 *  Gemini Statistic Utility                                                         *
 *-----------------------------------------------------------------------------------*
 *  Copyright (c) 2016, Peter Baumann                                                *
 *  All rights reserved.                                                             *
 *                                                                                   *
 *  Redistribution and use in source and binary forms, with or without               *
 *  modification, are permitted provided that the following conditions are met:      *
 *    1. Redistributions of source code must retain the above copyright              *
 *       notice, this list of conditions and the following disclaimer.               *
 *    2. Redistributions in binary form must reproduce the above copyright           *
 *       notice, this list of conditions and the following disclaimer in the         *
 *       documentation and/or other materials provided with the distribution.        *
 *    3. Neither the name of the organization nor the                                *
 *       names of its contributors may be used to endorse or promote products        *
 *       derived from this software without specific prior written permission.       *
 *                                                                                   *
 *  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND  *
 *  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED    *
 *  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE           *
 *  DISCLAIMED. IN NO EVENT SHALL PETER BAUMANN BE LIABLE FOR ANY                    *
 *  DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES       *
 *  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;     *
 *  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND      *
 *  ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT       *
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS    *
 *  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.                     *
 *                                                                                   *
 *************************************************************************************/

using GeminiStatisticUtility.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;

namespace GeminiStatisticUtility.ViewModels {

    [Export(typeof(ConnectionDataViewModel))]
    public class ConnectionDataViewModel : BindableBase {

        private IConnectionData _connectiondata;
        private IGeminiService _gemini;
        private IEventAggregator _eventAggregator;
        private bool _closewithlogin;
        private bool _userdataenabled;

        public ICommand LoginClickCommand { get; private set; }
        public bool UserDataEnabled { get { return _userdataenabled; } set { SetProperty(ref _userdataenabled, value); } }

        [ImportingConstructor]
        public ConnectionDataViewModel(IConnectionData condata, IGeminiService gemini, IEventAggregator eventaggregator) {
            this._connectiondata = condata;
            this._gemini = gemini;
            this._eventAggregator = eventaggregator;
            this.LoginClickCommand = new DelegateCommand<Window>(this.OnLoginClick);
            this._closewithlogin = false;
        }



        // ------------------------------------------------------------------------------
        // --------------------------- MODEL PROPERTY WRAPPER ---------------------------
        // ------------------------------------------------------------------------------

        public string URL {
            get { return this._connectiondata.URL; }
            set { this._connectiondata.URL = value; }
        }

        public string Username {
            get { return this._connectiondata.Username; }
            set { this._connectiondata.Username = value; OnPropertyChanged("Username"); }
        }

        public string Password {
            get { return this._connectiondata.Password; }
            set { this._connectiondata.Password = value; OnPropertyChanged("Password"); }
        }

        public bool WindowsAuthentication {
            get { return this._connectiondata.WindowsAuthentication; }
            set { this._connectiondata.WindowsAuthentication = value; SetUserDataEnabled();}
        }



        // ------------------------------------------------------------------------------
        // ------------------------------ GUI INTERACTION -------------------------------
        // ------------------------------------------------------------------------------

        private void OnLoginClick(Window window) {
            try { 
                this._gemini.NewConnection(this._connectiondata);
                _closewithlogin = true;
                window.Close(); 
            } catch (Exception e) { 
                _closewithlogin = false;
                MessageBox.Show(e.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning); 
            }
        }


        // Shutdown the whole Application, if the Login-window is closed with "X"
        public void OnWindowClose(object sender, EventArgs e) {
            if(!_closewithlogin) Application.Current.Shutdown();
        }



        // ------------------------------------------------------------------------------
        // ----------------------------- HELPER FUNCTIONS -------------------------------
        // ------------------------------------------------------------------------------

        // If the User-Login-Data gets disabled, reset Username and Password
        private void SetUserDataEnabled() {
            this.UserDataEnabled = !this.WindowsAuthentication;
            if (!this.UserDataEnabled) {
                this.Username = string.Empty;
                this.Password = string.Empty;
            }
        }


    }

}
