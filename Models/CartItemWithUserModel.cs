namespace PetalOrSomething.Models
{
    public class CartItemViewUserModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserLocation { get; set; }
        public string UserPhoneNumber { get; set; }
        public IEnumerable<CartItem> Items { get; set; }

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

    public class CartItemWithUserViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Model3DLink { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double ProductPrice { get; set; }
        public string Note { get; set; }
        public string Customization { get; set; }
    }
}