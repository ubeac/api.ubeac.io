using uBeac.Api.InputModels;
using uBeac.Models;
using uBeac.Api.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace uBeac.Api.Controllers
{
    public class ManufacturerController : AdminBaseEntityController<Manufacturer, ManufacturerInputModel>
    {
        private readonly IManufacturerFacade _manufacturerFacade;
        public ManufacturerController(IManufacturerFacade manufacturerFacade) : base(manufacturerFacade)
        {
            _manufacturerFacade = manufacturerFacade;
        }
        
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 60)]
        public async virtual Task<ResultSet<List<Manufacturer>>> GetAll()
        {
            return await _manufacturerFacade.GetAllAsync();
        }

    }
}
