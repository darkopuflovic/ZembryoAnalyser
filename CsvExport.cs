using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ZembryoAnalyser;

public static class CsvExport
{
    public static void ExportCsv(string fileName, List<ResultSetHR> results)
    {
        Dictionary<int, List<string>> data = GetValues(results);
        var els =
            from n in results.Select(p => p.Name)
            from c in new List<string> { "Index", "Time", "Value" }
            select $"{n} - {c}";
        GenerateCSV(fileName, els.ToList(), data);
    }

    public static void ExportCsv(string fileName, List<ResultSetMD> results)
    {
        Dictionary<int, List<string>> data = GetValues(results);
        var els =
            from n in results.Select(p => p.Name)
            from c in new List<string> { "Index", "Time", "Value" }
            select $"{n} - {c}";
        GenerateCSV(fileName, els.ToList(), data);
    }

    public static void ExportCsv(string fileName, List<ResultSetET> results)
    {
        Dictionary<int, List<string>> data = GetValues(results);
        var els =
            from n in results.Select(p => p.Name)
            from c in new List<string> { "Index", "Time", "Value" }
            select $"{n} - {c}";
        GenerateCSV(fileName, els.ToList(), data);
    }

    private static void GenerateCSV(string fileName, List<string> columns, Dictionary<int, List<string>> data)
    {
        StringBuilder sb = new();

        _ = sb.AppendLine(string.Join(",", columns));

        foreach (List<string> d in data.Values)
        {
            _ = sb.AppendLine(string.Join(",", d));
        }

        File.WriteAllText(fileName, sb.ToString());
    }

    private static Dictionary<int, List<string>> GetValues(List<ResultSetHR> allResults)
    {
        Dictionary<int, List<string>> dict = [];

        int count = allResults.FirstOrDefault()?.Result?.Count ?? 0;

        for (int i = 0; i < count; i++)
        {
            List<string> row = [];

            foreach (ResultSetHR el in allResults)
            {
                HRData dataRow = el.Result.ElementAt(i);

                row.Add(dataRow.Index.ToString(CultureInfo.InvariantCulture));
                row.Add(dataRow.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
                row.Add(Math.Round(dataRow.DataValue, 2).ToString("N2", CultureInfo.InvariantCulture));
            }

            dict.Add(i, row);
        }

        return dict;
    }

    private static Dictionary<int, List<string>> GetValues(List<ResultSetMD> allResults)
    {
        Dictionary<int, List<string>> dict = [];

        int count = allResults.FirstOrDefault()?.Result?.Count ?? 0;

        for (int i = 0; i < count; i++)
        {
            List<string> row = [];

            foreach (ResultSetMD el in allResults)
            {
                MDData dataRow = el.Result.ElementAt(i);

                row.Add(dataRow.Index.ToString(CultureInfo.InvariantCulture));
                row.Add(dataRow.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
                row.Add($"[{(int)dataRow.DataValue.X};{(int)dataRow.DataValue.Y}]");
            }

            dict.Add(i, row);
        }

        return dict;
    }

    private static Dictionary<int, List<string>> GetValues(List<ResultSetET> allResults)
    {
        Dictionary<int, List<string>> dict = [];

        int count = allResults.FirstOrDefault()?.Result?.Count ?? 0;

        for (int i = 0; i < count; i++)
        {
            List<string> row = [];

            foreach (ResultSetET el in allResults)
            {
                ETData dataRow = el.Result.ElementAt(i);

                row.Add(dataRow.Index.ToString(CultureInfo.InvariantCulture));
                row.Add(dataRow.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
                row.Add(dataRow.DataValue.ToString());
            }

            dict.Add(i, row);
        }

        return dict;
    }
}
