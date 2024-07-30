namespace AuthServer.Core.Models;

public class AspNetUserPhoneCode
{
    public string Id { get; set; }
    public string? PhoneLoginCode { get; set; }
    public DateTime? CreatedDate { get; set; }
    public UserApp UserApp { get; set; }
}
