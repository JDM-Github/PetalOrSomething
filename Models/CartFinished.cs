using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetalOrSomething.Models
{
    public class OrderUpdateViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }

    public class LineItem
    {
        public string Currency { get; set; }
        public int Amount { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
    }

    public class CartFinished(int userId, int productId, int quantity, string note)
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; } = userId;
        public int ProductId { get; set; } = productId;

        [ForeignKey("ProductId")]
        public FlowerInventory Product { get; set; }

        public int Quantity { get; set; } = quantity;
        public double TotalPrice => Quantity * (Product?.Price ?? 0);
        public string Note { get; set; } = note;
        public bool Selected { get; set; } = false;
        public bool IsOrdered { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

