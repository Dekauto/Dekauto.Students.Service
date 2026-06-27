namespace Dekauto.Students.Service.Students.Service.Domain.Entities.DTO
{
    public sealed class ImportStudentsResponseDto
    {
        public List<StudentExportDto> Students { get; init; } = new();
        public List<ImportWarningDto> ImportWarnings { get; init; } = new();
    }
}
