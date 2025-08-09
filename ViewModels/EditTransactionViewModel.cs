using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using BudgetSystem.Models;

namespace BudgetSystem.ViewModels
{
    public class EditTransactionViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999,999.99")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [StringLength(100, ErrorMessage = "Reference cannot exceed 100 characters.")]
        [Display(Name = "Reference")]
        public string? Reference { get; set; }

        [Display(Name = "Recurring Transaction")]
        public bool IsRecurring { get; set; }

        [Required]
        [Display(Name = "Account")]
        public int AccountId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Please provide a reason for this modification.")]
        [Display(Name = "Reason for Modification")]
        public string ModificationReason { get; set; } = string.Empty;

        // Receipt handling
        [Display(Name = "New Receipt")]
        public IFormFile? NewReceiptFile { get; set; }

        [Display(Name = "Remove Current Receipt")]
        public bool RemoveCurrentReceipt { get; set; }

        // Read-only properties for display
        public TransactionType Type { get; set; }
        public string? CurrentReceiptPath { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal OriginalAmount { get; set; }
        public Currency UserCurrency { get; set; }

        // Dropdown lists
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Accounts { get; set; } = new();

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
        public bool HasCurrentReceipt => !string.IsNullOrEmpty(CurrentReceiptPath);
        public string? CurrentReceiptFileName => HasCurrentReceipt ? Path.GetFileName(CurrentReceiptPath) : null;
    }
} 