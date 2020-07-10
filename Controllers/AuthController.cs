using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text;
using JpProject.AspNetCore.PasswordHasher.Argon2;


using AuthServer.Entities;
using AuthServer.Controllers.Models;
using AuthServer.Repository;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthContext _repo;
        private readonly AppSettings _settings;
        private readonly SecurityTokenHandler _jwtHandler = new JwtSecurityTokenHandler();
        private readonly SigningCredentials _jwtSecret;

        public AuthController(AuthContext context, IOptions<AppSettings> config, ILogger<AuthController> logger)
        {
            _logger = logger;
            _repo = context;
            _settings = config.Value;
            _jwtSecret = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_settings.Secret)),
                SecurityAlgorithms.HmacSha256Signature);

        }

        [HttpPost("create")]
        public ActionResult Create([FromBody] LoginRequest request)
        {
            var requestSource = HttpContext.Connection.RemoteIpAddress.ToString();
            if (_settings.WhitelistedIps.Contains(requestSource))
            {
                _repo
                    .Identities
                    .Add(new Identity
                    {
                        Username = request.Username,
                        HashedPassword = hashPassword(request.Password)
                    });
                _repo.SaveChanges();
                return Ok();

            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("login")]
        public ActionResult<TokenMessage> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation(request.ToString());
            var storedUser = _repo
                    .Identities
                    .AsEnumerable()
                    .SingleOrDefault(user => user.Username == request.Username && verifyHash(user.HashedPassword, request.Password));

            if (storedUser != null)
            {
                return Ok(new TokenMessage
                {
                    Token = generateNewToken(storedUser.Username),
                });
            }
            else
            {
                _logger.LogInformation($"Someone tried to log in, the cheeky bastartd");
                return Unauthorized();
            }
        }

        [HttpPost("validate")]
        public ActionResult Validate([FromBody] TokenMessage token)
        {
            try
            {
                parseToken(token.Token);
                return Ok();
            }
            catch
            {
                return Unauthorized();
            }
        }

        [HttpPost("renew")]
        public ActionResult<TokenMessage> Renew([FromBody] TokenMessage token)
        {
            try
            {
                var username = parseToken(token.Token);
                return Ok(new TokenMessage
                {
                    Token = generateNewToken(username),
                });
            }
            catch
            {
                return Unauthorized();
            }
        }

        [HttpGet("all")]
        public ActionResult<IEnumerable<Identity>> GetAllIdentities()
        {
            var requestSource = HttpContext.Connection.RemoteIpAddress.ToString();
            if (_settings.WhitelistedIps.Contains(requestSource))
            {
                return Ok(_repo.Identities.ToArray());
            }
            else
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
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ValidateActor = false,
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

        private bool verifyHash(string hashedPassword, string password)
        {
            var dummy = new LoginRequest();
            var hasher = new Argon2Id<LoginRequest>();
            return hasher.VerifyHashedPassword(dummy, hashedPassword, password) == PasswordVerificationResult.Success;
        }
    }
}
