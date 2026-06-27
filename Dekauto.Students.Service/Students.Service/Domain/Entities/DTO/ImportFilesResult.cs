using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;

namespace Dekauto.Students.Service.Students.Service.Domain.Entities.DTO
{
    public sealed class ImportFilesResult
    {
        public DiplomaSupplementData? Data { get; init; }
        public IReadOnlyList<ImportWarningDto> ImportWarnings { get; init; } = Array.Empty<ImportWarningDto>();
    }
}
