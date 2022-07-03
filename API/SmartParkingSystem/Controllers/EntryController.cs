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
    public class EntryController : ControllerBase
    {
        private readonly SmartParkingSystemContext _context;

        public EntryController(SmartParkingSystemContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("AllEntries")]
        public IActionResult GetEntries()
        {
            var entryList = _context.Entries.OrderBy(x => x.CheckinDate).ToList();
            return Ok(entryList);

        }

        [HttpGet("{id}")]
        public IActionResult GetEntryById(Guid id)
        {
            var entry = _context.Entries.SingleOrDefault(x => x.Id == id);
            if (entry is null)
            {
                throw new InvalidOperationException("Kayıt Bulunamadı!");
            }
            return Ok(entry);
        }

        [HttpPost("Add")]
        public IActionResult AddEntry([FromBody] Entry newEntry)
        {
            var entry = _context.Entries.SingleOrDefault(x => x.Plate == newEntry.Plate && x.CheckoutDate == null );
            if (entry is not null)
                return BadRequest();
            
            newEntry.Id = Guid.NewGuid();

            _context.Add(newEntry);
            _context.SaveChanges();

            return Ok();

        }

        [AllowAnonymous]
        [HttpPost("AddEntries")]
        public IActionResult AddEntries([FromBody] List<Entry> EntryList)
        {
            _context.Entries.AddRange(EntryList);
            return Ok();
        }

        [HttpPost("AddPlate")]
        public IActionResult AddPlate([FromBody] string plate)
        {
            var entry = _context.Entries.FirstOrDefault(x => x.Plate == plate && x.CheckoutDate == null);
            if (entry is not null)
            {
                entry.CheckoutDate = DateTime.Now;
                TimeSpan diff =(TimeSpan)(entry.CheckoutDate - entry.CheckinDate);
                CalculatePrice.price = 0;
                CalculatePrice.priceList = _context.Configs.ToList();
                entry.Price = CalculatePrice.Calculate(diff);
                _context.Update(entry);
            }
            else
            {
                entry = new Entry();
                entry.Id = Guid.NewGuid();
                entry.CheckinDate = DateTime.Now;
                entry.CheckoutDate = null;
                entry.Plate = plate.Trim();
                entry.Price = 0;
                _context.Add(entry);
            }

            _context.SaveChanges();

            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEntry(Guid id, [FromBody] Entry updatedEntry)
        {
            var entry = _context.Entries.SingleOrDefault(x => x.Id == id);

            if (entry is null)
                throw new InvalidOperationException("Güncellenecek kayıt bulunamadı!");

            entry.CheckoutDate= updatedEntry.CheckoutDate!= default ? updatedEntry.CheckoutDate : entry.CheckoutDate;
            entry.Price = updatedEntry.Price != default ? updatedEntry.Price : entry.Price;


            _context.SaveChanges();

            return Ok();

        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEntry(Guid id)
        {
            var entry = _context.Entries.SingleOrDefault(x => x.Id == id);
            if (entry is null)
                return BadRequest("Silinecek kayıt bulunamadı!");

            _context.Entries.Remove(entry);
            _context.SaveChanges();

            return Ok();
        }



    }
}

