using System.Collections.Generic;

namespace ZembryoAnalyser
{
    public class JsonResultSet
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public List<JsonData> Data { get; set; }
    }
}
