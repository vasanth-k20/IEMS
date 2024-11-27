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
    public class IncomeController : Controller
    {
        private readonly IEMSDbContext _context;

        public IncomeController(IEMSDbContext context)
        {
            _context = context;
        }

        // GET: Incomes
        public async Task<IActionResult> Index()
        {
            var totalIncome = await _context.Income.SumAsync(i => i.Amount);
            var totalExpenses = await _context.Expense.SumAsync(e => e.Amount);
            var remainingBalance = totalIncome - totalExpenses;

            // Calculate Credit Card specific income and expenses
            var totalCreditCardIncome = await _context.Income
                .Where(i => i.Account == "CreditCard")
                .SumAsync(i => i.Amount);

            var totalCreditCardExpenses = await _context.Expense
                .Where(e => e.Account == "CreditCard")
                .SumAsync(e => e.Amount);

            var creditCardRemainingBalance = totalCreditCardIncome - totalCreditCardExpenses;

            // Pass data to ViewData for the view
            ViewData["TotalIncome"] = totalIncome;
            ViewData["TotalExpenses"] = totalExpenses;
            ViewData["RemainingBalance"] = remainingBalance;
            ViewData["CreditCardIncome"] = totalCreditCardIncome;
            ViewData["CreditCardRemainingBalance"] = creditCardRemainingBalance;

            return View(await _context.Income.ToListAsync());
        }

        // GET: Incomes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var income = await _context.Income
                .FirstOrDefaultAsync(m => m.Id == id);
            if (income == null)
            {
                return NotFound();
            }

            return View(income);
        }

        // GET: Incomes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Incomes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Source,Amount,Date,Account")] Income income)
        {
            if (ModelState.IsValid)
            {
                _context.Add(income);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(income);
        }

        // GET: Incomes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var income = await _context.Income.FindAsync(id);
            if (income == null)
            {
                return NotFound();
            }
            return View(income);
        }

        // POST: Incomes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Source,Amount,Date,Account")] Income income)
        {
            if (id != income.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(income);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IncomeExists(income.Id))
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
            return View(income);
        }

        // GET: Incomes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var income = await _context.Income
                .FirstOrDefaultAsync(m => m.Id == id);
            if (income == null)
            {
                return NotFound();
            }

            return View(income);
        }

        // POST: Incomes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var income = await _context.Income.FindAsync(id);
            if (income != null)
            {
                _context.Income.Remove(income);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IncomeExists(int id)
        {
            return _context.Income.Any(e => e.Id == id);
        }

        // Helper methods for total income and remaining balance
        public async Task<decimal> GetTotalIncome()
        {
            return await _context.Income.SumAsync(i => i.Amount);
        }

        public async Task<decimal> GetRemainingBalance()
        {
            decimal totalIncome = await _context.Income.SumAsync(i => i.Amount);
            decimal totalExpenses = await _context.Expense.SumAsync(e => e.Amount);
            return totalIncome - totalExpenses;
        }

    }
}
