using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ZembryoAnalyser
{
    [DataContract]
    public class SettingsData
    {
        [DataMember]
        public Dictionary<string, object> Dictionary { get; set; }

        public SettingsData()
        {
            Dictionary = new Dictionary<string, object>();
        }
    }
}
