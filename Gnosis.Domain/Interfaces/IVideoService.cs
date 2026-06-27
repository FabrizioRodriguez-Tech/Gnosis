namespace Gnosis.Domain.Interfaces;

public interface IVideoService
{
    Task<string> GetRandomFocusVideoAsync();
}