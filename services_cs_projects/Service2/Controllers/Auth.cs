using System.Collections.Generic;

namespace Service2.Controllers
{
    public class Auth
    {
        public string ClientToken { get; set; }
        public string Accessor { get; set; }
        public List<string> Policies { get; set; }
        public List<string> TokenPolicies { get; set; }
        public Metadata Metadata { get; set; }
        public int LeaseDuration { get; set; }
        public bool Renewable { get; set; }
        public string EntityId { get; set; }
        public string TokenType { get; set; }
        public bool Orphan { get; set; }
    }
}