using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace ZembryoAnalyser
{
    public class SettingsDatabase
    {
        private SettingsData data;
        private readonly DataContractSerializer serializer;

        public SettingsDatabase()
        {
            data = new SettingsData();
            serializer = new DataContractSerializer(typeof(SettingsData));
            Load();
        }

        private void Save()
        {
            using IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly();
            using IsolatedStorageFileStream stream = store.OpenFile("settings.xml", FileMode.Create, FileAccess.Write);
            using XmlDictionaryWriter xdw = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8);
            serializer.WriteObject(xdw, data);
        }

        private void Load()
        {
            using IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForAssembly();
            if (store.FileExists("settings.xml"))
            {
                using IsolatedStorageFileStream stream = store.OpenFile("settings.xml", FileMode.Open, FileAccess.Read);

                if (stream.Length > 0)
                {
                    using XmlDictionaryReader xdr = XmlDictionaryReader.CreateTextReader(stream, Encoding.UTF8, new XmlDictionaryReaderQuotas(), null);
                    data = (SettingsData)serializer.ReadObject(xdr);
                }
            }
        }

        public void Set(string key, object value)
        {
            if (data.Dictionary.ContainsKey(key))
            {
                data.Dictionary[key] = value;
            }
            else
            {
                data.Dictionary.Add(key, value);
            }

            Save();
        }

        public object Get(string key)
        {
            return data.Dictionary.ContainsKey(key) ?
                   data.Dictionary.TryGetValue(key, out object val) && val != null ?
                        val : default : default;
        }

        public void Remove(string key)
        {
            if (data.Dictionary.ContainsKey(key))
            {
                if (data.Dictionary.Remove(key))
                {
                    Save();
                }
            }
        }

        public void Clear()
        {
            data.Dictionary.Clear();
            Save();
        }
    }
}
