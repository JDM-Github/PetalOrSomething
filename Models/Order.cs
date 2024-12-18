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
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public Account User { get; set; }

        [ForeignKey("ProductId")]
        public FlowerInventory Product { get; set; }
    }
}
