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

using GeminiStatisticUtility.Common;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace GeminiStatisticUtility.ViewModels {

    [Export(typeof(MainWindowViewModel))]
    public class MainWindowViewModel : BindableBase {

        private bool appstart;
        private IEventAggregator _eventAggregator;
        private UserControl _projectslistview;
        private UserControl _projectsstatview;
        private Window _loginwindowview;

        // ----------------------------------------------------------------------------------------------------
        // ViewModel "MainWindowViewModel" has access to the Views. Not the best Solution, but it's simple. 
        // (Still, no dependencies! ... and the basic binding satisfies at least some MVVM and DI standards)
        // ----------------------------------------------------------------------------------------------------
        [Import(ViewNames.ListViewName)]
        public UserControl ProjectsListView { get { return _projectslistview; } set { SetProperty(ref _projectslistview, value); } }

        [Import(ViewNames.StatisticViewName)]
        public UserControl ProjectsStatisticView { get { return _projectsstatview; } set { SetProperty(ref _projectsstatview, value); } }

        [Import(ViewNames.LoginWindowView)]
        public Window LoginWindowView { get { return _loginwindowview; } set { SetProperty(ref _loginwindowview, value); } }
        // ----------------------------------------------------------------------------------------------------


        [ImportingConstructor]
        public MainWindowViewModel(IEventAggregator eventaggregator) {
            this._eventAggregator = eventaggregator;
            this.appstart = true;
        }


        public void MainWindowViewLoaded(object sender, RoutedEventArgs e) {
            if (appstart) this.LoginWindowView.ShowDialog();
            appstart = false;
        }


    }

}
