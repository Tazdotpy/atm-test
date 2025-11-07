using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;

namespace ATM_Avalonia
{
    public partial class MainWindow : Window
    {
        // data classes
        public class Transaction
        {
            public string Type { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }

            public Transaction(string type, decimal amount, DateTime date)
            {
                Type = type;
                Amount = amount;
                Date = date;
            }
        }

        public class Account
        {
            public string CardNumber { get; set; }
            public string PIN { get; set; }
            public decimal Balance { get; set; }
            public List<Transaction> Transactions { get; set; }
            public int DailyTransactions { get; set; }

            public Account(string cardNumber, string pin, decimal balance)
            {
                CardNumber = cardNumber;
                PIN = pin;
                Balance = balance;
                Transactions = new List<Transaction>();
                DailyTransactions = 0;
            }
        }

        // variables 
        private List<Account> accounts;
        private Account currentAccount;

        public MainWindow()
        {
            InitializeComponent();
            btnFast100.Click += (_, __) => FastCash(100);
            btnFast500.Click += (_, __) => FastCash(500);
            btnFast1000.Click += (_, __) => FastCash(1000);
            btnFast2000.Click += (_, __) => FastCash(2000);

            




            accounts = new List<Account>
            {
                new Account("12345678", "1234", 10000m),
                new Account("87654321", "5678", 12000m)
            };

            

            
        }
    

        private void FastCash(decimal amount) // fast cash that will be outside the event handler because i am literally terrified about having to deal with an error agan
        {
            if (currentAccount == null)
            {
                MessageBox("Please log in first.");
                return;
            }

            if (amount > currentAccount.Balance)
            {
                MessageBox("Insufficient funds.");
                return;
            }

            if (currentAccount.DailyTransactions >= 10)
            {
                MessageBox("Daily transaction limit reached.");
                return;
            }

            currentAccount.Balance -= amount;
            currentAccount.Transactions.Add(new Transaction("Fast Cash Withdraw", amount, DateTime.Now));
            currentAccount.DailyTransactions++;

            lblBalance.Text = $"Balance: ${currentAccount.Balance}";

            MessageBox($"Fast Cash Withdrawal of ${amount} successful!\nNew Balance: ${currentAccount.Balance}");
        }

        // event handlers

        
        private void btnLogin_Click(object? sender, RoutedEventArgs e)
        {
            string card = txtCardNumber.Text ?? "";
            string pin = txtPin.Text ?? "";

            currentAccount = accounts.FirstOrDefault(a => a.CardNumber == card && a.PIN == pin);

            if (currentAccount != null)
            {
                lblWelcome.Text = $"Welcome, Card #{currentAccount.CardNumber}";
                lblBalance.Text = $"Balance: ${currentAccount.Balance}";
                panelMenu.IsVisible = true;
                btnLogin.IsEnabled = false;
                txtCardNumber.IsEnabled = txtPin.IsEnabled = false;
            }
            else
            {
                MessageBox("Invalid card number or PIN.");
            }
        }

        private void btnCheckBalance_Click(object? sender, RoutedEventArgs e)
        {
            lblBalance.Text = $"Current Balance: ${currentAccount.Balance}";
        }

        private void btnWithdraw_Click(object? sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtWithdrawAmount.Text, out decimal amount))
            {
                MessageBox("Please enter a valid number.");
                return;
            }

            if (amount <= 0 || amount > 1000)
            {
                MessageBox("You can only withdraw up to $1000 per transaction.");
                return;
            }

            if (amount > currentAccount.Balance)
            {
                MessageBox("Insufficient funds.");
                return;
            }

            if (currentAccount.DailyTransactions >= 10)
            {
                MessageBox("Daily transaction limit reached.");
                return;
            }

            currentAccount.Balance -= amount;
            currentAccount.Transactions.Add(new Transaction("Withdraw", amount, DateTime.Now));
            currentAccount.DailyTransactions++;

            lblBalance.Text = $"Balance: ${currentAccount.Balance}";
            txtWithdrawAmount.Text = string.Empty;

            MessageBox($"Withdrawal successful!\nNew Balance: ${currentAccount.Balance}");
        }

        // event handler for deposit 
        private void btnDeposit_Click(object? sender, RoutedEventArgs e)
        {
            if (currentAccount == null)
            {
                MessageBox("Please log in first.");
                return;
            }

            if (!decimal.TryParse(txtDepositAmount.Text, out decimal amount))
            {
                MessageBox("Please enter a valid number.");
                return;
            }

            if (amount <= 0 || amount > 10000)
            {
                MessageBox("Please enter an amount between $0.01 and $10,000.");
                return;
            }

            if (currentAccount.DailyTransactions >= 10)
            {
                MessageBox("Daily transaction limit reached.");
                return;
            }

            currentAccount.Balance += amount;
            currentAccount.Transactions.Add(new Transaction("Deposit", amount, DateTime.Now));
            currentAccount.DailyTransactions++;

            lblBalance.Text = $"Balance: ${currentAccount.Balance}";
            txtDepositAmount.Text = string.Empty;

            MessageBox($"Deposit successful!\nNew Balance: ${currentAccount.Balance}");
        }

        private void btnShowTransactions_Click(object? sender, RoutedEventArgs e)
        {
            listTransactions.ItemsSource = null;

            var last5 = currentAccount.Transactions
                                     .OrderByDescending(t => t.Date)
                                     .Take(5)
                                     .Select(t => $"{t.Date:g} - {t.Type}: ${t.Amount}")
                                     .ToList();

            listTransactions.ItemsSource = last5.Count > 0
                ? last5
                : new List<string> { "No transactions yet." };
        }

        private void btnLogout_Click(object? sender, RoutedEventArgs e)
        {
            MessageBox("Logged out.");
            currentAccount = null;
            panelMenu.IsVisible = false;
            btnLogin.IsEnabled = true;
            txtCardNumber.IsEnabled = txtPin.IsEnabled = true;
            txtCardNumber.Text = txtPin.Text = "";
            listTransactions.ItemsSource = null;
        }

        private async void MessageBox(string text)
        {
            var dialog = new Window
            {
                Width = 300,
                Height = 150,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(10),
                    Children =
                    {
                        new TextBlock { Text = text, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new Button
                        {
                            Content = "OK",
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Margin = new Avalonia.Thickness(0,10,0,0)
                        }
                    }
                }
            };

            var okButton = ((dialog.Content as StackPanel)?.Children[1] as Button)!;
            okButton.Click += (_, __) => dialog.Close();

            await dialog.ShowDialog(this);
        }

        
    }
}
