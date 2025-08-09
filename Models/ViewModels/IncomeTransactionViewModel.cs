using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetSystem.Models.ViewModels
{
    public class IncomeTransactionViewModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Transaction Date")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category")]
        [Display(Name = "Income Category")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an account")]
        [Display(Name = "Receiving Account")]
        public int AccountId { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes (Optional)")]
        public string? Notes { get; set; }

        [StringLength(100, ErrorMessage = "Reference cannot exceed 100 characters")]
        [Display(Name = "Reference/Invoice Number (Optional)")]
        public string? Reference { get; set; }

        [Display(Name = "Recurring Income")]
        public bool IsRecurring { get; set; } = false;

        [Display(Name = "Receipt Attachment")]
        public IFormFile? ReceiptFile { get; set; }

        // For dropdowns
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> Accounts { get; set; } = new();
        
        // For display
        public string? SelectedAccountName { get; set; }
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
} 