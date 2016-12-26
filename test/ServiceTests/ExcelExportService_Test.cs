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
using GeminiStatisticUtility.Common.Services;
using GeminiStatisticUtility.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace GeminiStatisticUtility_UnitTests.ServiceTests {

    [TestClass]
    public class ExcelExportServiceTest {

        // -----------------------------------------------------------------------
        // Create a new ExcelExportService and check if it is properly initialized         
        // -----------------------------------------------------------------------
        [TestMethod]
        public void ExcelExportService_Initialization_Test() {
            // Create new ExcelExportService
            IExportService excelexportservice = new ExcelExportService();
            // Check the ExcelExportService object
            Assert.IsNotNull(excelexportservice);
            Assert.IsInstanceOfType(excelexportservice, typeof(ExcelExportService));
        }


        // -----------------------------------------------------------------------------------------------
        // Create empty Project and Statistic Data, pass it to the ExcelExport and create a new Excel file
        // -----------------------------------------------------------------------------------------------
        [TestMethod]
        public void ExcelExportService_ExportData_Test() {
            // Create new ExcelExportService and the Test-Data
            ExcelExportService excelexportservice = new ExcelExportService();
            // Create test data to fill the Excel file
            IProject testproject = new ProjectModel(new ProjectEN());
            IProjectStatistic teststatistic = new ProjectStatisticModel();
            teststatistic.ChartData = new List<KeyValuePair<string, int>>();
            string testpath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\TEST.xlsx";
            // Execute ExportData Function
            excelexportservice.ExportData(testpath, "", testproject, teststatistic);
            System.Threading.Thread.Sleep(1000);
            // Check if a new Excel File was created
            bool fileexists = File.Exists(testpath);
            Assert.IsTrue(fileexists);
        }


    }

}
