using System.ComponentModel.DataAnnotations;

namespace PetalOrSomething.Models
{
    public class FeedbackWithTransactionViewModel
    {
        public Feedback Feedback { get; set; }
        public TransactionOrder Transaction { get; set; }
        public TransactionCustomOrder CustomTransaction { get; set; }
    }
    public class FeedbackView
    {
        public IEnumerable<FeedbackWithTransactionViewModel> Feedbacks { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string CategoryFilter { get; set; }
        public string SearchFilter { get; set; }
        public int TotalCount { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        public int Rate { get; set; }
        public string FeedbackNote { get; set; }
        public int? TransactionId { get; set; } = null;
        public int? TransactionCustomId { get; set; } = null;
        public bool IsCustomTransaction { get; set; }
    }
}

