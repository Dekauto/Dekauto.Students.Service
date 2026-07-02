using System.Text.RegularExpressions;

namespace Dekauto.Students.Service.Students.Service.Services
{
    /// <summary>
    /// Транспорт флага RequiresManualValidation через поле Name в БД
    /// (отдельной колонки в DisciplineGrade пока нет).
    /// </summary>
    internal static class DisciplineGradeReviewEncoding
    {
        private const string ReviewMarkerTail = " (ТРЕБУЕТ ПРОВЕРКИ)";
        private static readonly Regex ReviewMarkerRegex =
            new(@"\s*\(\s*ТРЕБУЕТ\s+ПРОВЕРКИ\s*\)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static string? EncodeName(string? disciplineName, bool requiresManualValidation)
        {
            if (!requiresManualValidation)
                return disciplineName;

            if (string.IsNullOrWhiteSpace(disciplineName))
                return "(ТРЕБУЕТ ПРОВЕРКИ)";

            if (disciplineName.Contains("ТРЕБУЕТ ПРОВЕРКИ", StringComparison.OrdinalIgnoreCase))
                return disciplineName;

            return disciplineName + ReviewMarkerTail;
        }

        public static (string? disciplineName, bool requiresManualValidation) DecodeName(string? storedName)
        {
            if (string.IsNullOrWhiteSpace(storedName))
                return (storedName, false);

            if (!storedName.Contains("ТРЕБУЕТ ПРОВЕРКИ", StringComparison.OrdinalIgnoreCase))
                return (storedName, false);

            var stripped = ReviewMarkerRegex.Replace(storedName, string.Empty).Trim();
            return (string.IsNullOrWhiteSpace(stripped) ? null : stripped, true);
        }
    }
}
