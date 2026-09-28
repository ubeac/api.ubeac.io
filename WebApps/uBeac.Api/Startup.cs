using System;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using uBeac.Api.Facades;
using uBeac.Api.InputModels;
using uBeac.Api.Middlewares;
using uBeac.Api.Repositories;
using uBeac.Api.Repositories.MongoDB;
using uBeac.Api.Services;
using uBeac.Mappings;
using uBeac.Models;
using uBeac.Repositories.MongoDB;
using uBeac.Serialization;
using uBeac.Web.Middlewares;

namespace uBeac.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = Configuration.GetValue<string>("IdsrvAuthority");
                    options.ApiName = "uBeacApi";
                    options.RequireHttpsMetadata = false;
                });

            services.AddAuthorization();

            services.AddMongoDatabses();

            services.AddRepositores();

            services.AddServices();

            services.AddFacades();

            services.AddMailServer();

            //services.AddMongoDBLog();

            services.AddLocalFileStorages();

            ////services.AddInMemoryCache();

            services.AddApiDocumantation(Configuration);

            services.AddResponseCompression();

            services.AddCustomCors(Configuration);

            services.AddSingleton<AesEncryption>();

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new CustomDictionarySerializer());
                });

            services.AddResponseCaching();

            services.AddHttpContextAccessor();

            //services.AddScoped<ISecurityContext, SecurityContext>();
            services.AddAccessControl();

            services.AddMappings();
            
        }

        public void Configure(IApplicationBuilder app)
        {
            //app.UseCors(o => o.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            if (Environment.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            app.UseAuthentication();
            app.UseTokenAuthentication();
            app.UseCustomCors();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();
            app.UseResponseCompression();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseResponseCaching();
            app.UseApiDocumantation(Environment);
            app.UseMvcWithDefaultRoute();
        }

    }

    public static class ConfigurationServicesExtensions
    {

        public static IServiceCollection AddMongoDatabses(this IServiceCollection services)
        {
            services.AddSingleton<IBsonSerializer, MongoDictionarySerializer>();

            services.AddMongo<MainDatabase>("uBeacDBConnection");
            services.AddMongo<SensorDataDatabase>("uBeacSensorDataDBConnection");
            services.AddMongo<GatewayDataDatabase>("uBeacGatewayDataDBConnection");
            services.AddMongo<DeviceSummaryDatabase>("uBeacDeviceSummaryDBConnection");

            BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.Attributes).SetSerializer(new MongoDictionarySerializer());
            });

            BsonClassMap.RegisterClassMap<SensorData>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(c => c.Data).SetSerializer(new MongoGeoJsonSerializer());
            });

            return services;
        }

        public static IServiceCollection AddRepositores(this IServiceCollection services)
        {
            services.AddRepository<IUserProfileRepository, UserProfileRepository>();
            services.AddRepository<IGatewayRepository, GatewayRepository>();
            services.AddRepository<IGatewayDataRepository, GatewayDataRepository>();
            services.AddRepository<IManufacturerRepository, ManufacturerRepository>();
            services.AddRepository<IProductRepository, ProductRepository>();
            services.AddRepository<IFirmwareRepository, FirmwareRepository>();
            services.AddRepository<ITeamRepository, TeamRepository>();
            services.AddRepository<IBuildingRepository, BuildingReposiroty>();
            services.AddRepository<IFloorRepository, FloorReposiroty>();
            services.AddRepository<IDashboardRepository, DashboardRepository>();
            services.AddRepository<IWidgetRepository, WidgetRepository>();
            services.AddRepository<IDeviceRepository, DeviceRepository>();
            services.AddRepository<IDeviceSummaryRepository, DeviceSummaryRepository>();
            services.AddRepository<ISensorRepository, SensorRepository>();
            services.AddRepository<ISensorDataRepository, SensorDataRepository>();
            services.AddRepository<IAccessRepository, AccessRepository>();
            services.AddRepository<IFileRepository, FileRepository>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddService<IAccessService, AccessService>();
            services.AddService<IBuildingService, BuildingService>();
            services.AddService<IDashboardService, DashboardService>();
            services.AddService<IDeviceService, DeviceService>();
            services.AddService<IBuildingFacade, BuildingFacade>();
            services.AddService<IFirmwareService, FirmwareService>();
            services.AddService<IFloorService, FloorService>();
            services.AddService<IGatewayService, GatewayService>();
            services.AddService<IManufacturerService, ManufacturerService>();
            services.AddService<IManufacturerService, ManufacturerService>();
            services.AddService<IProductService, ProductService>();
            services.AddService<ISensorService, SensorService>();
            services.AddService<ITeamService, TeamService>();
            services.AddService<IUserProfileService, UserProfileService>();
            services.AddService<IWidgetService, WidgetService>();
            services.AddService<IAccessService, AccessService>();
            services.AddService<IEmailService, EmailService>();
            services.AddService<IFileService, FileService>();

            return services;
        }

        public static IServiceCollection AddFacades(this IServiceCollection services)
        {
            services.AddFacade<IManufacturerFacade, ManufacturerFacade>();
            services.AddFacade<IBuildingFacade, BuildingFacade>();
            services.AddFacade<IDashboardFacade, DashboardFacade>();
            services.AddFacade<IDeviceFacade, DeviceFacade>();
            services.AddFacade<IFirmwareFacade, FirmwareFacade>();
            services.AddFacade<IFloorFacade, FloorFacade>();
            services.AddFacade<IGatewayFacade, GatewayFacade>();
            services.AddFacade<IProductFacade, ProductFacade>();
            services.AddFacade<ISensorFacade, SensorFacade>();
            services.AddFacade<ITeamFacade, TeamFacade>();
            services.AddFacade<IAccessFacade, AccessFacade>();
            services.AddFacade<IFileFacade, FileFacade>();

            return services;
        }

        public static IServiceCollection AddMappings(this IServiceCollection services)
        {
            var mapperConfigAction = new Action<MapperConfig>(cfg =>
            {
                cfg.CreateMap<ManufacturerInputModel, Manufacturer>();
                cfg.CreateMap<ProductInputModel, Product>();
                cfg.CreateMap<FirmwareInputModel, Firmware>();
                cfg.CreateMap<GatewayInputModelAdd, Gateway>();
                cfg.CreateMap<GatewayInputModelUpdate, Gateway>();
                cfg.CreateMap<TeamInputModelAdd, Team>();
                cfg.CreateMap<TeamInputModelUpdate, Team>();
                cfg.CreateMap<BuildingInputModelAdd, Building>();
                cfg.CreateMap<BuildingInputModelUpdate, Building>();
                cfg.CreateMap<FloorInputModelAdd, Floor>();
                cfg.CreateMap<FloorInputModelUpdate, Floor>();
                cfg.CreateMap<DashboardInputModelAdd, Dashboard>();
                cfg.CreateMap<DashboardInputModelUpdate, Dashboard>();
                cfg.CreateMap<WidgetInfo, Widget>();
                cfg.CreateMap<DeviceInputModelAdd, Device>();
                cfg.CreateMap<DeviceInputModelUpdate, Device>();
                cfg.CreateMap<SensorInputModelAdd, Sensor>();
                cfg.CreateMap<SensorInputModelUpdate, Sensor>();
                cfg.CreateMap<AccessInputModelAdd, Access>();
                cfg.CreateMap<AccessInputModelUpdate, Access>();
            });

            Mapping.Mapper = new Mapper(mapperConfigAction);

            return services;
        }

    }

}
