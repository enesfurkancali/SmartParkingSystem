using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartParkingSystem.Models;
using SmartParkingSystemAPI.Context;
using SmartParkingSystemAPI.TokenOperations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParkingSystemAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly SmartParkingSystemContext _context;
        private readonly SecurityContext _securityContext;
        readonly IConfiguration _configuration;

        public SecurityController(SmartParkingSystemContext context, SecurityContext securityContext, IConfiguration configuration)
        {
            _context = context;
            _securityContext = securityContext;
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] User user)
        {
            User usr = _context.Users.FirstOrDefault(x => x.Username == user.Username && x.Password == user.Password);

            if (usr is not null)
            {
                LoggedInUser loggedInUser = new LoggedInUser { Id = usr.Id, UserName = usr.Username, Password = usr.Password, Name=usr.Name, Surname=usr.Surname};
                TokenHandler handler = new TokenHandler(_configuration);
                Token token = handler.CreateAccessToken(loggedInUser);

                loggedInUser.Token = token.AccessToken;
                loggedInUser.RefreshToken = token.RefreshToken;
                loggedInUser.RefreshTokenExpireDate = token.Expiration.AddMinutes(30);
                _securityContext.SaveChanges();
                return Ok(token);
            }
            else
            {
                return BadRequest("Hatalı kullanıcı adı veya şifre!");
            }



        }
        [HttpGet("refreshToken")]
        public ActionResult<Token> RefreshToken([FromQuery] string token)
        {
            var user = _securityContext.LoggedInUsers.FirstOrDefault(x => x.RefreshToken == token && x.RefreshTokenExpireDate > DateTime.Now);
            if (user is not null)
            {
                TokenHandler handler = new TokenHandler(_configuration);
                Token newToken = handler.CreateAccessToken(user);

                user.Token = newToken.AccessToken;
                user.RefreshToken = newToken.RefreshToken;
                user.RefreshTokenExpireDate = newToken.Expiration.AddMinutes(30);
                _securityContext.SaveChanges();

                return newToken;
            }
            else
            {
                throw new InvalidOperationException("Valid bir Refresh Token bulunamadı!");
            }
        }
    }
}
