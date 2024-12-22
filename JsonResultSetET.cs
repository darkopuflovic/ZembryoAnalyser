using System.Collections.Generic;

namespace ZembryoAnalyser;

public class JsonResultSetET
{
    public string Name { get; set; }
    public string Color { get; set; }
    public List<JsonDataET> Data { get; set; }
}
