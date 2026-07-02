namespace Skillock.Application.Interfaces;

public interface IMatchTimeoutJob
{
    Task Execute(Guid betId);
}
