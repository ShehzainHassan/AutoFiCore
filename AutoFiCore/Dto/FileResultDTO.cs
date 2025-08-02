namespace AutoFiCore.Dto
{
    public class FileResultDTO
    {
        public byte[] Content { get; set; } = null!;
        public string ContentType { get; set; } = "text/csv";
        public string FileName { get; set; } = string.Empty;
    }
}