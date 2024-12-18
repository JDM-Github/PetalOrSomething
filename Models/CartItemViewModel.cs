namespace PetalOrSomething.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductDescription { get; set; }
        public decimal TotalPrice => Quantity * ProductPrice;
    }
}
