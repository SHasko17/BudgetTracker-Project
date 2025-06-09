using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using BudgetTracker.Models;

namespace BudgetTracker.Services
{
    public class DataService
    {
        public ObservableCollection<Transaction> GetTransactions()
        {
            return new ObservableCollection<Transaction>();
        }

        public ObservableCollection<string> GetCategories()
        {
            return new ObservableCollection<string>
            {
                "Étel", "Szórakozás", "Közlekedés", "Lakhatás", "Egészség", "Ruházat", "Egyéb"
            };
        }

        public BudgetSummary CalculateSummary(ObservableCollection<Transaction> transactions)
        {
            decimal totalIncome = transactions.Where(t => t.Type == "Bevétel").Sum(t => t.Amount);
            decimal totalExpense = transactions.Where(t => t.Type == "Kiadás").Sum(t => t.Amount);

            return new BudgetSummary
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = totalIncome - totalExpense
            };
        }
    }

    public class BudgetSummary
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }
    }
}
