using System;
using System.Collections.Generic;

namespace Contract.Service.Models
{
    public partial class Signature
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; } = null!;
        public string Reason { get; set; } = null!;
        public int Page { get; set; }
    }
}
