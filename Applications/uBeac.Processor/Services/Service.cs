using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Models;
using uBeac.Processor.Repositories;

namespace uBeac.Processor.Services
{

    public interface IService
    {
        Task<List<Firmware>> GetAllFirmwares(CancellationToken cancellationToken = default);
    }

    public class Service: IService
    {

        private readonly IRepository _repository;
        public Service(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Firmware>> GetAllFirmwares(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllFirmwares(cancellationToken);
        }
    }

}
