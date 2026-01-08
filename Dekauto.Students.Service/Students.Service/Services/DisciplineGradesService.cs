using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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

        /// <summary>
        /// Конвертирование из объекта src типа SRC в объект типа DEST через сериализацию и десереализацию в JSON-объект.
        /// </summary>
        private DEST JsonSerializationConvert<SRC, DEST>(SRC src)
        {
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }

        public async Task AddAsync(DisciplineGradeDto gradeDto, Guid studentId)
        {
            if (gradeDto == null) throw new ArgumentNullException(nameof(gradeDto));

            // Проверка существования студента
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

            // Обновляем поля через маппинг
            var updatedInfo = FromDto(gradeDto);

            // Сохраняем ID и StudentId от существующей записи, так как их нет в DTO
            updatedInfo.Id = currentEntity.Id;
            updatedInfo.StudentId = currentEntity.StudentId;

            await disciplineGradesRepository.UpdateAsync(updatedInfo);
        }

        private DisciplineGradeDto ToDto(DisciplineGrade entity)
        {
            if (entity == null) return null;

            // Используем автоматический маппинг для совпадающих полей (Semester, Year, ControlType, AudHours, CreditUnits)
            var dto = JsonSerializationConvert<DisciplineGrade, DisciplineGradeDto>(entity);

            // Ручной маппинг для отличающихся имен и типов
            dto.DisciplineName = entity.Name;

            // Конвертация Score (String -> Double?)
            // В БД Score - это varchar, в DTO - double?
            if (double.TryParse(entity.Score, NumberStyles.Any, CultureInfo.InvariantCulture, out double scoreVal))
            {
                dto.Score = scoreVal;
            }
            else
            {
                dto.Score = null; // Если там "зачет" или текст, возвращаем null
            }

            return dto;
        }

        private DisciplineGrade FromDto(DisciplineGradeDto dto)
        {
            if (dto == null) return null;

            // Используем автоматический маппинг для совпадающих полей
            var entity = JsonSerializationConvert<DisciplineGradeDto, DisciplineGrade>(dto);

            // Ручной маппинг для отличающихся имен и типов
            entity.Name = dto.DisciplineName;

            // Конвертация Score (Double? -> String)
            if (dto.Score.HasValue)
            {
                entity.Score = dto.Score.Value.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                // Значение по умолчанию или null, если оценка не проставлена
                entity.Score = "Оценка (от 0 до 15, или зачет)";
            }

            return entity;
        }

        private IEnumerable<DisciplineGradeDto> ToDtos(IEnumerable<DisciplineGrade> entities)
        {
            if (entities == null) return new List<DisciplineGradeDto>();

            var dtos = new List<DisciplineGradeDto>();
            foreach (var entity in entities)
            {
                dtos.Add(ToDto(entity));
            }
            return dtos;
        }

    }

}
