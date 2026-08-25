using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

public class AuthAndPasswordApiTests
{
    private readonly HttpClient _client;

    public AuthAndPasswordApiTests()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5269") }; // Adapter l'URL si nécessaire
    }

    [Fact]
    public async Task FullUserFlow_ShouldRegisterLoginAndFetchPasswords()
    {
        // Générer un email unique pour le test
        string testEmail = $"testuser{Guid.NewGuid()}@example.com";
        string password = "TestPassword123!";

        // 1️⃣ Inscription de l'utilisateur
        var registerData = new { email = testEmail, password };
        var registerResponse = await _client.PostAsync("/api/auth/register", new StringContent(
            JsonSerializer.Serialize(registerData), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        Console.WriteLine("✅ Inscription réussie");

        // 2️⃣ Connexion et récupération du JWT
        var loginData = new { email = testEmail, password };
        var loginResponse = await _client.PostAsync("/api/auth/login", new StringContent(
            JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<LoginResult>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrEmpty(loginResult.Token));
        Console.WriteLine("✅ Connexion réussie, JWT récupéré");

        // Ajouter le token dans les headers pour les requêtes suivantes
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // 3️⃣ Récupération des mots de passe
        var passwordsResponse = await _client.GetAsync("/api/password-entries");
        Assert.Equal(HttpStatusCode.OK, passwordsResponse.StatusCode);

        var passwordsContent = await passwordsResponse.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(passwordsContent));
        Console.WriteLine("✅ Récupération des mots de passe réussie");
    }

    private class LoginResult
    {
        public string Token { get; set; }
    }
}
