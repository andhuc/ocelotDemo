namespace Contract.Service.Models.DTO
{
    public class SignatureDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public int Page { get; set; }
    }
}
