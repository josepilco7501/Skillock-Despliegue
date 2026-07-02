using Skillock.Domain.Enums;

namespace Skillock.Application.Interfaces;

public interface IEsportApiClientFactory
{
    /// <summary>
    /// Retorna el cliente correcto según el juego indicado.
    /// </summary>
    IEsportApiClient GetClient(EsportGame game);
}

