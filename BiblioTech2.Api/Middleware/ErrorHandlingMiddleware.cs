using BiblioTech.Api.Exceptions;
using BiblioTech2.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech2.Api.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware>logger)
{
    private static readonly Dictionary<string, string> MensajeRestricciones = new()
    {
        ["UQ_LIBROS_ISBN"] = "Ya existe un libro con ese ISBN",
        ["FK_LibroAutor_Autores"] = "El autor libros Asociados",
        ["UQ_Socios_Documento"] = "Ya existe un socio con este Documento",
        ["UQ_Socios_Email"] = "Ya existe un socio con este correo",
        ["FK_Prestamos_Libros"] = "El libro tiene prestamos asociados",
        ["FK_Prestamo_Socios"] = "El socio tiene prestamos asociados",
        ["CK_Libros_Diponibles"] = "Los ejemplares no pueden ser negativos ni mayores que los totales",
        ["CK_Socios_Email"] = "El correo no tiene un formato valido"
    };

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await next(contexto);
        }
        catch(Exception ex) when (!contexto.Response.HasStarted)
        {
            await ResponderAsync(contexto, ex);
        }
    }

    private async Task ResponderAsync(HttpContext contexto, Exception ex)
    {
        var (codigo, titulo, detalle) = ex switch
        {
            NoEncontradoException e => (StatusCodes.Status404NotFound, "Recurso no encontrado", e.Message),
            ReglaNegocioException e => (StatusCodes.Status409Conflict, "Regla de negocio", e.Message),
            DbUpdateException e => (StatusCodes.Status409Conflict, "Conflicto con los datos", TraducirErrorBaseDatos(e)),
            _ => (StatusCodes.Status500InternalServerError, "Error interno",
                  "Ocurrió un error inesperado. Intenta de nuevo más tarde.")
        };
        if (codigo == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(ex, "Error no controlado en {Ruta}", contexto.Request.Path);
        }
        else
        {
            logger.LogWarning("{Codigo} en {Ruta}: {Mensaje}", codigo, contexto.Request.Path, detalle);
        }

        var problema = new ProblemDetails
        {
            Status = codigo,
            Title = titulo,
            Detail = detalle,
            Instance = contexto.Request.Path
        };

        contexto.Response.StatusCode = codigo;
        await contexto.Response.WriteAsJsonAsync(problema, options : null, contentType:
            "application/problem+json");

    }

    private static string TraducirErrorBaseDatos(DbUpdateException ex)
    {
        var mensajeSql = ex.InnerException?.Message ?? ex.Message;
        foreach (var (restriccion, mensaje) in MensajeRestricciones)
        {
            if(mensajeSql.Contains(restriccion, StringComparison.OrdinalIgnoreCase))
            {
                return mensaje;
            }
        }
        return "La operacion viola una restriccion de la base de datos";
    }



}
