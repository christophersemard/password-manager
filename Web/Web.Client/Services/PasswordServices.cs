using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Blazored.LocalStorage;
using Core.Models;
using Web.Client.Services;

namespace Web.Client.Services
{
    public class PasswordService 
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthService _authService;

        public PasswordService(HttpClient httpClient, ILocalStorageService localStorage, AuthService authService)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authService = authService;
        }
        
        // Récupère la liste des PasswordEntry pour l'utilisateur connecté
        public async Task<List<PasswordEntry>> GetPasswordsAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var result = await _httpClient.GetFromJsonAsync<List<PasswordEntry>>("api/password-entries");
            if (result == null) return new List<PasswordEntry>();

            // 🔑 Récupérer la clé AES dérivée
            byte[] key = _authService.GetEncryptionKey();

            // 🔓 Déchiffrer chaque mot de passe
            foreach (var entry in result)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(entry.EncryptedPassword))
                    {
                        Console.WriteLine($"❌ Erreur : EncryptedPassword est vide pour {entry.Title}");
                        continue;
                    }

                    // Vérifier que Base64 est valide
                    try
                    {
                        byte[] encryptedData = Convert.FromBase64String(entry.EncryptedPassword);

                        // 🔓 Déchiffrement
                        entry.DecryptedPassword = EncryptionService.Decrypt(encryptedData, key);
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine($"❌ Erreur de décodage Base64 pour {entry.Title} : {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur inattendue lors du déchiffrement de {entry.Title} : {ex.Message}");
                }
            }

            return result;
        }


        
        // Supprime une entrée de mot de passe
        public async Task DeletePasswordAsync(int id)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            await _httpClient.DeleteAsync($"api/password-entries/{id}");
        }
        
        // Crée une nouvelle entrée
        public async Task CreatePasswordAsync(PasswordEntry entry)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 🔑 Récupérer la clé AES
            byte[] key = _authService.GetEncryptionKey();
            if (key == null || key.Length == 0)
            {
                Console.WriteLine("❌ Erreur : Clé AES non disponible !");
                return;
            }
            // 🔒 Chiffrer le mot de passe
            if (!string.IsNullOrWhiteSpace(entry.DecryptedPassword))
            {
                entry.EncryptedPassword = EncryptionService.Encrypt(entry.DecryptedPassword, key);
            }
            else
            {
                Console.WriteLine("❌ Erreur : `DecryptedPassword` est vide !");
                return;
            }

            entry.DecryptedPassword = null; // 🚨 Sécurisation - Suppression du mot de passe en clair avant l’envoi

            // 📡 Envoyer à l’API
            var response = await _httpClient.PostAsJsonAsync("api/password-entries", entry);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Nouvelle entrée de mot de passe créée avec succès !");
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Erreur lors de la création : {errorMessage}");
            }
        }


        
        // Met à jour une entrée existante
        public async Task UpdatePasswordAsync(PasswordEntry entry)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // 🔑 Récupérer la clé AES
            byte[] key = _authService.GetEncryptionKey();
            if (key == null || key.Length == 0)
            {
                Console.WriteLine("❌ Erreur : Clé AES non disponible !");
                return;
            }
            // 🔒 Chiffrer le mot de passe avant d’envoyer
            if (!string.IsNullOrWhiteSpace(entry.DecryptedPassword))
            {
                entry.EncryptedPassword = EncryptionService.Encrypt(entry.DecryptedPassword, key);
            }
            else
            {
                Console.WriteLine("❌ Erreur : `DecryptedPassword` est vide !");
                return;
            }

            entry.DecryptedPassword = null; // 🚨 Sécurisation - Suppression du mot de passe en clair

            // 📡 Envoyer la mise à jour à l’API
            var response = await _httpClient.PutAsJsonAsync($"api/password-entries/{entry.Id}", entry);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Entrée mise à jour avec succès !");
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Erreur lors de la mise à jour : {errorMessage}");
            }
        }


    }
}
