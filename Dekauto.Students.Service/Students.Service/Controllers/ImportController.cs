using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/import")]
    [ApiController]
    [Authorize(Policy = "OnlyAdmin")] // Требует аутентификации в роли "Администратор" для всех методов
    public class ImportController : ControllerBase
    {
        private readonly IImportProvider importProvider;
        private readonly ILogger<ImportController> logger;
        public ImportController(IImportProvider importProvider, ILogger<ImportController> logger)
        {
            this.importProvider = importProvider;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> ImportFilesFromFrontendAsync([FromForm] ImportFilesAdapter files)
        {
            try
            {
                if (files is null)
                    throw new ArgumentNullException(nameof(files));

                return Ok(await importProvider.ImportFilesAsync(files));
            }
            catch (ArgumentNullException ex)
            {
                var mes = "Некоторые (или все) файлы не передались на сервер. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status400BadRequest, mes);
            }
            catch (ImportServiceClientException ex)
            {
                if (ex.StatusCode == StatusCodes.Status400BadRequest && !string.IsNullOrWhiteSpace(ex.ResponseBody))
                {
                    return new ContentResult
                    {
                        Content = ex.ResponseBody,
                        ContentType = "application/json; charset=utf-8",
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                var mes = "Сервис импорта вернул ошибку. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, "{Mes} HTTP {Code}. Тело: {Body}", mes, ex.StatusCode, ex.ResponseBody);
                return StatusCode(
                    ex.StatusCode is >= 400 and < 500 ? ex.StatusCode : StatusCodes.Status502BadGateway,
                    ex.ResponseBody ?? mes);
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }
    }
}
