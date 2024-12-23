using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ZembryoAnalyser;

public static class ExcelExport
{
    public static void ExportXLSX(string fileName, List<ResultSetHR> data)
    {
        using SpreadsheetDocument excel = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);

        WorkbookPart part = excel.AddWorkbookPart();

        _ = AddStyleSheet(excel);

        List<WorksheetPart> listWorksheetParts = [];
        WorksheetPart worksheetPart = default;

        for (int k = 0; k < data.Count; k++)
        {
            List<HRData> results = data.ElementAt(k).Result;

            worksheetPart = excel.WorkbookPart.AddNewPart<WorksheetPart>();

            listWorksheetParts.Add(worksheetPart);

            Worksheet worksheet = new();

            Columns columns = new();

            Column firstColumn = new()
            {
                Min = 1,
                Max = 1,
                Width = 10,
                CustomWidth = true
            };

            Column secondColumn = new()
            {
                Min = 2,
                Max = 2,
                Width = 20,
                CustomWidth = true
            };

            Column thirdColumn = new()
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

            SheetData sheetData = new();

            Row row = new();

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

            foreach (HRData result in results)
            {
                row = new Row();

                row.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellReference = $"A{result.Index + 1}",
                    CellValue = new CellValue($"{result.Index}."),
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
                    CellValue = new CellValue(Math.Round(result.DataValue, 2).ToString("N2", CultureInfo.InvariantCulture)),
                    StyleIndex = 0
                });

