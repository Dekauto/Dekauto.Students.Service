using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IDisciplineGradesService
    {
        Task<IEnumerable<DisciplineGradeDto>> GetByStudentIdAsync(Guid studentId);
        Task<DisciplineGradeDto> GetByIdAsync(Guid id);
        Task AddAsync(DisciplineGradeDto gradeDto, Guid studentId);
        Task UpdateAsync(Guid id, DisciplineGradeDto gradeDto);
        Task DeleteAsync(Guid id);
    }
}
