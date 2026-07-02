namespace Skillock.Application.Interfaces;

public interface IBetExpirationJob
{
    Task Execute(Guid betId);
}
