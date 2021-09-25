using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ZembryoAnalyser
{
    public static class CsvExport
    {
        public static void ExportCsv(string fileName, List<ResultSet> results)
        {
            List<Dictionary<string, string>> data = GetValues(results);

            StringBuilder sb = new();

            Dictionary<string, string> el = data.FirstOrDefault();

            if (el != null)
            {
                _ = sb.AppendLine(string.Join(",", el.Keys));
            }

            foreach (Dictionary<string, string> d in data)
            {
                _ = sb.AppendLine(string.Join(",", d.Values));
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        public static List<Dictionary<string, string>> GetValues(List<ResultSet> allResults)
        {
            List<Dictionary<string, string>> list = new();

            int count = allResults.FirstOrDefault()?.Result?.Count ?? 0;

            for (int i = 0; i < count; i++)
            {
                int j = 0;
                Dictionary<string, string> row = new();

                foreach (ResultSet el in allResults)
                {
                    Data dataRow = el.Result.ElementAt(i);

                    if (row.ContainsKey($"{el.Name} - Index"))
                    {
                        row[$"{el.Name} - Index"] = dataRow.Index.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        row.Add($"{el.Name} - Index", dataRow.Index.ToString(CultureInfo.InvariantCulture));
                    }

                    if (row.ContainsKey($"{el.Name} - Time"))
                    {
                        row[$"{el.Name} - Time"] = dataRow.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        row.Add($"{el.Name} - Time", dataRow.Time.ToString("hh':'mm':'ss'.'fff", CultureInfo.InvariantCulture));
                    }

                    if (row.ContainsKey($"{el.Name} - Value"))
                    {
                        row[$"{el.Name} - Value"] = Math.Round(dataRow.DataValue, 2).ToString("N2", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        row.Add($"{el.Name} - Value", Math.Round(dataRow.DataValue, 2).ToString("N2", CultureInfo.InvariantCulture));
                    }

                    j++;
                }

                list.Add(row);
            }

            return list;
        }
    }
}
