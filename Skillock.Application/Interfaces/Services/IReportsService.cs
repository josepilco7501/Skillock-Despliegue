using System;
using System.Threading;

namespace Skillock.Application.Interfaces;

public interface IReportsService
{
    /// <summary>
    /// Genera un PDF con el reporte de todas las apuestas completadas y devuelve el contenido en bytes.
    /// </summary>
    Task<byte[]> GenerateCompletedBetsReportAsync(CancellationToken cancellationToken = default);
}

