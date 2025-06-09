using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidCategoryName(string categoryName)
        {
            return !string.IsNullOrWhiteSpace(categoryName);
        }

        public static ValidationResult ValidateTransactionInput(object transactionType, string amountText, object category)
        {
            if (transactionType == null)
            {
                return new ValidationResult(false, "Válassza ki a tranzakció típusát!");
            }

            if (string.IsNullOrWhiteSpace(amountText))
            {
                return new ValidationResult(false, "Adja meg az összeget!");
            }

            if (!decimal.TryParse(amountText, out decimal amount) || amount <= 0)
            {
                return new ValidationResult(false, "Érvényes összeget adjon meg!");
            }

            if (category == null)
            {
                return new ValidationResult(false, "Válasszon ki egy kategóriát!");
            }

            return new ValidationResult(true, string.Empty);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public ValidationResult(bool isValid, string errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }
    }
}
