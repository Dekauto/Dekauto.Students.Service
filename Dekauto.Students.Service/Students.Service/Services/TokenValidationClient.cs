using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Dekauto.Students.Service.Students.Service.Services
{
    public interface ITokenValidationClient
    {
        Task<bool> IsJtiActiveAsync(string jti);
    }

    public class TokenValidationClient : ITokenValidationClient
    {
        private readonly HttpClient httpClient;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<TokenValidationClient> logger;

        public TokenValidationClient(HttpClient httpClient, IMemoryCache memoryCache, ILogger<TokenValidationClient> logger)
        {
            this.httpClient = httpClient;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        public async Task<bool> IsJtiActiveAsync(string jti)
        {
            if (string.IsNullOrEmpty(jti)) return false;

            // 1. Проверяем кэш. Ключ уникален для каждого JTI.
            string cacheKey = $"jti_status_{jti}";

            if (memoryCache.TryGetValue(cacheKey, out bool cachedIsActive))
            {
                return cachedIsActive;
            }

            // 2. Если в кэше нет, делаем запрос к Auth Service
            try
            {
                // Путь должен совпадать с тем, что в InternalTokenController
                var response = await httpClient.GetAsync($"internal/tokens/{jti}/status");

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning($"Auth service returned {response.StatusCode} for JTI check.");
                    return false; // Если сервис лежит или ошибка - считаем токен невалидным для безопасности
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TokenStatusResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                bool isActive = result?.IsActive ?? false;

                // 3. Сохраняем в кэш.
                // TTL (время жизни) = баланс между нагрузкой и безопасностью.
                // 1 минута означает, что после блокировки юзер сможет делать запросы еще максимум 1 минуту.
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

                memoryCache.Set(cacheKey, isActive, cacheOptions);

                return isActive;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error contacting Auth Service.");
                return false; // Fail closed (запрещаем доступ при ошибке)
            }
        }

        // Вспомогательный класс для десериализации ответа
        private class TokenStatusResponse
        {
            public bool IsActive { get; set; }
        }
    }
}
