using Inmobiliaria_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria_Backend.Structure_MVC.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class CaptchaController : ControllerBase
    {
        private readonly ICaptchaService _captcha;

        public CaptchaController(ICaptchaService captcha)
        {
            _captcha = captcha;
        }

        public record ValidateDto(string token, string action);

        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.token))
                return BadRequest(new { ok = false, mensaje = "Token requerido" });

            var ok = await _captcha.ValidateAsync(dto.token, dto.action, 0.5f);
            return Ok(new { ok });
        }
        [HttpPost("debug")]
        public async Task<IActionResult> Debug([FromBody] ValidateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.token))
                return BadRequest(new { ok = false, mensaje = "Token requerido" });

            var payload = await _captcha.RawAsync(dto.token);
            return Ok(payload); // { success, score, action, hostname, errorCodes }
        }
    }
}
