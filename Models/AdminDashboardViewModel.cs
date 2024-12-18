namespace PetalOrSomething.Models
{
    class AdminDashboardViewModel
    {
        public int TotalFlowers { get; set; }
        public int TotalStock { get; set; }
        public int TotalExpiredStock { get; set; }
        public int ExpiredPercentage { get; set; }
        public int NotExpiredPercentage { get; set; }
    }
}
