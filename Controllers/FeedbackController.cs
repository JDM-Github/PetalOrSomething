using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetalOrSomething.Data;
using PetalOrSomething.Models;
using System.Collections.Generic;
using System.Linq;

namespace PetalOrSomething.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Feedback>> GetFeedbacks()
        {
            return _context.Feedbacks.ToList();
        }

        public ActionResult<Feedback> GetFeedback(int id)
        {
            var feedback = _context.Feedbacks.Find(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return feedback;
        }

        [HttpPost]
        public ActionResult<Feedback> PostFeedback(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetFeedback), new { id = feedback.Id }, feedback);
        }

        public IActionResult PutFeedback(int id, Feedback feedback)
        {
            if (id != feedback.Id)
            {
                return BadRequest();
            }

            _context.Entry(feedback).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();

            return NoContent();
        }

        public IActionResult DeleteFeedback(int id)
        {
            var feedback = _context.Feedbacks.Find(id);
            if (feedback == null)
            {
                return NotFound();
            }

            _context.Feedbacks.Remove(feedback);
            _context.SaveChanges();

            return NoContent();
        }

        public async Task<IActionResult> Index(int page = 1, string category = "All", string search = "")
        {
            int pageSize = 10;
            var query = _context.Feedbacks.AsQueryable();
            if (category != "All")
            {
                if (category == "nOrd")
                {
                    query = query.Where(f => f.IsCustomTransaction == false);
                }
                else if (category == "cOrd")
                {
                    query = query.Where(f => f.IsCustomTransaction == true);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.FeedbackNote.Contains(search));
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var feedbacks = await (
                from feedback in _context.Feedbacks
                join transaction in _context.TransactionOrders
                    on feedback.TransactionId equals transaction.Id into transactions
                from transaction in transactions.DefaultIfEmpty()
                join customTransaction in _context.TransactionCustomOrders
                    on feedback.TransactionCustomId equals customTransaction.Id into customTransactions
                from customTransaction in customTransactions.DefaultIfEmpty() 
                select new
                {
                    Feedback = feedback,
                    Transaction = transaction,
                    CustomTransaction = customTransaction
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mappedFeedbacks = feedbacks.Select(f => new FeedbackWithTransactionViewModel
            {
                Feedback = f.Feedback,
                Transaction = f.Transaction,
                CustomTransaction = f.CustomTransaction
            }).ToList();

            var totalCount = await query.CountAsync();
            var startIndex = (page - 1) * pageSize + 1;
            var endIndex = page * pageSize > totalCount ? totalCount : page * pageSize;

            var model = new FeedbackView
            {
                Feedbacks = mappedFeedbacks,
                CurrentPage = page,
                TotalPages = totalPages,
                CategoryFilter = category,
                SearchFilter = search,
                TotalCount = totalCount,
                StartIndex = startIndex,
                EndIndex = endIndex,
            };

            return View(model);
        }
    }
}
