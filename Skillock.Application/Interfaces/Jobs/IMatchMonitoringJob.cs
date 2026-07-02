namespace Skillock.Application.Interfaces;

public interface IMatchMonitoringJob
{
    Task Execute(Guid betId);
}
