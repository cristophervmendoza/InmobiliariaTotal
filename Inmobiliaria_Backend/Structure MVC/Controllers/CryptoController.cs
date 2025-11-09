using Inmobiliaria_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inmobiliaria_Backend.Structure_MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly IPasswordService _pwd;
        public CryptoController(IPasswordService pwd) => _pwd = pwd;

        [HttpPost("hash")]
        public IActionResult Hash([FromBody] HashRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { ok = false, mensaje = "Password requerido" });

            var hash = _pwd.Hash(req.Password);
            return Ok(new { ok = true, hash });
        }

        [HttpPost("verify")]
        public IActionResult Verify([FromBody] VerifyRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.Hash))
                return BadRequest(new { ok = false, mensaje = "Password y hash requeridos" });

            var match = _pwd.Verify(req.Password, req.Hash);
            var needsRehash = match && _pwd.NeedsRehash(req.Hash);
            return Ok(new { ok = true, match, needsRehash });
        }
    }

    public record HashRequest(string Password);
    public record VerifyRequest(string Password, string Hash);
}
