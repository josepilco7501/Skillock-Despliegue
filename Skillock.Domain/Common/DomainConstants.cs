namespace Skillock.Domain.Common;

public class DomainConstants
{
    public const int UnitTimeExpiracion = 5;
    public static readonly TimeSpan TiempoExpiracionApuesta = TimeSpan.FromMinutes(UnitTimeExpiracion);
    public const decimal PlatformFeePercent = 0.07m;
    public const decimal MontoMaximoApuesta = 1000m;
    public static readonly Guid PlatformWalletId = Guid.Parse("00000000-0000-0000-0000-000000000001");
}