using CouponsCodeSystemServer.Data;
using CouponsCodeSystemServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CouponsCodeSystemServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        public UsersController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost("AddUser")]
        public async Task<IActionResult> AddUser(User user)
        {
            var checkUser = await _appDbContext.Users.AnyAsync(usr => usr.Email == user.Email);
            if (checkUser!)
            {
                return BadRequest(new { message = "User Already Exist" });
            }
            user.Password = HashPassword(user.Password);
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();
            return Ok(new { User = user });
        }

        [HttpGet("AllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _appDbContext.Users.ToArrayAsync();
            return Ok(users);
        }

        // [HttpPost("hash-password")]
        private string HashPassword(string password)
        {
            // Hash the password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        // [HttpGet("verify-password")]
        private bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            //  TODO: Verify the input password with the stored hashed password
            return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
        }

        [HttpPost("IsUserValid")]
        public async Task<IActionResult> IsUserValid(string email, string password)
        {
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !VerifyPassword(user.Password, password))
            {
                return Unauthorized("Invalid username or password");
            }
            else
            {
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }
        }
        private static string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("ThisIsASecretKeyThatIsAtLeast32Chars!");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            // System.Console.WriteLine(token);
            return tokenHandler.WriteToken(token);
        }
        private static JwtSecurityToken ConvertJwtStringToJwtSecurityToken(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            return token;
        }
        private static Dictionary<string, string> GetPayload(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var payload = new Dictionary<string, string>();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);

                foreach (var claim in jwtToken.Claims)
                {
                    payload[claim.Type] = claim.Value;
                }
            }
            else
            {
                throw new ArgumentException("Invalid JWT format");
            }

            return payload;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser(string token)
        {
            if (!IsValidJwt(token))
            {
                return BadRequest("Invalid or expired JWT token.");
            }
            //TODO: decode the token
            Dictionary<string, string> tokenPayload = GetPayload(token);
            //TODO: check if token expiered
            var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(tokenPayload["exp"])).UtcDateTime;
            if (DateTime.UtcNow > expirationTime)
            {
                return Unauthorized(new { message = "Token Expierd" });
            }
            //if expiered: return error 401(unAuthorized)
            //else
            var user = await _appDbContext.Users.FirstOrDefaultAsync((u) => u.Email == tokenPayload["email"]);
            if (user == null)
            {
                return StatusCode(404);
            }
            //TODO: fetch the user from the database
            // if not found return error(server error)....
            // else return the user protected(no password)

            return Ok(new { User = user });
        }

        private static bool IsValidJwt(string token)
        {
            try
            {
                // Ensure the token is not null or empty
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                // Parse and validate the JWT
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                    return false;

                // Define validation parameters
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,  // If you want to validate the issuer, set this to true
                    ValidateAudience = false,  // If you want to validate the audience, set this to true
                    ValidateLifetime = true,  // Validate expiration date
                    ClockSkew = TimeSpan.Zero,  // Set the clock skew to zero (optional)
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("ThisIsASecretKeyThatIsAtLeast32Chars!"))  // Use your signing key
                };

                // Validate the token signature
                handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // If the code reaches here, the JWT is valid
                return true;
            }
            catch (Exception)
            {
                // If any exception occurs during parsing or validation, the token is invalid
                return false;
            }
        }
    }
}

