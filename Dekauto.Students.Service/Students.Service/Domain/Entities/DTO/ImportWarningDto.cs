namespace Dekauto.Students.Service.Students.Service.Domain.Entities.DTO
{
    public sealed class ImportWarningDto
    {
        public string Code { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? FileName { get; init; }
        public string? SheetName { get; init; }
        public string? GroupName { get; init; }
        public string? StudentDisplayName { get; init; }
        public IReadOnlyList<int> MatchedRows { get; init; } = Array.Empty<int>();
    }
}
