namespace Dekauto.Students.Service.Students.Service.Infrastructure;

public interface ITeachersCatalogClient
{
    Task NotifyStudentDeletedAsync(Guid studentId, CancellationToken ct = default);
}

public class TeachersCatalogClient : ITeachersCatalogClient
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<TeachersCatalogClient> logger;

    public TeachersCatalogClient(IHttpClientFactory httpClientFactory, ILogger<TeachersCatalogClient> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public async Task NotifyStudentDeletedAsync(Guid studentId, CancellationToken ct = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient("TeachersService");
            var response = await client.DeleteAsync($"internal/students/{studentId}", ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Teachers: не удалось удалить студента {StudentId}, статус {StatusCode}",
                    studentId,
                    (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Teachers: ошибка при удалении студента {StudentId} из каталога преподавателя", studentId);
        }
    }
}
