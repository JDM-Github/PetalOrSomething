using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;

namespace PetalOrSomething.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public Account User { get; set; }

        public string Model3DLink { get; set; }

        public string CustomizationJson { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }
        public string Note { get; set; }
        public bool Selected { get; set; } = false;
        public bool IsOrdered { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CartItem() { }

        public CartItem(int userId, string customization, string productName, int quantity, string note)
        {
            UserId = userId;
            CustomizationJson = customization;
            ProductName = productName;
            Quantity = quantity;
            Note = note;
        }
    }

}
