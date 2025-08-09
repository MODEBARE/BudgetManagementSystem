using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetSystem.Data;
using BudgetSystem.Models;
using BudgetSystem.Models.ViewModels;

namespace BudgetSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<DashboardController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if profile setup is needed (Story 1.2)
            if (!user.IsProfileComplete)
            {
                TempData["Info"] = "Please complete your profile setup to get started.";
                return RedirectToAction("Profile", "Setup");
            }

            // Check if account setup is needed (Story 1.3)
            var hasAccounts = await _context.Accounts.AnyAsync(a => a.UserId == user.Id && a.IsActive);
            if (!hasAccounts)
            {
                TempData["Info"] = "Let's set up your first accounts to start tracking your finances.";
                return RedirectToAction("Accounts", "Setup");
            }

            // Gather all dashboard data
            var viewModel = await BuildDashboardViewModel(user);
            
            return View(viewModel);
        }

        private async Task<DashboardViewModel> BuildDashboardViewModel(ApplicationUser user)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            
            // Get user's accounts
            var accounts = await _context.Accounts
                .Where(a => a.UserId == user.Id && a.IsActive)
                .ToListAsync();

            // Get current month's transactions
            var monthlyTransactions = await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.UserId == user.Id && 
                           t.TransactionDate.Month == currentMonth && 
                           t.TransactionDate.Year == currentYear)
                .ToListAsync();

            // Get recent transactions (last 10)
            var recentTransactions = await _context.Transactions
                .Include(t => t.Account)
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .ToListAsync();

            // Get budgets
            var budgets = await _context.Budgets
                .Where(b => b.UserId == user.Id)
                .ToListAsync();

            // Get current month's income and expenses from transactions
            var monthlyIncome = monthlyTransactions
                .Where(t => t.Type == TransactionType.Credit)
                .Sum(t => t.Amount);

            var monthlyExpenses = monthlyTransactions
                .Where(t => t.Type == TransactionType.Debit)
                .Sum(t => t.Amount);

            // Calculate total account balance
            var totalBalance = accounts.Sum(a => a.CurrentBalance);

            // Calculate budget usage (simplified - total expenses vs total budget amount)
            var totalBudgetAmount = budgets.Sum(b => b.Amount);
            var budgetUsedPercentage = totalBudgetAmount > 0 ? (monthlyExpenses / totalBudgetAmount) * 100 : 0;

            var viewModel = new DashboardViewModel
            {
                UserName = $"{user.FirstName} {user.LastName}",
                UserType = user.UserType,
                CompanyName = user.CompanyName ?? string.Empty,
                PreferredCurrency = user.PreferredCurrency,
                
                // Financial Summary
                TotalIncome = monthlyIncome,
                TotalExpenses = monthlyExpenses,
                TotalAccountBalance = totalBalance,
                BudgetUsedPercentage = Math.Round(budgetUsedPercentage, 1),
                
                // Progress Tracking
                IsProfileComplete = user.IsProfileComplete,
                HasAccounts = accounts.Any(),
                HasBudgets = budgets.Any(),
                HasTransactions = recentTransactions.Any(),
                
                // Account Summary
                Accounts = accounts.Select(a => new AccountSummary
                {
                    Id = a.Id,
                    Name = a.Name,
                    AccountType = a.AccountType,
                    CurrentBalance = a.CurrentBalance,
                    Currency = a.Currency,
                    IsActive = a.IsActive
                }).ToList(),
                TotalAccounts = accounts.Count,
                
                // Recent Activity
                RecentTransactions = recentTransactions.Select(t => new RecentTransaction
                {
                    Id = t.Id,
                    Description = t.Description,
                    Amount = t.Amount,
                    Type = t.Type,
                    TransactionDate = t.TransactionDate,
                    AccountName = t.Account?.Name ?? "Unknown",
                    Category = t.Category ?? "Uncategorized"
                }).ToList(),
                
                // Quick Stats
                TransactionsThisMonth = monthlyTransactions.Count,
                ActiveBudgets = budgets.Count,
                LastLoginAt = user.LastLoginAt ?? user.CreatedAt
            };

            return viewModel;
        }
    }
} 