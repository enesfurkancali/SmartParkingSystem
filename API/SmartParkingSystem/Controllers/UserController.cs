using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartParkingSystem.Models;
using SmartParkingSystemAPI.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParkingSystemAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly SmartParkingSystemContext _context;

        public UserController(SmartParkingSystemContext context)
        {
            _context = context;
        }

        [HttpGet("AllUsers")]
        public IActionResult GetUsers()
        {
            var userList = _context.Users.OrderBy(x => x.Username).ToList();
            return Ok(userList);

        }

        [HttpGet("{id}")]
        public IActionResult GetUserById(Guid id)
        {
            var user = _context.Users.SingleOrDefault(x => x.Id == id);
            if (user is null)
            {
                throw new InvalidOperationException("Kullanıcı Bulunamadı!");
            }
            return Ok(user);
        }

        [HttpPost("Add")]
        public IActionResult AddUser([FromBody] User newUser)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username== newUser.Username);
            if (user is not null)
                return BadRequest();

            newUser.Id = Guid.NewGuid();
      
            _context.Add(newUser);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(Guid id, [FromBody] User updatedUser)
        {
            var user = _context.Users.SingleOrDefault(x => x.Id == id);

            if (user is null)
                throw new InvalidOperationException("Güncellenecek kullanıcı bulunamadı!");

            user.Username = updatedUser.Username != default ? updatedUser.Username: user.Username;
            user.Name= updatedUser.Name!= default ? updatedUser.Name: user.Name;
            user.Surname = updatedUser.Surname!= default ? updatedUser.Surname: user.Surname;
       

            _context.SaveChanges();

            return Ok();

        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(Guid id)
        {
            var user = _context.Users.SingleOrDefault(x => x.Id == id);
            if (user is null)
                return BadRequest("Silinecek kullanıcı bulunamadı!");

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok();
        }



    }
}

