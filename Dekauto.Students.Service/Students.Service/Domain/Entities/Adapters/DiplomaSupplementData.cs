using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;

namespace Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters
{
    public class DiplomaSupplementData
    {
        public bool? DiplomaWithHonors { get; set; } // с отличием
        public string? Name { get; set; } // Имя
        public string? Surname { get; set; } // Фамилия
        public string? Patronymic { get; set; } // Отчество
        public DateOnly? BirthdayDate { get; set; } // Дата рождения
        public string? EducationReceived { get; set; } // Наименование документа о предыдущем образовании
        public DateOnly? EducationReceivedDate { get; set; } // Год выдачи документа образования

        // Наименования дисциплин (модулей), практик, курсовых работ
        // +Количество зачетных единиц / академических часов / астрономических часов
        // +Оценка
        public List<DisciplineGradeDto> DisciplineResults { get; set; } = new();
    }
}
