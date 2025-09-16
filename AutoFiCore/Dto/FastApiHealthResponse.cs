namespace AutoFiCore.Dto
{
    public class FastApiHealthResponse
    {
        public bool db { get; set; }
        public bool ml_models_loaded { get; set; }
        public bool orchestrator_ready { get; set; }
        public string version { get; set; } = string.Empty;
    }
}
