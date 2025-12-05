using static System.Net.Mime.MediaTypeNames;
using System.Net.Http.Headers;
using System.Net.Http;

namespace DiyanetNamazVakti.Api.Service.Implementations;

public class AwqatSalahApiService : IAwqatSalahConnectService
{
    private readonly ICacheService _cacheService;
    private readonly HttpClient _client;
    private readonly IAwqatSalahSettings _awqatSalahSettings;
    private readonly int _tokenLifeTimeMinutes;

    public AwqatSalahApiService(ICacheService cacheService, IHttpClientFactory clientFactory, IAwqatSalahSettings awqatSalahSettings)
    {
        _awqatSalahSettings = awqatSalahSettings;
        _cacheService = cacheService;
        _tokenLifeTimeMinutes = awqatSalahSettings.TokenLifetimeMinutes + awqatSalahSettings.RefreshTokenLifetimeMinutes;
        _client = clientFactory.CreateClient("AwqatSalahApi");
    }

    public async Task<T> CallService<T>(string path, MethodOption method, object model, CancellationToken cancellationToken) where T : class, new()
    {
        if (string.IsNullOrEmpty(path))
            ArgumentNullException.ThrowIfNull(path);

        var request = string.Format(path);

        await AddToken(cancellationToken);

        // Her istek için yeni bir HttpRequestMessage oluştur ve token'ı ekle
        var baseAddress = _client.BaseAddress ?? new Uri("https://awqatsalah.diyanet.gov.tr/");
        var requestMessage = new HttpRequestMessage();
        if (method == MethodOption.Get)
        {
            requestMessage.Method = HttpMethod.Get;
            requestMessage.RequestUri = new Uri(baseAddress, request);
        }
        else
        {
            requestMessage.Method = HttpMethod.Post;
            requestMessage.RequestUri = new Uri(baseAddress, request);
            var postModel = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, Application.Json);
            requestMessage.Content = postModel;
        }
        
        // Token'ı request message'a ekle
        var token = await _cacheService.GetOrCreateAsync(nameof(CacheNameConstants.TokenCacheName), async () => await AwqatSalahLogin(cancellationToken), DateTime.Now.AddMinutes(_tokenLifeTimeMinutes));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        HttpResponseMessage result = await _client.SendAsync(requestMessage, cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<RestDataResult<T>>(JsonConstants.SerializerOptions, cancellationToken);
            return response!.Data;
        }
        else
        {
            // Hata durumunda response body'yi logla
            var errorContent = await result.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"[AwqatSalahApiService] Error {result.StatusCode}: {errorContent}");
        }
        return null;
    }

    private async Task AddToken(CancellationToken cancellationToken)
    {
        var token = await _cacheService.GetOrCreateAsync(nameof(CacheNameConstants.TokenCacheName), async () => await AwqatSalahLogin(cancellationToken), DateTime.Now.AddMinutes(_tokenLifeTimeMinutes));

        if (!_client.DefaultRequestHeaders.Contains("Authorization"))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }

        if (token.ExpireTime <= DateTime.Now)
        {
            if ((DateTime.Now - token.ExpireTime).TotalMinutes < _awqatSalahSettings.RefreshTokenLifetimeMinutes)
            {
                _cacheService.Remove(CacheNameConstants.TokenCacheName);
                token = await _cacheService.GetOrCreateAsync(CacheNameConstants.TokenCacheName,
                    async () => await AwqatSalahRefreshToken(token.RefreshToken, cancellationToken), DateTime.Now.AddMinutes(_tokenLifeTimeMinutes));

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            }
            else
            {
                _cacheService.Remove(CacheNameConstants.TokenCacheName);
                await AddToken(cancellationToken);
            }
        }
    }

    private async Task<TokenWithExpireModel> AwqatSalahLogin(CancellationToken cancellationToken)
    {
        var loginCredential = new StringContent(JsonSerializer.Serialize(new LoginModel() { Password = _awqatSalahSettings.Password, Email = _awqatSalahSettings.UserName }), Encoding.UTF8, Application.Json);

        var resultAuth = await _client.PostAsync("Auth/Login", loginCredential, cancellationToken: cancellationToken);

        if (!resultAuth.IsSuccessStatusCode)
            throw new HttpRequestException(resultAuth.StatusCode.ToString());

        using var stream = await resultAuth.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var token = jsonDoc.RootElement.GetProperty("data").Deserialize<TokenWithExpireModel>(JsonConstants.SerializerOptions)!;
        token.ExpireTime = DateTime.Now.AddMinutes(_awqatSalahSettings.TokenLifetimeMinutes);
        return token;
    }

    private async Task<TokenWithExpireModel> AwqatSalahRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        var resultAuth = await _client.GetAsync($"Auth/RefreshToken/{refreshToken}", cancellationToken: cancellationToken);
        if (!resultAuth.IsSuccessStatusCode)
            throw new HttpRequestException(resultAuth.StatusCode.ToString());

        using var stream = await resultAuth.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var token = jsonDoc.RootElement.GetProperty("data").Deserialize<TokenWithExpireModel>(JsonConstants.SerializerOptions)!;
        token.ExpireTime = DateTime.Now.AddMinutes(_awqatSalahSettings.TokenLifetimeMinutes);
        return token;
    }

    private class RestDataResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}