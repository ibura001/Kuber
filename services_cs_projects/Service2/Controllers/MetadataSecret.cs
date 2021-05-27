namespace Service2.Controllers
{
    public class MetadataSecret
    {
        public string CreatedTime { get; set; }
        public string DeletionTime { get; set; }
        public bool Destroyed { get; set; }
        public int Version { get; set; }
    }
}