using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class DisciplineGradesRepository : IDisciplineGradesRepository
    {
        private DekautoContext context;

        public DisciplineGradesRepository(DekautoContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(DisciplineGrade disciplineGrade)
        {
            context.DisciplineGrades.Add(disciplineGrade);
            await context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                context.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<DisciplineGrade> range)
        {
            context.DisciplineGrades.RemoveRange(range);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DisciplineGrade>> GetAllAsync()
        {
            return await context.DisciplineGrades.ToListAsync();
        }

        public async Task<DisciplineGrade> GetByIdAsync(Guid id)
        {
            return await context.DisciplineGrades.FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<DisciplineGrade>> GetByStudentIdAsync(Guid studentId)
        {
            return await context.DisciplineGrades
                .Where(g => g.StudentId == studentId)
                .OrderBy(g => g.Semester)
                .ThenBy(g => g.Name)
                .ToListAsync();
        }

        public async Task UpdateAsync(DisciplineGrade updatedGrade)
        {
            var currentGrade = await context.DisciplineGrades.FirstOrDefaultAsync(g => g.Id == updatedGrade.Id);
            if (currentGrade == null) throw new KeyNotFoundException($"DisciplineGrade {updatedGrade.Id} not found");

            context.Entry(currentGrade).CurrentValues.SetValues(updatedGrade);
            await context.SaveChangesAsync();
        }
    }
}