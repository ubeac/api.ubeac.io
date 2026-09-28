using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using uBeac.Api.Services;
using uBeac.Models;
using uBeac.Security;

namespace uBeac.Api.Facades
{
    public interface IManufacturerFacade : IBaseEntityFacade<Manufacturer>
    {
        Task<ResultSet<List<Manufacturer>>> GetAllAsync(CancellationToken cancellationToken = default);
    }
    public class ManufacturerFacade : BaseEntityFacade<Manufacturer>, IManufacturerFacade
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly ISecurityContext _securityContext;

        public ManufacturerFacade(IManufacturerService manufacturerService, ISecurityContext securityContext) : base(manufacturerService, securityContext)
        {
            _manufacturerService = manufacturerService;
            _securityContext = securityContext;
        }

        public async Task<ResultSet<List<Manufacturer>>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await _manufacturerService.GetAllAsync(cancellationToken);
            // removing sensitive data
            // todo: think more about higher level permissions
            if (!_securityContext.HasAccess<Team>(AccessLevels.Admin, Guid.Empty))
                result.ForEach(x => x.Products.ForEach(y => y.Firmwares.ForEach(z => z.Processor = string.Empty)));

            return new ResultSet<List<Manufacturer>>(result);
        }
    }
}
