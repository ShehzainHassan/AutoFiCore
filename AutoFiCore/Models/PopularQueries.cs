namespace AutoFiCore.Models
{
    public class PopularQueries
    {
        public int Id { get; set; }

        public string DisplayText { get; set; } = string.Empty;

        public int Count { get; set; } = 1;

        public DateTime LastAsked { get; set; } = DateTime.UtcNow;

        public double[] Embedding { get; set; }
    }
}
