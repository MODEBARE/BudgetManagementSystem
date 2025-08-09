using System.ComponentModel.DataAnnotations;
using BudgetSystem.Models;

namespace BudgetSystem.ViewModels
{
    public class ProfileSetupViewModel
    {
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(100, ErrorMessage = "Address cannot exceed 100 characters")]
        [Display(Name = "Street Address")]
        public string? Address { get; set; }

        [StringLength(200, ErrorMessage = "City cannot exceed 200 characters")]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(50, ErrorMessage = "State/Province cannot exceed 50 characters")]
        [Display(Name = "State/Province")]
        public string? State { get; set; }

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        [Display(Name = "Postal/Zip Code")]
        public string? PostalCode { get; set; }

        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Required(ErrorMessage = "Please select a preferred currency")]
        [Display(Name = "Preferred Currency")]
        public Currency PreferredCurrency { get; set; } = Currency.USD;

        // Financial Preferences
        [Display(Name = "Monthly Income (Optional)")]
        public decimal? MonthlyIncome { get; set; }

        [Display(Name = "Financial Goals")]
        public string? FinancialGoals { get; set; }

        [Display(Name = "Budget Alert Preferences")]
        public bool EnableBudgetAlerts { get; set; } = true;

        [Display(Name = "Email Notifications")]
        public bool EnableEmailNotifications { get; set; } = true;
    }
} 