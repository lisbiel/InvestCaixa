namespace InvestCaixa.UnitTests.IntegrationTests;

using FluentAssertions;
using InvestCaixa.Application.DTOs.Request;
using InvestCaixa.Application.DTOs.Response;
using InvestCaixa.UnitTests.Fixtures;
using InvestCaixa.UnitTests.Helpers;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

[Collection("Integration Tests")]
public class AuthControllerIntegrationTests
{
    private readonly IntegrationTestFixture _fixture;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthControllerIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var loginRequest = TestDataBuilder.CriarLoginRequest("admin", "Admin@123");
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        // Remove authorization header para login
        var client = _fixture.Client;
        var originalHeaders = client.DefaultRequestHeaders.Authorization;
        client.DefaultRequestHeaders.Remove("Authorization");

        try
        {
            // Act
            var response = await client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.ToString().Should().Contain("application/json");

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginResponse>(responseBody, _jsonOptions);

            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        }
        finally
        {
            // Restaurar authorization header
            if (originalHeaders != null)
                client.DefaultRequestHeaders.Authorization = originalHeaders;
        }
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornar401()
    {
        // Arrange
        var loginRequest = new LoginRequest { Usuario = "admin", Senha = "WrongPassword" };
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var client = _fixture.Client;
        client.DefaultRequestHeaders.Remove("Authorization");

        try
        {
            // Act
            var response = await client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            client.DefaultRequestHeaders.Add("Authorization", 
                "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsIm5hbWUiOiJhZG1pbiIsImF1ZCI6IkludmVzdG1lbnRTaW11bGF0aW9uLkNsaWVudCIsImlzcyI6IkludmVzdG1lbnRTaW11bGF0aW9uLkFQSSIsImV4cCI6OTk5OTk5OTk5OX0.X3cJB6Yd9Kt_2d8nQ5pZ9K0jR7wM8vL2nO1aP3bQ5cE");
        }
    }

    [Fact]
    public async Task RefreshToken_ComTokenValido_DeveRetornarNovoToken()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest { RefreshToken = "test-refresh-token" };
        var content = new StringContent(
            JsonSerializer.Serialize(refreshRequest),
            Encoding.UTF8,
            "application/json");

        var client = _fixture.Client;
        client.DefaultRequestHeaders.Remove("Authorization");

        try
        {
            // Act
            var response = await client.PostAsync("/api/auth/refresh", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginResponse>(responseBody, _jsonOptions);

            result.Should().NotBeNull();
            result!.Token.Should().NotBeNullOrEmpty();
        }
        finally
        {
            client.DefaultRequestHeaders.Add("Authorization", 
                "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsIm5hbWUiOiJhZG1pbiIsImF1ZCI6IkludmVzdG1lbnRTaW11bGF0aW9uLkNsaWVudCIsImlzcyI6IkludmVzdG1lbnRTaW11bGF0aW9uLkFQSSIsImV4cCI6OTk5OTk5OTk5OX0.X3cJB6Yd9Kt_2d8nQ5pZ9K0jR7wM8vL2nO1aP3bQ5cE");
        }
    }

    [Fact]
    public async Task Login_SemCredenciais_DeveRetornarErroDeValidacao()
    {
        // Arrange
        var loginRequest = new LoginRequest { Usuario = "", Senha = "" };
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        var client = _fixture.Client;
        client.DefaultRequestHeaders.Remove("Authorization");

        try
        {
            // Act
            var response = await client.PostAsync("/api/auth/login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            client.DefaultRequestHeaders.Add("Authorization", 
                "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsIm5hbWUiOiJhZG1pbiIsImF1ZCI6IkludmVzdG1lbnRTaW11bGF0aW9uLkNsaWVudCIsImlzcyI6IkludmVzdG1lbnRTaW11bGF0aW9uLkFQSSIsImV4cCI6OTk5OTk5OTk5OX0.X3cJB6Yd9Kt_2d8nQ5pZ9K0jR7wM8vL2nO1aP3bQ5cE");
        }
    }
}
