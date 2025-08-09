using System.ComponentModel.DataAnnotations;
using BudgetSystem.Models;

namespace BudgetSystem.ViewModels
{
    public class AccountSetupViewModel
    {
        public List<AccountCreationModel> Accounts { get; set; } = new List<AccountCreationModel>();

        public AccountSetupViewModel()
        {
            // Pre-populate with common account types
            Accounts.Add(new AccountCreationModel 
            { 
                Name = "Primary Checking", 
                AccountType = AccountType.Checking,
                IsSelected = true 
            });
            Accounts.Add(new AccountCreationModel 
            { 
                Name = "Savings Account", 
                AccountType = AccountType.Savings,
                IsSelected = false 
            });
            Accounts.Add(new AccountCreationModel 
            { 
                Name = "Credit Card", 
                AccountType = AccountType.CreditCard,
                IsSelected = false 
            });
        }
    }

    public class AccountCreationModel
    {
        [Display(Name = "Create this account")]
        public bool IsSelected { get; set; }

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
    }

    public class CreateAccountViewModel
    {
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
    }
} 