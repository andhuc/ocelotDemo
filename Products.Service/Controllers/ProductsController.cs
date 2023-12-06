using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.Service.Models;

namespace Products.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly sampleContext _dbContext;

        public ProductsController(sampleContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Products/
        [HttpGet("get")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            var products = await _dbContext.Products
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    Category = p.Category.CategoryName,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await _dbContext.Products
                .Where(p => p.ProductId == id)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    Category = p.Category.CategoryName,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: Products/
        [HttpPost("post")]
        public async Task<ActionResult<Product>> PostProduct(ProductDTO product)
        {
            if (ModelState.IsValid)
            {
                Product newProduct = new Product();

                newProduct.ProductName = product.ProductName;
                newProduct.Price = product.Price;
                newProduct.CategoryId = product.CategoryId;

                _dbContext.Products.Add(newProduct);
                await _dbContext.SaveChangesAsync();
                return Ok();
            }

            return BadRequest(ModelState);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDTO productDTO)
        {
            // Retrieve the existing product from the database
            var existingProduct = await _dbContext.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            // Update only the properties you want to change
            existingProduct.ProductName = productDTO.ProductName;
            existingProduct.Price = productDTO.Price;
            existingProduct.CategoryId = productDTO.CategoryId;

            // Optionally update other properties as needed

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }


        // DELETE: Products/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            return Ok(product);
        }

        private bool ProductExists(int id)
        {
            return _dbContext.Products.Any(e => e.ProductId == id);
        }

        // ProductDTO class to represent the data you want to expose
        public class ProductDTO
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int? CategoryId { get; set; }
            public string Category { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }
    }
}
