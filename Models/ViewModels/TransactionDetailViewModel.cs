namespace BudgetSystem.Models.ViewModels
{
    public class TransactionDetailViewModel
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
        public string? ReceiptPath { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Account information
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public Currency AccountCurrency { get; set; }
        public decimal AccountCurrentBalance { get; set; }
        
        // User information
        public Currency UserCurrency { get; set; }
        
        // Computed properties
        public bool HasReceipt => !string.IsNullOrEmpty(ReceiptPath);
        public string TypeDisplay => Type == TransactionType.Credit ? "Income" : "Expense";
        public string TypeIcon => Type == TransactionType.Credit ? "fa-plus-circle" : "fa-minus-circle";
        public string TypeColor => Type == TransactionType.Credit ? "success" : "danger";
        public string AmountDisplay => $"{(Type == TransactionType.Credit ? "+" : "-")}{GetCurrencySymbol()}{Amount:N2}";
        
        public string? ReceiptFileName => HasReceipt ? Path.GetFileName(ReceiptPath) : null;
        public string? ReceiptFileExtension => HasReceipt ? Path.GetExtension(ReceiptPath)?.ToLowerInvariant() : null;
        
        public string GetReceiptFileIcon()
        {
            if (!HasReceipt) return "fa-file";
            
            return ReceiptFileExtension switch
            {
                ".pdf" => "fa-file-pdf",
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".tif" or ".webp" => "fa-file-image",
                ".doc" or ".docx" => "fa-file-word",
                ".txt" => "fa-file-alt",
                _ => "fa-file"
            };
        }
        
        public bool IsReceiptImage()
        {
            if (!HasReceipt) return false;
            
            return ReceiptFileExtension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".tif" or ".webp" => true,
                _ => false
            };
        }
        
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
        
        public string GetAccountIcon()
        {
            return AccountType switch
            {
                AccountType.Checking => "fa-university",
                AccountType.Savings => "fa-piggy-bank",
                AccountType.CreditCard => "fa-credit-card",
                AccountType.Investment => "fa-chart-line",
                AccountType.Cash => "fa-money-bill-wave",
                AccountType.Loan => "fa-hand-holding-usd",
                _ => "fa-wallet"
            };
        }
        
        public string GetCategoryIcon()
        {
            return Category switch
            {
                // Income categories
                "Salary" => "fa-briefcase",
                "Freelance" => "fa-laptop",
                "Investment" => "fa-chart-line",
                "Business" => "fa-building",
                "Bonus" => "fa-gift",
                "Gift" => "fa-heart",
                "Rental Income" => "fa-home",
                "Dividend" => "fa-percentage",
                "Interest" => "fa-percent",
                
                // Expense categories
                "Food & Dining" => "fa-utensils",
                "Transportation" => "fa-car",
                "Utilities" => "fa-bolt",
                "Housing" => "fa-home",
                "Healthcare" => "fa-heartbeat",
                "Entertainment" => "fa-film",
                "Shopping" => "fa-shopping-bag",
                "Education" => "fa-graduation-cap",
                "Insurance" => "fa-shield-alt",
                "Savings" => "fa-piggy-bank",
                
                _ => "fa-tag"
            };
        }
    }
} 