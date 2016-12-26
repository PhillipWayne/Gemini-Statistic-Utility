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
using GeminiStatisticUtility.Common.Interfaces;
using GeminiStatisticUtility.Common.Types;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace GeminiStatisticUtility.Models {

    [Export(typeof(IProjectStatistic))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ProjectStatisticModel : BindableBase, IProjectStatistic {

        public List<IssueEN> RelevantStatisticItems { get; set; }
        public List<KeyValuePair<string, int>> ChartData { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PairMVVM<DateTime> StartTimeframe { get; set; }
        public PairMVVM<DateTime> EndTimeframe { get; set; }
        public bool[] ItemFilter { get; set; }
        public List<string> DiagramSortTypes { get; set; }
        public string DiagramSortSelected { get; set; }
        public string VersionName { get; set; }

        public int ItemsTotal { get; set; }

        public ProjectStatisticModel() {
            this.StartTimeframe = new PairMVVM<DateTime>(DateTime.Now, DateTime.Now);
            this.EndTimeframe = new PairMVVM<DateTime>(DateTime.Now, DateTime.Now);
            this.StartDate = new DateTime();
            this.EndDate = new DateTime();
            this.ItemFilter = new bool[] { true, false, false, false };
            this.DiagramSortTypes = new List<string>() { "Type", "Status", "Resolution", "Priority" };
            this.DiagramSortSelected = this.DiagramSortTypes[0];
            this.VersionName = string.Empty;
        }


    }
}