                sheetData.Append(row);
            }

            worksheet.Append(sheetData);

            worksheetPart.Worksheet = worksheet;
            worksheetPart.Worksheet.Save();
        }

        OpenXmlWriter openXMLWriter = OpenXmlWriter.Create(excel.WorkbookPart);

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
    }

    public static void ExportXLSX(string fileName, List<ResultSetMD> data)
    {
        using SpreadsheetDocument excel = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);

        WorkbookPart part = excel.AddWorkbookPart();

        _ = AddStyleSheet(excel);

        List<WorksheetPart> listWorksheetParts = [];
        WorksheetPart worksheetPart = default;

        for (int k = 0; k < data.Count; k++)
        {
            List<MDData> results = data.ElementAt(k).Result;

            worksheetPart = excel.WorkbookPart.AddNewPart<WorksheetPart>();

            listWorksheetParts.Add(worksheetPart);

            Worksheet worksheet = new();

            Columns columns = new();

            Column firstColumn = new()
            {
                Min = 1,
                Max = 1,
                Width = 10,
                CustomWidth = true
            };

            Column secondColumn = new()
            {
                Min = 2,
                Max = 2,
                Width = 20,
                CustomWidth = true
            };

            Column thirdColumn = new()
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

            SheetData sheetData = new();

            Row row = new();

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

            foreach (MDData result in results)
            {
                row = new Row();

                row.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellReference = $"A{result.Index + 1}",
                    CellValue = new CellValue($"{result.Index}."),
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
                    CellValue = new CellValue($"[{(int)result.DataValue.X}, {(int)result.DataValue.Y}]"),
                    StyleIndex = 0
                });

                sheetData.Append(row);
            }

            worksheet.Append(sheetData);

            worksheetPart.Worksheet = worksheet;
            worksheetPart.Worksheet.Save();
        }

        OpenXmlWriter openXMLWriter = OpenXmlWriter.Create(excel.WorkbookPart);

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
    }

    public static void ExportXLSX(string fileName, List<ResultSetET> data)
    {
        double? md = data.FirstOrDefault()?.Result?.FirstOrDefault()?.MinimalDistance;
        bool hasMD = !double.IsNaN(md ?? double.NaN);

        using SpreadsheetDocument excel = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);

        WorkbookPart part = excel.AddWorkbookPart();

        _ = AddStyleSheet(excel);

        List<WorksheetPart> listWorksheetParts = [];
        WorksheetPart worksheetPart = default;

        for (int k = 0; k < data.Count; k++)
        {
            List<ETData> results = data.ElementAt(k).Result;

            worksheetPart = excel.WorkbookPart.AddNewPart<WorksheetPart>();

            listWorksheetParts.Add(worksheetPart);

            Worksheet worksheet = new();

            Columns columns = new();

            Column firstColumn = new()
            {
                Min = 1,
                Max = 1,
                Width = 10,
                CustomWidth = true
            };

            Column secondColumn = new()
            {
                Min = 2,
                Max = 2,
                Width = 20,
                CustomWidth = true
            };

            Column thirdColumn = new()
            {
                Min = 3,
                Max = 3,
                Width = 15,
                CustomWidth = true
            };

            columns.Append(firstColumn);
            columns.Append(secondColumn);
            columns.Append(thirdColumn);

            if (hasMD)
            {
                columns.Append(new Column()
                {
                    Min = 4,
                    Max = 4,
                    Width = 15,
                    CustomWidth = true
                });
            }

            worksheet.Append(columns);

            SheetData sheetData = new();

            Row row = new();

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

            if (hasMD)
            {
                row.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellReference = $"D1",
                    CellValue = new CellValue("Minimal distance"),
                    StyleIndex = 1
                });
            }

            sheetData.Append(row);

            foreach (ETData result in results)
            {
                row = new Row();

                row.Append(new Cell
                {
                    DataType = CellValues.String,
                    CellReference = $"A{result.Index + 1}",
                    CellValue = new CellValue($"{result.Index}."),
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
                    CellValue = new CellValue(result.DataValue.ToString(CultureInfo.InvariantCulture)),
                    StyleIndex = 0
                });

                if (hasMD)
                {
                    row.Append(new Cell
                    {
                        DataType = CellValues.String,
                        CellReference = $"D{result.Index + 1}",
                        CellValue = new CellValue(result.MinimalDistance.ToString("N2", CultureInfo.InvariantCulture)),
                        StyleIndex = 0
                    });
                }

                sheetData.Append(row);
            }

            worksheet.Append(sheetData);

            worksheetPart.Worksheet = worksheet;
            worksheetPart.Worksheet.Save();
        }

        OpenXmlWriter openXMLWriter = OpenXmlWriter.Create(excel.WorkbookPart);

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
    }

    private static WorkbookStylesPart AddStyleSheet(SpreadsheetDocument spreadsheet)
    {
        WorkbookStylesPart stylesheet = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>();
        Stylesheet workbookstylesheet = new();

        Font regularFont = new()
        {
            FontSize = new FontSize
            {
                Val = 16
            }
        };

        Font headerFont = new()
        {
            Bold = new Bold(),
            Color = new Color
            {
                Rgb = "FF800000"
            },
            FontSize = new FontSize
            {
                Val = 16
            }
        };

        Fonts fonts = new();

        fonts.Append(regularFont);
        fonts.Append(headerFont);

        Fill regularFill = new(new PatternFill { PatternType = PatternValues.None });
        Fill gray125 = new(new PatternFill { PatternType = PatternValues.Gray125 });
        Fill headerFill = new()
        {
            PatternFill = new PatternFill
            {
                PatternType = PatternValues.Solid,
                ForegroundColor = new ForegroundColor { Rgb = new HexBinaryValue { Value = "FFFFCC99" } },
                BackgroundColor = new BackgroundColor { Indexed = 64U }
            }
        };

        Fills fills = new();

        fills.Append(regularFill);
        fills.Append(gray125);
        fills.Append(headerFill);

        Border regularBorder = new();
        Border headerBorder = new()
        {
            BottomBorder = new BottomBorder
            {
                Style = BorderStyleValues.Thin,
                Color = new Color
                {
                    Rgb = "FF8a4500"
                }
            },
            TopBorder = new TopBorder
            {
                Style = BorderStyleValues.Thin,
                Color = new Color
                {
                    Rgb = "FF8a4500"
                }
            },
            LeftBorder = new LeftBorder
            {
                Style = BorderStyleValues.Thin,
                Color = new Color
                {
                    Rgb = "FF8a4500"
                }
            },
            RightBorder = new RightBorder
            {
                Style = BorderStyleValues.Thin,
                Color = new Color
                {
                    Rgb = "FF8a4500"
                }
            }
        };

        Borders borders = new();

        borders.Append(regularBorder);
        borders.Append(headerBorder);

        CellFormat regularFormat = new()
        {
            FontId = 0,
            FillId = 0,
            BorderId = 0,
            Alignment = new Alignment()
            {
                Horizontal = HorizontalAlignmentValues.Left,
                Vertical = VerticalAlignmentValues.Center
            }
        };

        CellFormat headerFormat = new()
        {
            FontId = 1,
            FillId = 2,
            BorderId = 1,
            Alignment = new Alignment()
            {
                Horizontal = HorizontalAlignmentValues.Left,
                Vertical = VerticalAlignmentValues.Center
            }
        };

        CellFormats cellformats = new();
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
