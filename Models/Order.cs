using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetalOrSomething.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("FlowerInventory")]
        public int FlowerInventoryId { get; set; }

        public FlowerInventory FlowerInventory { get; set; }
        [Required]
        [Url]
        public string Design3DLink { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string Status { get; set; } = "Pending";
    }
}
