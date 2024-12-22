using System.Collections.Generic;
using System.Windows.Media;

namespace ZembryoAnalyser;

public class ResultSetET
{
    public string Name { get; set; }
    public SolidColorBrush Color { get; set; }
    public List<ETData> Result { get; set; }
}
