using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetSystem.Data;
using BudgetSystem.Models;

namespace BudgetSystem.Controllers
{
    [Authorize]
    public class ReceiptController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ReceiptController> _logger;

        public ReceiptController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<ReceiptController> logger)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: Receipt/View/5
        public async Task<IActionResult> View(int transactionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == user.Id);

            if (transaction == null)
            {
                return NotFound("Transaction not found or access denied.");
            }

            if (string.IsNullOrEmpty(transaction.ReceiptPath))
            {
                return NotFound("No receipt attached to this transaction.");
            }

            try
            {
                // Construct the full file path with fallback for WebRootPath
                var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "receipts", user.Id.ToString());
                var filePath = Path.Combine(uploadsFolder, transaction.ReceiptPath);

                                 _logger.LogInformation("Receipt request: UserId={UserId}, TransactionId={TransactionId}, ReceiptPath={ReceiptPath}, WebRootPath={WebRootPath}, UploadsFolder={UploadsFolder}, FullPath={FullPath}", 
                    user.Id.ToString(), transactionId, transaction.ReceiptPath, _environment.WebRootPath, uploadsFolder, filePath);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Receipt file not found: {FilePath}", filePath);
                    return NotFound($"Receipt file not found: {filePath}");
                }

                // Get file info and content type
                var fileInfo = new FileInfo(filePath);
                var contentType = GetContentType(fileInfo.Extension);

                _logger.LogInformation("Serving receipt: {FileName}, ContentType: {ContentType}, Size: {Size} bytes", 
                    fileInfo.Name, contentType, fileInfo.Length);

                // Read and return the file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, contentType, fileInfo.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving receipt for transaction {TransactionId}", transactionId);
                return StatusCode(500, "Error retrieving receipt file.");
            }
        }

        // GET: Receipt/Download/5
        public async Task<IActionResult> Download(int transactionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.UserId == user.Id);

            if (transaction == null)
            {
                return NotFound("Transaction not found or access denied.");
            }

            if (string.IsNullOrEmpty(transaction.ReceiptPath))
            {
                return NotFound("No receipt attached to this transaction.");
            }

            try
            {
                // Construct the full file path with fallback for WebRootPath
                var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "receipts", user.Id.ToString());
                var filePath = Path.Combine(uploadsFolder, transaction.ReceiptPath);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Receipt file not found for download: {FilePath}", filePath);
                    return NotFound("Receipt file not found.");
                }

                // Get file info and create download name
                var fileInfo = new FileInfo(filePath);
                var downloadName = $"Receipt_{transaction.Description.Replace(" ", "_").Replace("/", "_")}_{transaction.TransactionDate:yyyy-MM-dd}{fileInfo.Extension}";
                var contentType = GetContentType(fileInfo.Extension);

                _logger.LogInformation("Downloading receipt: {OriginalName} as {DownloadName}", fileInfo.Name, downloadName);

                // Read and return the file as download
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, contentType, downloadName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading receipt for transaction {TransactionId}", transactionId);
                return StatusCode(500, "Error downloading receipt file.");
            }
        }

        // GET: Receipt/Test - Test endpoint to verify file access
        public async Task<IActionResult> Test()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { error = "User not authenticated" });
            }

            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "receipts", user.Id.ToString());
            
            // Get a sample transaction with receipt for testing
            var sampleTransaction = await _context.Transactions
                .Where(t => t.UserId == user.Id && !string.IsNullOrEmpty(t.ReceiptPath))
                .FirstOrDefaultAsync();
            
            var result = new
            {
                userId = user.Id.ToString(),
                webRootPath = _environment.WebRootPath,
                webRootPathUsed = webRootPath,
                uploadsFolder = uploadsFolder,
                folderExists = Directory.Exists(uploadsFolder),
                files = Directory.Exists(uploadsFolder) ? Directory.GetFiles(uploadsFolder).Select(f => Path.GetFileName(f)).ToArray() : new string[0],
                sampleTransaction = sampleTransaction != null ? new
                {
                    id = sampleTransaction.Id,
                    receiptPath = sampleTransaction.ReceiptPath,
                    fullFilePath = sampleTransaction.ReceiptPath != null ? Path.Combine(uploadsFolder, sampleTransaction.ReceiptPath) : "null",
                    fileExists = sampleTransaction.ReceiptPath != null && System.IO.File.Exists(Path.Combine(uploadsFolder, sampleTransaction.ReceiptPath))
                } : null,
                currentDirectory = Directory.GetCurrentDirectory(),
                pathSeparator = Path.DirectorySeparatorChar.ToString()
            };

            return Json(result);
        }

        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" or ".tif" => "image/tiff",
                ".webp" => "image/webp",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }
} 