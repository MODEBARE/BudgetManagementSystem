using System.ComponentModel.DataAnnotations;
using BudgetSystem.Models;

namespace BudgetSystem.ViewModels
{
    public class DeleteTransactionViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? Reference { get; set; }
        public bool IsRecurring { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public decimal AccountCurrentBalance { get; set; }
        public Currency UserCurrency { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasReceipt { get; set; }
        public bool CanDelete { get; set; }
        public string? DeleteRestrictionReason { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Please provide a reason for deletion.")]
        [Display(Name = "Reason for Deletion")]
        public string DeletionReason { get; set; } = string.Empty;

        // Helper methods
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

        public string TypeDisplay => Type == TransactionType.Credit ? "Income" : "Expense";
        public string TypeColor => Type == TransactionType.Credit ? "success" : "danger";
        public string TypeIcon => Type == TransactionType.Credit ? "fa-plus-circle" : "fa-minus-circle";
        public string AmountDisplay => $"{(Type == TransactionType.Credit ? "+" : "-")}{GetCurrencySymbol()}{Amount:N2}";
        public decimal NewAccountBalance => Type == TransactionType.Credit 
            ? AccountCurrentBalance - Amount 
            : AccountCurrentBalance + Amount;
    }
} 