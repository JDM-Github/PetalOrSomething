

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetalOrSomething.Models
{
    public class TransactionCustomOrder
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OrderId { get; set; }

        public List<CartItem> Products { get; set; }

        public string ReferenceNumber { get; set; }
        public string TransactionId { get; set; }
        public string PaymentLink { get; set; } = "";

        public double TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "-----";
        public string Status { get; set; }
        public string OrderStatus { get; set; } = "-----";


        public double ShippingFee { get; set; } = 25;
        public bool WillPickUp { get; set; } = false;
        public double DownPayment { get; set; } = 0;
        public double RemainingBalance { get; set; } = 0;

        public string FeedBack { get; set; } = "";
        public bool IsFeedback { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpirationTime { get; set; }
    }



    public class TransactionOrderAdmin
    {
        public TransactionOrder Order { get; set; }
        public string UserEmail { get; set; }
    }

    public class TransactionOrderAdminView
    {
        public IEnumerable<TransactionOrderAdmin> Orders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchFilter { get; set; }
        public string StatusFilter { get; set; }
        public string OrderStatusFilter { get; set; }
        public int TotalCount { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    public class TransactionCustomOrderAdmin
    {
        public TransactionCustomOrder Order { get; set; }
        public string UserEmail { get; set; }
    }

    public class TransactionCustomOrderAdminView
    {
        public IEnumerable<TransactionCustomOrderAdmin> Orders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchFilter { get; set; }
        public string StatusFilter { get; set; }
        public string OrderStatusFilter { get; set; }
        public int TotalCount { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    public class TransactionOrderView
    {
        public IEnumerable<TransactionOrder> Orders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchFilter { get; set; }
        public string StatusFilter { get; set; }
        public string OrderStatusFilter { get; set; }
        public int TotalCount { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
    public class TransactionCustomOrderView
    {
        public IEnumerable<TransactionCustomOrder> Orders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchFilter { get; set; }
        public string StatusFilter { get; set; }
        public string OrderStatusFilter { get; set; }
        public int TotalCount { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    public class TransactionOrder
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OrderId { get; set; }
        public List<CartFinished> Products { get; set; }

        public string ReferenceNumber { get; set; }
        public string TransactionId { get; set; }
        public string PaymentLink { get; set; } = "";

        public double TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "-----";
        public string Status { get; set; }
        public string OrderStatus { get; set; } = "-----";


        public double ShippingFee { get; set; } = 25;
        public bool WillPickUp { get; set; } = false;
        public double DownPayment { get; set; } = 0;
        public double RemainingBalance { get; set; } = 0;

        public string FeedBack { get; set; } = "";
        public bool IsFeedback { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpirationTime { get; set; }
    }
}
