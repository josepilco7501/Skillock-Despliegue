using Skillock.Domain.Enums;

namespace Skillock.Domain.DTOs.Requests;
public record CrearApuestaRequest(
    EsportGame Game,
    int TeamSizeA,
    Guid GameAccountId,
    string MatchId,
    decimal MontoInicial,
    DateTime MatchStartedAt
);

public record UnirseComoRivalRequest(int TeamSizeB);
public record ProponerMontoRequest(decimal MontoPerTeam);
public record ConfirmarMontoRequest();
public record ElegirModalidadRequest(FundingMode Modalidad);
public record AportarFondosRequest(Guid BetId, Guid BetPartyId, decimal Monto, Guid UserId);
public record InvitarMiembroRequest(Guid BetId, Guid BetPartyId, Guid UsuarioInvitadoId, Guid LiderId);
public record CancelarApuestaRequest(Guid BetId, Guid SolicitanteId, string? Motivo = null);
public record ReportarFalloRequest(Guid BetId, Guid SolicitanteId, string Motivo);

public record PaginacionRequest(int Pagina = 1, int Tamano = 20)
{
    public int TamanoSeguro => Math.Min(Tamano, 100);
}

