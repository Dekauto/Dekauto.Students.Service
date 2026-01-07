using Dekauto.Students.Service.Students.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Serilog.Context;
using System.Net;
using System.Security.Claims;

namespace Dekauto.Students.Service.Students.Service.Middlewares
{
    public class RemoteTokenRevocationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RemoteTokenRevocationMiddleware> logger;

        public RemoteTokenRevocationMiddleware(RequestDelegate next, ILogger<RemoteTokenRevocationMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITokenValidationClient validationClient)
        {
            var endpoint = context.GetEndpoint();

            // 1. Логируем пропуск проверки для анонимных эндпоинтов
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                logger.LogDebug("Token check skipped: endpoint {Path} allows anonymous access.", context.Request.Path);
                await next(context);
                return;
            }

            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Используем LogContext, чтобы Jti и UserId добавились ко ВСЕМ логам внутри этого блока
                using (LogContext.PushProperty("Jti", jti))
                using (LogContext.PushProperty("UserId", userId))
                {
                    if (!string.IsNullOrEmpty(jti))
                    {
                        logger.LogInformation("Validating token {Jti} for user {UserId} via Auth Service.", jti, userId);

                        bool isActive = await validationClient.IsJtiActiveAsync(jti);

                        if (!isActive)
                        {
                            logger.LogWarning("Access denied: Token {Jti} has been revoked for user {UserId}.", jti, userId);

                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            await context.Response.WriteAsync("Token is revoked.");
                            return;
                        }

                        logger.LogDebug("Token {Jti} is active. Proceeding to {Path}.", jti, context.Request.Path);
                    }
                    else
                    {
                        logger.LogWarning("User is authenticated but JTI claim is missing in the token.");
                    }
                }
            }

            await next(context);
        }
    }

}