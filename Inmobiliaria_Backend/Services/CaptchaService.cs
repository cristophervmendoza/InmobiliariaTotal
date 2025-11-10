using Inmobiliaria_Backend.Structure_MVC.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Inmobiliaria_Backend.Services
{
    public interface ICaptchaService
    {
        Task<bool> ValidateAsync(string token, string expectedAction, float minScore = 0.5f);
        Task<RecaptchaResponse?> RawAsync(string token);
    }

    public class CaptchaService : ICaptchaService
    {
        private readonly HttpClient _http;
        private readonly RecaptchaOptions _options;
        private readonly ILogger<CaptchaService> _logger;

        public CaptchaService(HttpClient http, IOptions<RecaptchaOptions> opt, ILogger<CaptchaService> logger)
        {
            _http = http;
            _options = opt.Value;
            _logger = logger;
        }

        public async Task<bool> ValidateAsync(string token, string expectedAction, float minScore = 0.5f)
        {
            var resp = await RawAsync(token);
            if (resp is null) return false;

            // Logs informativos para diagnóstico
            _logger.LogInformation("reCAPTCHA v3: success={Success}, action={Action}, score={Score}, host={Host}",
                resp.success, resp.action, resp.score, resp.hostname);

            if (resp.errorCodes != null && resp.errorCodes.Count > 0)
                _logger.LogWarning("reCAPTCHA errors: {errs}", string.Join(",", resp.errorCodes));

            // Validaciones duras
            if (!resp.success) return false;
            if (resp.score < minScore) return false;
            if (!string.IsNullOrWhiteSpace(expectedAction) &&
                !string.Equals(resp.action, expectedAction, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        public async Task<RecaptchaResponse?> RawAsync(string token)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _options.SecretKey),
                new KeyValuePair<string, string>("response", token)
            });

            using var httpResp = await _http.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
            if (!httpResp.IsSuccessStatusCode)
            {
                _logger.LogWarning("reCAPTCHA siteverify HTTP {Status}", httpResp.StatusCode);
                return null;
            }

            var payload = await httpResp.Content.ReadFromJsonAsync<RecaptchaResponse>();
            // Log de errores específicos (si existen)
            if (payload?.errorCodes != null && payload.errorCodes.Count > 0)
                _logger.LogWarning("reCAPTCHA errors: {errs}", string.Join(",", payload.errorCodes));

            return payload;
        }
    }
}
