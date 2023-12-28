using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Products.Service.Models;
using Products.Service.Services;

namespace Products.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly sampleContext _dbContext;
        private readonly SystemHistoryService _systemHistoryService;

        public ProductsController(sampleContext dbContext, SystemHistoryService systemHistoryService)
        {
            _dbContext = dbContext;
            _systemHistoryService = systemHistoryService;
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

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetFilteredProducts(int page = 1, string? productName = null, int? categoryId = null)
        {
            // Validate and adjust page and pageSize values if needed
            if (page < 1)
            {
                page = 1;
            }

            int pageSize = 8;

            // Calculate the number of items to skip based on the page size and number
            var skipAmount = (page - 1) * pageSize;

            // Convert the search input to lowercase
            var lowerCaseProductName = productName?.ToLower();

            // Build the base query
            var query = _dbContext.Products.AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(lowerCaseProductName))
            {
                query = query.Where(p => p.ProductName.ToLower().Contains(lowerCaseProductName));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Execute the query and apply ordering, skipping, and taking
            var products = await query
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

        [HttpGet("test")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> Test()
        {
            var products = await _dbContext.Products
                .OrderBy(p => p.ProductId)  // You may want to order by a specific property
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

        // POST: Products/post
        [HttpPost("post")]
        public async Task<ActionResult<Product>> PostProduct(ProductDTO product, int? userId = null)
        {
            if (ModelState.IsValid)
            {
                // Check if the category with the provided CategoryId exists
                var existingCategory = await _dbContext.Categories.FindAsync(product.CategoryId);
                if (existingCategory == null)
                {
                    // Category doesn't exist, return BadRequest
                    return BadRequest($"Category with ID {product.CategoryId} does not exist.");
                }

                // check name
                if (product.ProductName.Trim().IsNullOrEmpty()) return BadRequest("Name cant be empty!");

                // check price
                if (product.Price <= 0) return BadRequest($"Price must be > 0");

                Product newProduct = new Product();

                newProduct.ProductName = product.ProductName;
                newProduct.Price = product.Price;
                newProduct.Category = existingCategory;
                newProduct.Status = true;

                // Create a new SystemHistory entry
                var systemHistory = createSystemHistory(
                    "Products",
                    "Create",
                    await getActor(userId),
                    new
                    {
                        ProductName = newProduct.ProductName,
                        Price = newProduct.Price,
                        CategoryId = newProduct.Category?.CategoryId,
                        Category = newProduct.Category.CategoryName,
                        Status = newProduct.Status
                    }
                );

                try
                {

                    // Add the system history entry
                    await _systemHistoryService.CreateAsync(systemHistory);

                    _dbContext.Products.Add(newProduct);
                    await _dbContext.SaveChangesAsync();

                    return Ok();

                } catch (Exception ex)
                {
                    return BadRequest($"Error updating product: {ex.Message}");
                }
            }

            return BadRequest(ModelState);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDTO productDTO, int? userId = null)
        {
            // Retrieve the existing product from the database
            var existingProduct = await _dbContext.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return BadRequest($"Product ID {productDTO.CategoryId} does not exist.");
            }

            //check name
            if (productDTO.ProductName.Trim().IsNullOrEmpty()) return BadRequest("Name cant be empty!");

            //check price
            if (productDTO.Price <=0) return BadRequest($"Price must be > 0");

            // Check if the category with the provided CategoryId exists
            var existingCategory = await _dbContext.Categories.FindAsync(productDTO.CategoryId);
            if (existingCategory == null)
            {
                // Category doesn't exist, return BadRequest
                return BadRequest($"Category with ID {productDTO.CategoryId} does not exist.");
            }

            var systemHistory = createSystemHistory(
                "Products",
                "Update",
                await getActor(userId),
                new
                {
                    ProductId = existingProduct.ProductId, // Include any relevant fields
                    ProductName = existingProduct.ProductName,
                    Price = existingProduct.Price,
                    CategoryId = existingProduct.Category?.CategoryId,
                    Category = existingCategory.CategoryName,
                    Status = existingProduct.Status
                }
            );

            // Update only the properties you want to change
            existingProduct.ProductName = productDTO.ProductName;
            existingProduct.Price = productDTO.Price;
            existingProduct.Category = existingCategory;
            existingProduct.Status = productDTO.Status;

            try
            {
                // Add the system history entry
                await _systemHistoryService.CreateAsync(systemHistory);

                await _dbContext.SaveChangesAsync();

                return Ok();

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
            catch (Exception ex)
            {
                return BadRequest($"Error updating product: {ex.Message}");
            }
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

        private SystemHistory createSystemHistory(string table, string action, object actor, object value)
        {
            return new SystemHistory
            {
                action = action,
                actor = actor,
                table = table,
                time = DateTime.UtcNow,
                value = value,
            };
        }

        private bool ProductExists(int id)
        {
            return _dbContext.Products.Any(e => e.ProductId == id);
        }

        private async Task<object> getActor(int? userId)
        {
            if (userId == null) return null;

            var user = await _dbContext.Users.FindAsync(userId);

            /*if (user == null) return null;
            else return user;*/
            return user == null ? null : new
            {
                UserId = userId,
                Username = user.Username
            };
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
            public bool? Status { get; set; }
        }
    }
}
