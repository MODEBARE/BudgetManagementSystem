using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetSystem.Models.ViewModels
{
    public class TransactionListViewModel
    {
        public List<TransactionListItem> Transactions { get; set; } = new();
        public TransactionFilters Filters { get; set; } = new();
        public PaginationInfo Pagination { get; set; } = new();
        public TransactionSummary Summary { get; set; } = new();
        
        // Filter options
        public List<SelectListItem> AccountOptions { get; set; } = new();
        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<SelectListItem> TypeOptions { get; set; } = new();
        
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
    
    public class TransactionListItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public bool HasReceipt { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class TransactionFilters
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }
        
        [Display(Name = "Transaction Type")]
        public TransactionType? Type { get; set; }
        
        [Display(Name = "Account")]
        public int? AccountId { get; set; }
        
        [Display(Name = "Category")]
        public string? Category { get; set; }
        
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }
        
        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }
        
        [Display(Name = "Minimum Amount")]
        public decimal? MinAmount { get; set; }
        
        [Display(Name = "Maximum Amount")]
        public decimal? MaxAmount { get; set; }
        
        [Display(Name = "Sort By")]
        public string SortBy { get; set; } = "TransactionDate";
        
        [Display(Name = "Sort Order")]
        public string SortOrder { get; set; } = "desc";
        
        public bool ShowRecurringOnly { get; set; } = false;
        public bool ShowWithReceiptsOnly { get; set; } = false;
    }
    
    public class PaginationInfo
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public int StartRecord => (CurrentPage - 1) * PageSize + 1;
        public int EndRecord => Math.Min(CurrentPage * PageSize, TotalCount);
    }
    
    public class TransactionSummary
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalTransfers { get; set; }
        public decimal NetAmount => TotalIncome - TotalExpenses;
        public int IncomeCount { get; set; }
        public int ExpenseCount { get; set; }
        public int TransferCount { get; set; }
        public int TotalCount => IncomeCount + ExpenseCount + TransferCount;
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
    }
} 