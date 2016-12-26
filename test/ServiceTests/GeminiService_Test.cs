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
using GeminiStatisticUtility.Models;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GeminiStatisticUtility_UnitTests.ServiceTests {

    [TestClass]
    public class GeminiServiceTest {

        private const string defaultpath = "http://devenv/Gemini/";

        // ------------------------------------------------------------------
        // Create a new GeminiService and check if it is properly initialized         
        // ------------------------------------------------------------------
        [TestMethod]
        public void GeminiService_Initialization_Test() {
            // Create new DiagramService
            IGeminiService geminiservice = new GeminiService(new EventAggregator(), new GeneralDataModel());
            // Check the DiagramService object
            Assert.IsNotNull(geminiservice);
            Assert.IsInstanceOfType(geminiservice, typeof(GeminiService));
        }


        // -------------------------------------------------------------------------------------------
        // Try to establish a new Gemini Connection, with the "defaultpath" and Windows-Authentication
        // -------------------------------------------------------------------------------------------
        [TestMethod]
        public void GeminiService_NewConnection_Test() {
            Exception testexception = null;
            IGeminiService geminiservice = new GeminiService(new EventAggregator(), new GeneralDataModel());
            IConnectionData testdata = new ConnectionDataModel();
            testdata.URL = defaultpath;
            // Try Default-Connecton with Windows-Authentication
            try { geminiservice.NewConnection(testdata); }
            catch (Exception e) { testexception = e; }
            // Check if an Exeption was thrown by "NewConnection"
            Assert.IsNull(testexception);
        }


    }

}
