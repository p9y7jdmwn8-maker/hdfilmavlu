namespace VideoSite.Models;

public sealed class LoginViewModel
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
