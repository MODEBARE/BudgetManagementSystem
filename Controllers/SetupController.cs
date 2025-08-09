using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BudgetSystem.Data;
using BudgetSystem.Models;
using BudgetSystem.ViewModels;

namespace BudgetSystem.Controllers
{
    [Authorize]
    public class SetupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SetupController> _logger;

        public SetupController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<SetupController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // Story 1.2: Profile Setup
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileSetupViewModel
            {
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                Country = user.Country,
                PreferredCurrency = user.PreferredCurrency
            };

            ViewBag.IsProfileComplete = user.IsProfileComplete;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileSetupViewModel model)
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

            // Update user profile
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.State = model.State;
            user.PostalCode = model.PostalCode;
            user.Country = model.Country;
            user.PreferredCurrency = model.PreferredCurrency;
            user.IsProfileComplete = true;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Message"] = "Profile updated successfully!";
                
                // Check if user has accounts, if not redirect to account setup
                var hasAccounts = _context.Accounts.Any(a => a.UserId == user.Id);
                if (!hasAccounts)
                {
                    return RedirectToAction("Accounts");
                }
                
                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // Story 1.3: Account Setup
        [HttpGet]
        public async Task<IActionResult> Accounts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new AccountSetupViewModel();
            
            // Set default currency from user preference
            foreach (var account in model.Accounts)
            {
                account.Currency = user.PreferredCurrency;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accounts(AccountSetupViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var selectedAccounts = model.Accounts.Where(a => a.IsSelected).ToList();
            
            if (!selectedAccounts.Any())
            {
                ModelState.AddModelError("", "Please select at least one account to create.");
                return View(model);
            }

            // Validate selected accounts
            foreach (var account in selectedAccounts)
            {
                if (string.IsNullOrWhiteSpace(account.Name))
                {
                    ModelState.AddModelError("", $"Account name is required for {account.AccountType} account.");
                    return View(model);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Create accounts
                foreach (var accountModel in selectedAccounts)
                {
                    var account = new Account
                    {
                        Name = accountModel.Name.Trim(),
                        Description = accountModel.Description?.Trim(),
                        AccountType = accountModel.AccountType,
                        InitialBalance = accountModel.InitialBalance,
                        CurrentBalance = accountModel.InitialBalance,
                        Currency = accountModel.Currency,
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.Accounts.Add(account);
                }

                await _context.SaveChangesAsync();

                TempData["Message"] = $"Successfully created {selectedAccounts.Count} account(s)!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accounts for user {UserId}", user.Id);
                ModelState.AddModelError("", "An error occurred while creating your accounts. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CreateAccountViewModel
            {
                Currency = user.PreferredCurrency
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
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

                TempData["Message"] = $"Account '{model.Name}' created successfully!";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account for user {UserId}", user.Id);
                ModelState.AddModelError("", "An error occurred while creating your account. Please try again.");
                return View(model);
            }
        }
    }
} 