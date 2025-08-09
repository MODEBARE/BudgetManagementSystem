using System.ComponentModel.DataAnnotations;
using BudgetSystem.Models;

namespace BudgetSystem.ViewModels
{
    public class MyAccountsViewModel
    {
        public List<AccountSummary> Accounts { get; set; } = new();
        public Currency UserCurrency { get; set; }
        public decimal TotalBalance => Accounts.Where(a => a.IsActive).Sum(a => a.CurrentBalance);
        public int ActiveAccountsCount => Accounts.Count(a => a.IsActive);
        public int InactiveAccountsCount => Accounts.Count(a => !a.IsActive);

        public string GetCurrencySymbol()
        {
            return UserCurrency switch
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
        public string? Description { get; set; }
        public AccountType AccountType { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal InitialBalance { get; set; }
        public Currency Currency { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TransactionCount { get; set; }

        public string GetAccountTypeIcon()
        {
            return AccountType switch
            {
                AccountType.Checking => "fas fa-university",
                AccountType.Savings => "fas fa-piggy-bank",
                AccountType.CreditCard => "fas fa-credit-card",
                AccountType.Investment => "fas fa-chart-line",
                AccountType.Cash => "fas fa-money-bill-wave",
                AccountType.Loan => "fas fa-hand-holding-usd",
                _ => "fas fa-wallet"
            };
        }

        public string GetAccountTypeColor()
        {
            return AccountType switch
            {
                AccountType.Checking => "primary",
                AccountType.Savings => "success",
                AccountType.CreditCard => "warning",
                AccountType.Investment => "info",
                AccountType.Cash => "secondary",
                AccountType.Loan => "danger",
                _ => "light"
            };
        }

        public string GetBalanceChange()
        {
            var change = CurrentBalance - InitialBalance;
            return change >= 0 ? $"+{change:N2}" : $"{change:N2}";
        }

        public string GetBalanceChangeClass()
        {
            var change = CurrentBalance - InitialBalance;
            return change >= 0 ? "text-success" : "text-danger";
        }
    }

    public class EditAccountViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Account name is required")]
        [StringLength(100, ErrorMessage = "Account name cannot exceed 100 characters")]
        [Display(Name = "Account Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description (Optional)")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select an account type")]
        [Display(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        [Display(Name = "Initial Balance")]
        [Range(0, double.MaxValue, ErrorMessage = "Initial balance cannot be negative")]
        public decimal InitialBalance { get; set; } = 0;

        [Required(ErrorMessage = "Please select a currency")]
        [Display(Name = "Currency")]
        public Currency Currency { get; set; } = Currency.USD;

        [Display(Name = "Account Status")]
        public bool IsActive { get; set; } = true;
    }

    public class AccountDetailsViewModel
    {
        public Account Account { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();
        public AccountTransactionStats TransactionStats { get; set; } = new();
        public Currency UserCurrency { get; set; }

        public string GetCurrencySymbol()
        {
            return UserCurrency switch
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

    public class AccountTransactionStats
    {
        public int TotalTransactions { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalTransfersIn { get; set; }
        public decimal TotalTransfersOut { get; set; }
        public decimal NetIncome => TotalIncome - TotalExpenses;
        public decimal NetTransfers => TotalTransfersIn - TotalTransfersOut;
        public DateTime? LastTransactionDate { get; set; }
        public DateTime? FirstTransactionDate { get; set; }
    }

    public class DeleteAccountViewModel
    {
        public Account Account { get; set; } = new();
        public bool HasTransactions { get; set; }
        public int TransactionCount { get; set; }
    }
} 