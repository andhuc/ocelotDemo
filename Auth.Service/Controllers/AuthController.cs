using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly sampleContext _dbContext;

    public AuthController(IConfiguration configuration, sampleContext dbContext)
    {
        _configuration = configuration;
        _dbContext = dbContext;
    }

    [HttpPost("sign-in")]
    public IActionResult Post([FromBody] UserForm user)
    {
        // Check if the user credentials are valid (you need to implement this logic)
        User foundUser = ValidateUserCredentials(user.Username, user.Password);

        if (foundUser == null)
        {
            // Return some error response if the credentials are not valid
            return BadRequest("Invalid credentials");
        }

        var claims = new List<Claim>
        {
            new Claim("Username", foundUser.Username),
            new Claim("Role", foundUser.Role.ToString(), ClaimValueTypes.Integer32),
            new Claim("UserId", foundUser.UserId.ToString(), ClaimValueTypes.Integer32)
        };
        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
             notBefore: now,
             expires: now.Add(TimeSpan.FromMinutes(2)),
             claims: claims,
             signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT"])), SecurityAlgorithms.HmacSha256)
         );

        AuthToken authToken = new AuthToken
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
            Expires = TimeSpan.FromMinutes(30).TotalSeconds
        };

        return Ok(authToken);
    }

    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromBody] TokenValidationRequest tokenRequest)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT"]);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(tokenRequest.AccessToken, validationParameters, out validatedToken);

            var expiresClaim = principal.FindFirst("exp");
            if (expiresClaim == null)
            {
                return BadRequest(new { Valid = false, Message = "Token does not contain expiration claim." });
            }

            var expirationTime = UnixTimeStampToDateTime(double.Parse(expiresClaim.Value));

            if (expirationTime < DateTime.UtcNow)
            {
                return BadRequest(new { Valid = false, Message = "Token has expired." });
            }

            return Ok(new { Valid = true, Expires = expirationTime });
        }
        catch (Exception)
        {
            // Token validation failed
            return BadRequest(new { Valid = false, Message = "Invalid token." });
        }
    }

    private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)unixTimeStamp);
        return dateTimeOffset.UtcDateTime;
    }


    private User ValidateUserCredentials(string username, string password)
    {
        // Implement logic to validate the user credentials against your database
        // For example, query the database to check if the username and password match a user record

        // Replace the logic below with your actual database validation logic
        var user = _dbContext.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

        return user;
    }
}

public class UserForm
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class AuthToken
{
    public string AccessToken { get; set; }
    public double Expires { get; set; }
}
public class TokenValidationRequest
{
    public string AccessToken { get; set; }
}
