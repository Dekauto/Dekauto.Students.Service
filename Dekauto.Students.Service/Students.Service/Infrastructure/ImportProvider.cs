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

        private async Task<IEnumerable<StudentExportDto>> SendNewStudentsImportAsync(ImportFilesAdapter files)
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
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<StudentExportDto>>();
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
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DiplomaSupplementData>();
        }

        public async Task<DiplomaSupplementData?> ImportFilesAsync(ImportFilesAdapter files)
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

                await ProcessStudentImport(files);
                return null;
            }
            else
            {
                if (files.plan == null)
                    throw new ArgumentNullException(nameof(files.plan));
                return await ProcessCardImport(files);
            }
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

        private async Task ProcessStudentImport(ImportFilesAdapter files)
        {
            // Отправка запроса в сервис "Импорт" и получение готового массива
            logger.LogInformation("Отправка запроса в сервис \"Импорт\" и получение готового массива...");
            var newStudents = await SendNewStudentsImportAsync(files);
            logger.LogInformation("Получен массив с готовыми объектами.");

            // Конвертация и добавление в БД
            logger.LogInformation("Получен ответ с готовыми объектами. Начата конвертация и добавление в БД.");
            var groupNames = GetAllUniqueGroupNames(newStudents);
            var existingStudentsInGroups = await groupsService.GetAllStudentsForGroupsAsync(groupNames);
            await studentsService.ImportStudentsAsync(newStudents, existingStudentsInGroups);

            logger.LogInformation("Все объекты были инпортированы в базу данных. Импорт завершен.");

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
