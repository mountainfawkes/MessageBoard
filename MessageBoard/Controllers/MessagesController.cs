using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessageBoard.Models;

namespace MessageBoard.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class MessagesController : ControllerBase
  {
    private readonly MessageBoardContext _db;

    public MessagesController(MessageBoardContext db)
    {
      _db = db;
    }
      private bool MessageExists(int id)
    {
      return _db.Messages.Any(m => m.MessageId == id);
    }
    private string ConvertToStringDate(DateTime d) => d.ToShortDateString();

    // POST METHODS
    [HttpPost]
    public async Task<ActionResult<Message>> Post(Message message)
    {
      _db.Messages.Add(message);
      await _db.SaveChangesAsync();
      return CreatedAtAction("Post", new { id = message.MessageId }, message);

    }

    // GET METHODS
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Message>>> Get(string author, string createdAt)
    {
      var query = _db.Messages.AsQueryable();

      if (author != null)
      {
        query = query.Where(entry => entry.Author == author);
      }

      if (createdAt != null)
      {
        DateTime createdAtDate = DateTime.Parse(createdAt);
        Console.WriteLine($"-------createdAtHour: {createdAtDate.Hour}---------createdAtMinute: {createdAtDate.Minute}");
        if (createdAtDate.Hour > 0 || createdAtDate.Minute > 0)
        {
          query = query.Where(entry => entry.CreatedAt.Month == createdAtDate.Month && entry.CreatedAt.Day == createdAtDate.Day && entry.CreatedAt.Year == createdAtDate.Year && entry.CreatedAt.Hour == createdAtDate.Hour && entry.CreatedAt.Minute == createdAtDate.Minute);
        }
        else
        {
          query = query.Where(entry => entry.CreatedAt.Month == createdAtDate.Month && entry.CreatedAt.Day == createdAtDate.Day && entry.CreatedAt.Year == createdAtDate.Year);
        }
      }
      
      return await query.ToListAsync();
    }

   

    [HttpGet("{id}")]
    public async Task<ActionResult<Message>> GetMessage(int id)
    {
      var message = await _db.Messages.FindAsync(id);
      if (message == null)
      {
        return NotFound();
      }
      return message;
    }

    // PUT METHODS
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Message message)
    {
      if (id != message.MessageId)
      {
        return BadRequest();
      }
      _db.Entry(message).State = EntityState.Modified;
      try
      {
        await _db.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!MessageExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }
      return NoContent();
      }

    //DELETE METHODS
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
      var message = await _db.Messages.FindAsync(id);
      if (message == null)
      {
        return NotFound();
      }
      _db.Messages.Remove(message);
      await _db.SaveChangesAsync();

      return NoContent();
    }
  }
}
