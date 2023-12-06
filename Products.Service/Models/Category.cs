using System;
using System.Collections.Generic;

namespace Products.Service.Models
{
    public partial class Category
    {
        public Category()
        {
        }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
