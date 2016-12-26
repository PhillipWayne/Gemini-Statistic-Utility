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
using CounterSoft.Gemini.WebServices;
using GeminiStatisticUtility.Common.Events;
using GeminiStatisticUtility.Common.Interfaces;
using GeminiStatisticUtility.Models;
using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GeminiStatisticUtility.Common.Services {

    [Export(typeof(IGeminiService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GeminiService : IGeminiService {

        private ServiceManager _gemini;
        private List<IProject> _allprojects;
        private IGeneralData _generaldata;
        private IEventAggregator _eventAggregator;


        [ImportingConstructor]
        public GeminiService(IEventAggregator eventaggregator, IGeneralData generaldata) {
            this._allprojects = new List<IProject>();
            this._generaldata = generaldata;
            this._eventAggregator = eventaggregator;
        }


        // ------------------------------------------------------------------------------
        // ---------------------------- INTERFACE FUNCTIONS -----------------------------
        // ------------------------------------------------------------------------------

        // create and test a new Gemini connection
        public void NewConnection(IConnectionData data) {
            this._gemini = new ServiceManager(data.URL, data.Username, data.Password, string.Empty, data.WindowsAuthentication);
            // Test the new connection ...
            try { 
                var testquery = this._gemini.ProjectsService.GetProjects();
                LoadGeminiData();
            } catch (Exception e) { 
                throw new Exception("Can not connect to Gemini \n" + e.Message); 
            }
        }


        // As soon as a Gemini connections is established try to
        // get the basic Project Data for all available Gemini Projects,
        // and start loading all the additional Data in the background ...
        private void LoadGeminiData() {
            var tmplist = new List<IProject>();
            foreach (var p in this.getAllProjects()) { tmplist.Add(new ProjectModel(p)); }
            // Sort the Project List alphabetically by name
            this._allprojects = tmplist.OrderBy(i => i.Project.ProjectName).ToList();
            // Get the "General" Gemini Data
            this._generaldata.Resolutions = getResolutions();
            this._generaldata.Statuses = getStatuses();
            // publish the new loaded data
            var eventargs = new ProjectDataLoadedEventArgs(this._allprojects, this._generaldata);
            this._eventAggregator.GetEvent<ProjectDataLoadedEvent>().Publish(eventargs);
            // Start loading all the additional data in the background
            GetCompleteProjectData();
        }



        // ------------------------------------------------------------------------------
        // ----------------------- PROJECT INFORMATION GETTERS --------------------------
        // ------------------------------------------------------------------------------

        private void GetCompleteProjectData() {
            var tmplist = new List<List<IProject>>();
            // Split the Project List into 8 equal sized SubLists
            int count = (int)Math.Ceiling((double)this._allprojects.Count / 8);
            for (int i = 0; i < 8; i++) { tmplist.Add(this._allprojects.Skip(i * count).Take(count).ToList()); }
            // Start 8 Background-Loading Threads to get all the additional Project information
            foreach (var list in tmplist) { Task.Run(() => ProjectDataBackgroundLoading(list)); }
        }


        // Background Thread Function: Get all the additional Project data
        private void ProjectDataBackgroundLoading(List<IProject> projectlist) {
            foreach (IProject p in projectlist) {
                p.ChangeLog = getProjectChangeLog(p.ID);
                p.RoadMap = getProjectRoadMap(p.ID);
                p.Types = getTypes(p.ID);
                p.Priorities = getPriorities(p.ID);
                p.Versions = getVersions(p.ID);
                p.AllLoaded = true;
                UpdatedProjectEventArgs eventargs = new UpdatedProjectEventArgs(p);
                this._eventAggregator.GetEvent<UpdatedProjectEvent>().Publish(eventargs);
            }
        }



        // ------------------------------------------------------------------------------
        // ----------------------- PROJECT INFORMATION GETTERS --------------------------
        // ------------------------------------------------------------------------------

        // Basic project Information
        private ProjectEN getProject(int id) { return this._gemini.ProjectsService.GetProject(id); }
        private List<ProjectEN> getAllProjects() { return this._gemini.ProjectsService.GetProjects().ToList(); }
        // Additional project Information
        private List<IssueEN> getProjectChangeLog(int id) { return this._gemini.ProjectsService.GetProjectChangeLog(id).ToList(); }
        private List<IssueEN> getProjectRoadMap(int id) { return this._gemini.ProjectsService.GetProjectRoadMap(id).ToList(); }
        private List<IssueTypeEN> getTypes(int id) { return this._gemini.ProjectsService.GetTypes(id).ToList(); }
        private List<IssuePriorityEN> getPriorities(int id) { return this._gemini.ProjectsService.GetPriorities(id).ToList(); }
        private List<VersionEN> getVersions(int id) { return this._gemini.ProjectsService.GetVersions(id).ToList(); }



        // ------------------------------------------------------------------------------
        // ----------------------- GEMINI INFORMATION GETTERS --------------------------
        // ------------------------------------------------------------------------------

        // Get all Gemini Resolution types, and store Name and ID in a Dictionary 
        private Dictionary<string, int> getResolutions() {
            var tmpres = new Dictionary<string, int>();
            foreach (IssueResolutionEN res in this._gemini.AdminService.GetIssueResolution().ToList()) {
                if (!tmpres.ContainsKey(res.Description)) tmpres.Add(res.Description, res.ResolutionID);
            }
            return tmpres;
        }


        // Get all Gemini Status types, and store Name and ID in a Dictionary 
        private Dictionary<string, int> getStatuses() {
            var tmpstat = new Dictionary<string, int>();
            foreach (IssueStatusEN res in this._gemini.AdminService.GetIssueStatus().ToList()) {
                if (!tmpstat.ContainsKey(res.Description)) tmpstat.Add(res.Description, res.StatusID);
            }
            return tmpstat;
        }


    }

}
