using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Dekauto.Students.Service.Students.Service.Services
{
    public class DisciplineGradesService : IDisciplineGradesService
    {
        private readonly IDisciplineGradesRepository disciplineGradesRepository;
        private readonly DekautoContext context;

        public DisciplineGradesService(IDisciplineGradesRepository disciplineGradesRepository, DekautoContext context)
        {
            this.disciplineGradesRepository = disciplineGradesRepository;
            this.context = context;
        }

        private DEST JsonSerializationConvert<SRC, DEST>(SRC src)
        {
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }

        #region IDtoConverter Implementation

        // Реализация метода интерфейса IDtoConverter (async)
        public Task<DisciplineGrade> FromDtoAsync(DisciplineGradeDto dto)
        {
            return Task.FromResult(FromDto(dto));
        }

        // Синхронный хелпер для одиночного объекта
        private DisciplineGrade FromDto(DisciplineGradeDto dto)
        {
            if (dto == null) return null;

            // Базовый маппинг совпадающих полей (Semester, Year, ControlType, AudHours, CreditUnits, Score)
            var entity = JsonSerializationConvert<DisciplineGradeDto, DisciplineGrade>(dto);

            // РУЧНОЙ МАППИНГ отличающихся полей
            entity.Name = dto.DisciplineName;

            // При создании через DTO Id обычно пустой, но можно явно инициализировать
            // entity.Id = Guid.Empty; 

            return entity;
        }

        // Реализация метода интерфейса для коллекции (Entities -> DTOs)
        public IEnumerable<DisciplineGradeDto> ToDtos(IEnumerable<DisciplineGrade> entities)
        {
            if (entities == null) return new List<DisciplineGradeDto>();
            return entities.Select(ToDto).ToList();
        }

        // Реализация метода интерфейса (Entity -> DTO)
        public DisciplineGradeDto ToDto(DisciplineGrade entity)
        {
            if (entity == null) return null;

            var dto = JsonSerializationConvert<DisciplineGrade, DisciplineGradeDto>(entity);

            // РУЧНОЙ МАППИНГ отличающихся полей
            dto.DisciplineName = entity.Name;

            return dto;
        }

        // Метод для маппинга коллекции DTO -> Entities (используется при импорте)
        public IEnumerable<DisciplineGrade> FromDtos(IEnumerable<DisciplineGradeDto> dtos)
        {
            if (dtos == null) return new List<DisciplineGrade>();
            return dtos.Select(FromDto).ToList();
        }

        #endregion

        #region Service Logic

        public async Task AddAsync(DisciplineGradeDto gradeDto, Guid studentId)
        {
            if (gradeDto == null) throw new ArgumentNullException(nameof(gradeDto));

            var studentExists = await context.Students.AnyAsync(s => s.Id == studentId);
            if (!studentExists) throw new KeyNotFoundException($"Student with ID {studentId} not found");

            var entity = FromDto(gradeDto);
            entity.StudentId = studentId;

            await disciplineGradesRepository.AddAsync(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            await disciplineGradesRepository.DeleteByIdAsync(id);
        }

        public async Task<DisciplineGradeDto> GetByIdAsync(Guid id)
        {
            var entity = await disciplineGradesRepository.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException($"Discipline grade with ID {id} not found");

            return ToDto(entity);
        }

        public async Task<IEnumerable<DisciplineGradeDto>> GetByStudentIdAsync(Guid studentId)
        {
            var grades = await disciplineGradesRepository.GetByStudentIdAsync(studentId);
            return ToDtos(grades);
        }

        public async Task UpdateAsync(Guid id, DisciplineGradeDto gradeDto)
        {
            if (gradeDto == null) throw new ArgumentNullException(nameof(gradeDto));

            var currentEntity = await disciplineGradesRepository.GetByIdAsync(id);
            if (currentEntity == null) throw new KeyNotFoundException($"Discipline grade with ID {id} not found");

            var updatedInfo = FromDto(gradeDto);

            // Восстанавливаем ID и связь, так как DTO их не содержит
            updatedInfo.Id = currentEntity.Id;
            updatedInfo.StudentId = currentEntity.StudentId;

            await disciplineGradesRepository.UpdateAsync(updatedInfo);
        }

        #endregion
    }
}