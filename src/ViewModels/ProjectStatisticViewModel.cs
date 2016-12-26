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

using CounterSoft.Gemini.Commons.Entity;
using GeminiStatisticUtility.Common.Events;
using GeminiStatisticUtility.Common.Interfaces;
using GeminiStatisticUtility.Common.Types;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeminiStatisticUtility.ViewModels {

    [Export(typeof(ProjectStatisticViewModel))]
    public class ProjectStatisticViewModel : BindableBase {

        private IProject _selectedproject;
        private VersionEN _selectedversion;
        private IDiagramService _diagramservice;
        private IExportService _exportservice;
        private IGeneralData _generaldata;
        private IEventAggregator _eventAggregator;
        private IProjectStatistic _statisticmodel;
        public string _itemstotal;
        public int _dataloaded;
        public bool _cancalculate;
        public bool _canexport;

        public ICommand CalculateCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }

        public IProject SelectedProject { get { return _selectedproject; } set { SetProperty(ref _selectedproject, value); } }
        public VersionEN SelectedVersion { get { return _selectedversion; } set { SetProperty(ref _selectedversion, value); SetVersionName(); } }
        public IDiagramService DiagramService { get { return _diagramservice; } set { SetProperty(ref _diagramservice, value); } }
        public string ItemsTotal { get { return this._itemstotal; } set { SetProperty(ref this._itemstotal, value); SetItemsTotal(); } }
        public int DataLoaded { get { return this._dataloaded; } set { SetProperty(ref this._dataloaded, value); } }
        public bool CanCalculate { get { return this._cancalculate; } set { SetProperty(ref this._cancalculate, value); } }
        public bool CanExport { get { return this._canexport; } set { SetProperty(ref this._canexport, value); } }


        [ImportingConstructor]
        public ProjectStatisticViewModel(   IEventAggregator eventaggregator, IProject selectedproject, IExportService exportservice,
                                            IDiagramService diagramservice, IGeneralData generaldata, IProjectStatistic statistic) {
            this.SelectedProject = selectedproject;
            this._statisticmodel = statistic;
            this._generaldata = generaldata;
            this.DiagramService = diagramservice;
            this._exportservice = exportservice;
            this._eventAggregator = eventaggregator;
            this._eventAggregator.GetEvent<SelectedProjectEvent>().Subscribe(OnNewProjectSelection);
            this._eventAggregator.GetEvent<UpdatedProjectEvent>().Subscribe(OnProjectUpdated);
            this.CalculateCommand = new DelegateCommand(this.OnCalculateCommand);
            this.ExportCommand = new DelegateCommand(this.OnExportCommand);
            this.StartDate = DateTime.Now;
            this.EndDate = DateTime.Now;
            this.CanCalculate = false;
            this.CanExport = false;
        }



        // ------------------------------------------------------------------------------
        // -------------------------- MODEL PROPERTY WRAPPERS ---------------------------
        // ------------------------------------------------------------------------------

        public PairMVVM<DateTime> StartTimeframe {
            get { return this._statisticmodel.StartTimeframe; }
            set { this._statisticmodel.StartTimeframe = value; OnPropertyChanged("StartTimeframe"); }
        }

        public PairMVVM<DateTime> EndTimeframe {
            get { return this._statisticmodel.EndTimeframe; }
            set { this._statisticmodel.EndTimeframe = value; OnPropertyChanged("EndTimeframe"); }
        }

        public DateTime StartDate {
            get {   return this._statisticmodel.StartDate; }
            set {   var old = this._statisticmodel.StartDate; 
                    this._statisticmodel.StartDate = value;
                    OnDateChange(old, true);
                    OnPropertyChanged("StartDate"); } 
        }

        public DateTime EndDate {
            get {   return this._statisticmodel.EndDate; }
            set {   var old = this._statisticmodel.EndDate;
                    this._statisticmodel.EndDate = value;
                    OnDateChange(old, false);
                    OnPropertyChanged("EndDate");
            } 
        }

        public bool[] ItemFilter {
            get { return this._statisticmodel.ItemFilter; }
            set { this._statisticmodel.ItemFilter = value; OnPropertyChanged("ItemFilter"); }
        }

        public List<string> DiagramSortTypes {
            get { return this._statisticmodel.DiagramSortTypes; }
            set { this._statisticmodel.DiagramSortTypes = value; OnPropertyChanged("DiagramSortTypes"); }
        }

        public string DiagramSortSelected {
            get {   return this._statisticmodel.DiagramSortSelected; }
            // Set the new Sort selection and reload the diagram with according data
            set {   this._statisticmodel.DiagramSortSelected = value; 
                    OnPropertyChanged("DiagramSortSelected"); 
                    RefreshDiagramData(); }
        }



        // ------------------------------------------------------------------------------
        // ------------------------------- EVENT HANDLING -------------------------------
        // ------------------------------------------------------------------------------

        private void OnNewProjectSelection(SelectedProjectEventArgs e) { 
            this.SelectedProject = e.Project;
            if (e.Project.Versions != null) { this.SelectedVersion = e.Project.Versions.ElementAtOrDefault(0); }
            this.DataLoaded = (e.Project.AllLoaded) ? 100 : 10;
            this.CanCalculate = (DataLoaded == 100) ? true : false;
            this.CanExport = false;
            // Reset the View - get the new values
            this.StartTimeframe.First = SelectedProject.Project.DateCreated;
            this.EndTimeframe.First = SelectedProject.Project.DateCreated;
            this.EndDate = DateTime.Now;
            this.StartDate = DateTime.Now;
            this._statisticmodel.RelevantStatisticItems = new List<IssueEN>();
            this._statisticmodel.ChartData = new List<KeyValuePair<string, int>>();
            // RESET DIAGRAM
            // ...
        }


        private void OnProjectUpdated(UpdatedProjectEventArgs e) {
            // Prevent "Null reference Exception" (... appeared only occasionally?!?)
            if (this.SelectedProject != null && e.Project != null) {
                // Update only, if the currently visible Project information changed
                if (e.Project.ID == this.SelectedProject.ID) {
                    this.SelectedProject = null;
                    this.SelectedProject = e.Project;
                    if (e.Project.Versions != null) { this.SelectedVersion = e.Project.Versions.ElementAtOrDefault(0); }
                    this.DataLoaded = (e.Project.AllLoaded) ? 100 : 10;
                    this.CanCalculate = (DataLoaded == 100) ? true : false;
                }
            }
        }



        // ------------------------------------------------------------------------------
        // ------------------------------ GUI INTERACTION -------------------------------
        // ------------------------------------------------------------------------------

        private void OnDateChange(DateTime oldValue, bool isstartdate) {
            if (this.StartDate != DateTime.MinValue && this.EndDate != DateTime.MinValue) {
                // If the startdate is greater than the enddate, 
                // display an warning message and reset the value
                if (this.StartDate.Date > this.EndDate.Date) {
                    MessageBox.Show("Start-Date cannot be greater than End-Date", "Note");
                    if (isstartdate) { this.StartDate = oldValue; }
                    else { this.EndDate = oldValue; }
                }
                this._statisticmodel.StartDate = this.StartDate;
                this._statisticmodel.EndDate = this.EndDate;
            }
        }


        private void OnCalculateCommand() {
            if (this.SelectedVersion != null) {
                this._statisticmodel.RelevantStatisticItems = new List<IssueEN>();
                var allProjectItems = new List<IssueEN>();
                // Put all the Changelog and Roadmap items into one big item list
                if (this.SelectedProject.ChangeLog != null && this.SelectedProject.RoadMap != null) {
                    allProjectItems.AddRange(this.SelectedProject.ChangeLog);
                    allProjectItems.AddRange(this.SelectedProject.RoadMap);
                }
                foreach (IssueEN item in allProjectItems) {
                    // check if the item is in the selected timeframe of a specific filter
                    bool inTimeframe = false;
                    if (this.ItemFilter[0]) { inTimeframe = CheckDate(item.DateCreated); }
                    else if (this.ItemFilter[1]) { inTimeframe = CheckDate(item.DateRevised); }
                    else if (this.ItemFilter[2]) { inTimeframe = CheckDate(item.ResolvedDate); }
                    else if (this.ItemFilter[3]) { inTimeframe = CheckDate(item.ClosedDate); }
                    // check if the item is in any way associated with the selected version
                    bool inVersion = false;
                    foreach (AffectedVersionEN affversion in item.AffectedVersion) {
                        if (affversion.Version == this.SelectedVersion) { inVersion = true; }
                    }
                    if (item.FixedInVersionDesc == this.SelectedVersion.VersionDesc) { inVersion = true; }
                    // if all criteria are met, add the item to the list of relevant items
                    if (inTimeframe && inVersion) { _statisticmodel.RelevantStatisticItems.Add(item); }
                }
                // Display the calculated results
                this.ItemsTotal = _statisticmodel.RelevantStatisticItems.Count.ToString();
                this.CanExport = true;
                RefreshDiagramData();
            }
        }


        private void OnExportCommand() {
            string tmppath = string.Empty;
            // Get "Save to" path with the Windows-Save-Dialog
            SaveFileDialog savedialog = new SaveFileDialog();
            savedialog.FileName = "GeminiStatistic_Export_" + DateTime.Now.ToString("dd/MM/yyyy");
            savedialog.DefaultExt = ".xlsx";
            savedialog.AddExtension = true;
            savedialog.ShowDialog();
            if (savedialog.FileName != string.Empty) { tmppath = savedialog.FileName; }
            // Create a .jpg file of the calcualted Diagram and start the export
            if (this._diagramservice.Diagram.Series[0] != null) {
                // Convert a WPFToolkit "Chart" to a "DrawingVisual":
                // -----------------------------------------------------------------------------------------------------------
                // SOURCE: http://stackoverflow.com/questions/10132845/how-to-export-chart-from-wpf-toolkit-ms-chart-to-png-it-doesnt-work-it-crea
                // BY: by Scott on April 13 2012 at 15:48
                Rect bounds = VisualTreeHelper.GetDescendantBounds(this._diagramservice.Diagram);
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);
                DrawingVisual isolatedVisual = new DrawingVisual();
                using (DrawingContext drawing = isolatedVisual.RenderOpen()) {
                    drawing.DrawRectangle(Brushes.White, null, new Rect(new Point(), bounds.Size)); // Optional Background
                    drawing.DrawRectangle(new VisualBrush(this._diagramservice.Diagram), null, new Rect(new Point(), bounds.Size));
                }
                renderBitmap.Render(isolatedVisual);
                // Encode the RenderTarget and create a .jpg file
                string imagepath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\tmpimage.jpg";
                using (FileStream outStream = new FileStream(imagepath, FileMode.Create)) {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    encoder.Save(outStream);
                }
                // -----------------------------------------------------------------------------------------------------------
                // Start the export
                try { this._exportservice.ExportData(tmppath, imagepath, _selectedproject, _statisticmodel); }
                catch (Exception e) { MessageBox.Show("Error creating EXCEL-File\n\n" + e.Message, "Error"); }
                finally { File.Delete(imagepath); }
            }
        }



        // ------------------------------------------------------------------------------
        // -------------------------- DIAGRAM DATA FUNCTIONS ----------------------------
        // ------------------------------------------------------------------------------

        private void RefreshDiagramData() {
            // Using the "SelectedProject" and the "RelevantStatisticItems"
            if (this.SelectedProject != null && _statisticmodel.RelevantStatisticItems != null) {
                this._statisticmodel.ChartData = new List<KeyValuePair<string, int>>();
                if (this.DiagramSortTypes.Count == 4) {
                    if (this.DiagramSortSelected == this.DiagramSortTypes[0]) {
                        this._statisticmodel.ChartData = GetTypeDiagramData(_statisticmodel.RelevantStatisticItems); 
                    } else if (this.DiagramSortSelected == this.DiagramSortTypes[1]) {
                        this._statisticmodel.ChartData = GetStatusDiagramData(_statisticmodel.RelevantStatisticItems); 
                    } else if (this.DiagramSortSelected == this.DiagramSortTypes[2]) {
                        this._statisticmodel.ChartData = GetResolutionDiagramData(_statisticmodel.RelevantStatisticItems); 
                    } else if (this.DiagramSortSelected == this.DiagramSortTypes[3]) {
                        this._statisticmodel.ChartData = GetPriorityDiagramData(_statisticmodel.RelevantStatisticItems); 
                    }
                }
                this.DiagramService.SetChartData<string, int>(this._statisticmodel.ChartData, DiagramType.PIE);
            }
        }


        // Create Diagram Data sorted by the Item Types
        private List<KeyValuePair<string, int>> GetTypeDiagramData(List<IssueEN> items) {
            var tmpdata = new Dictionary<string, int>();
            // get all the types available in the selected Project
            foreach (var type in this.SelectedProject.Types) { tmpdata.Add(type.Description, 0); }
            // count all the type occurrences, for all calculated items
            foreach (var item in items) { if (tmpdata.ContainsKey(item.IssueTypeDesc)) tmpdata[item.IssueTypeDesc]++; }
            // remove data if type count is zero
            var badKeys = tmpdata.Where(kvpair => kvpair.Value == 0).ToList();
            foreach (var badKey in badKeys) { tmpdata.Remove(badKey.Key); }
            return tmpdata.ToList();
        }


        // Create Diagram Data sorted by the Item Status
        private List<KeyValuePair<string, int>> GetStatusDiagramData(List<IssueEN> items) {
            var tmpdata = new Dictionary<string, int>();
            // get all the status names available in the selected Project
            foreach (var status in this._generaldata.Statuses) { tmpdata.Add(status.Key, 0); }
            // count all the status occurrences, for all calculated items
            foreach (var item in items) { if (tmpdata.ContainsKey(item.IssueStatusDesc)) tmpdata[item.IssueStatusDesc]++; }
            // remove data if status count is zero
            var badKeys = tmpdata.Where(kvpair => kvpair.Value == 0).ToList();
            foreach (var badKey in badKeys) { tmpdata.Remove(badKey.Key); }
            return tmpdata.ToList();
        }


        // Create Diagram Data sorted by the Item Resolutions
        private List<KeyValuePair<string, int>> GetResolutionDiagramData(List<IssueEN> items) {
            var tmpdata = new Dictionary<string, int>();
            // get all the resolution names available in the selected Project
            foreach (var res in this._generaldata.Resolutions) { tmpdata.Add(res.Key, 0); }
            // count all the resolution occurrences, for all calculated items
            foreach (var item in items) { if (tmpdata.ContainsKey(item.IssueResolutionDesc)) tmpdata[item.IssueResolutionDesc]++; }
            // remove data if resolution count is zero
            var badKeys = tmpdata.Where(kvpair => kvpair.Value == 0).ToList();
            foreach (var badKey in badKeys) { tmpdata.Remove(badKey.Key); }
            return tmpdata.ToList();
        }


        // Create Diagram Data sorted by the Item Priorities
        private List<KeyValuePair<string, int>> GetPriorityDiagramData(List<IssueEN> items) {
            var tmpdata = new Dictionary<string, int>();
            // get all the priority names available in the selected Project
            foreach (var prio in this.SelectedProject.Priorities) { tmpdata.Add(prio.Description, 0); }
            // count all the resolution occurrences, for all calculated items
            foreach (var item in items) { if (tmpdata.ContainsKey(item.IssuePriorityDesc)) tmpdata[item.IssuePriorityDesc]++; }
            // remove data if resolution count is zero
            var badKeys = tmpdata.Where(kvpair => kvpair.Value == 0).ToList();
            foreach (var badKey in badKeys) { tmpdata.Remove(badKey.Key); }
            return tmpdata.ToList();
        }



        // ------------------------------------------------------------------------------
        // ----------------------------- HELPER FUNCTIONS -------------------------------
        // ------------------------------------------------------------------------------

        // Check if "date" is between the "Startdate" and the "Enddate"
        private bool CheckDate(DateTime date) {
            if(date.Date > this.StartDate.Date && date.Date < this.EndDate.Date) return true;
            else return false;
        }

        // True if directory exists and is valid a file
        public bool CheckPath(string path) { 
            return (Directory.Exists(path) && File.Exists(path)); 
        }

        // Save the currently selected Version Name
        private void SetVersionName() {
            if(this.SelectedVersion != null) { this._statisticmodel.VersionName = SelectedVersion.VersionName; }
        }

        // Parse ItemsTotal and pass the INT value to the model
        private void SetItemsTotal() {
            if (this.ItemsTotal != string.Empty) { this._statisticmodel.ItemsTotal = Int32.Parse(ItemsTotal); }
        }


    }
}
