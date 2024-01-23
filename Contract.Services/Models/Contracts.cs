namespace Contract.Service.Models
{
    public class Contracts
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Path { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public bool? IsSigned { get; set; }
    }
}
