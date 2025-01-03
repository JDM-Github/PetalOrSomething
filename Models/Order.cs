using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetalOrSomething.Models
{
    public class OrderViewModel
    {
        public IEnumerable<Order> Orders { get; set; }
        public string SearchFilter { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public string SortFilter { get; set; }
        public string StatusFilter { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int TotalCount { get; set; }
    }

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
        public double TotalPrice { get; set; }
        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [ForeignKey("UserId")]
        public Account User { get; set; }

        [Required]
        [ForeignKey("ProductId")]
        public FlowerInventory Product { get; set; }
    }
}
