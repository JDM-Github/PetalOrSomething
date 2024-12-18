namespace PetalOrSomething.ViewModels
{
    public class CartItemWithUserViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductDescription { get; set; }
        public string Model3DLink { get; set; }
    }
}