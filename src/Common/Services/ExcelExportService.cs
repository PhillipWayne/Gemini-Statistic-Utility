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

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using GeminiStatisticUtility.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GeminiStatisticUtility.Common.Services {

    [Export(typeof(IExportService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ExcelExportService : IExportService {


        public void ExportData(string exportpath, string imagepath, IProject project, IProjectStatistic statistic) {
            // Create a spreadsheet document by supplying the filepath.
            SpreadsheetDocument exceldoc = SpreadsheetDocument.Create(exportpath, SpreadsheetDocumentType.Workbook);
            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = exceldoc.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();
            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());
            // Add Sheets to the Workbook.
            Sheets sheets = workbookpart.Workbook.AppendChild<Sheets>(new Sheets());
            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { Id = workbookpart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "data" };
            sheets.Append(sheet);
            // Add a StylesPart to the WorkbookPart ( Stores different font settings, etc. )
            WorkbookStylesPart stylespart = exceldoc.WorkbookPart.AddNewPart<WorkbookStylesPart>();
            stylespart.Stylesheet = CreateStyleSheet();
            // Save the workbook ( with one worksheet and one stylesheet )
            stylespart.Stylesheet.Save();
            workbookpart.Workbook.Save();

            // Add all the Excel-Table-Data, at hardcoded positions
            AddGeneralInformation(project, worksheetPart);
            AddSelectedSettings(statistic, worksheetPart);
            AddDiagramData(statistic, worksheetPart);
            // Add the Diagram image, if one exists
            if (imagepath != string.Empty) { InsertImage(worksheetPart, imagepath); }

            // Close the document.
            exceldoc.Close();
        }



        // ------------------------------------------------------------------------------
        // ---------------------- TABLE DATA CREATION FUNCTIONS -------------------------
        // ----------------- (HARDCODED POSITIONS IN THE SPREADSHEET) -------------------
        // ------------------------------------------------------------------------------

        private void AddGeneralInformation(IProject project, WorksheetPart worksheetPart) {
            InsertText(1, "A", project.Project.ProjectName, true, worksheetPart);
            InsertText(1, "B", ("[ " + project.Project.ProjectID.ToString() + " ]"), true, worksheetPart);
        }


        private void AddSelectedSettings(IProjectStatistic statistic, WorksheetPart worksheetPart) {
            InsertText(3, "A", "Project Version:", true, worksheetPart);
            InsertText(3, "B", statistic.VersionName, false, worksheetPart);
            InsertText(4, "A", "Timeframe:", true, worksheetPart);
            string timeframe = statistic.StartDate.ToString("dd/MM/yyyy") + "  -  " + statistic.EndDate.ToString("dd/MM/yyyy");
            InsertText(4, "B", timeframe, false, worksheetPart);
            InsertText(5, "A", "Items:", true, worksheetPart);
            string itemfilter = string.Empty;
            if (statistic.ItemFilter[0]) { itemfilter = "Ceated"; }
            else if (statistic.ItemFilter[1]) { itemfilter = "Revised"; }
            else if (statistic.ItemFilter[2]) { itemfilter = "Resolved"; }
            else if (statistic.ItemFilter[3]) { itemfilter = "Closed"; }
            InsertText(5, "B", itemfilter, false, worksheetPart);
        }


        private void AddDiagramData(IProjectStatistic statistic, WorksheetPart worksheetPart) {
            InsertText(7, "A", statistic.DiagramSortSelected, true, worksheetPart);
            InsertText(7, "B", "Count", true, worksheetPart);
            InsertText(7, "C", "Percent", true, worksheetPart);
            uint rowcount = 8;
            foreach (KeyValuePair<string, int> data in statistic.ChartData) {
                string tmpkey = data.Key;
                if (tmpkey[tmpkey.Length - 1] == '*') { tmpkey = tmpkey.Remove(tmpkey.Length - 1); }
                InsertText(rowcount, "A", tmpkey, false, worksheetPart);
                InsertText(rowcount, "B", data.Value.ToString(), false, worksheetPart);
                InsertText(rowcount, "C", ((Convert.ToDouble(data.Value) / statistic.ItemsTotal) * 100).ToString("0.00"), false, worksheetPart);
                ++rowcount;
            }
            InsertText(rowcount, "A", "Total:", true, worksheetPart);
            InsertText(rowcount, "B", statistic.ItemsTotal.ToString(), true, worksheetPart);
            InsertText(rowcount, "C", "100 %", true, worksheetPart);
        }



        // --------------------------------------------------------------------------------
        // ------------------------------ HELPER FUNCTIONS --------------------------------
        // --------------------------------------------------------------------------------

        // Add some Text to a Cell in the Excel Worksheet
        private void InsertText(uint row, string column, string text, bool bold, WorksheetPart worksheetPart) {
            Cell cell = InsertCellInWorksheet(column, row, worksheetPart);
            cell.CellValue = new CellValue(text);
            cell.DataType = new EnumValue<CellValues>(CellValues.String);
            if (bold) { cell.StyleIndex = 1; }
            // Save the changed worksheet
            worksheetPart.Worksheet.Save();
        }


        // Predefine some Style Values ( Fonts, Fills, ... )
        private Stylesheet CreateStyleSheet() {
            // Create a new Stylesheet
            var tmpstyle = new Stylesheet();
            // Add Font Styles ( default and bold )
            tmpstyle.Fonts = new Fonts();
            tmpstyle.Fonts.Count = 2;
            tmpstyle.Fonts.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Font(new FontSize() { Val = 11 }, new FontName() { Val = "Calibri" }));
            tmpstyle.Fonts.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Font(new Bold(), new FontSize() { Val = 11 }, new FontName() { Val = "Calibri" }));
            // Add Cell-Fill Styles ( only default )
            tmpstyle.Fills = new Fills();
            tmpstyle.Fills.Count = 1;
            tmpstyle.Fills.AppendChild(new Fill());
            // Add Cell-Border Styles ( only default )
            tmpstyle.Borders = new Borders();
            tmpstyle.Borders.Count = 1;
            tmpstyle.Borders.AppendChild(new Border());
            // Define some Cell-Formats ( index 0 = default, index 1 = bold font )
            tmpstyle.CellFormats = new CellFormats();
            tmpstyle.CellFormats.AppendChild(new CellFormat());
            tmpstyle.CellFormats.AppendChild(new CellFormat { FormatId = 1, FontId = 1, BorderId = 0, FillId = 0, ApplyFill = true });
            tmpstyle.CellFormats.Count = 2;
            // Return the Stylesheet
            return tmpstyle;
        }


        // MICROSOFT - MSDN - OPENXML SDK TUTORIAL FUNCTION
        // Given a column name, a row index, and a WorksheetPart, inserts a cell into the worksheet. 
        // If the cell already exists, return it. 
        private Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart) {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;
            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;
            if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0) {
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            } else {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }
            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0) {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            } else {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach (Cell cell in row.Elements<Cell>()) {
                    if (string.Compare(cell.CellReference.Value, cellReference, true) > 0) {
                        refCell = cell;
                        break;
                    }
                }
                Cell newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);
                worksheet.Save();
                return newCell;
            }
        }


        // -----------------------------------------------------------------------------------------
        // http://polymathprogrammer.com/2010/11/10/how-to-insert-multiple-images-in-excel-open-xml/
        // November 10, 2010 by Vincent Tan
        // -----------------------------------------------------------------------------------------
        // Funcion to insert an .jpg image into an EXCEL worksheet
        private void InsertImage(WorksheetPart worksheetpart, string imagepath) {
            long xpos = 4000000;
            long ypos = 100000;
            Worksheet worksheet = worksheetpart.Worksheet;
            DrawingsPart dp;
            ImagePart imgp;
            WorksheetDrawing wsd;
            ImagePartType ipt = ImagePartType.Jpeg;

            // Create new or use existing Drawing part
            if (worksheetpart.DrawingsPart == null) {
                dp = worksheetpart.AddNewPart<DrawingsPart>();
                imgp = dp.AddImagePart(ipt, worksheetpart.GetIdOfPart(dp));
                wsd = new WorksheetDrawing();
            } else {
                dp = worksheetpart.DrawingsPart;
                imgp = dp.AddImagePart(ipt);
                dp.CreateRelationshipToPart(imgp);
                wsd = dp.WorksheetDrawing;
            }

            using (FileStream fs = new FileStream(imagepath, FileMode.Open)) { imgp.FeedData(fs); }

            int imageNumber = dp.ImageParts.Count<ImagePart>();
            if (imageNumber == 1) {
                Drawing drawing = new Drawing();
                drawing.Id = dp.GetIdOfPart(imgp);
                worksheet.Append(drawing);
            }

            NonVisualDrawingProperties nvdp = new NonVisualDrawingProperties();
            nvdp.Id = new UInt32Value((uint)(1024 + imageNumber));
            nvdp.Name = "Picture " + imageNumber.ToString();
            nvdp.Description = "";
            DocumentFormat.OpenXml.Drawing.PictureLocks picLocks = new DocumentFormat.OpenXml.Drawing.PictureLocks();
            picLocks.NoChangeAspect = true;
            picLocks.NoChangeArrowheads = true;
            NonVisualPictureDrawingProperties nvpdp = new NonVisualPictureDrawingProperties();
            nvpdp.PictureLocks = picLocks;
            NonVisualPictureProperties nvpp = new NonVisualPictureProperties();
            nvpp.NonVisualDrawingProperties = nvdp;
            nvpp.NonVisualPictureDrawingProperties = nvpdp;

            DocumentFormat.OpenXml.Drawing.Stretch stretch = new DocumentFormat.OpenXml.Drawing.Stretch();
            stretch.FillRectangle = new DocumentFormat.OpenXml.Drawing.FillRectangle();

            BlipFill blipFill = new BlipFill();
            DocumentFormat.OpenXml.Drawing.Blip blip = new DocumentFormat.OpenXml.Drawing.Blip();
            blip.Embed = dp.GetIdOfPart(imgp);
            blip.CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print;
            blipFill.Blip = blip;
            blipFill.SourceRectangle = new DocumentFormat.OpenXml.Drawing.SourceRectangle();
            blipFill.Append(stretch);

            DocumentFormat.OpenXml.Drawing.Transform2D t2d = new DocumentFormat.OpenXml.Drawing.Transform2D();
            DocumentFormat.OpenXml.Drawing.Offset offset = new DocumentFormat.OpenXml.Drawing.Offset();
            offset.X = 0;
            offset.Y = 0;
            t2d.Offset = offset;
            Bitmap bm = new Bitmap(imagepath);

            DocumentFormat.OpenXml.Drawing.Extents extents = new DocumentFormat.OpenXml.Drawing.Extents();

            extents.Cx = (long)bm.Width * (long)((float)914400 / bm.HorizontalResolution);
            extents.Cy = (long)bm.Height * (long)((float)914400 / bm.VerticalResolution);

            bm.Dispose();
            t2d.Extents = extents;
            ShapeProperties sp = new ShapeProperties();
            sp.BlackWhiteMode = DocumentFormat.OpenXml.Drawing.BlackWhiteModeValues.Auto;
            sp.Transform2D = t2d;
            DocumentFormat.OpenXml.Drawing.PresetGeometry prstGeom = new DocumentFormat.OpenXml.Drawing.PresetGeometry();
            prstGeom.Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle;
            prstGeom.AdjustValueList = new DocumentFormat.OpenXml.Drawing.AdjustValueList();
            sp.Append(prstGeom);
            sp.Append(new DocumentFormat.OpenXml.Drawing.NoFill());

            DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture picture = new DocumentFormat.OpenXml.Drawing.Spreadsheet.Picture();
            picture.NonVisualPictureProperties = nvpp;
            picture.BlipFill = blipFill;
            picture.ShapeProperties = sp;

            Position pos = new Position();
            pos.X = xpos;
            pos.Y = ypos;
            Extent ext = new Extent();
            ext.Cx = extents.Cx;
            ext.Cy = extents.Cy;
            AbsoluteAnchor anchor = new AbsoluteAnchor();
            anchor.Position = pos;
            anchor.Extent = ext;
            anchor.Append(picture);
            anchor.Append(new ClientData());
            wsd.Append(anchor);
            wsd.Save(dp);
        }


    }

}
