using Cart.Service.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

[ApiController]
[Route("[controller]")]
public class CartController : ControllerBase
{
    private static readonly Dictionary<int, List<CartItem>> _carts = new Dictionary<int, List<CartItem>>();
    private readonly sampleContext _context;

    public CartController(sampleContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public ActionResult<IEnumerable<CartItem>> Get(int userId)
    {
        if (_carts.TryGetValue(userId, out var userCart))
        {
            return Ok(userCart);
        }
        else
        {
            return Ok(new List<CartItem>()); // Return an empty list if the user has no cart
        }
    }

    [HttpPost("{userId}")]
    public ActionResult AddToCart(int userId, CartItem item)
    {
        if (!_carts.ContainsKey(userId))
        {
            _carts[userId] = new List<CartItem>();
        }

        // Check if the product exists in the database
        var product = _context.Products.FirstOrDefault(p => p.ProductId == item.ProductId);

        if (product == null)
        {
            return BadRequest($"Product with ID {item.ProductId} not found.");
        }

        // Check if the cart already contains an item with the same productId
        var existingItem = _carts[userId].FirstOrDefault(x => x.ProductId == item.ProductId);

        if (existingItem != null)
        {
            // check quantity
            if (existingItem.Quantity + item.Quantity < 0 || item.Quantity == 0)
                return BadRequest("Invalid quantity!");

            // If an item with the same productId exists, update its quantity
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            // If no item with the same productId exists, add the new item
            _carts[userId].Add(item);
        }

        // Save changes to the database
        _context.SaveChanges();

        return Ok();
    }

}

public class CartItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
