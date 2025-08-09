using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetSystem.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        public AccountType AccountType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InitialBalance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        public Currency Currency { get; set; } = Currency.USD;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation property
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }
    }

    public enum AccountType
    {
        Checking = 0,
        Savings = 1,
        CreditCard = 2,
        Investment = 3,
        Cash = 4,
        Loan = 5,
        Other = 6
    }
} 