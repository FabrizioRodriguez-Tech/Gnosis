using System.Threading.Tasks;
using Gnosis.Business.Models;

namespace Gnosis.Business.Services
{
    public interface IIAService
    {
        Task<IAResponse> ConsultarAsync(IARequest request);
    }
}
