namespace HistoryService.Entities
{
    public class Entry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public List<string> Path { get; set; }
    }

}
