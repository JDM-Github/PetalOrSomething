public class Order
{
    public int Id { get; set; }
    public List<int> CartItemIds { get; set; } = new List<int>();
    public DateTime OrderDate { get; set; } = DateTime.Now;
}
