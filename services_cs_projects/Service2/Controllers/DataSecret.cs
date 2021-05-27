using System.Collections.Generic;

namespace Service2.Controllers
{
    public class DataSecret
    {
        public string ConnectionString { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public MetadataSecret Metadata { get; set; }
    }
}