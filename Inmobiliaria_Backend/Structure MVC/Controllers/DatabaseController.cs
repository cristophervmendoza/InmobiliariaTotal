using backend_csharpcd_inmo.Structure_MVC.Utils;
using Microsoft.AspNetCore.Mvc;

namespace backend_csharpcd_inmo.Structure_MVC.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        /// <summary>
        /// Endpoint para probar la conexión a MySQL
        /// </summary>
        /// <returns>Devuelve el estado de la conexión</returns>
        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            var result = db_single.GetConnection();

            if (result.Exito)
            {
                // Cerramos la conexión antes de devolver respuesta
                result.Conexion?.Close();
                return Ok(new
                {
                    ok = true,
                    mensaje = result.Mensaje
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    ok = false,
                    mensaje = result.Mensaje
                });
            }
        }
    }
}
