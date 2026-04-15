namespace Dekauto.Students.Service.Students.Service.Domain.Entities.DTO
{
    public class DisciplineGradeDto
    {
        public string? DisciplineName { get; set; }
        public string? Score { get; set; }
        public short? Semester { get; set; }
        public short? Year { get; set; }
        public string? ControlType { get; set; }

        public double? AudHours { get; set; }
        public double? CreditUnits { get; set; }

        public int? PlanOrder { get; set; }

        /// <summary>Блок учебного плана (соответствует SupplementPlanBucket в экспорте).</summary>
        public int? PlanBucket { get; set; }

        public bool RequiresManualValidation { get; set; }

        public bool IsCardOnlyUnmatchedPlan { get; set; }
    }
}
