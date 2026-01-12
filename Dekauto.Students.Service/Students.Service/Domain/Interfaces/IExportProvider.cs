using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IExportProvider
    {
        Task<ExportFileResult> ExportStudentCardAsync(Guid studentId);
        Task<ExportFileResult> ExportGroupCardsAsync(Guid groupId);
        Task<ExportFileResult> ExportDiplomaSupplementAsync(DiplomaSupplementRequest diplomaData);
    }
}
