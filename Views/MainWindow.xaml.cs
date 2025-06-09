using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using BudgetTracker.Models;
using BudgetTracker.Services;
using BudgetTracker.Helpers;

namespace BudgetTracker.Views
{
    public partial class MainWindow : Window
    {
        private readonly DataService _dataService;
        private readonly ExcelService _excelService;
        private ObservableCollection<Transaction> transactions;
        private ObservableCollection<string> categories;

        public MainWindow()
        {
            InitializeComponent();
            _dataService = new DataService();
            _excelService = new ExcelService();
            InitializeData();
            SetupUI();
        }

        private void InitializeData()
        {
            transactions = _dataService.GetTransactions();
            categories = _dataService.GetCategories();
            transactions.CollectionChanged += (s, e) => UpdateSummary();
        }

        private void SetupUI()
        {
            TransactionsDataGrid.ItemsSource = transactions;
            CategoryComboBox.ItemsSource = categories;
            TransactionTypeComboBox.SelectedIndex = 0;

            if (categories.Count > 0)
                CategoryComboBox.SelectedIndex = 0;

            UpdateSummary();
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string newCategory = NewCategoryTextBox.Text.Trim();

            if (!ValidationHelper.IsValidCategoryName(newCategory))
            {
                MessageBox.Show("Adja meg a kategória nevét!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (categories.Contains(newCategory))
            {
                MessageBox.Show("Ez a kategória már létezik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            categories.Add(newCategory);
            NewCategoryTextBox.Clear();
            MessageBox.Show("Kategória sikeresen hozzáadva!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Válasszon ki egy kategóriát a törléshez!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedCategory = CategoryComboBox.SelectedItem.ToString();

            var result = MessageBox.Show($"Biztosan törölni szeretné a '{selectedCategory}' kategóriát?",
                                       "Megerősítés", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                categories.Remove(selectedCategory);
                MessageBox.Show("Kategória sikeresen törölve!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            var validationResult = ValidationHelper.ValidateTransactionInput(
                TransactionTypeComboBox.SelectedItem,
                AmountTextBox.Text,
                CategoryComboBox.SelectedItem);

            if (!validationResult.IsValid)
            {
                MessageBox.Show(validationResult.ErrorMessage, "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var transaction = new Transaction
            {
                Date = DateTime.Now,
                Type = ((ComboBoxItem)TransactionTypeComboBox.SelectedItem).Content.ToString(),
                Amount = decimal.Parse(AmountTextBox.Text),
                Category = CategoryComboBox.SelectedItem.ToString(),
                Description = DescriptionTextBox.Text.Trim()
            };

            transactions.Add(transaction);
            ClearTransactionInputs();
            MessageBox.Show("Tranzakció sikeresen hozzáadva!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearTransactionInputs()
        {
            AmountTextBox.Clear();
            DescriptionTextBox.Clear();
            TransactionTypeComboBox.SelectedIndex = 0;
            if (categories.Count > 0)
                CategoryComboBox.SelectedIndex = 0;
        }

        private void UpdateSummary()
        {
            var summary = _dataService.CalculateSummary(transactions);

            TotalIncomeText.Text = $"{summary.TotalIncome:N0} Ft";
            TotalExpenseText.Text = $"{summary.TotalExpense:N0} Ft";
            BalanceText.Text = $"{summary.Balance:N0} Ft";
            BalanceText.Foreground = summary.Balance >= 0 ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel fájlok (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"Budget_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _excelService.ExportToExcel(transactions.ToList(), saveFileDialog.FileName);
                    MessageBox.Show("Export sikeresen befejezve!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt az export során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel fájlok (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var importedData = _excelService.ImportFromExcel(openFileDialog.FileName);

                    if (!importedData.Transactions.Any())
                    {
                        MessageBox.Show("A fájl nem tartalmaz érvényes adatokat!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Új kategóriák hozzáadása
                    foreach (var category in importedData.NewCategories)
                    {
                        if (!categories.Contains(category))
                            categories.Add(category);
                    }

                    // Tranzakciók hozzáadása
                    foreach (var transaction in importedData.Transactions)
                        transactions.Add(transaction);

                    MessageBox.Show($"Import sikeresen befejezve! {importedData.Transactions.Count} tranzakció importálva.",
                                  "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt az import során: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Biztosan törölni szeretné az összes tranzakciót?",
                                       "Megerősítés", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                transactions.Clear();
                MessageBox.Show("Összes tranzakció törölve!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}