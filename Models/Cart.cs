using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetalOrSomething.Models
{
    public class CartItem
    {
        public CartItem(string model3DLink, int userId, string productName, int quantity)
        {
            Model3DLink = model3DLink;
            UserId = userId;
            Quantity = quantity;
            ProductName = productName;
        }

        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public string Model3DLink { get; set; }

        public double Price { get; set; } = 0;
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
