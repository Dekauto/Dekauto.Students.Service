namespace Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters
{
    public class DiplomaSupplementRequest
    {
        public DiplomaSupplementData data { get; set; } // Готовые данные для передачи в экспорт
        public string manufacturer { get; set; } // Производитель/формат шаблона
        public string educationLevel { get; set; } // Уровень обучения
    }
}
