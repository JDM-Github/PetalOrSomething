using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PetalOrSomething.Models
{
    public class FlowerStock
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; set; }

        [ForeignKey("FlowerInventory")]
        public int FlowerInventoryId { get; set; }
        public FlowerInventory FlowerInventory { get; set; }
    }

    public class FlowerInventory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Model3DLink { get; set; } = "";
        public string Model2DLink { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Rating { get; set; } = 4.5M;
        public decimal Price { get; set; }
        public List<FlowerStock> Stocks { get; set; } = new List<FlowerStock>();

        [NotMapped]
        public int TotalStock => Stocks.Sum(stock => stock.Quantity);

        [NotMapped]
        public int TotalStockOfNotExpired => Stocks
            .Where(stock => stock.ExpiryDate > DateTime.Now)
            .Sum(stock => stock.Quantity);

        public void RemoveStock(int quantity)
        {
            if (TotalStockOfNotExpired == 0)
            {
                throw new InvalidOperationException("No available stock to remove.");
            }

            var stocksToReduce = Stocks
                .Where(stock => stock.ExpiryDate > DateTime.Now)
                .OrderBy(stock => stock.ExpiryDate);

            foreach (var stock in stocksToReduce)
            {
                if (quantity <= 0) break;

                if (stock.Quantity <= quantity)
                {
                    quantity -= stock.Quantity;
                    stock.Quantity = 0;
                }
                else
                {
                    stock.Quantity -= quantity;
                    quantity = 0;
                }
            }
        }
        public void UpdateExpiredStocks()
        {
            Stocks.RemoveAll(stock => stock.Quantity > 0 && stock.ExpiryDate <= DateTime.Now);
        }
    }
}