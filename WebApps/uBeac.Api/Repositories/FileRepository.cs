using System;
using uBeac.Models;
using uBeac.Repositories.MongoDB;

namespace uBeac.Api.Repositories
{
    public interface IFileRepository: IGenericRepository<Guid, File>
    {
    }
}

namespace uBeac.Api.Repositories.MongoDB
{
    public class FileRepository : GenericRepository<Guid, File>, IFileRepository
    {
        public FileRepository(MainDatabase database) : base(database)
        {
        }

        protected override string CollectionName => typeof(File).Name;

    }
}
