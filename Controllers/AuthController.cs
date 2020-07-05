using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Security;
using System.Text;
using JpProject.AspNetCore.PasswordHasher.Argon2;


using AuthServer.Entities;
using AuthServer.Controllers.Models;
using AuthServer.Repository;

namespace AuthServer.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthContext _repo;
        private readonly IConfiguration _config;
        private readonly SecurityTokenHandler _jwtHandler = new JwtSecurityTokenHandler();
        private readonly SigningCredentials _jwtSecret;

        public AuthController(AuthContext context, IConfiguration config)
        {
            _repo = context;
            _config = config;
            _jwtSecret = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config.GetValue<string>("Secret"))),
                SecurityAlgorithms.HmacSha256Signature);
        }

        [HttpPost]
        [Route("login")]
        public ActionResult<TokenResponse> Login([FromBody] LoginRequest request)
        {
            var hashedPassword = hashPassword(request.Password);
            Credential user = _repo
                    .Users
                    .AsEnumerable()
                    .Where(user => user.Username == request.Username && user.Password == hashedPassword)
                    .SingleOrDefault();
            if (user != null)
            {
                return Ok(new TokenResponse
                {
                    Token = generateNewToken(user.Username),
                });
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("validate")]
        public ActionResult Validate([FromBody] string token)
        {
            try
            {
                parseToken(token);
                return Ok();
            }
            catch
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("renew")]
        public ActionResult<TokenResponse> Renew([FromBody] string token)
        {
            try
            {
                var username = parseToken(token);
                return Ok(new TokenResponse
                {
                    Token = generateNewToken(username),
                });
            }
            catch
            {
                return Unauthorized();
            }
        }

        private string generateNewToken(string username)
        {
            var tokenData = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, username),
                    }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = _jwtSecret,
            };

            var token = _jwtHandler.CreateToken(tokenData);
            return _jwtHandler.WriteToken(token);
        }

        private string parseToken(string token)
        {
            SecurityToken validToken;
            var claims = _jwtHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtSecret.Key,
            }, out validToken);
            if (validToken != null)
            {
                return claims.Claims.First().Value;
            }
            else
            {
                throw new Exception();
            }
        }

        private string hashPassword(string password)
        {
            var dummy = new LoginRequest();
            var hasher = new Argon2Id<LoginRequest>();
            return hasher.HashPassword(dummy, password);
        }
    }
}
