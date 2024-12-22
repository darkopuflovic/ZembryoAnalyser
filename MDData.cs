using System;

namespace ZembryoAnalyser;

public class MDData
{
    public int Index { get; set; }
    public TimeSpan Time { get; set; }
    public (double X, double Y) DataValue { get; set; }
}
