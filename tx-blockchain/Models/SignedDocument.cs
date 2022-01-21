namespace tx_blockchain.Models
{
    public class SignedDocument
    {
        public string Hash { get; set; }
        public string Signer { get; set; }
        public string DocumentId { get; set; }
    }
}
