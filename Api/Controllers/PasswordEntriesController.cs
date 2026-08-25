using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Core.Models;
using Core.Dtos;
using Api.Data;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/password-entries")]
    public class PasswordEntriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PasswordEntriesController> _logger;

        public PasswordEntriesController(ApplicationDbContext context, ILogger<PasswordEntriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Récupérer toutes les entrées de l'utilisateur connecté
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PasswordEntry>>> GetPasswordEntries()
        {
            
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var passwordEntries = await _context.PasswordEntries
                .Where(pe => pe.UserId == userId)
                .Include(pe=> pe.User).Include(pe=> pe.Category).ToListAsync();
            return Ok(passwordEntries);
        }

        // Ajouter une nouvelle entrée
        [HttpPost]
        public async Task<ActionResult<PasswordEntry>> CreatePasswordEntry(CreatePasswordEntryDto passwordEntryDto)
        {
            if (string.IsNullOrEmpty(passwordEntryDto.EncryptedPassword))
            {
                return BadRequest("Le mot de passe chiffré est requis.");
            }
            
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var passwordEntry = new PasswordEntry
            {
                UserId = userId,
                Title = passwordEntryDto.Title,
                Username = passwordEntryDto.Username,
                EncryptedPassword = passwordEntryDto.EncryptedPassword,
                Url = passwordEntryDto.Url,
                CategoryId = passwordEntryDto.CategoryId,
                Notes = passwordEntryDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Création d'une entrée de mot de passe pour l'utilisateur {UserId}", userId);

            _context.PasswordEntries.Add(passwordEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Entrée {PasswordEntryId} ajoutée pour l'utilisateur {UserId}", passwordEntry.Id, userId);

            return CreatedAtAction(nameof(GetPasswordEntry), new { id = passwordEntry.Id }, passwordEntry);
        }

        // Récupérer une entrée spécifique par ID
        [HttpGet("{id}")]
        public async Task<ActionResult<PasswordEntry>> GetPasswordEntry(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
           
            var passwordEntry = await _context.PasswordEntries
                .Where(pe => pe.Id == id && pe.UserId == userId)
                .FirstOrDefaultAsync();

            if (passwordEntry == null)
            {
                return NotFound("Entrée non trouvée ou non accessible.");
            }

            return passwordEntry;
        }

        // Mettre à jour une entrée existante
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePasswordEntry(int id, UpdatePasswordEntryDto passwordEntryDto)
        {
   
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var passwordEntry = await _context.PasswordEntries
                .Where(pe => pe.Id == id && pe.UserId == userId)
                .FirstOrDefaultAsync();

            if (passwordEntry == null)
            {
                return NotFound("Entrée non trouvée ou non accessible.");
            }

            passwordEntry.Title = passwordEntryDto.Title ?? passwordEntry.Title;
            passwordEntry.Username = passwordEntryDto.Username ?? passwordEntry.Username;
            passwordEntry.EncryptedPassword = passwordEntryDto.EncryptedPassword ?? passwordEntry.EncryptedPassword;
            passwordEntry.Url = passwordEntryDto.Url ?? passwordEntry.Url;
            passwordEntry.CategoryId = passwordEntryDto.CategoryId ?? passwordEntry.CategoryId;
            passwordEntry.Notes = passwordEntryDto.Notes ?? passwordEntry.Notes;
            passwordEntry.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Mise à jour de l'entrée {PasswordEntryId} pour l'utilisateur {UserId}", id, userId);

            _context.Entry(passwordEntry).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Supprimer une entrée
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePasswordEntry(int id)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var passwordEntry = await _context.PasswordEntries
                .Where(pe => pe.Id == id && pe.UserId == userId)
                .FirstOrDefaultAsync();

            if (passwordEntry == null)
            {
                return NotFound("Entrée non trouvée ou non accessible.");
            }

            _logger.LogInformation("Suppression de l'entrée {PasswordEntryId} pour l'utilisateur {UserId}", id, userId);

            _context.PasswordEntries.Remove(passwordEntry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
