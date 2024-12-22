using System.Collections.Generic;

namespace ZembryoAnalyser;

public class JsonResultSetHR
{
    public string Name { get; set; }
    public string Color { get; set; }
    public List<JsonDataHR> Data { get; set; }
}
