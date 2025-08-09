using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetSystem.Models
{
    public class Income
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime IncomeDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Source { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Property
        public virtual ApplicationUser User { get; set; } = null!;
    }
} 