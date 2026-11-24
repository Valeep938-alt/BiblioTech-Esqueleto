using BiblioTech2.Api.Respuestas;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTech2.Api.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected ActionResult Responder<T>(Resultado<T> resultado)
            => StatusCode(CodigoHttp(resultado), RespuestaApi<T>.Desde(resultado));
        protected ActionResult Responder (Resultado resultado)
            => StatusCode(CodigoHttp(resultado), RespuestaApi<object>.Desde(resultado));




        private static int CodigoHttp(Resultado resultado) => resultado.Tipo switch 
        {
            TipoResultado.Exito => StatusCodes.Status200OK,
            TipoResultado.NoEncontrado => StatusCodes.Status404NotFound,
            TipoResultado.ReglaNegocio => StatusCodes.Status409Conflict,
            TipoResultado.Validacion => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

    }
}
