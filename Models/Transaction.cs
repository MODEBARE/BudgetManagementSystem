using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetSystem.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? Category { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(250)]
        public string? ReceiptPath { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }

        public bool IsRecurring { get; set; } = false;

        // Transfer-specific fields
        public int? DestinationAccountId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TransferFee { get; set; } = 0;

        // Audit trail fields
        public DateTime? LastModifiedAt { get; set; }
        
        [StringLength(450)]
        public string? LastModifiedBy { get; set; }
        
        [StringLength(1000)]
        public string? ModificationReason { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
        
        [StringLength(450)]
        public string? DeletedBy { get; set; }
        
        [StringLength(1000)]
        public string? DeletionReason { get; set; }

        // Foreign keys
        [Required]
        public int AccountId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual Account? Account { get; set; }
        public virtual Account? DestinationAccount { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }

    public enum TransactionType
    {
        Credit = 0,  // Money coming in (Income)
        Debit = 1,   // Money going out (Expense)
        Transfer = 2 // Money transfer between accounts
    }

    public static class IncomeCategories
    {
        public const string Salary = "Salary";
        public const string Freelance = "Freelance";
        public const string Investment = "Investment";
        public const string Business = "Business";
        public const string Bonus = "Bonus";
        public const string Gift = "Gift";
        public const string Rental = "Rental Income";
        public const string Dividend = "Dividend";
        public const string Interest = "Interest";
        public const string Other = "Other Income";

        public static List<string> GetAll()
        {
            return new List<string>
            {
                Salary, Freelance, Investment, Business, Bonus,
                Gift, Rental, Dividend, Interest, Other
            };
        }
    }

    public static class ExpenseCategories
    {
        public const string Food = "Food & Dining";
        public const string Transportation = "Transportation";
        public const string Utilities = "Utilities";
        public const string Housing = "Housing";
        public const string Healthcare = "Healthcare";
        public const string Entertainment = "Entertainment";
        public const string Shopping = "Shopping";
        public const string Education = "Education";
        public const string Insurance = "Insurance";
        public const string Savings = "Savings";
        public const string Other = "Other Expense";

        public static List<string> GetAll()
        {
            return new List<string>
            {
                Food, Transportation, Utilities, Housing, Healthcare,
                Entertainment, Shopping, Education, Insurance, Savings, Other
            };
        }
    }

    public static class TransferCategories
    {
        public const string AccountTransfer = "Account Transfer";
        public const string SavingsTransfer = "Savings Transfer";
        public const string InvestmentTransfer = "Investment Transfer";
        public const string LoanPayment = "Loan Payment";
        public const string CreditCardPayment = "Credit Card Payment";
        public const string Other = "Other Transfer";

        public static List<string> GetAll()
        {
            return new List<string>
            {
                AccountTransfer, SavingsTransfer, InvestmentTransfer,
                LoanPayment, CreditCardPayment, Other
            };
        }
    }
} 