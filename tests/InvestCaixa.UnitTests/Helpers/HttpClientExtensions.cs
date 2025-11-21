using System.Net.Http.Headers;

namespace InvestCaixa.UnitTests.Helpers;

public static class HttpClientExtensions
{
    /// <summary>
    /// Adiciona token JWT válido para testes que requerem autenticação.
    /// </summary>
    /// <param name="client">HttpClient do teste</param>
    /// <param name="userId">ID do usuário (padrão: 1)</param>
    /// <param name="userName">Nome do usuário (padrão: test@test.com)</param>
    /// <returns>O mesmo HttpClient com header Authorization configurado</returns>
    public static HttpClient WithTestAuth(this HttpClient client, int userId = 1, string userName = "test@test.com")
    {
        var token = JwtTestHelper.GenerateTestToken(userId, userName);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Remove cabeçalho de autenticação (para testes anônimos).
    /// </summary>
    /// <param name="client">HttpClient do teste</param>
    /// <returns>O mesmo HttpClient sem autenticação</returns>
    public static HttpClient WithoutAuth(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }
}