namespace AuthServer.Core.Models;

public class UserRefreshToken
{
    public string? UserAppId { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime ExpirationDate { get; set; }
}
