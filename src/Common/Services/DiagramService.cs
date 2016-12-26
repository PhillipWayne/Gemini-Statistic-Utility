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
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls.DataVisualization.Charting;

namespace GeminiStatisticUtility.Common.Services {

    [Export(typeof(IDiagramService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DiagramService : BindableBase, IDiagramService {

        private Chart _diagram;
        private DataPointSeries _data;

        public Chart Diagram { get { return this._diagram; } private set { SetProperty(ref this._diagram, value); } }


        public DiagramService() { this._diagram = new Chart(); }


        public void SetChartData<T, U>(List<KeyValuePair<T, U>> data, DiagramType type) {
            this._data = ChartTypeConverter(type);
            this._data.ItemsSource = data;
            this._data.DependentValuePath = "Value";
            this._data.IndependentValuePath = "Key";
            // Reload the Diagram
            this.Diagram = new Chart();
            this._data.SeriesHost = null;
            this.Diagram.Series.Add(_data);
        }


        // Get a new Data-Series according to the DiagramType (ENUM)
        private DataPointSeries ChartTypeConverter(DiagramType type) {
            if (type == DiagramType.LINE) return new LineSeries();
            else if (type == DiagramType.COLUMN) return new ColumnSeries();
            else if (type == DiagramType.AREA) return new AreaSeries();
            else if (type == DiagramType.PIE) return new PieSeries();
            // Get Pie as default
            else return new PieSeries();
        }


    }

}
