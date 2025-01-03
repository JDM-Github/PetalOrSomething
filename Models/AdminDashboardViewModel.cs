namespace PetalOrSomething.Models
{
    public class PopularProductViewModel
    {
        public string Name { get; set; }
        public int TotalQuantity { get; set; }
        public double TotalSales { get; set; }
    }

    public class TransactionViewModel
    {
        public int TransactionId { get; set; }
        public TransactionOrder Order { get; set; }
        public double Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    class AdminDashboardViewModel
    {
        public int TotalFlowers { get; set; }
        public int TotalStock { get; set; }
        public int TotalExpiredStock { get; set; }
        public int ExpiredPercentage { get; set; }
        public int NotExpiredPercentage { get; set; }
        public double TotalRevenue { get; set; }

        public List<PopularProductViewModel> MostPopularProducts { get; set; }
        public List<TransactionViewModel> RecentTransactions { get; set; }

    }
}
