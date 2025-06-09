using BudgetTracker.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace BudgetTracker.Services
{
    public class ExcelService
    {
        public ExcelService()
        {
        }

        public void ExportToExcel(List<Transaction> transactions, string filePath)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Tranzakciók");

            // Fejlécek
            worksheet.Cells[1, 1].Value = "Dátum";
            worksheet.Cells[1, 2].Value = "Típus";
            worksheet.Cells[1, 3].Value = "Összeg";
            worksheet.Cells[1, 4].Value = "Kategória";
            worksheet.Cells[1, 5].Value = "Leírás";

            // Adatok
            for (int i = 0; i < transactions.Count; i++)
            {
                var transaction = transactions[i];
                worksheet.Cells[i + 2, 1].Value = transaction.Date.ToString("yyyy.MM.dd");
                worksheet.Cells[i + 2, 2].Value = transaction.Type;
                worksheet.Cells[i + 2, 3].Value = transaction.Amount;
                worksheet.Cells[i + 2, 4].Value = transaction.Category;
                worksheet.Cells[i + 2, 5].Value = transaction.Description;
            }

            // Összesítő
            var summaryRow = transactions.Count + 4;
            worksheet.Cells[summaryRow, 1].Value = "Összes bevétel:";
            worksheet.Cells[summaryRow, 2].Value = transactions.Where(t => t.Type == "Bevétel").Sum(t => t.Amount);

            worksheet.Cells[summaryRow + 1, 1].Value = "Összes kiadás:";
            worksheet.Cells[summaryRow + 1, 2].Value = transactions.Where(t => t.Type == "Kiadás").Sum(t => t.Amount);

            worksheet.Cells[summaryRow + 2, 1].Value = "Egyenleg:";
            worksheet.Cells[summaryRow + 2, 2].Value = transactions.Where(t => t.Type == "Bevétel").Sum(t => t.Amount)
                                                     - transactions.Where(t => t.Type == "Kiadás").Sum(t => t.Amount);

            worksheet.Cells.AutoFitColumns();

            var fileInfo = new FileInfo(filePath);
            package.SaveAs(fileInfo);
        }

        public ImportResult ImportFromExcel(string filePath)
        {
            var result = new ImportResult();

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var dateText = worksheet.Cells[row, 1].Value?.ToString();
                    var type = worksheet.Cells[row, 2].Value?.ToString();
                    var amountText = worksheet.Cells[row, 3].Value?.ToString();
                    var category = worksheet.Cells[row, 4].Value?.ToString();
                    var description = worksheet.Cells[row, 5].Value?.ToString() ?? "";

                    if (string.IsNullOrEmpty(dateText) || string.IsNullOrEmpty(type) ||
                        string.IsNullOrEmpty(amountText) || string.IsNullOrEmpty(category))
                        continue;

                    var transaction = new Transaction
                    {
                        Date = DateTime.TryParse(dateText, out DateTime date) ? date : DateTime.Now,
                        Type = type,
                        Amount = decimal.TryParse(amountText, out decimal amount) ? amount : 0,
                        Category = category,
                        Description = description
                    };

                    result.Transactions.Add(transaction);

                    if (!result.NewCategories.Contains(category))
                        result.NewCategories.Add(category);
                }
                catch
                {
                    continue;
                }
            }

            return result;
        }
    }

    public class ImportResult
    {
        public List<Transaction> Transactions { get; set; } = new();
        public List<string> NewCategories { get; set; } = new();
    }
}
