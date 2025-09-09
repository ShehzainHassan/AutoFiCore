namespace AutoFiCore.Dto
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }
}
