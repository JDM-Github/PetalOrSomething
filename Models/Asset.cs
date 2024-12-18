using System.ComponentModel.DataAnnotations;

namespace PetalOrSomething.Models
{
    public class Asset
    {
        [Key]
        public int Id { get; set; }

        public string Model3DLink { get; set; }

        public decimal Price { get; set; }
    }
}
