using System;
using System.Threading;
using System.Threading.Tasks;
using LiquidProjections;
using Newtonsoft.Json;
using SqlStreamStore.Streams;

namespace SqlStreamStore.Demo
{
    public class BalanceProjection
    {
        private readonly IEventMap<Balance> _map;
        public Balance Balance { get; } = new Balance(0, DateTime.UtcNow);
        
        public BalanceProjection(IStreamStore streamStore, StreamId streamId)
        {
            var mapBuilder = new EventMapBuilder<Balance>();
            
            mapBuilder.Map<Deposited>().As((deposited, balance) =>
            {
                balance.Add(deposited.Amount);
            });

            mapBuilder.Map<Withdrawn>().As((withdrawn, balance) =>
            {
                balance.Subtract(withdrawn.Amount);
            });
            
            _map = mapBuilder.Build(new ProjectorMap<Balance>()
            {
                Custom = (context, projector) => projector()
            });
            
            streamStore.SubscribeToStream(streamId, null, StreamMessageReceived);
        }
        
        private async Task<object> DeserializeJsonEvent(StreamMessage streamMessage, CancellationToken cancellationToken)
        {
            var json = await streamMessage.GetJsonData(cancellationToken);
            
            switch (streamMessage.Type)
            {
                case "Deposited":
                    return JsonConvert.DeserializeObject<Deposited>(json);
                case "Withdrawn":
                    return JsonConvert.DeserializeObject<Withdrawn>(json);
                default:
                    throw new InvalidOperationException("Unknown event type.");
            }
        }
        
        private async Task StreamMessageReceived(IStreamSubscription subscription, StreamMessage streamMessage, CancellationToken cancellationToken)
        {
            var @event = await DeserializeJsonEvent(streamMessage, cancellationToken);
            await _map.Handle(@event, Balance);
        }
    }
    
    public class Balance
    {
        public Balance(decimal amount, DateTime asOf)
        {
            Amount = amount;
            AsOf = asOf;
        }
        
        public decimal Amount { get; private set; }
        public DateTime AsOf { get;private set; }

        public void Add(decimal value)
        {
            AsOf = DateTime.UtcNow;
            Amount += value;
        }

        public void Subtract(decimal value)
        {
            AsOf = DateTime.UtcNow;
            Amount -= value;

        }
    }
}