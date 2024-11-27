using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IEMS.Data;
using IEMS.Models;

namespace IEMS.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly IEMSDbContext _context;

        public ExpenseController(IEMSDbContext context)
        {
            _context = context;
        }

        // GET: IEMS
        public async Task<IActionResult> Index()
        {
            var totalIncome = await _context.Income.SumAsync(i => i.Amount);
            var totalExpenses = await _context.Expense.SumAsync(e => e.Amount);
            var remainingBalance = totalIncome - totalExpenses;

            ViewData["TotalIncome"] = totalIncome;
            ViewData["TotalExpenses"] = totalExpenses;
            ViewData["RemainingBalance"] = remainingBalance;

            var expenses = await _context.Expense.ToListAsync();
            return View(expenses);
        }

        // GET: IEMS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // GET: IEMS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: IEMS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,ItemName,Amount,ExpenseDate,Category,CustomCategory,Account,Description,FileData")] Expense expense, IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                // Validate the uploaded file
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".txt" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("FileData", "Only PDF, JPG, PNG, and TXT files are allowed.");
                    return View(expense);
                }

                // Convert the uploaded file into a byte array
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    expense.FileData = memoryStream.ToArray();
                    expense.FileName = file.FileName;
                    expense.FileType = file.ContentType;
                }

                expense.FileName = file.FileName;
                expense.FileType = file.ContentType;

            }
            else
            {
                ModelState.AddModelError("FileData", "No file was uploaded.");
                return View(expense);
            }

            if (ModelState.IsValid)
            {
                // Calculate remaining balance for all accounts
                decimal remainingBalance = await GetRemainingBalance();

                // General warning for any expense that exceeds overall balance
                if (expense.Amount > remainingBalance)
                {
                    ModelState.AddModelError("", "Warning: This expense exceeds your available balance and will result in a negative balance!");
                }

                // Additional checks for CreditCard account expenses
                if (expense.Account == "CreditCard")
                {
                    // Calculate total CreditCard income and expenses
                    decimal totalCreditCardIncome = await _context.Income
                        .Where(i => i.Account == "CreditCard")
                        .SumAsync(i => i.Amount);

                    decimal totalCreditCardExpenses = await _context.Expense
                        .Where(e => e.Account == "CreditCard")
                        .SumAsync(e => e.Amount);

                    decimal creditCardRemainingBalance = totalCreditCardIncome - totalCreditCardExpenses;

                    // Prevent saving if the expense exceeds the CreditCard balance
                    if (expense.Amount > creditCardRemainingBalance)
                    {
                        ModelState.AddModelError("", "Error: This expense exceeds your CreditCard available balance and cannot be created.");
                        return View(expense); // Return to form for correction
                    }

                    // Retrieve the most recent CreditCard income date
                    var recentCreditCardIncomeDate = await _context.Income
                        .Where(i => i.Account == "CreditCard")
                        .OrderByDescending(i => i.Date)
                        .Select(i => i.Date)
                        .FirstOrDefaultAsync();

                    // Check if the expense date is more than one month after the recent income date
                    if (recentCreditCardIncomeDate != null && (expense.ExpenseDate - recentCreditCardIncomeDate).TotalDays > 30)
                    {
                        ModelState.AddModelError("", "Error: This expense date is more than one month after the latest CreditCard income date and cannot be created.");
                        return View(expense); // Return to form for correction
                    }
                }

                // If all validations pass, add and save the expense
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }


        // GET: IEMS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }
            return View(expense);
        }

        // POST: IEMS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ItemId,ItemName,Amount,ExpenseDate,Category,CustomCategory,Account,Description")] Expense expense, IFormFile? file)
        {
            if (id != expense.ItemId)
            {
                return NotFound();
            }

            // Retrieve the existing expense record
            var existingExpense = await _context.Expense.FirstOrDefaultAsync(e => e.ItemId == id);
            if (existingExpense == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update other fields
                    existingExpense.ItemName = expense.ItemName;
                    existingExpense.Amount = expense.Amount;
                    existingExpense.ExpenseDate = expense.ExpenseDate;
                    existingExpense.Category = expense.Category;
                    existingExpense.CustomCategory = expense.CustomCategory;
                    existingExpense.Account = expense.Account;
                    existingExpense.Description = expense.Description;

                    // Handle file upload
                    if (file != null && file.Length > 0)
                    {
                        // Validate the uploaded file
                        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".txt" };
                        var fileExtension = Path.GetExtension(file.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("FileData", "Only PDF, JPG, PNG, and TXT files are allowed.");
                            return View(expense);
                        }

                        // Replace the old file with the new one
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            existingExpense.FileData = memoryStream.ToArray();
                            existingExpense.FileName = file.FileName;
                            existingExpense.FileType = file.ContentType;
                        }
                    }

                    // Save changes to the database
                    _context.Update(existingExpense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpenseExists(expense.ItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(expense);
        }


        // GET: IEMS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expense
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: IEMS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expense.FindAsync(id);
            if (expense != null)
            {
                _context.Expense.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MonthlyReport()
        {
            // Fetch all expenses from the database
            var expenses = await _context.Expense.ToListAsync();

            // Group and aggregate on the client side
            var monthlyExpenses = expenses
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalAmount = g.Sum(e => e.Amount),
                    Expenses = g.ToList()
                })
                .OrderByDescending(g => g.Month)
                .ToList();

            return View(monthlyExpenses);
        }


        private bool ExpenseExists(int id)
        {
            return _context.Expense.Any(e => e.ItemId == id);
        }
        public async Task<decimal> GetTotalExpenses()
        {
            return await _context.Expense.SumAsync(e => e.Amount);
        }

        public async Task<decimal> GetRemainingBalance()
        {
            decimal totalIncome = await _context.Income.SumAsync(i => i.Amount);
            decimal totalExpenses = await _context.Expense.SumAsync(e => e.Amount);
            return totalIncome - totalExpenses;
        }
         
        public async Task<IActionResult> GetFile(int id)
        {
            var expense = await _context.Expense.FindAsync(id);
            if (expense == null || expense.FileData == null)
            {
                return NotFound("File not found.");
            }

            // Return the file content with appropriate content type and name
            return File(expense.FileData, expense.FileType ?? "application/octet-stream", expense.FileName ?? "uploaded_file");
        }

    }
}
