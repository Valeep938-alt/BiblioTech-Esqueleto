using BiblioTech2.Api.Data;
using BiblioTech2.Api.DTOs.Autores;
using BiblioTech2.Api.Models;
using BiblioTech2.Api.Respuestas;
using BiblioTech2.Api.Services.Interfaces;
using BiblioTech2.Api.Constants;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace BiblioTech2.Api.Services
{
    public class AutorService(BibliotecaContext db) : IAutorService
    {
        public async Task<Resultado<List<AutorDto>>> ListarAsync()
        {
            var autores = await db.Autores
                .OrderBy(a => a.Nombre)
                .Select(a => new AutorDto
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    Nacionalidad = a.Nacionalidad,
                    CantidadLibros = a.Libros.Count()
                })
                .ToListAsync();
            return Resultado<List<AutorDto>>.ok(autores, MensajeAutor.Listado);
        }

        public async Task<Resultado<AutorDto>> ObtenerAsync(int id)
        {
            var autor = await db.Autores
                .Where(a => a.Id == id)
                .Select(a => new AutorDto
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    Nacionalidad = a.Nacionalidad,
                    CantidadLibros = a.Libros.Count
                })
                .FirstOrDefaultAsync();
            if (autor == null)
                return Resultado<AutorDto>.NoEncontrado(MensajeAutor.NoExiste(id));
            
            return Resultado<AutorDto>.ok(autor, MensajeAutor.Encontrado);
        }

        public async Task<Resultado<AutorDto>> CrearAsync(AutorGuardadoDto dto)
        {
            var autor = new Autor
            {
                Nombre = dto.Nombre.Trim(),
                Nacionalidad = string.IsNullOrWhiteSpace(dto.Nacionalidad)
                    ? null
                    : dto.Nacionalidad.Trim()
            };

            db.Autores.Add(autor);
            await db.SaveChangesAsync();
            var creado = new AutorDto
            {
                Id = autor.Id,
                Nombre = autor.Nombre,
                Nacionalidad = autor.Nacionalidad,
            };
            return Resultado<AutorDto>.ok(creado, MensajeAutor.Creado);
        }

        public async Task<Resultado> ActualizarAsync(int id, AutorGuardadoDto dto)
        {
            var autor = await db.Autores.FindAsync(id);
            if (autor == null)
                return Resultado.NoEncontrado(MensajeAutor.NoExiste(id));

            autor.Nombre = dto.Nombre.Trim();
            autor.Nacionalidad = string.IsNullOrWhiteSpace(dto.Nacionalidad)
                ? null
                : dto.Nacionalidad.Trim();

            await db.SaveChangesAsync();

            return Resultado.ok(MensajeAutor.Actualizado);
        }

        public async Task<Resultado> EliminarAsync(int id)
        {
            var autor = await db.Autores.FindAsync(id);
            if (autor == null)
                return Resultado.NoEncontrado(MensajeAutor.NoExiste(id));

            var cantidadLibros = await db.Libros.CountAsync(l => l.Autores.Any(a => a.Id == id));
            if (cantidadLibros > 0)
            {
                return Resultado.NoEncontrado(MensajeAutor.TieneLibros(id));
            }
            db.Autores.Remove(autor);
            await db.SaveChangesAsync();

            return Resultado.ok(MensajeAutor.Eliminado);
        }
    }
}