using Skillock.Application.Interfaces;
using Skillock.Domain.Enums;

namespace Skillock.Infrastructure.Services.EsportApiClients;

public class EsportApiClientFactory(IEnumerable<IEsportApiClient> clients) : IEsportApiClientFactory
{
    public IEsportApiClient GetClient(EsportGame game)
        => clients.FirstOrDefault(c => c.JuegoSoportado == game)
           ?? throw new NotSupportedException($"No hay cliente para {game}");
}

