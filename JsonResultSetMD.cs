using System.Collections.Generic;

namespace ZembryoAnalyser;

public class JsonResultSetMD
{
    public string Name { get; set; }
    public string Color { get; set; }
    public List<JsonDataMD> Data { get; set; }
}
