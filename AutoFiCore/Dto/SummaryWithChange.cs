namespace AutoFiCore.Dto
{
    public class SummaryWithChange<T>
    {
        public Dictionary<string, T> Data { get; set; } = new Dictionary<string, T>();
        public double PercentageChange { get; set; }
    
    }
}
