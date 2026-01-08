namespace Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters
{
    public class DiplomaSupplementData
    {
        public bool? diplomaWithHonors { get; set; } // с отличием или нет
        public string? surname { get; set; } // фамилия
        public string? Name { get; set; } // имя
        public string? Patronymic { get; set; } // отчество
        public bool? Gender { get; set; } // пол
        public DateOnly? BirthdayDate { get; set; } // дата рождения
    }
}
