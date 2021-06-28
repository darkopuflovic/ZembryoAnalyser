using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZembryoAnalyser
{
    public static class CsvExport
    {
        public static void ExportCsv(string fileName, List<Dictionary<string, string>> data)
        {
            var sb = new StringBuilder();

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
    }
}
