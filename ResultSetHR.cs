using System.Collections.Generic;
using System.Windows.Media;

namespace ZembryoAnalyser;

public class ResultSetHR
{
    public string Name { get; set; }
    public SolidColorBrush Color { get; set; }
    public List<HRData> Result { get; set; }
}
