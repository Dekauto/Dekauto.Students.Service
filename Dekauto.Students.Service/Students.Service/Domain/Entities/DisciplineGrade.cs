using System.Text.Json.Serialization;

namespace Dekauto.Students.Service.Students.Service.Domain.Entities;

public partial class DisciplineGrade
{
    public Guid Id { get; set; }

    public Guid? StudentId { get; set; }

    public string? Name { get; set; }

    public string? Score { get; set; }

    /// <summary>
    /// Номер семестра
    /// </summary>
    public short? Semester { get; set; }

    /// <summary>
    /// Год для семестра
    /// </summary>
    public short? Year { get; set; }

    public string? ControlType { get; set; }

    public double? AudHours { get; set; }

    public double? TotalHours { get; set; }

    public double? CreditUnits { get; set; }

    [JsonIgnore]
    public virtual Student? Student { get; set; }
}
