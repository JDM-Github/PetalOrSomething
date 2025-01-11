using System.ComponentModel.DataAnnotations;

namespace PetalOrSomething.Models
{

    public class ProductEditViewModel
    {
        public string SearchQuery { get; set; }
        public string CategoryFilter { get; set; }
        public List<Asset> Assets { get; set; }
    }

    public class AssetViewModel
    {
        public List<Asset> Assets { get; set; }
        public string SearchFilter { get; set; }
        public string SortFilter { get; set; }
        public string PriceRangeFilter { get; set; }
        public string FilterAvailable { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalCount { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
    public class Asset
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Model3DLink { get; set; } = "";
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
