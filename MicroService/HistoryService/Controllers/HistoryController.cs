using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HistoryService.Data;
using HistoryService.Entities;
using Microsoft.EntityFrameworkCore;

namespace HistoryService.Controllers
{

    public class HistoryEntryService
    {
        private readonly HistoryServiceContext _dbContext;

        public HistoryEntryService(HistoryServiceContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveHistoryEntry(Entry historyEntry)
        {
            _dbContext.HistoryEntries.Add(historyEntry);
            await _dbContext.SaveChangesAsync();
        }

        public List<Entry> GetHistoryEntries()
        {
            return _dbContext.HistoryEntries.ToList();
        }
        public IEnumerable<Entry> GetHistoryEntriesByUserId(int userId)
        {
            return _dbContext.HistoryEntries.Where(entry => entry.UserId == userId.ToString()).ToList();
        }
        public async Task DeleteEntry(int id)
        {
            var entry = await _dbContext.HistoryEntries.FindAsync(id);
            _dbContext.HistoryEntries.Remove(entry);
            await _dbContext.SaveChangesAsync();
        }
    }


    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly HistoryEntryService _historyService;

        public HistoryController(HistoryEntryService historyService)
        {
            _historyService = historyService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddHistoryEntry(Entry historyEntry)
        {
            Console.WriteLine("Ajout d'une entrée");
            await _historyService.SaveHistoryEntry(historyEntry);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntry(int id)
        {
            await _historyService.DeleteEntry(id);
            return NoContent();
        }

        [HttpGet("all")]
        public IActionResult GetAllHistoryEntries()
        {
            var historyEntries = _historyService.GetHistoryEntries();
            return Ok(historyEntries);
        }


        [HttpGet("{userId}")]
        public IActionResult GetHistoryEntriesByUserId(int userId)
        {

            var historyEntries = _historyService.GetHistoryEntriesByUserId(userId);

            if (historyEntries == null || !historyEntries.Any())
            {
                return NotFound($"Aucune entrée d'historique trouvée pour l'utilisateur avec l'ID {userId}.");
            }

            return Ok(historyEntries);
        }





    }

}
