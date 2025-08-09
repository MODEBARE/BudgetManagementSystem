using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using BudgetSystem.Models;

namespace BudgetSystem.ViewModels
{
    public class TransferViewModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Transfer Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Source account is required")]
        [Display(Name = "From Account")]
        public int SourceAccountId { get; set; }

        [Required(ErrorMessage = "Destination account is required")]
        [Display(Name = "To Account")]
        public int DestinationAccountId { get; set; }

        [Required(ErrorMessage = "Transfer date is required")]
        [Display(Name = "Transfer Date")]
        [DataType(DataType.Date)]
        public DateTime TransferDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Reference cannot exceed 100 characters")]
        [Display(Name = "Reference/Transaction ID")]
        public string? Reference { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; } = Models.TransferCategories.AccountTransfer;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Transfer fee cannot be negative")]
        [Display(Name = "Transfer Fee")]
        public decimal TransferFee { get; set; } = 0;

        // Navigation properties for form dropdowns
        public List<SelectListItem> SourceAccounts { get; set; } = new();
        public List<SelectListItem> DestinationAccounts { get; set; } = new();
        public List<SelectListItem> TransferCategories { get; set; } = new();

        // Account information for display
        public Account? SourceAccount { get; set; }
        public Account? DestinationAccount { get; set; }

        // Calculated properties
        public decimal TotalDeduction => Amount + TransferFee;
        public bool HasSufficientFunds { get; set; } = true;
        public decimal SourceAccountBalance { get; set; }
        public decimal DestinationAccountBalance { get; set; }
    }

    public class TransferConfirmationViewModel
    {
        public TransferViewModel Transfer { get; set; } = new();
        public string SourceAccountName { get; set; } = string.Empty;
        public string DestinationAccountName { get; set; } = string.Empty;
        public decimal SourceAccountCurrentBalance { get; set; }
        public decimal DestinationAccountCurrentBalance { get; set; }
        public decimal SourceAccountNewBalance { get; set; }
        public decimal DestinationAccountNewBalance { get; set; }
    }
} 