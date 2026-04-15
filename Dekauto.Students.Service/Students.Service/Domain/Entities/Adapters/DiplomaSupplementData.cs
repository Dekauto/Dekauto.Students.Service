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

        /// <summary>Направление подготовки (специальность) из карточки студента.</summary>
        public string? CourseOfTraining { get; set; }

        /// <summary>Целевой объём ОП в з.е. из листа "Свод" (например L8).</summary>
        public double? TargetProgramCredits { get; set; }

        /// <summary>Сумма конт. раб. по блокам 1–3 «ПланСвод» для листа «3 Освоение программы».</summary>
        public double? TargetContactHoursFromPlan { get; set; }

        /// <summary>Итого з.е. блока 3 (ГИА) из «ПланСвод».</summary>
        public double? TargetGiaCreditsFromPlan { get; set; }

        /// <summary>Итого з.е. блока 2 «Практика» из «ПланСвод» (колонка «Факт»).</summary>
        public double? TargetPracticeCreditsFromPlan { get; set; }

        /// <summary>Лист «4 доп.сведения», ячейка B6 — наименование ОПОП из карточки.</summary>
        public string? SupplementAdditionalSheetOpopName { get; set; }

        /// <summary>Лист «4 доп.сведения», ячейка B7 — строка «Форма обучения: …».</summary>
        public string? SupplementAdditionalSheetStudyFormLine { get; set; }

        // Наименования дисциплин (модулей), практик, курсовых работ
        // +Количество зачетных единиц / академических часов / астрономических часов
        // +Оценка
        public List<DisciplineGradeDto> DisciplineResults { get; set; } = new();
    }
}