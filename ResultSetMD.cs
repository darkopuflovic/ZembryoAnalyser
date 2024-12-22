using System.Collections.Generic;
using System.Windows.Media;

namespace ZembryoAnalyser;

public class ResultSetMD
{
    public string Name { get; set; }
    public SolidColorBrush Color { get; set; }
    public List<MDData> Result { get; set; }
}
