using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BudgetSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        // For SME features later
        public string? CompanyName { get; set; }
        
        public UserType UserType { get; set; } = UserType.Individual;

        // Profile Setup Fields (Story 1.2)
        // Note: PhoneNumber is inherited from IdentityUser
        
        [StringLength(100)]
        public string? Address { get; set; }
        
        [StringLength(200)]
        public string? City { get; set; }
        
        [StringLength(50)]
        public string? State { get; set; }
        
        [StringLength(20)]
        public string? PostalCode { get; set; }
        
        [StringLength(50)]
        public string? Country { get; set; }
        
        public Currency PreferredCurrency { get; set; } = Currency.USD;
        
        public bool IsProfileComplete { get; set; } = false;

        // Navigation properties
        public virtual ICollection<Budget>? Budgets { get; set; }
        public virtual ICollection<Expense>? Expenses { get; set; }
        public virtual ICollection<Income>? Incomes { get; set; }
        public virtual ICollection<Account>? Accounts { get; set; }
    }

    public enum UserType
    {
        Individual = 0,
        SME = 1
    }

    public enum Currency
    {
        USD = 0,  // US Dollar
        EUR = 1,  // Euro
        GBP = 2,  // British Pound
        NGN = 3,  // Nigerian Naira
        CAD = 4,  // Canadian Dollar
        AUD = 5,  // Australian Dollar
        JPY = 6,  // Japanese Yen
        CHF = 7,  // Swiss Franc
        CNY = 8,  // Chinese Yuan
        INR = 9   // Indian Rupee
    }
} 