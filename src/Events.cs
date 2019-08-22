using System;

namespace SqlStreamStore.Demo
{
    public abstract class AccountEvent
    {
        public Guid TransactionId { get; }
        public decimal Amount { get; }
        public DateTime DateTime { get; }
        
        public AccountEvent(Guid transactionId, decimal amount, DateTime dateTime)
        {
            TransactionId = transactionId;
            Amount = amount;
            DateTime = dateTime;
        }
    }
    
    public class Deposited : AccountEvent
    {
        public Deposited(Guid transactionId, decimal amount, DateTime dateTime) : base(transactionId, amount, dateTime)
        {
        }
    }

    public class Withdrawn : AccountEvent
    {
        public Withdrawn(Guid transactionId, decimal amount, DateTime dateTime) : base(transactionId, amount, dateTime)
        {
        }
    }
}