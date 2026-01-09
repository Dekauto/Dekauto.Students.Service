using Dekauto.Students.Service.Students.Service.Domain.Entities;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IDisciplineGradesRepository : IRepository<DisciplineGrade>
    {
        // Получение всех оценок конкретного студента
        Task<IEnumerable<DisciplineGrade>> GetByStudentIdAsync(Guid studentId);

        Task DeleteRangeAsync(IEnumerable<DisciplineGrade> range);
    }
}