using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab10.Infrastructure.Context;

namespace Lab10.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly Lab10Context _context;

    // Inyectamos directamente el contexto para probar la conexión
    public TestController(Lab10Context context)
    {
        _context = context;
    }

    [HttpGet("ping")]
    public async Task<IActionResult> PingDatabase()
    {
        try
        {
            // Realiza una consulta ultra rápida a la tabla de cursos
            var totalCursos = await _context.Courses.CountAsync();
            
            return Ok(new 
            { 
                Message = "¡Conexión y Arquitectura funcionando perfectamente! 🚀", 
                DatabaseStatus = "Conectado a SQL LocalDB",
                TotalCoursesInDb = totalCursos 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                Message = "Error al conectar con la base de datos ❌", 
                Details = ex.Message 
            });
        }
    }
}
