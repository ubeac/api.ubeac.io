using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Repositories;
using uBeac.Models;
using System.Linq;

namespace uBeac.Api.Services
{
    public interface IManufacturerService: IBaseEntityService<Manufacturer>
    {
    }

    public class ManufacturerService : BaseEntityService<Manufacturer>, IManufacturerService
    {
        public readonly IManufacturerRepository _manufacturerRepository;
        public readonly IProductRepository _productRepository;
        public readonly IFirmwareRepository _firmwareRepository;

        public ManufacturerService(IManufacturerRepository manufacturerRepository, 
                                   IProductRepository productRepository, 
                                   IFirmwareRepository firmwareRepository) : base(manufacturerRepository)
        {
            _manufacturerRepository = manufacturerRepository;
            _productRepository = productRepository;
            _firmwareRepository = firmwareRepository;
        }

        public override async Task<List<Manufacturer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var manufacturerTask = _manufacturerRepository.GetAllAsync(cancellationToken);
            var productTask = _productRepository.GetAllAsync(cancellationToken);
            var firmwareTask = _firmwareRepository.GetAllAsync(cancellationToken);

            await Task.WhenAll(new List<Task> { manufacturerTask, productTask, firmwareTask });

            var manufacturersDic = manufacturerTask.Result.ToDictionary(x => x.Id, y => y);
            var productsDic = productTask.Result.ToDictionary(x => x.Id, y => y);
            var firmwaresDic = firmwareTask.Result.ToDictionary(x => x.Id, y => y);

            foreach (var firmware in firmwaresDic.Values)
            {
                productsDic[firmware.ProductId].Firmwares.Add(firmware);
            }

            foreach (var product in productsDic.Values)
            {
                manufacturersDic[product.ManufacturerId].Products.Add(product);                
            }

            return manufacturersDic.Values.ToList();
        }
    }
}
