using Dekauto.Students.Service.Students.Service.Controllers;
using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class ImportProvider : IImportProvider
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IStudentsService studentsService;
        private readonly ILogger<ExportController> logger;
        private readonly IGroupsService groupsService;

        public ImportProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory,
            IStudentsService studentsService, ILogger<ExportController> logger,
            IGroupsService groupsService)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.studentsService = studentsService;
            this.logger = logger;
            this.groupsService = groupsService;
        }

        private async Task<ImportStudentsResponseDto> SendNewStudentsImportAsync(ImportFilesAdapter files)
        {
            var http = httpClientFactory.CreateClient("ImportService");
            var content = new MultipartFormDataContent();

            if (files.ld != null)
            {
                var fileContent = new StreamContent(files.ld.OpenReadStream());
                content.Add(fileContent, "ld", files.ld.FileName);
            }

            if (files.contract != null)
            {
                var fileContent = new StreamContent(files.contract.OpenReadStream());
                content.Add(fileContent, "contract", files.contract.FileName);
            }

            if (files.journal != null)
            {
                var fileContent = new StreamContent(files.journal.OpenReadStream());
                content.Add(fileContent, "journal", files.journal.FileName);
            }

            if (files.statement != null)
            {
                var fileContent = new StreamContent(files.statement.OpenReadStream());
                content.Add(fileContent, "statement", files.statement.FileName);
            }

            if (files.plan != null)
            {
                var fileContent = new StreamContent(files.plan.OpenReadStream());
                content.Add(fileContent, "plan", files.plan.FileName);
            }

            var endpoint = configuration["Services:Import:import_students"];
            var response = await http.PostAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new ImportServiceClientException((int)response.StatusCode, body);
            }

            return await response.Content.ReadFromJsonAsync<ImportStudentsResponseDto>()
                   ?? new ImportStudentsResponseDto();
        }

        private async Task<DiplomaSupplementData> SendStudentCardImportAsync(ImportFilesAdapter files)
        {
            if (files.studentCard == null) throw new ArgumentNullException(nameof(files.studentCard));
            if (files.plan == null) throw new ArgumentNullException(nameof(files.plan));

            var http = httpClientFactory.CreateClient("ImportService");
            var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(files.studentCard.OpenReadStream());
            content.Add(fileContent, "studentCard", files.studentCard.FileName);
            var planContent = new StreamContent(files.plan.OpenReadStream());
            content.Add(planContent, "plan", files.plan.FileName);

            var endpoint = configuration["Services:Import:import_student_card"];
            var response = await http.PostAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new ImportServiceClientException((int)response.StatusCode, body);
            }

            return await response.Content.ReadFromJsonAsync<DiplomaSupplementData>();
        }

        public async Task<ImportFilesResult?> ImportFilesAsync(ImportFilesAdapter files)
        {
            logger.LogInformation("Начата передача импорта файлов...");
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (files.studentCard == null)
            {
                if (files.ld == null) throw new ArgumentNullException(nameof(files.ld));
                if (files.contract == null) throw new ArgumentNullException(nameof(files.contract));
                if (files.journal == null) throw new ArgumentNullException(nameof(files.journal));
                if (files.plan == null) throw new ArgumentNullException(nameof(files.plan));
                if (files.statement == null) throw new ArgumentNullException(nameof(files.statement));

                var warnings = await ProcessStudentImport(files);
                return new ImportFilesResult { ImportWarnings = warnings };
            }

            if (files.plan == null)
                throw new ArgumentNullException(nameof(files.plan));

            var data = await ProcessCardImport(files);
            return new ImportFilesResult { Data = data };
        }

        private async Task<DiplomaSupplementData> ProcessCardImport(ImportFilesAdapter files)
        {
            // Отправка запроса в сервис "Импорт" и получение готового массива
            logger.LogInformation("Отправка запроса в сервис \"Импорт\" и получение готовых данных для приложения диплома...");
            DiplomaSupplementData data = await SendStudentCardImportAsync(files);
            logger.LogInformation("Получен массив с готовыми данными.");

            // Конвертация и добавление в БД
            logger.LogInformation("Получен ответ с готовыми данными. Отправляем клиенту...");
            return data;
        }

        private async Task<IReadOnlyList<ImportWarningDto>> ProcessStudentImport(ImportFilesAdapter files)
        {
            // Отправка запроса в сервис "Импорт" и получение готового массива
            logger.LogInformation("Отправка запроса в сервис \"Импорт\" и получение готового массива...");
            var importResponse = await SendNewStudentsImportAsync(files);
            logger.LogInformation("Получен массив с готовыми объектами.");

            // Конвертация и добавление в БД
            logger.LogInformation("Получен ответ с готовыми объектами. Начата конвертация и добавление в БД.");
            var newStudents = importResponse.Students;
            var groupNames = GetAllUniqueGroupNames(newStudents);
            var existingStudentsInGroups = await groupsService.GetAllStudentsForGroupsAsync(groupNames);
            await studentsService.ImportStudentsAsync(newStudents, existingStudentsInGroups);

            logger.LogInformation("Все объекты были инпортированы в базу данных. Импорт завершен.");
            return EnrichImportWarnings(importResponse.ImportWarnings, files.statement?.FileName);
        }

        private static IReadOnlyList<ImportWarningDto> EnrichImportWarnings(
            IReadOnlyList<ImportWarningDto> warnings,
            string? statementFileName)
        {
            if (string.IsNullOrWhiteSpace(statementFileName))
                return warnings;

            return warnings
                .Select(w => string.IsNullOrWhiteSpace(w.FileName)
                    ? new ImportWarningDto
                    {
                        Code = w.Code,
                        Message = w.Message,
                        FileName = statementFileName,
                        SheetName = w.SheetName,
                        GroupName = w.GroupName,
                        StudentDisplayName = w.StudentDisplayName,
                        MatchedRows = w.MatchedRows
                    }
                    : w)
                .ToList();
        }

        private IEnumerable<string> GetAllUniqueGroupNames(IEnumerable<StudentExportDto> students)
        {
            if (students is null)
            {
                throw new ArgumentNullException(nameof(students));
            }

            var groupNames = students
                .Where(s => !s.GroupName.IsNullOrEmpty())
                .Select(s => s.GroupName)
                .Distinct();

            return groupNames;
        }
    }
}
