using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HistoryService.Data;
using HistoryService.Entities;

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
            await _historyService.SaveHistoryEntry(historyEntry);
            return Ok();
        }

        [HttpGet("all")]
        public IActionResult GetAllHistoryEntries()
        {
            var historyEntries = _historyService.GetHistoryEntries();
            return Ok(historyEntries);
        }
    }

}
