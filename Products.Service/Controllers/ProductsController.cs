using Castle.Core.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Products.Service.Models;
using static Products.Service.Controllers.ProductsController;

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

        // GET: Products/get
        [HttpGet("get")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts(int page = 1)
        {
            // Validate and adjust page and pageSize values if needed
            if (page < 1)
            {
                page = 1;
            }

            int pageSize = 8;

            // Calculate the number of items to skip based on the page size and number
            var skipAmount = (page - 1) * pageSize;

            var products = await _dbContext.Products
                .OrderBy(p => p.ProductId)  // You may want to order by a specific property
                .Skip(skipAmount)
                .Take(pageSize)
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    Category = p.Category.CategoryName,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Status = p.Status
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

        // POST: Products/post
        [HttpPost("post")]
        public async Task<ActionResult<Product>> PostProduct(ProductDTO product)
        {
            if (ModelState.IsValid)
            {
                Product newProduct = new Product();

                newProduct.ProductName = product.ProductName;
                newProduct.Price = product.Price;

                // Check if the category with the provided CategoryId exists
                var existingCategory = await _dbContext.Categories.FindAsync(product.CategoryId);

                //check name
                if (product.ProductName.Trim().IsNullOrEmpty()) return BadRequest("Name cant be empty!");

                //check price
                if (product.Price <= 0) return BadRequest($"Price must be > 0");

                if (existingCategory == null)
                {
                    // Category doesn't exist, return BadRequest
                    return BadRequest($"Category with ID {product.CategoryId} does not exist.");
                }

                newProduct.Category = existingCategory;

                newProduct.Status = true;

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
                return BadRequest($"Product ID {productDTO.CategoryId} does not exist.");
            }

            // Update only the properties you want to change
            existingProduct.ProductName = productDTO.ProductName;

            //check name
            if (productDTO.ProductName.Trim().IsNullOrEmpty()) return BadRequest("Name cant be empty!");

            //check price
            if (productDTO.Price <=0) return BadRequest($"Price must be > 0");

            existingProduct.Price = productDTO.Price;

            // Check if the category with the provided CategoryId exists
            var existingCategory = await _dbContext.Categories.FindAsync(productDTO.CategoryId);

            if (existingCategory == null)
            {
                // Category doesn't exist, return BadRequest
                return BadRequest($"Category with ID {productDTO.CategoryId} does not exist.");
            }

            existingProduct.Category = existingCategory;

            existingProduct.Status = productDTO.Status;

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

        // GET: Products/get
        [HttpGet("category")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategory()
        {

            var categories = await _dbContext.Categories
                .ToListAsync();

            return Ok(categories);
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
            public string? Category { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public Boolean Status { get; set; }
        }
    }
}
