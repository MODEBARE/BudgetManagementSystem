using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetSystem.Data;
using BudgetSystem.Models;
using BudgetSystem.ViewModels;

namespace BudgetSystem.Controllers
{
    [Authorize]
    public class MyAccountsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MyAccountsController> _logger;

        public MyAccountsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<MyAccountsController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: MyAccounts
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var accounts = await _context.Accounts
                .Where(a => a.UserId == user.Id)
                .OrderBy(a => a.Name)
                .ToListAsync();

            var viewModel = new MyAccountsViewModel
            {
                Accounts = accounts.Select(a => new AccountSummary
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    AccountType = a.AccountType,
                    CurrentBalance = a.CurrentBalance,
                    InitialBalance = a.InitialBalance,
                    Currency = a.Currency,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    TransactionCount = _context.Transactions.Count(t => t.AccountId == a.Id)
                }).ToList(),
                UserCurrency = user.PreferredCurrency
            };

            return View(viewModel);
        }

        // GET: MyAccounts/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new CreateAccountViewModel
            {
                Currency = user.PreferredCurrency
            };

            return View(viewModel);
        }

        // POST: MyAccounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check for duplicate account names
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Name.ToLower() == model.Name.ToLower().Trim());

            if (existingAccount != null)
            {
                ModelState.AddModelError("Name", "An account with this name already exists.");
                return View(model);
            }

            try
            {
                var account = new Account
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    AccountType = model.AccountType,
                    InitialBalance = model.InitialBalance,
                    CurrentBalance = model.InitialBalance,
                    Currency = model.Currency,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Account '{model.Name}' created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account for user {UserId}", user.Id);
                ModelState.AddModelError("", "An error occurred while creating your account. Please try again.");
                return View(model);
            }
        }

        // GET: MyAccounts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index");
            }

            var viewModel = new EditAccountViewModel
            {
                Id = account.Id,
                Name = account.Name,
                Description = account.Description,
                AccountType = account.AccountType,
                InitialBalance = account.InitialBalance,
                Currency = account.Currency,
                IsActive = account.IsActive
            };

            return View(viewModel);
        }

        // POST: MyAccounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAccountViewModel model)
        {
            if (id != model.Id)
            {
                TempData["Error"] = "Invalid account ID.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index");
            }

            // Check for duplicate account names (excluding current account)
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == user.Id && 
                                        a.Id != id && 
                                        a.Name.ToLower() == model.Name.ToLower().Trim());

            if (existingAccount != null)
            {
                ModelState.AddModelError("Name", "An account with this name already exists.");
                return View(model);
            }

            try
            {
                // Calculate balance adjustment if initial balance changed
                var balanceAdjustment = model.InitialBalance - account.InitialBalance;
                
                account.Name = model.Name.Trim();
                account.Description = model.Description?.Trim();
                account.AccountType = model.AccountType;
                account.InitialBalance = model.InitialBalance;
                account.CurrentBalance += balanceAdjustment; // Adjust current balance
                account.Currency = model.Currency;
                account.IsActive = model.IsActive;
                account.UpdatedAt = DateTime.UtcNow;

                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Account '{model.Name}' updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account {AccountId} for user {UserId}", id, user.Id);
                ModelState.AddModelError("", "An error occurred while updating your account. Please try again.");
                return View(model);
            }
        }

        // GET: MyAccounts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index");
            }

            // Get recent transactions for this account
            var recentTransactions = await _context.Transactions
                .Where(t => t.AccountId == id || t.DestinationAccountId == id)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .ToListAsync();

            // Get transaction statistics
            var transactionStats = await GetAccountTransactionStats(id);

            var viewModel = new AccountDetailsViewModel
            {
                Account = account,
                RecentTransactions = recentTransactions,
                TransactionStats = transactionStats,
                UserCurrency = user.PreferredCurrency
            };

            return View(viewModel);
        }

        // POST: MyAccounts/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index");
            }

            try
            {
                account.IsActive = !account.IsActive;
                account.UpdatedAt = DateTime.UtcNow;

                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();

                var status = account.IsActive ? "activated" : "deactivated";
                TempData["Success"] = $"Account '{account.Name}' has been {status}.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for account {AccountId}", id);
                TempData["Error"] = "An error occurred while updating the account status.";
            }

            return RedirectToAction("Index");
        }

        // GET: MyAccounts/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index");
            }

            // Check if account has transactions
            var hasTransactions = await _context.Transactions
                .AnyAsync(t => t.AccountId == id || t.DestinationAccountId == id);

            var viewModel = new DeleteAccountViewModel
            {
                Account = account,
                HasTransactions = hasTransactions,
                TransactionCount = hasTransactions ? 
                    await _context.Transactions.CountAsync(t => t.AccountId == id || t.DestinationAccountId == id) : 0
            };

            return View(viewModel);
        }

        // POST: MyAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index");
            }

            try
            {
                // Check if account has transactions
                var hasTransactions = await _context.Transactions
                    .AnyAsync(t => t.AccountId == id || t.DestinationAccountId == id);

                if (hasTransactions)
                {
                    TempData["Error"] = "Cannot delete account with existing transactions. Please deactivate the account instead.";
                    return RedirectToAction("Index");
                }

                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Account '{account.Name}' has been deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account {AccountId}", id);
                TempData["Error"] = "An error occurred while deleting the account.";
            }

            return RedirectToAction("Index");
        }

        private async Task<AccountTransactionStats> GetAccountTransactionStats(int accountId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.AccountId == accountId || t.DestinationAccountId == accountId)
                .ToListAsync();

            return new AccountTransactionStats
            {
                TotalTransactions = transactions.Count,
                TotalIncome = transactions.Where(t => t.AccountId == accountId && t.Type == TransactionType.Credit).Sum(t => t.Amount),
                TotalExpenses = transactions.Where(t => t.AccountId == accountId && t.Type == TransactionType.Debit).Sum(t => t.Amount),
                TotalTransfersIn = transactions.Where(t => t.DestinationAccountId == accountId && t.Type == TransactionType.Transfer).Sum(t => t.Amount),
                TotalTransfersOut = transactions.Where(t => t.AccountId == accountId && t.Type == TransactionType.Transfer).Sum(t => t.Amount),
                LastTransactionDate = transactions.Any() ? transactions.Max(t => t.TransactionDate) : (DateTime?)null,
                FirstTransactionDate = transactions.Any() ? transactions.Min(t => t.TransactionDate) : (DateTime?)null
            };
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
} 