using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BudgetSystem.Data;
using BudgetSystem.Models;
using BudgetSystem.Models.ViewModels;
using BudgetSystem.ViewModels;
using System.Text;

namespace BudgetSystem.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<TransactionsController> _logger;
        private readonly IConfiguration _configuration;

        public TransactionsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            ILogger<TransactionsController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: Transactions/AddIncome
        public async Task<IActionResult> AddIncome()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = await PrepareIncomeViewModel(user);
            return View(viewModel);
        }

        // POST: Transactions/AddIncome
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddIncome(IncomeTransactionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the account
                    var account = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.Id == model.AccountId && a.UserId == user.Id && a.IsActive);

                    if (account == null)
                    {
                        ModelState.AddModelError("AccountId", "Selected account not found or inactive.");
                        model = await PrepareIncomeViewModel(user, model);
                        return View(model);
                    }

                    // Handle file upload
                    string? receiptPath = null;
                    if (model.ReceiptFile != null && model.ReceiptFile.Length > 0)
                    {
                        receiptPath = await SaveReceiptFile(model.ReceiptFile, user.Id);
                        if (receiptPath == null)
                        {
                            ModelState.AddModelError("ReceiptFile", "Error uploading receipt file.");
                            model = await PrepareIncomeViewModel(user, model);
                            return View(model);
                        }
                    }

                    // Create transaction
                    var transaction = new Transaction
                    {
                        Description = model.Description,
                        Amount = model.Amount,
                        Type = TransactionType.Credit,
                        TransactionDate = model.TransactionDate,
                        Category = model.Category,
                        Notes = model.Notes,
                        Reference = model.Reference,
                        IsRecurring = model.IsRecurring,
                        ReceiptPath = receiptPath,
                        AccountId = model.AccountId,
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Update account balance
                    account.CurrentBalance += model.Amount;
                    account.UpdatedAt = DateTime.UtcNow;

                    // Save to database
                    _context.Transactions.Add(transaction);
                    _context.Accounts.Update(account);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Income of {model.GetCurrencySymbol()}{model.Amount:N2} added successfully to {account.Name}!";
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding income transaction for user {UserId}", user.Id);
                    ModelState.AddModelError("", "An error occurred while saving the transaction. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            model = await PrepareIncomeViewModel(user, model);
            return View(model);
        }

        private async Task<IncomeTransactionViewModel> PrepareIncomeViewModel(ApplicationUser user, IncomeTransactionViewModel? existingModel = null)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == user.Id && a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var viewModel = existingModel ?? new IncomeTransactionViewModel();
            
            // Prepare categories dropdown
            viewModel.Categories = IncomeCategories.GetAll()
                .Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c,
                    Selected = c == viewModel.Category
                }).ToList();

            // Prepare accounts dropdown
            viewModel.Accounts = accounts.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Name} ({GetCurrencySymbol(a.Currency)}{a.CurrentBalance:N2})",
                Selected = a.Id == viewModel.AccountId
            }).ToList();

            viewModel.UserCurrency = user.PreferredCurrency;

            return viewModel;
        }

        private async Task<string?> SaveReceiptFile(IFormFile file, string userId)
        {
            try
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                {
                    return null;
                }

                if (file.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    return null;
                }

                // Create upload directory
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "receipts", userId);
                Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path for storage
                return $"/uploads/receipts/{userId}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving receipt file for user {UserId}", userId);
                return null;
            }
        }

        // GET: Transactions/AddExpense
        public async Task<IActionResult> AddExpense()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = await PrepareExpenseViewModel(user);
            return View(viewModel);
        }

        // POST: Transactions/AddExpense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(ExpenseTransactionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the account
                    var account = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.Id == model.AccountId && a.UserId == user.Id && a.IsActive);

                    if (account == null)
                    {
                        ModelState.AddModelError("AccountId", "Selected account not found or inactive.");
                        model = await PrepareExpenseViewModel(user, model);
                        return View(model);
                    }

                    // Check if account has sufficient balance (except for credit cards)
                    if (account.AccountType != AccountType.CreditCard && account.CurrentBalance < model.Amount)
                    {
                        ModelState.AddModelError("Amount", $"Insufficient funds. Available balance: {GetCurrencySymbol(account.Currency)}{account.CurrentBalance:N2}");
                        model = await PrepareExpenseViewModel(user, model);
                        return View(model);
                    }

                    // Handle file upload
                    string? receiptPath = null;
                    if (model.ReceiptFile != null && model.ReceiptFile.Length > 0)
                    {
                        receiptPath = await SaveReceiptFile(model.ReceiptFile, user.Id);
                        if (receiptPath == null)
                        {
                            ModelState.AddModelError("ReceiptFile", "Error uploading receipt file.");
                            model = await PrepareExpenseViewModel(user, model);
                            return View(model);
                        }
                    }

                    // Create transaction
                    var transaction = new Transaction
                    {
                        Description = model.Description,
                        Amount = model.Amount,
                        Type = TransactionType.Debit,
                        TransactionDate = model.TransactionDate,
                        Category = model.Category,
                        Notes = model.Notes,
                        Reference = model.Reference,
                        IsRecurring = model.IsRecurring,
                        ReceiptPath = receiptPath,
                        AccountId = model.AccountId,
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Update account balance
                    account.CurrentBalance -= model.Amount;
                    account.UpdatedAt = DateTime.UtcNow;

                    // Save to database
                    _context.Transactions.Add(transaction);
                    _context.Accounts.Update(account);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Expense of {model.GetCurrencySymbol()}{model.Amount:N2} recorded successfully from {account.Name}!";
                    return RedirectToAction("Index", "Dashboard");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding expense transaction for user {UserId}", user.Id);
                    ModelState.AddModelError("", "An error occurred while saving the transaction. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            model = await PrepareExpenseViewModel(user, model);
            return View(model);
        }

        private async Task<ExpenseTransactionViewModel> PrepareExpenseViewModel(ApplicationUser user, ExpenseTransactionViewModel? existingModel = null)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == user.Id && a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var viewModel = existingModel ?? new ExpenseTransactionViewModel();
            
            // Prepare categories dropdown
            viewModel.Categories = ExpenseCategories.GetAll()
                .Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c,
                    Selected = c == viewModel.Category
                }).ToList();

            // Prepare accounts dropdown
            viewModel.Accounts = accounts.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Name} ({GetCurrencySymbol(a.Currency)}{a.CurrentBalance:N2})",
                Selected = a.Id == viewModel.AccountId
            }).ToList();

            viewModel.UserCurrency = user.PreferredCurrency;

            return viewModel;
        }

        // GET: Transactions/Index
        public async Task<IActionResult> Index(TransactionFilters? filters, int page = 1, int pageSize = 20)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            filters ??= new TransactionFilters();
            
            var viewModel = await BuildTransactionListViewModel(user, filters, page, pageSize);
            return View(viewModel);
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found or you don't have permission to view it.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new TransactionDetailViewModel
            {
                Id = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Type = transaction.Type,
                TransactionDate = transaction.TransactionDate,
                Category = transaction.Category ?? "Uncategorized",
                Notes = transaction.Notes,
                Reference = transaction.Reference,
                IsRecurring = transaction.IsRecurring,
                ReceiptPath = transaction.ReceiptPath,
                CreatedAt = transaction.CreatedAt,
                
                AccountId = transaction.AccountId,
                AccountName = transaction.Account?.Name ?? "Unknown Account",
                AccountType = transaction.Account?.AccountType ?? AccountType.Other,
                AccountCurrency = transaction.Account?.Currency ?? user.PreferredCurrency,
                AccountCurrentBalance = transaction.Account?.CurrentBalance ?? 0,
                
                UserCurrency = user.PreferredCurrency
            };

            return View(viewModel);
        }

        // GET: Transactions/ViewReceipt/5
        public async Task<IActionResult> ViewReceipt(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found or you don't have permission to view it.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(transaction.ReceiptPath))
            {
                TempData["Error"] = "No receipt is attached to this transaction.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                // Construct the full file path
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts", user.Id.ToString());
                var filePath = Path.Combine(uploadsFolder, transaction.ReceiptPath);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Receipt file not found: {FilePath} for transaction {TransactionId}", filePath, id);
                    TempData["Error"] = "Receipt file not found on server.";
                    return RedirectToAction("Details", new { id });
                }

                // Get file info
                var fileInfo = new FileInfo(filePath);
                var contentType = GetContentType(fileInfo.Extension);

                // Log the file access
                _logger.LogInformation("User {UserId} accessed receipt for transaction {TransactionId}: {FileName}", 
                    user.Id, id, transaction.ReceiptPath);

                // Return the file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, contentType, transaction.ReceiptPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving receipt for transaction {TransactionId}", id);
                TempData["Error"] = "An error occurred while retrieving the receipt.";
                return RedirectToAction("Details", new { id });
            }
        }

        // GET: Transactions/DownloadReceipt/5
        public async Task<IActionResult> DownloadReceipt(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found or you don't have permission to access it.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(transaction.ReceiptPath))
            {
                TempData["Error"] = "No receipt is attached to this transaction.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                // Construct the full file path
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts", user.Id.ToString());
                var filePath = Path.Combine(uploadsFolder, transaction.ReceiptPath);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Receipt file not found: {FilePath} for transaction {TransactionId}", filePath, id);
                    TempData["Error"] = "Receipt file not found on server.";
                    return RedirectToAction("Details", new { id });
                }

                // Get file info and create a meaningful download name
                var fileInfo = new FileInfo(filePath);
                var downloadName = $"Receipt_{transaction.Description.Replace(" ", "_")}_{transaction.TransactionDate:yyyy-MM-dd}{fileInfo.Extension}";
                var contentType = GetContentType(fileInfo.Extension);

                // Log the download
                _logger.LogInformation("User {UserId} downloaded receipt for transaction {TransactionId}: {FileName}", 
                    user.Id, id, transaction.ReceiptPath);

                // Return the file as download
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(fileBytes, contentType, downloadName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading receipt for transaction {TransactionId}", id);
                TempData["Error"] = "An error occurred while downloading the receipt.";
                return RedirectToAction("Details", new { id });
            }
        }

        // GET: Transactions/Transfer
        public async Task<IActionResult> Transfer()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = await PrepareTransferViewModel(user);
            return View(viewModel);
        }

        // POST: Transactions/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Custom validation
            if (model.SourceAccountId == model.DestinationAccountId)
            {
                ModelState.AddModelError("DestinationAccountId", "Source and destination accounts cannot be the same.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get both accounts
                    var sourceAccount = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.Id == model.SourceAccountId && a.UserId == user.Id && a.IsActive);
                    
                    var destinationAccount = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.Id == model.DestinationAccountId && a.UserId == user.Id && a.IsActive);

                    if (sourceAccount == null)
                    {
                        ModelState.AddModelError("SourceAccountId", "Source account not found or inactive.");
                        model = await PrepareTransferViewModel(user, model);
                        return View(model);
                    }

                    if (destinationAccount == null)
                    {
                        ModelState.AddModelError("DestinationAccountId", "Destination account not found or inactive.");
                        model = await PrepareTransferViewModel(user, model);
                        return View(model);
                    }

                    // Check sufficient funds
                    var totalDeduction = model.Amount + model.TransferFee;
                    if (sourceAccount.CurrentBalance < totalDeduction)
                    {
                        ModelState.AddModelError("Amount", $"Insufficient funds. Available balance: {GetCurrencySymbol(sourceAccount.Currency)}{sourceAccount.CurrentBalance:N2}");
                        model = await PrepareTransferViewModel(user, model);
                        return View(model);
                    }

                    // Show confirmation page
                    var confirmationModel = new TransferConfirmationViewModel
                    {
                        Transfer = model,
                        SourceAccountName = sourceAccount.Name,
                        DestinationAccountName = destinationAccount.Name,
                        SourceAccountCurrentBalance = sourceAccount.CurrentBalance,
                        DestinationAccountCurrentBalance = destinationAccount.CurrentBalance,
                        SourceAccountNewBalance = sourceAccount.CurrentBalance - totalDeduction,
                        DestinationAccountNewBalance = destinationAccount.CurrentBalance + model.Amount
                    };

                    return View("ConfirmTransfer", confirmationModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error preparing transfer for user {UserId}", user.Id);
                    ModelState.AddModelError("", "An error occurred while preparing the transfer. Please try again.");
                }
            }

            // If we got this far, something failed, redisplay form
            model = await PrepareTransferViewModel(user, model);
            return View(model);
        }

        // POST: Transactions/ConfirmTransfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmTransfer(TransferConfirmationViewModel confirmationModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = confirmationModel.Transfer;

            try
            {
                // Re-validate accounts (security check)
                var sourceAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == model.SourceAccountId && a.UserId == user.Id && a.IsActive);
                
                var destinationAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == model.DestinationAccountId && a.UserId == user.Id && a.IsActive);

                if (sourceAccount == null || destinationAccount == null)
                {
                    TempData["Error"] = "Invalid account selection. Please try again.";
                    return RedirectToAction("Transfer");
                }

                // Re-check sufficient funds
                var totalDeduction = model.Amount + model.TransferFee;
                if (sourceAccount.CurrentBalance < totalDeduction)
                {
                    TempData["Error"] = "Insufficient funds. Account balance may have changed.";
                    return RedirectToAction("Transfer");
                }

                // Create the transfer transactions (two transactions for proper accounting)
                var outgoingTransaction = new Transaction
                {
                    Description = model.Description,
                    Amount = model.Amount,
                    Type = TransactionType.Transfer,
                    TransactionDate = model.TransferDate,
                    Category = model.Category,
                    Notes = model.Notes,
                    Reference = model.Reference,
                    AccountId = model.SourceAccountId,
                    DestinationAccountId = model.DestinationAccountId,
                    TransferFee = model.TransferFee,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var incomingTransaction = new Transaction
                {
                    Description = $"Transfer from {sourceAccount.Name}",
                    Amount = model.Amount,
                    Type = TransactionType.Transfer,
                    TransactionDate = model.TransferDate,
                    Category = model.Category,
                    Notes = model.Notes,
                    Reference = model.Reference,
                    AccountId = model.DestinationAccountId,
                    DestinationAccountId = model.SourceAccountId,
                    TransferFee = 0, // Fee is only applied to source account
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                // Update account balances
                sourceAccount.CurrentBalance -= totalDeduction;
                sourceAccount.UpdatedAt = DateTime.UtcNow;

                destinationAccount.CurrentBalance += model.Amount;
                destinationAccount.UpdatedAt = DateTime.UtcNow;

                // Save to database
                _context.Transactions.AddRange(outgoingTransaction, incomingTransaction);
                _context.Accounts.UpdateRange(sourceAccount, destinationAccount);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Transfer of {GetCurrencySymbol(sourceAccount.Currency)}{model.Amount:N2} from {sourceAccount.Name} to {destinationAccount.Name} completed successfully!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing transfer for user {UserId}", user.Id);
                TempData["Error"] = "An error occurred while processing the transfer. Please try again.";
                return RedirectToAction("Transfer");
            }
        }

        private async Task<TransferViewModel> PrepareTransferViewModel(ApplicationUser user, TransferViewModel? existingModel = null)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == user.Id && a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var viewModel = existingModel ?? new TransferViewModel();

            // Prepare source accounts dropdown
            viewModel.SourceAccounts = accounts.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Name} ({GetCurrencySymbol(a.Currency)}{a.CurrentBalance:N2})",
                Selected = a.Id == viewModel.SourceAccountId
            }).ToList();

            // Prepare destination accounts dropdown (excluding selected source account)
            viewModel.DestinationAccounts = accounts
                .Where(a => a.Id != viewModel.SourceAccountId)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.Name} ({GetCurrencySymbol(a.Currency)}{a.CurrentBalance:N2})",
                    Selected = a.Id == viewModel.DestinationAccountId
                }).ToList();

            // Prepare transfer categories dropdown
            viewModel.TransferCategories = Models.TransferCategories.GetAll()
                .Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c,
                    Selected = c == viewModel.Category
                }).ToList();

            // Set account balances for validation
            if (viewModel.SourceAccountId > 0)
            {
                var sourceAccount = accounts.FirstOrDefault(a => a.Id == viewModel.SourceAccountId);
                if (sourceAccount != null)
                {
                    viewModel.SourceAccountBalance = sourceAccount.CurrentBalance;
                    viewModel.HasSufficientFunds = sourceAccount.CurrentBalance >= viewModel.TotalDeduction;
                }
            }

            return viewModel;
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

        private async Task<TransactionListViewModel> BuildTransactionListViewModel(
            ApplicationUser user, 
            TransactionFilters filters, 
            int page, 
            int pageSize)
        {
            var query = _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.UserId == user.Id && !t.IsDeleted);

            // Apply filters
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(t => t.Description.Contains(filters.SearchTerm) ||
                                        (t.Notes != null && t.Notes.Contains(filters.SearchTerm)) ||
                                        (t.Reference != null && t.Reference.Contains(filters.SearchTerm)));
            }

            if (filters.Type.HasValue)
            {
                query = query.Where(t => t.Type == filters.Type.Value);
            }

            if (filters.AccountId.HasValue)
            {
                query = query.Where(t => t.AccountId == filters.AccountId.Value);
            }

            if (!string.IsNullOrEmpty(filters.Category))
            {
                query = query.Where(t => t.Category == filters.Category);
            }

            if (filters.FromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= filters.FromDate.Value);
            }

            if (filters.ToDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= filters.ToDate.Value);
            }

            if (filters.MinAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= filters.MinAmount.Value);
            }

            if (filters.MaxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= filters.MaxAmount.Value);
            }

            if (filters.ShowRecurringOnly)
            {
                query = query.Where(t => t.IsRecurring);
            }

            if (filters.ShowWithReceiptsOnly)
            {
                query = query.Where(t => t.ReceiptPath != null);
            }

            // Apply sorting
            query = filters.SortBy switch
            {
                "Amount" => filters.SortOrder == "asc" 
                    ? query.OrderBy(t => t.Amount)
                    : query.OrderByDescending(t => t.Amount),
                "Description" => filters.SortOrder == "asc"
                    ? query.OrderBy(t => t.Description)
                    : query.OrderByDescending(t => t.Description),
                "Category" => filters.SortOrder == "asc"
                    ? query.OrderBy(t => t.Category)
                    : query.OrderByDescending(t => t.Category),
                "AccountName" => filters.SortOrder == "asc"
                    ? query.OrderBy(t => t.Account!.Name)
                    : query.OrderByDescending(t => t.Account!.Name),
                "CreatedAt" => filters.SortOrder == "asc"
                    ? query.OrderBy(t => t.CreatedAt)
                    : query.OrderByDescending(t => t.CreatedAt),
                _ => filters.SortOrder == "asc"
                    ? query.OrderBy(t => t.TransactionDate)
                    : query.OrderByDescending(t => t.TransactionDate)
            };

            // Calculate summary
            var allTransactions = await query.ToListAsync();
            var summary = new TransactionSummary
            {
                TotalIncome = allTransactions.Where(t => t.Type == TransactionType.Credit).Sum(t => t.Amount),
                TotalExpenses = allTransactions.Where(t => t.Type == TransactionType.Debit).Sum(t => t.Amount),
                TotalTransfers = allTransactions.Where(t => t.Type == TransactionType.Transfer).Sum(t => t.Amount),
                IncomeCount = allTransactions.Count(t => t.Type == TransactionType.Credit),
                ExpenseCount = allTransactions.Count(t => t.Type == TransactionType.Debit),
                TransferCount = allTransactions.Count(t => t.Type == TransactionType.Transfer),
                PeriodStart = filters.FromDate,
                PeriodEnd = filters.ToDate
            };

            // Apply pagination
            var totalCount = allTransactions.Count;
            var transactions = allTransactions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionListItem
                {
                    Id = t.Id,
                    Description = t.Description,
                    Amount = t.Amount,
                    Type = t.Type,
                    TransactionDate = t.TransactionDate,
                    Category = t.Category ?? "Uncategorized",
                    AccountName = t.Account?.Name ?? "Unknown",
                    AccountType = t.Account?.AccountType ?? AccountType.Other,
                    Reference = t.Reference,
                    Notes = t.Notes,
                    HasReceipt = !string.IsNullOrEmpty(t.ReceiptPath),
                    IsRecurring = t.IsRecurring,
                    CreatedAt = t.CreatedAt
                })
                .ToList();

            // Prepare filter options
            var accounts = await _context.Accounts
                .Where(a => a.UserId == user.Id && a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var categories = allTransactions
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            var viewModel = new TransactionListViewModel
            {
                Transactions = transactions,
                Filters = filters,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                },
                Summary = summary,
                UserCurrency = user.PreferredCurrency,
                
                AccountOptions = accounts.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name,
                    Selected = a.Id == filters.AccountId
                }).ToList(),
                
                CategoryOptions = categories.Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c,
                    Selected = c == filters.Category
                }).ToList(),
                
                TypeOptions = new List<SelectListItem>
                {
                    new() { Value = "", Text = "All Types", Selected = !filters.Type.HasValue },
                    new() { Value = "0", Text = "Income", Selected = filters.Type == TransactionType.Credit },
                    new() { Value = "1", Text = "Expense", Selected = filters.Type == TransactionType.Debit },
                    new() { Value = "2", Text = "Transfer", Selected = filters.Type == TransactionType.Transfer }
                }
            };

            return viewModel;
        }

        // GET: Transactions/ExportCsv
        public async Task<IActionResult> ExportCsv(TransactionFilters? filters)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            filters ??= new TransactionFilters();
            
            // Get all transactions without pagination for export
            var query = _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.UserId == user.Id && !t.IsDeleted);

            // Apply filters (inline implementation)
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(t => t.Description.Contains(filters.SearchTerm) ||
                                        (t.Notes != null && t.Notes.Contains(filters.SearchTerm)) ||
                                        (t.Reference != null && t.Reference.Contains(filters.SearchTerm)));
            }

            if (filters.Type.HasValue)
            {
                query = query.Where(t => t.Type == filters.Type.Value);
            }

            if (filters.AccountId.HasValue)
            {
                query = query.Where(t => t.AccountId == filters.AccountId.Value);
            }

            if (!string.IsNullOrEmpty(filters.Category))
            {
                query = query.Where(t => t.Category == filters.Category);
            }

            if (filters.FromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= filters.FromDate.Value);
            }

            if (filters.ToDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= filters.ToDate.Value);
            }

            if (filters.MinAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= filters.MinAmount.Value);
            }

            if (filters.MaxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= filters.MaxAmount.Value);
            }

            if (filters.ShowRecurringOnly)
            {
                query = query.Where(t => t.IsRecurring);
            }

            if (filters.ShowWithReceiptsOnly)
            {
                query = query.Where(t => !string.IsNullOrEmpty(t.ReceiptPath));
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionExportItem
                {
                    Date = t.TransactionDate,
                    Type = t.Type.ToString(),
                    Description = t.Description,
                    Category = t.Category ?? "Uncategorized",
                    Account = t.Account!.Name,
                    Amount = t.Amount,
                    Reference = t.Reference ?? "",
                    Notes = t.Notes ?? ""
                })
                .ToListAsync();

            var csvContent = GenerateCsvContent(transactions, user.PreferredCurrency);
            var fileName = $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
        }

        // GET: Transactions/ExportPdf
        public async Task<IActionResult> ExportPdf(TransactionFilters? filters)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            filters ??= new TransactionFilters();
            
            // Get all transactions without pagination for export
            var query = _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.UserId == user.Id && !t.IsDeleted);

            // Apply filters (inline implementation)
            if (!string.IsNullOrEmpty(filters.SearchTerm))
            {
                query = query.Where(t => t.Description.Contains(filters.SearchTerm) ||
                                        (t.Notes != null && t.Notes.Contains(filters.SearchTerm)) ||
                                        (t.Reference != null && t.Reference.Contains(filters.SearchTerm)));
            }

            if (filters.Type.HasValue)
            {
                query = query.Where(t => t.Type == filters.Type.Value);
            }

            if (filters.AccountId.HasValue)
            {
                query = query.Where(t => t.AccountId == filters.AccountId.Value);
            }

            if (!string.IsNullOrEmpty(filters.Category))
            {
                query = query.Where(t => t.Category == filters.Category);
            }

            if (filters.FromDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= filters.FromDate.Value);
            }

            if (filters.ToDate.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= filters.ToDate.Value);
            }

            if (filters.MinAmount.HasValue)
            {
                query = query.Where(t => t.Amount >= filters.MinAmount.Value);
            }

            if (filters.MaxAmount.HasValue)
            {
                query = query.Where(t => t.Amount <= filters.MaxAmount.Value);
            }

            if (filters.ShowRecurringOnly)
            {
                query = query.Where(t => t.IsRecurring);
            }

            if (filters.ShowWithReceiptsOnly)
            {
                query = query.Where(t => !string.IsNullOrEmpty(t.ReceiptPath));
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionExportItem
                {
                    Date = t.TransactionDate,
                    Type = t.Type.ToString(),
                    Description = t.Description,
                    Category = t.Category ?? "Uncategorized",
                    Account = t.Account!.Name,
                    Amount = t.Amount,
                    Reference = t.Reference ?? "",
                    Notes = t.Notes ?? ""
                })
                .ToListAsync();

            var pdfBytes = GeneratePdfContent(transactions, user.PreferredCurrency, user.FirstName + " " + user.LastName);
            var fileName = $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }

        private string GenerateCsvContent(List<TransactionExportItem> transactions, Currency currency)
        {
            var currencySymbol = GetCurrencySymbol(currency);
            var csv = new StringBuilder();
            
            // Add header
            csv.AppendLine("Date,Type,Description,Category,Account,Amount,Reference,Notes");
            
            // Add data rows
            foreach (var transaction in transactions)
            {
                csv.AppendLine($"{transaction.Date:yyyy-MM-dd}," +
                              $"{EscapeCsvField(transaction.Type)}," +
                              $"{EscapeCsvField(transaction.Description)}," +
                              $"{EscapeCsvField(transaction.Category)}," +
                              $"{EscapeCsvField(transaction.Account)}," +
                              $"{currencySymbol}{transaction.Amount:N2}," +
                              $"{EscapeCsvField(transaction.Reference)}," +
                              $"{EscapeCsvField(transaction.Notes)}");
            }
            
            return csv.ToString();
        }

        private byte[] GeneratePdfContent(List<TransactionExportItem> transactions, Currency currency, string userName)
        {
            using var stream = new MemoryStream();
            var writer = new iText.Kernel.Pdf.PdfWriter(stream);
            var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var document = new iText.Layout.Document(pdf);
            
            var currencySymbol = GetCurrencySymbol(currency);
            
            // Add title
            var title = new iText.Layout.Element.Paragraph("Transaction History Report")
                .SetFontSize(20)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            document.Add(title);
            
            // Add user info and date
            var userInfo = new iText.Layout.Element.Paragraph($"User: {userName}")
                .SetFontSize(12);
            document.Add(userInfo);
            
            var reportDate = new iText.Layout.Element.Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                .SetFontSize(10)
                .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY);
            document.Add(reportDate);
            
            // Add summary
            var totalIncome = transactions.Where(t => t.Type == "Credit").Sum(t => t.Amount);
            var totalExpenses = transactions.Where(t => t.Type == "Debit").Sum(t => t.Amount);
            var netAmount = totalIncome - totalExpenses;
            
            var summary = new iText.Layout.Element.Paragraph($"Summary: Income: {currencySymbol}{totalIncome:N2} | " +
                                                           $"Expenses: {currencySymbol}{totalExpenses:N2} | " +
                                                           $"Net: {currencySymbol}{netAmount:N2}")
                .SetFontSize(12)
                .SetMarginBottom(20);
            document.Add(summary);
            
            if (transactions.Any())
            {
                // Create table
                var table = new iText.Layout.Element.Table(new float[] { 2, 1.5f, 3, 2, 2, 2 })
                    .SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));
                
                // Add headers
                var headers = new[] { "Date", "Type", "Description", "Category", "Account", "Amount" };
                foreach (var header in headers)
                {
                    table.AddHeaderCell(new iText.Layout.Element.Cell()
                        .Add(new iText.Layout.Element.Paragraph(header))
                        .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY));
                }
                
                // Add data rows
                foreach (var transaction in transactions.Take(100)) // Limit to 100 rows for PDF
                {
                    table.AddCell(transaction.Date.ToString("yyyy-MM-dd"));
                    table.AddCell(transaction.Type);
                    table.AddCell(transaction.Description.Length > 30 ? 
                                 transaction.Description.Substring(0, 30) + "..." : 
                                 transaction.Description);
                    table.AddCell(transaction.Category);
                    table.AddCell(transaction.Account);
                    table.AddCell($"{currencySymbol}{transaction.Amount:N2}");
                }
                
                document.Add(table);
                
                if (transactions.Count > 100)
                {
                    var note = new iText.Layout.Element.Paragraph($"Note: Only first 100 transactions shown. Total: {transactions.Count}")
                        .SetFontSize(10)
                        .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY)
                        .SetMarginTop(10);
                    document.Add(note);
                }
            }
            else
            {
                var noData = new iText.Layout.Element.Paragraph("No transactions found for the selected criteria.")
                    .SetFontSize(12)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                document.Add(noData);
            }
            
            document.Close();
            return stream.ToArray();
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";
                
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            
            return field;
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id && !t.IsDeleted);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found or you don't have permission to edit it.";
                return RedirectToAction("Index");
            }

            // Check if transaction can be edited (based on age)
            var config = _configuration.GetSection("TransactionSettings");
            var allowEditAfterDays = config.GetValue<int>("AllowEditAfterDays", 90);
            var daysSinceCreated = (DateTime.UtcNow - transaction.CreatedAt).Days;

            if (daysSinceCreated > allowEditAfterDays)
            {
                TempData["Error"] = $"Transactions older than {allowEditAfterDays} days cannot be edited for data integrity.";
                return RedirectToAction("Details", new { id });
            }

            var viewModel = await PrepareEditViewModel(user, transaction);
            return View(viewModel);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditTransactionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != model.Id)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id && !t.IsDeleted);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found or you don't have permission to edit it.";
                return RedirectToAction("Index");
            }

            // Check edit age restriction again
            var config = _configuration.GetSection("TransactionSettings");
            var allowEditAfterDays = config.GetValue<int>("AllowEditAfterDays", 90);
            var daysSinceCreated = (DateTime.UtcNow - transaction.CreatedAt).Days;

            if (daysSinceCreated > allowEditAfterDays)
            {
                TempData["Error"] = $"Transactions older than {allowEditAfterDays} days cannot be edited.";
                return RedirectToAction("Details", new { id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get new account if changed
                    var newAccount = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.Id == model.AccountId && a.UserId == user.Id && a.IsActive);

                    if (newAccount == null)
                    {
                        ModelState.AddModelError("AccountId", "Selected account not found or inactive.");
                        model = await PrepareEditViewModel(user, transaction, model);
                        return View(model);
                    }

                    // Calculate balance adjustments
                    var originalAccount = transaction.Account!;
                    var originalAmount = transaction.Amount;
                    var amountChanged = originalAmount != model.Amount;
                    var accountChanged = transaction.AccountId != model.AccountId;

                    // Handle receipt updates
                    string? receiptPath = transaction.ReceiptPath;
                    if (model.RemoveCurrentReceipt)
                    {
                        if (!string.IsNullOrEmpty(receiptPath))
                        {
                            DeleteReceiptFile(receiptPath);
                        }
                        receiptPath = null;
                    }

                    if (model.NewReceiptFile != null && model.NewReceiptFile.Length > 0)
                    {
                        // Remove old receipt if exists
                        if (!string.IsNullOrEmpty(receiptPath))
                        {
                            DeleteReceiptFile(receiptPath);
                        }
                        
                        receiptPath = await SaveReceiptFile(model.NewReceiptFile, user.Id);
                        if (receiptPath == null)
                        {
                            ModelState.AddModelError("NewReceiptFile", "Error uploading receipt file.");
                            model = await PrepareEditViewModel(user, transaction, model);
                            return View(model);
                        }
                    }

                    // Reverse original transaction impact on old account
                    if (transaction.Type == TransactionType.Credit)
                    {
                        originalAccount.CurrentBalance -= originalAmount;
                    }
                    else
                    {
                        originalAccount.CurrentBalance += originalAmount;
                    }

                    // Apply new transaction impact on new account
                    if (transaction.Type == TransactionType.Credit)
                    {
                        newAccount.CurrentBalance += model.Amount;
                    }
                    else
                    {
                        newAccount.CurrentBalance -= model.Amount;
                    }

                    // Update transaction
                    transaction.Description = model.Description;
                    transaction.Amount = model.Amount;
                    transaction.TransactionDate = model.TransactionDate;
                    transaction.Category = model.Category;
                    transaction.Notes = model.Notes;
                    transaction.Reference = model.Reference;
                    transaction.IsRecurring = model.IsRecurring;
                    transaction.AccountId = model.AccountId;
                    transaction.ReceiptPath = receiptPath;
                    
                    // Audit trail
                    transaction.LastModifiedAt = DateTime.UtcNow;
                    transaction.LastModifiedBy = user.Id;
                    transaction.ModificationReason = model.ModificationReason;

                    // Update accounts
                    originalAccount.UpdatedAt = DateTime.UtcNow;
                    newAccount.UpdatedAt = DateTime.UtcNow;

                    _context.Transactions.Update(transaction);
                    _context.Accounts.Update(originalAccount);
                    if (accountChanged)
                    {
                        _context.Accounts.Update(newAccount);
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Transaction updated successfully!";
                    return RedirectToAction("Details", new { id });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating transaction {TransactionId} for user {UserId}", id, user.Id);
                    ModelState.AddModelError("", "An error occurred while updating the transaction. Please try again.");
                }
            }

            model = await PrepareEditViewModel(user, transaction, model);
            return View(model);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id && !t.IsDeleted);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found or you don't have permission to delete it.";
                return RedirectToAction("Index");
            }

            // Check if transaction can be deleted (based on age)
            var config = _configuration.GetSection("TransactionSettings");
            var allowDeletionAfterDays = config.GetValue<int>("AllowDeletionAfterDays", 30);
            var daysSinceCreated = (DateTime.UtcNow - transaction.CreatedAt).Days;

            var viewModel = new DeleteTransactionViewModel
            {
                Id = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                Type = transaction.Type,
                TransactionDate = transaction.TransactionDate,
                Category = transaction.Category ?? "Uncategorized",
                Notes = transaction.Notes,
                Reference = transaction.Reference,
                IsRecurring = transaction.IsRecurring,
                AccountName = transaction.Account!.Name,
                AccountCurrentBalance = transaction.Account.CurrentBalance,
                UserCurrency = user.PreferredCurrency,
                CreatedAt = transaction.CreatedAt,
                HasReceipt = !string.IsNullOrEmpty(transaction.ReceiptPath),
                CanDelete = daysSinceCreated <= allowDeletionAfterDays,
                DeleteRestrictionReason = daysSinceCreated > allowDeletionAfterDays 
                    ? $"Transactions older than {allowDeletionAfterDays} days cannot be deleted for audit compliance."
                    : null
            };

            return View(viewModel);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, DeleteTransactionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id && !t.IsDeleted);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found or you don't have permission to delete it.";
                return RedirectToAction("Index");
            }

            // Check deletion age restriction
            var config = _configuration.GetSection("TransactionSettings");
            var allowDeletionAfterDays = config.GetValue<int>("AllowDeletionAfterDays", 30);
            var daysSinceCreated = (DateTime.UtcNow - transaction.CreatedAt).Days;

            if (daysSinceCreated > allowDeletionAfterDays)
            {
                TempData["Error"] = $"Transactions older than {allowDeletionAfterDays} days cannot be deleted.";
                return RedirectToAction("Details", new { id });
            }

            if (string.IsNullOrWhiteSpace(model.DeletionReason))
            {
                ModelState.AddModelError("DeletionReason", "Deletion reason is required.");
                return View("Delete", model);
            }

            try
            {
                // Reverse transaction impact on account balance
                var account = transaction.Account!;
                if (transaction.Type == TransactionType.Credit)
                {
                    account.CurrentBalance -= transaction.Amount;
                }
                else
                {
                    account.CurrentBalance += transaction.Amount;
                }

                // Soft delete - don't actually remove from database
                transaction.IsDeleted = true;
                transaction.DeletedAt = DateTime.UtcNow;
                transaction.DeletedBy = user.Id;
                transaction.DeletionReason = model.DeletionReason;

                // Update account
                account.UpdatedAt = DateTime.UtcNow;

                _context.Transactions.Update(transaction);
                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Transaction deleted successfully. Account balance has been adjusted.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId} for user {UserId}", id, user.Id);
                TempData["Error"] = "An error occurred while deleting the transaction. Please try again.";
                return RedirectToAction("Details", new { id });
            }
        }

        private async Task<EditTransactionViewModel> PrepareEditViewModel(ApplicationUser user, Transaction transaction, EditTransactionViewModel? existingModel = null)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == user.Id && a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var viewModel = existingModel ?? new EditTransactionViewModel
            {
                Id = transaction.Id,
                Description = transaction.Description,
                Amount = transaction.Amount,
                TransactionDate = transaction.TransactionDate,
                Category = transaction.Category ?? "",
                Notes = transaction.Notes,
                Reference = transaction.Reference,
                IsRecurring = transaction.IsRecurring,
                AccountId = transaction.AccountId
            };

            viewModel.Type = transaction.Type;
            viewModel.CurrentReceiptPath = transaction.ReceiptPath;
            viewModel.CreatedAt = transaction.CreatedAt;
            viewModel.OriginalAmount = transaction.Amount;
            viewModel.UserCurrency = user.PreferredCurrency;

            // Prepare categories dropdown based on transaction type
            if (transaction.Type == TransactionType.Credit)
            {
                viewModel.Categories = IncomeCategories.GetAll()
                    .Select(c => new SelectListItem
                    {
                        Value = c,
                        Text = c,
                        Selected = c == viewModel.Category
                    }).ToList();
            }
            else
            {
                viewModel.Categories = ExpenseCategories.GetAll()
                    .Select(c => new SelectListItem
                    {
                        Value = c,
                        Text = c,
                        Selected = c == viewModel.Category
                    }).ToList();
            }

            // Prepare accounts dropdown
            viewModel.Accounts = accounts.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.Name} ({GetCurrencySymbol(a.Currency)}{a.CurrentBalance:N2})",
                Selected = a.Id == viewModel.AccountId
            }).ToList();

            return viewModel;
        }

        private void DeleteReceiptFile(string receiptPath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, receiptPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting receipt file: {ReceiptPath}", receiptPath);
            }
        }

        private string GetCurrencySymbol(Currency currency)
        {
            return currency switch
            {
                Currency.USD => "$",
                Currency.EUR => "€",
                Currency.GBP => "£",
                Currency.NGN => "₦",
                Currency.CAD => "C$",
                Currency.AUD => "A$",
                Currency.JPY => "¥",
                Currency.CHF => "Fr",
                Currency.CNY => "¥",
                Currency.INR => "₹",
                _ => "$"
            };
        }
    }

    public class TransactionExportItem
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
} 