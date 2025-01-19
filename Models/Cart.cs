using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;

namespace PetalOrSomething.Models
{

    public class UnifiedCartItem
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice { get; set; }
        public bool IsFinished { get; set; }
        public bool Selected { get; set; } = false;

        public string Model3DLink { get; set; }
        public string Model2DLink { get; set; }
        public string Note { get; set; }
        public string CustomizationJson { get; set; } = "";

    }

    public class UnifiedCartViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserLocation { get; set; }
        public string UserPhoneNumber { get; set; }
        public List<UnifiedCartItem> Items { get; set; } = new List<UnifiedCartItem>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchFilter { get; set; }
        public int TotalCount { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public string WillPickUp { get; set; } = "Delivery";
        public string PaymentType { get; set; } = "Full";
        public int DownPaymentPercentage { get; set; } = 20;
    }


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
