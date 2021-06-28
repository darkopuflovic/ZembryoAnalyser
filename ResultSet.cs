using System.Collections.Generic;
using System.Windows.Media;

namespace ZembryoAnalyser
{
    public class ResultSet
    {
        public string Name { get; set; }
        public SolidColorBrush Color { get; set; }
        public List<Data> Result { get; set; }
    }
}
