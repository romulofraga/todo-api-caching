using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class DriverController : ControllerBase
  {
    private readonly AppDbContext _context;
    private readonly ILogger<TodoItemsController> _logger;
    private readonly ICacheService _cacheService;

    DateTimeOffset expiryTime = DateTimeOffset.Now.AddSeconds(30);

    public DriverController(
        AppDbContext context,
        ILogger<TodoItemsController> logger,
        ICacheService cacheService
        )
    {
      _context = context;
      _logger = logger;
      _cacheService = cacheService;
    }

    // GET: api/Driver
    [HttpGet("drivers")]
    public async Task<IActionResult> GetDrivers()
    {
      var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");
      if (cacheData != null && cacheData.Count() > 0) return Ok(cacheData);
      if (_context.Drivers == null)
      {
        return NotFound();
      }
      cacheData = await _context.Drivers.ToListAsync();

      _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiryTime);

      return Ok(cacheData);
    }

    // GET: api/Driver/5
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("{id}")]
    public async Task<ActionResult<Driver>> GetDriver(int id)
    {
      if (_context.Drivers == null)
      {
        return NotFound();
      }
      var driver = await _context.Drivers.FindAsync(id);

      if (driver == null)
      {
        return NotFound();
      }

      return driver;
    }

    // PUT: api/Driver/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDriver(int id, Driver driver)
    {
      if (id != driver.Id)
      {
        return BadRequest();
      }

      _context.Entry(driver).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!DriverExists(id))
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

    // POST: api/Driver
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("addDriver")]
    public async Task<ActionResult<Driver>> PostDriver(Driver driver)
    {
      if (_context.Drivers == null)
      {
        return Problem("Entity set 'AppDbContext.Drivers'  is null.");
      }
      var addedObject = await _context.Drivers.AddAsync(driver);

      _cacheService.SetData<Driver>($"driver{driver.Id}", addedObject.Entity, expiryTime);

      await _context.SaveChangesAsync();

      return Ok(addedObject.Entity);
    }

    // DELETE: api/Driver/5
    [HttpDelete("deleteDriver")]
    public async Task<IActionResult> DeleteDriver(int id)
    {

      if (_context.Drivers == null)
      {
        return NotFound();
      }

      var driver = await _context.Drivers.FindAsync(id);
      if (driver == null)
      {
        return NotFound();
      }

      _context.Remove(driver);
      _cacheService.RemoveData($"driver{driver.Id}");
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool DriverExists(int id)
    {
      return (_context.Drivers?.Any(e => e.Id == id)).GetValueOrDefault();
    }
  }
}
