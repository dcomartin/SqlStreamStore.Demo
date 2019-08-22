using System;
using System.Threading.Tasks;
using SqlStreamStore.Streams;

namespace SqlStreamStore.Demo
{
    static class Program
    {
        private static InMemoryStreamStore _streamStore;
        private static Account _account;
        private static BalanceProjection _balanceProjection;

        static async Task Main()
        {
            var streamId = new StreamId($"Account:{Guid.NewGuid()}");
            _streamStore = new InMemoryStreamStore();
            _account = new Account(_streamStore, streamId);
            _balanceProjection = new BalanceProjection(_streamStore, streamId);
                
            var key = string.Empty;
            while (key != "X")
            {
                Console.WriteLine("D: Deposit");
                Console.WriteLine("W: Withdrawal");
                Console.WriteLine("B: Balance");
                Console.WriteLine("T: Transactions");
                Console.WriteLine("X: Exit");
                Console.Write("> ");
                key = Console.ReadLine()?.ToUpperInvariant();
                Console.WriteLine();
                
                switch (key)
                {
                    case "D":
                        var depositAmount = GetAmount();
                        if (depositAmount.IsValid)
                        {
                            var depositTrx = await _account.Deposit(depositAmount.Amount);
                            Console.WriteLine($"Deposited: {depositAmount.Amount:C} ({depositTrx})");
                        }
                        break;
                    case "W":
                        var withdrawalAmount = GetAmount();
                        if (withdrawalAmount.IsValid)
                        {
                            var withdrawalTrx = await _account.Withdrawal(withdrawalAmount.Amount);
                            Console.WriteLine($"Withdrawn: {withdrawalAmount.Amount:C} ({withdrawalTrx})");
                        }
                        break;
                    case "B":
                        Balance();
                        break;
                    case "T":
                        await _account.Transactions();
                        break;
                }
                
                Console.WriteLine();
            }
        }

        private static (decimal Amount, bool IsValid) GetAmount()
        {
            Console.Write("Amount: ");
            if (decimal.TryParse(Console.ReadLine(), out var amount))
            {
                return (amount, true);
            }

            Console.WriteLine("Invalid Amount.");
            return (0, false);
        }
        
        private static void Balance()
        {
            Console.WriteLine($"Balance: {_balanceProjection.Balance.Amount:C} as of {_balanceProjection.Balance.AsOf}");
        }
    }
}