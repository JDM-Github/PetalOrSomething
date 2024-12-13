using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetalOrSomething.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public double TotalPrice => Price * Quantity;

        [ForeignKey("Cart")]
        public int CartId { get; set; }
        public Cart Cart { get; set; }
    }

    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
