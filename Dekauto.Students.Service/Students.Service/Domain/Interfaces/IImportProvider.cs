using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IImportProvider
    {
        Task<ImportFilesResult?> ImportFilesAsync(ImportFilesAdapter files);
    }
}
