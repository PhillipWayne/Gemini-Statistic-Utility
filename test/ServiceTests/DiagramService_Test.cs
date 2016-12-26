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
using GeminiStatisticUtility.Common.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GeminiStatisticUtility_UnitTests.ServiceTests {

    [TestClass]
    public class DiagramServiceTest {

        // ----------------------------------------------------------------------------------
        // Check if the Diagram is properly initialized, when a new DiagramService is created
        // ----------------------------------------------------------------------------------
        [TestMethod]
        public void DiagramService_Initialization_Test() {
            // Create new DiagramService
            IDiagramService diagramservice = new DiagramService();
            // Check the DiagramService object
            Assert.IsNotNull(diagramservice.Diagram);
            Assert.IsInstanceOfType(diagramservice, typeof(DiagramService));
        }


        // ---------------------------------------------------------------------------------------
        // Add some Testdata via "SetChartData" to a Diagram and check if it is stored as a Series
        // ---------------------------------------------------------------------------------------
        [TestMethod]
        public void DiagramService_SetChartData_Test() {
            // Create new DiagramService and Test-Data
            DiagramService diagramservice = new DiagramService();
            var testdata = new List<KeyValuePair<int, string>>() { new KeyValuePair<int, string>(1, "test") };
            // Execute SetChartData Function
            diagramservice.SetChartData<int, string>(testdata, DiagramType.LINE);
            // Check if a new Diagram was created and a DataSeries was added
            Assert.IsNotNull(diagramservice.Diagram);
            Assert.IsNotNull(diagramservice.Diagram.Series);
        }


    }

}
