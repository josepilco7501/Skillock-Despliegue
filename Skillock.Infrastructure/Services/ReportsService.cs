using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Skillock.Application.Interfaces;
using Skillock.Domain.Enums;
using Skillock.Domain.Models;

namespace Skillock.Infrastructure.Services;

public class ReportsService(IUnitOfWork unitOfWork) : IReportsService
{
    public async Task<byte[]> GenerateCompletedBetsReportAsync(CancellationToken cancellationToken = default)
    {
        // Obtener todas las apuestas completadas
        var (items, _) = await unitOfWork.Bets.GetByStatusAsync(BetStatus.Completed, 1, int.MaxValue, cancellationToken);
        var bets = items.ToList().AsReadOnly();

        // Crear documento QuestPDF
        var document = new CompletedBetsDocument(bets);

        await using var ms = new MemoryStream();
        document.GeneratePdf(ms);
        return ms.ToArray();
    }

    private class CompletedBetsDocument : IDocument
    {
        private readonly IReadOnlyList<Bet> _bets;
        public CompletedBetsDocument(IReadOnlyList<Bet> bets) => _bets = bets;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text("Reporte de apuestas completadas").FontSize(16).Bold();

                page.Content().Column(col =>
                {
                    col.Spacing(5);

                    // Tabla header
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Id").Bold();
                        row.RelativeItem().Text("Game").Bold();
                        row.RelativeItem().Text("Lider A").Bold();
                        row.RelativeItem().Text("Lider B").Bold();
                        row.RelativeItem().Text("Agreed").Bold();
                        row.RelativeItem().Text("PremioNeto").Bold();
                        row.RelativeItem().Text("MatchId").Bold();
                        row.RelativeItem().Text("CompletedAt").Bold();
                    });

                    foreach (var b in _bets)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(b.Id.ToString());
                            row.RelativeItem().Text(b.Game.ToString());
                            row.RelativeItem().Text(ObtenerLider(b.TeamA));
                            row.RelativeItem().Text(ObtenerLider(b.TeamB));
                            row.RelativeItem().Text(b.AgreedAmountPerTeam?.ToString("F2") ?? "-");
                            row.RelativeItem().Text(b.PremioNeto?.ToString("F2") ?? "-");
                            row.RelativeItem().Text(b.MatchId ?? "-");
                            row.RelativeItem().Text(b.CompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-");
                        });
                    }
                });

                page.Footer().AlignCenter().Text("Skillock - Reporte de apuestas completadas");
            });
        }

        private static string ObtenerLider(BetParty? party)
        {
            if (party is null)
                return "-";

            var leader = party.Members.FirstOrDefault(m => m.Role == PartyRole.Leader);
            if (leader is null)
                return "-";

            return leader.User.Username;
        }
    }
}


