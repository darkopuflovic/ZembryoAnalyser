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

            for (int k = 0; k < data.Count; k++)
            {
                List<Data> results = data.ElementAt(k).Result;

                worksheetPart = excel.WorkbookPart.AddNewPart<WorksheetPart>();

                listWorksheetParts.Add(worksheetPart);

                var worksheet = new Worksheet();

                var columns = new Columns();

                var firstColumn = new Column
                {
                    Min = 1,
                    Max = 1,
                    Width = 10,
                    CustomWidth = true
                };

                var secondColumn = new Column
                {
                    Min = 2,
                    Max = 2,
                    Width = 20,
                    CustomWidth = true
                };

                var thirdColumn = new Column
                {
                    Min = 3,
                    Max = 3,
                    Width = 15,
                    CustomWidth = true
                };

                columns.Append(firstColumn);
                columns.Append(secondColumn);
                columns.Append(thirdColumn);

                worksheet.Append(columns);

                SheetData sheetData = new SheetData();

                Row row = new Row();

                row.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellReference = $"A1",
                    CellValue = new CellValue("Index"),
                    StyleIndex = 1
                });

                row.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellReference = $"B1",
                    CellValue = new CellValue("Time"),
                    StyleIndex = 1
                });

                row.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellReference = $"C1",
                    CellValue = new CellValue("Value"),
                    StyleIndex = 1
                });

                sheetData.Append(row);

                foreach (Data result in results)
                {
                    row = new Row();

                    row.Append(new Cell
                    {
                        DataType = CellValues.String,
                        CellReference = $"A{result.Index + 1}",
                        CellValue = new CellValue(result.Index.ToString(CultureInfo.InvariantCulture)),
                        StyleIndex = 0
                    });

                    row.Append(new Cell
                    {
                        DataType = CellValues.String,
                        CellReference = $"B{result.Index + 1}",
                        CellValue = new CellValue(result.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture)),
                        StyleIndex = 0
                    });

                    row.Append(new Cell
                    {
                        DataType = CellValues.String,
                        CellReference = $"C{result.Index + 1}",
                        CellValue = new CellValue(Math.Round(result.DataValue, 2).ToString(CultureInfo.InvariantCulture)),
                        StyleIndex = 0
                    });

                    sheetData.Append(row);
                }

                worksheet.Append(sheetData);

                worksheetPart.Worksheet = worksheet;
                worksheetPart.Worksheet.Save();
            }

            var openXMLWriter = OpenXmlWriter.Create(excel.WorkbookPart);

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

            var regularFont = new Font
            {
                FontSize = new FontSize
                {
                    Val = 16
                }
            };

            var headerFont = new Font
            {
                Bold = new Bold(),
                Color = new DocumentFormat.OpenXml.Spreadsheet.Color
                {
                    Rgb = "FF800000"
                },
                FontSize = new FontSize
                {
                    Val = 16
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
