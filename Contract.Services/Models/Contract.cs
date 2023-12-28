using System;
using System.Collections.Generic;

namespace Contract.Service.Models
{
    public partial class Contract
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Path { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }
}
