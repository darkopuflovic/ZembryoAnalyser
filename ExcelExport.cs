using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ZembryoAnalyser
{
    public static class ExcelExport
    {
        public static void ExportXLSX(string fileName, List<ResultSet> data)
        {
            using var excel = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);

            WorkbookPart part = excel.AddWorkbookPart();

            AddStyleSheet(excel);

            var listWorksheetParts = new List<WorksheetPart>();
            WorksheetPart worksheetPart = default;
            OpenXmlWriter openXMLWriter = default;

            for (int k = 0; k < data.Count; k++)
            {
                List<Data> results = data.ElementAt(k).Result;

                worksheetPart = excel.WorkbookPart.AddNewPart<WorksheetPart>();

                listWorksheetParts.Add(worksheetPart);

                openXMLWriter = OpenXmlWriter.Create(worksheetPart);
                openXMLWriter.WriteStartElement(new Worksheet());
                openXMLWriter.WriteStartElement(new SheetData());

                var rowHeader = new List<OpenXmlAttribute>
                {
                    new OpenXmlAttribute("r", null, "1")
                };

                var headerStyle = new List<OpenXmlAttribute>
                {
                    new OpenXmlAttribute("t", null, "str"),
                    new OpenXmlAttribute("s", null, "1")
                };

                openXMLWriter.WriteStartElement(new Row(), rowHeader);

                openXMLWriter.WriteStartElement(new Cell(), headerStyle);
                openXMLWriter.WriteElement(new CellValue("Index"));
                openXMLWriter.WriteEndElement();

                openXMLWriter.WriteStartElement(new Cell(), headerStyle);
                openXMLWriter.WriteElement(new CellValue("Time"));
                openXMLWriter.WriteEndElement();

                openXMLWriter.WriteStartElement(new Cell(), headerStyle);
                openXMLWriter.WriteElement(new CellValue("Value"));
                openXMLWriter.WriteEndElement();

                openXMLWriter.WriteEndElement();

                foreach (Data result in results)
                {
                    var rowAttribute = new List<OpenXmlAttribute>
                    {
                        new OpenXmlAttribute("r", null, (result.Index + 1).ToString(CultureInfo.InvariantCulture))
                    };

                    openXMLWriter.WriteStartElement(new Row(), rowAttribute);

                    var cellStyle = new List<OpenXmlAttribute>
                    {
                        new OpenXmlAttribute("t", null, "str")
                    };

                    openXMLWriter.WriteStartElement(new Cell(), cellStyle);
                    openXMLWriter.WriteElement(new CellValue(result.Index.ToString(CultureInfo.InvariantCulture)));
                    openXMLWriter.WriteEndElement();

                    openXMLWriter.WriteStartElement(new Cell(), cellStyle);
                    openXMLWriter.WriteElement(new CellValue(result.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)));
                    openXMLWriter.WriteEndElement();

                    openXMLWriter.WriteStartElement(new Cell(), cellStyle);
                    openXMLWriter.WriteElement(new CellValue(Math.Round(result.DataValue, 2).ToString(CultureInfo.InvariantCulture)));
                    openXMLWriter.WriteEndElement();

                    openXMLWriter.WriteEndElement();
                }

                openXMLWriter.WriteEndElement();
                openXMLWriter.WriteEndElement();

                openXMLWriter.Close();
            }

            openXMLWriter = OpenXmlWriter.Create(excel.WorkbookPart);

            openXMLWriter.WriteStartElement(new Workbook());
            openXMLWriter.WriteStartElement(new Sheets());

            for (int i = 0; i < data.Count; i++)
            {
                openXMLWriter.WriteElement(new Sheet()
                {
                    Name = data.ElementAt(i).Name,
                    SheetId = UInt32Value.FromUInt32((uint)i + 1),
                    Id = excel.WorkbookPart.GetIdOfPart(listWorksheetParts.ElementAt(i))
                });
            }

            openXMLWriter.WriteEndElement();
            openXMLWriter.WriteEndElement();

            openXMLWriter.Close();
            excel.Close();
        }

        private static WorkbookStylesPart AddStyleSheet(SpreadsheetDocument spreadsheet)
        {
            var stylesheet = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>();
            var workbookstylesheet = new Stylesheet();

            var regularFont = new Font();
            var headerFont = new Font
            {
                Bold = new Bold(),
                Color = new DocumentFormat.OpenXml.Spreadsheet.Color
                {
                    Rgb = "FF800000"
                }
            };

            var fonts = new DocumentFormat.OpenXml.Spreadsheet.Fonts();

            fonts.Append(regularFont);
            fonts.Append(headerFont);

            var regularFill = new Fill(new PatternFill { PatternType = PatternValues.None });
            var gray125 = new Fill(new PatternFill { PatternType = PatternValues.Gray125 });
            var headerFill = new Fill
            {
                PatternFill = new PatternFill
                {
                    PatternType = PatternValues.Solid,
                    ForegroundColor = new ForegroundColor { Rgb = new HexBinaryValue { Value = "FFFFCC99" } },
                    BackgroundColor = new BackgroundColor { Indexed = 64U }
                }
            };

            var fills = new Fills();

            fills.Append(regularFill);
            fills.Append(gray125);
            fills.Append(headerFill);

            var regularBorder = new DocumentFormat.OpenXml.Spreadsheet.Border();
            var headerBorder = new DocumentFormat.OpenXml.Spreadsheet.Border
            {
                BottomBorder = new BottomBorder
                {
                    Style = BorderStyleValues.Thin,
                    Color = new DocumentFormat.OpenXml.Spreadsheet.Color
                    {
                        Rgb = "FF8a4500"
                    }
                },
                TopBorder = new TopBorder
                {
                    Style = BorderStyleValues.Thin,
                    Color = new DocumentFormat.OpenXml.Spreadsheet.Color
                    {
                        Rgb = "FF8a4500"
                    }
                },
                LeftBorder = new LeftBorder
                {
                    Style = BorderStyleValues.Thin,
                    Color = new DocumentFormat.OpenXml.Spreadsheet.Color
                    {
                        Rgb = "FF8a4500"
                    }
                },
                RightBorder = new RightBorder
                {
                    Style = BorderStyleValues.Thin,
                    Color = new DocumentFormat.OpenXml.Spreadsheet.Color
                    {
                        Rgb = "FF8a4500"
                    }
                }
            };

            var borders = new Borders();

            borders.Append(regularBorder);
            borders.Append(headerBorder);

            var regularFormat = new CellFormat()
            {
                FontId = 0,
                FillId = 0,
                BorderId = 0
            };

            var headerFormat = new CellFormat()
            {
                FontId = 1,
                FillId = 2,
                BorderId = 1
            };

            var cellformats = new CellFormats();
            cellformats.Append(regularFormat);
            cellformats.Append(headerFormat);

            workbookstylesheet.Append(fonts);
            workbookstylesheet.Append(fills);
            workbookstylesheet.Append(borders);
            workbookstylesheet.Append(cellformats);

            stylesheet.Stylesheet = workbookstylesheet;
            stylesheet.Stylesheet.Save();

            return stylesheet;
        }
    }
}
