namespace PetalOrSomething.Models
{
    public class InventoryViewModel
    {
        public IEnumerable<FlowerInventory> Inventories { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string CategoryFilter { get; set; }
        public string SearchFilter { get; set; }
        public int TotalCount { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}