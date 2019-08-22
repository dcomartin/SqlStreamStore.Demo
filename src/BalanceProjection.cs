using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SqlStreamStore.Streams;

namespace SqlStreamStore.Demo
{
    public class BalanceProjection
    {
        public Balance Balance { get; private set; } = new Balance(0, DateTime.UtcNow);
        
        public BalanceProjection(IStreamStore streamStore, StreamId streamId)
        {
            streamStore.SubscribeToStream(streamId, null, StreamMessageReceived);
        }
        
        private async Task StreamMessageReceived(IStreamSubscription subscription, StreamMessage streamMessage, CancellationToken cancellationToken)
        {
            switch (streamMessage.Type)
            {
                case "Deposited":
                    var depositedJson = await streamMessage.GetJsonData(cancellationToken);
                    var deposited = JsonConvert.DeserializeObject<Deposited>(depositedJson);
                    Balance = Balance.Add(deposited.Amount);
                    break;
                case "Withdrawn":
                    var withdrawnJson = await streamMessage.GetJsonData(cancellationToken);
                    var withdrawn = JsonConvert.DeserializeObject<Withdrawn>(withdrawnJson);
                    Balance = Balance.Subtract(withdrawn.Amount);
                    break;
            }
        }
    }
    
    public struct Balance
    {
        public Balance(decimal amount, DateTime asOf)
        {
            Amount = amount;
            AsOf = asOf;
        }
        
        public decimal Amount { get;  }
        public DateTime AsOf { get; }

        public Balance Add(decimal value)
        {
            return new Balance(Amount + value, DateTime.UtcNow);
        }

        public Balance Subtract(decimal value)
        {
            return new Balance(Amount - value, DateTime.UtcNow);
        }
    }
}