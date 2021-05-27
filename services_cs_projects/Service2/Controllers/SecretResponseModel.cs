namespace Service2.Controllers
{
    public class SecretResponseModel
    {
        public string RequestId { get; set; }
        public string LeaseId { get; set; }
        public bool Renewable { get; set; }
        public int LeaseDuration { get; set; }
        public DataSecret Data { get; set; }
        public object WrapInfo { get; set; }
        public object Warnings { get; set; }
        public object Auth { get; set; }
    }
}