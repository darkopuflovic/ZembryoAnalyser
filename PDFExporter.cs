using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ZembryoAnalyser;

public static class PDFExporter
{
    public static void ExportPDF(string fileName, List<ResultSetHR> results, MemoryStream image)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        PdfDocument document = new();

        foreach (ResultSetHR result in results)
        {
            PdfPage pdfPage = document.AddPage();
            pdfPage.Height = 842;
            pdfPage.Width = 595;

            double pageMargin = 20;
            double textMargin = 23;
            double maxTableWidth = pdfPage.Width - (2 * pageMargin);
            double valueColumnWidth = maxTableWidth / 3;
            double rowHeight = 20;

            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XTextFormatter tf = new(graph);

            XFont fontNaslov = new("Times New Roman", 16, XFontStyle.Bold);
            XFont fontPodNaslov = new("Times New Roman", 12, XFontStyle.Regular);

            Color col = result.Color.Color;
            XSolidBrush headerFillColor = new(XColor.FromArgb(col.R, col.G, col.B));
            XSolidBrush headerTextColor = new(ContrastColor(headerFillColor.Color));
            XSolidBrush subTitleBrush = new(XColor.FromArgb(255, 244, 176, 132));
            XPen borderColor = XPens.Black;
            XSolidBrush textColor = XBrushes.Black;

            tf.Alignment = XParagraphAlignment.Center;

            graph.DrawRectangle(borderColor, headerFillColor, pageMargin, pageMargin, maxTableWidth, rowHeight);
            tf.DrawString(result.Name, fontNaslov, headerTextColor, new XRect(pageMargin, pageMargin, maxTableWidth, rowHeight));

            tf.Alignment = XParagraphAlignment.Left;

            graph.DrawRectangle(borderColor, subTitleBrush, pageMargin, pageMargin + rowHeight, valueColumnWidth, rowHeight);
            tf.DrawString("Index", fontPodNaslov, XBrushes.Black, new XRect(textMargin, textMargin + rowHeight, valueColumnWidth, rowHeight));

            graph.DrawRectangle(borderColor, subTitleBrush, pageMargin + valueColumnWidth, pageMargin + rowHeight, valueColumnWidth, rowHeight);
            tf.DrawString("Time", fontPodNaslov, XBrushes.Black, new XRect(textMargin + valueColumnWidth, textMargin + rowHeight, valueColumnWidth, rowHeight));

            graph.DrawRectangle(borderColor, subTitleBrush, pageMargin + (valueColumnWidth * 2), pageMargin + rowHeight, valueColumnWidth, rowHeight);
            tf.DrawString("Value", fontPodNaslov, XBrushes.Black, new XRect(textMargin + (valueColumnWidth * 2), textMargin + rowHeight, valueColumnWidth, rowHeight));

            int rowCount = 2;

            foreach (HRData data in result.Result)
            {
                graph.DrawRectangle(borderColor, pageMargin, pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                tf.DrawString(data.Index.ToString(CultureInfo.InvariantCulture), fontPodNaslov, XBrushes.Black, new XRect(textMargin, textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));

                graph.DrawRectangle(borderColor, pageMargin + valueColumnWidth, pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                tf.DrawString(data.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture), fontPodNaslov, XBrushes.Black, new XRect(textMargin + valueColumnWidth, textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));

                graph.DrawRectangle(borderColor, pageMargin + (valueColumnWidth * 2), pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                tf.DrawString(data.DataValue.ToString("N2", CultureInfo.InvariantCulture), fontPodNaslov, XBrushes.Black, new XRect(textMargin + (valueColumnWidth * 2), textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));

                if (CheckNewPage(pdfPage, rowCount, pageMargin, rowHeight))
                {
                    pdfPage = document.AddPage();
                    graph = XGraphics.FromPdfPage(pdfPage);
                    tf = new XTextFormatter(graph);
                    rowCount = 0;
                }
                else
                {
                    rowCount++;
                }
            }
        }

        XImage img = XImage.FromStream(image);
        PdfPage page = document.AddPage();
        page.Height = img.PointHeight;
        page.Width = img.PointWidth;
        XGraphics graphics = XGraphics.FromPdfPage(page);
        graphics.DrawImage(img, 0, 0);

        document.Save(fileName);
    }

    public static void ExportPDF(string fileName, List<ResultSetMD> results, MemoryStream image)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        PdfDocument document = new();

        foreach (ResultSetMD result in results)
        {
            PdfPage pdfPage = document.AddPage();
            pdfPage.Height = 842;
            pdfPage.Width = 595;

            double pageMargin = 20;
            double textMargin = 23;
            double maxTableWidth = pdfPage.Width - (2 * pageMargin);
            double valueColumnWidth = maxTableWidth / 3;
            double rowHeight = 20;

            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XTextFormatter tf = new(graph);

            XFont fontNaslov = new("Times New Roman", 16, XFontStyle.Bold);
            XFont fontPodNaslov = new("Times New Roman", 12, XFontStyle.Regular);

            Color col = result.Color.Color;
            XSolidBrush headerFillColor = new(XColor.FromArgb(col.R, col.G, col.B));
            XSolidBrush headerTextColor = new(ContrastColor(headerFillColor.Color));
            XSolidBrush subTitleBrush = new(XColor.FromArgb(255, 244, 176, 132));
            XPen borderColor = XPens.Black;
            XSolidBrush textColor = XBrushes.Black;

            tf.Alignment = XParagraphAlignment.Center;

            graph.DrawRectangle(borderColor, headerFillColor, pageMargin, pageMargin, maxTableWidth, rowHeight);
            tf.DrawString(result.Name, fontNaslov, headerTextColor, new XRect(pageMargin, pageMargin, maxTableWidth, rowHeight));

            tf.Alignment = XParagraphAlignment.Left;

            graph.DrawRectangle(borderColor, subTitleBrush, pageMargin, pageMargin + rowHeight, valueColumnWidth, rowHeight);
            tf.DrawString("Index", fontPodNaslov, XBrushes.Black, new XRect(textMargin, textMargin + rowHeight, valueColumnWidth, rowHeight));

            graph.DrawRectangle(borderColor, subTitleBrush, pageMargin + valueColumnWidth, pageMargin + rowHeight, valueColumnWidth, rowHeight);
            tf.DrawString("Time", fontPodNaslov, XBrushes.Black, new XRect(textMargin + valueColumnWidth, textMargin + rowHeight, valueColumnWidth, rowHeight));

            graph.DrawRectangle(borderColor, subTitleBrush, pageMargin + (valueColumnWidth * 2), pageMargin + rowHeight, valueColumnWidth, rowHeight);
            tf.DrawString("Value", fontPodNaslov, XBrushes.Black, new XRect(textMargin + (valueColumnWidth * 2), textMargin + rowHeight, valueColumnWidth, rowHeight));

            int rowCount = 2;

            foreach (MDData data in result.Result)
            {
                graph.DrawRectangle(borderColor, pageMargin, pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                tf.DrawString(data.Index.ToString(CultureInfo.InvariantCulture), fontPodNaslov, XBrushes.Black, new XRect(textMargin, textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));

                graph.DrawRectangle(borderColor, pageMargin + valueColumnWidth, pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                tf.DrawString(data.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture), fontPodNaslov, XBrushes.Black, new XRect(textMargin + valueColumnWidth, textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));

                graph.DrawRectangle(borderColor, pageMargin + (valueColumnWidth * 2), pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                tf.DrawString($"[{(int)data.DataValue.X}, {(int)data.DataValue.Y}]", fontPodNaslov, XBrushes.Black, new XRect(textMargin + (valueColumnWidth * 2), textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));

                if (CheckNewPage(pdfPage, rowCount, pageMargin, rowHeight))
                {
                    pdfPage = document.AddPage();
                    graph = XGraphics.FromPdfPage(pdfPage);
                    tf = new XTextFormatter(graph);
                    rowCount = 0;
                }
                else
                {
                    rowCount++;
                }
            }
        }

        XImage img = XImage.FromStream(image);
        PdfPage page = document.AddPage();
        page.Height = img.PointHeight;
        page.Width = img.PointWidth;
        XGraphics graphics = XGraphics.FromPdfPage(page);
        graphics.DrawImage(img, 0, 0);

        document.Save(fileName);
    }

    public static void ExportPDF(string fileName, List<ResultSetET> results, MemoryStream image)
    {
        double? md = results.FirstOrDefault()?.Result?.FirstOrDefault()?.MinimalDistance;
        bool hasMD = !double.IsNaN(md ?? double.NaN);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        PdfDocument document = new();

        foreach (ResultSetET result in results)
        {
            PdfPage pdfPage = document.AddPage();
            pdfPage.Height = 842;
            pdfPage.Width = 595;

            double pageMargin = 20;
            double textMargin = 23;
            double maxTableWidth = pdfPage.Width - (2 * pageMargin);
            double valueColumnWidth = maxTableWidth / (hasMD ? 4 : 3);
            double rowHeight = 20;

            XGraphics graph = XGraphics.FromPdfPage(pdfPage);
            XTextFormatter tf = new(graph);

            XFont fontNaslov = new("Times New Roman", 16, XFontStyle.Bold);
            XFont fontPodNaslov = new("Times New Roman", 12, XFontStyle.Regular);

            Color col = result.Color.Color;
            XSolidBrush headerFillColor = new(XColor.FromArgb(col.R, col.G, col.B));
            XSolidBrush headerTextColor = new(ContrastColor(headerFillColor.Color));
            XSolidBrush subTitleBrush = new(XColor.FromArgb(255, 244, 176, 132));
            XPen borderColor = XPens.Black;
            XSolidBrush textColor = XBrushes.Black;

            tf.Alignment = XParagraphAlignment.Center;

            graph.DrawRectangle(borderColor, headerFillColor, pageMargin, pageMargin, maxTableWidth, rowHeight);
            tf.DrawString(result.Name, fontNaslov, headerTextColor, new XRect(pageMargin, pageMargin, maxTableWidth, rowHeight));

            tf.Alignment = XParagraphAlignment.Left;

            graph.DrawRectangle(borderColor, subTitleBrush, pageMargin, pageMargin + rowHeight, valueColumnWidth, rowHeight);
            tf.DrawString("Index", fontPodNaslov, XBrushes.Black, new XRect(textMargin, textMargin + rowHeight, valueColumnWidth, rowHeight));

            graph.DrawRectangle(borderColor, subTitleBrush, pageMargin + valueColumnWidth, pageMargin + rowHeight, valueColumnWidth, rowHeight);
            tf.DrawString("Time", fontPodNaslov, XBrushes.Black, new XRect(textMargin + valueColumnWidth, textMargin + rowHeight, valueColumnWidth, rowHeight));

            graph.DrawRectangle(borderColor, subTitleBrush, pageMargin + (valueColumnWidth * 2), pageMargin + rowHeight, valueColumnWidth, rowHeight);
            tf.DrawString("Value", fontPodNaslov, XBrushes.Black, new XRect(textMargin + (valueColumnWidth * 2), textMargin + rowHeight, valueColumnWidth, rowHeight));

            if (hasMD)
            {
                graph.DrawRectangle(borderColor, subTitleBrush, pageMargin + (valueColumnWidth * 3), pageMargin + rowHeight, valueColumnWidth, rowHeight);
                tf.DrawString("Edge distance", fontPodNaslov, XBrushes.Black, new XRect(textMargin + (valueColumnWidth * 3), textMargin + rowHeight, valueColumnWidth, rowHeight));
            }

            int rowCount = 2;

            foreach (ETData data in result.Result)
            {
                graph.DrawRectangle(borderColor, pageMargin, pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                tf.DrawString(data.Index.ToString(CultureInfo.InvariantCulture), fontPodNaslov, XBrushes.Black, new XRect(textMargin, textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));

                graph.DrawRectangle(borderColor, pageMargin + valueColumnWidth, pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                tf.DrawString(data.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture), fontPodNaslov, XBrushes.Black, new XRect(textMargin + valueColumnWidth, textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));

                graph.DrawRectangle(borderColor, pageMargin + (valueColumnWidth * 2), pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                tf.DrawString(data.DataValue.ToString(CultureInfo.InvariantCulture), fontPodNaslov, XBrushes.Black, new XRect(textMargin + (valueColumnWidth * 2), textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));

                if (hasMD)
                {
                    graph.DrawRectangle(borderColor, pageMargin + (valueColumnWidth * 3), pageMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight);
                    tf.DrawString(data.MinimalDistance.ToString("N2", CultureInfo.InvariantCulture), fontPodNaslov, XBrushes.Black, new XRect(textMargin + (valueColumnWidth * 3), textMargin + (rowHeight * rowCount), valueColumnWidth, rowHeight));
                }

                if (CheckNewPage(pdfPage, rowCount, pageMargin, rowHeight))
                {
                    pdfPage = document.AddPage();
                    graph = XGraphics.FromPdfPage(pdfPage);
                    tf = new XTextFormatter(graph);
                    rowCount = 0;
                }
                else
                {
                    rowCount++;
                }
            }
        }

        XImage img = XImage.FromStream(image);
        PdfPage page = document.AddPage();
        page.Height = img.PointHeight;
        page.Width = img.PointWidth;
        XGraphics graphics = XGraphics.FromPdfPage(page);
        graphics.DrawImage(img, 0, 0);

        document.Save(fileName);
    }

    private static bool CheckNewPage(PdfPage page, int rowCount, double pageMargin, double rowHeight)
    {
        double point = (pageMargin * 2) + (rowHeight * (rowCount + 1)) + rowHeight;
        return point > page.Height;
    }

    private static XColor ContrastColor(XColor color)
    {
        double luma = ((0.299 * color.R) + ((0.587 * color.G) + (0.114 * color.B))) / 255;
        return luma > 0.5 ? XColors.Black : XColors.White;
    }
}
