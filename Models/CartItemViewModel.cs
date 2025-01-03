namespace PetalOrSomething.Models
{
    public class CartFinishedViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserLocation { get; set; }
        public string UserPhoneNumber { get; set; }
        public IEnumerable<CartFinished> Items { get; set; }
        public double TotalAmount => Items.Sum(item => item.TotalPrice);
        public string WillPickUp { get; set; } = "Delivery";
        public string PaymentType { get; set; } = "Full";
        public int DownPaymentPercentage { get; set; } = 20;

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchFilter { get; set; }
        public int TotalCount { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}
