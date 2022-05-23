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
    public class ConfigController : ControllerBase
    {
        private readonly SmartParkingSystemContext _context;

        public ConfigController(SmartParkingSystemContext context)
        {
            _context = context;
        }

        [HttpGet("AllConfigs")]
        public IActionResult GetConfigs()
        {
            var configList = _context.Configs.OrderBy(x => x.Name).ToList();
            return Ok(configList);

        }

        [HttpGet("{id}")]
        public IActionResult GetConfigById(Guid id)
        {
            var config = _context.Configs.SingleOrDefault(x => x.Id == id);
            if (config is null)
            {
                throw new InvalidOperationException("Kayıt Bulunamadı!");
            }
            return Ok(config);
        }

        [HttpPost("Add")]
        public IActionResult AddConfig([FromBody] Config newConfig)
        {
            var config = _context.Configs.SingleOrDefault(x => x.Name == newConfig.Name);
            if (config is not null)
                return BadRequest();
            config = new Config();
            config.Id = Guid.NewGuid();
            config.Name= newConfig.Name;
            config.Value= config.Value;

            _context.Add(config);
            _context.SaveChanges();

            return Ok();

        }

        [HttpPut("{id}")]
        public IActionResult UpdateConfig(Guid id, [FromBody] Config updatedConfig)
        {
            var config = _context.Configs.SingleOrDefault(x => x.Id == id);

            if (config is null)
                throw new InvalidOperationException("Güncellenecek kayıt bulunamadı!");

            config.Name = updatedConfig.Name != default ? updatedConfig.Name : config.Name;
            config.Value = updatedConfig.Value!= default ? updatedConfig.Value: config.Value;


            _context.SaveChanges();

            return Ok();

        }

        [HttpDelete("{id}")]
        public IActionResult DeleteConfig(Guid id)
        {
            var config = _context.Configs.SingleOrDefault(x => x.Id == id);
            if (config is null)
                return BadRequest("Silinecek kayıt bulunamadı!");

            _context.Configs.Remove(config);
            _context.SaveChanges();

            return Ok();
        }



    }
}

