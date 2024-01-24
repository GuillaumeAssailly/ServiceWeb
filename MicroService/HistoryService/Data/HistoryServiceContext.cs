using HistoryService.Entities;
using Microsoft.EntityFrameworkCore;


namespace HistoryService.Data
{
    public class HistoryServiceContext : DbContext
    {
        public DbSet<Entry> HistoryEntries { get; set; } = default!;
        public HistoryServiceContext(DbContextOptions<HistoryServiceContext> options)
            : base(options)
        {
        }

    }
}