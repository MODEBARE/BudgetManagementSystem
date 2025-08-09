using BudgetSystem.Models;

namespace BudgetSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Currency PreferredCurrency { get; set; }
        
        // Financial Summary
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetSavings => TotalIncome - TotalExpenses;
        public decimal TotalAccountBalance { get; set; }
        public decimal BudgetUsedPercentage { get; set; }
        
        // Progress Tracking
        public bool IsProfileComplete { get; set; }
        public bool HasAccounts { get; set; }
        public bool HasBudgets { get; set; }
        public bool HasTransactions { get; set; }
        
        // Account Summary
        public List<AccountSummary> Accounts { get; set; } = new();
        public int TotalAccounts { get; set; }
        
        // Recent Activity
        public List<RecentTransaction> RecentTransactions { get; set; } = new();
        
        // Quick Stats
        public int TransactionsThisMonth { get; set; }
        public int ActiveBudgets { get; set; }
        public DateTime LastLoginAt { get; set; }
        
        // Setup Progress
        public int SetupProgressPercentage => CalculateSetupProgress();
        
        private int CalculateSetupProgress()
        {
            int progress = 0;
            if (IsProfileComplete) progress += 25;
            if (HasAccounts) progress += 25;
            if (HasBudgets) progress += 25;
            if (HasTransactions) progress += 25;
            return progress;
        }
        
        public string GetCurrencySymbol()
        {
            return PreferredCurrency switch
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
    
    public class AccountSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public decimal CurrentBalance { get; set; }
        public Currency Currency { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class RecentTransaction
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateTime TransactionDate { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
} 