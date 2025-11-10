namespace Inmobiliaria_Backend.Structure_MVC.Models;

public class RecaptchaResponse
{
    public bool success { get; set; }
    public float score { get; set; }
    public string action { get; set; } = string.Empty;
    public DateTime challenge_ts { get; set; }
    public string hostname { get; set; } = string.Empty;
    public List<string>? errorCodes { get; set; }
}
