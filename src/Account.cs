using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SqlStreamStore.Streams;

namespace SqlStreamStore.Demo
{
    public class Account
    {
        private readonly StreamId _streamId;
        private readonly IStreamStore _streamStore;
        
        public Account(IStreamStore streamStore, StreamId streamId)
        {
            _streamId = streamId;
            _streamStore = streamStore;
        }
        
        public async Task<Guid> Deposit(decimal amount)
        {
            var trx = Guid.NewGuid();
            var deposit = new Deposited(trx, amount, DateTime.UtcNow);
            await _streamStore.AppendToStream(_streamId, ExpectedVersion.Any, new NewStreamMessage(trx, "Deposited", JsonConvert.SerializeObject(deposit)));
            return trx;
        }
        
        public async Task<Guid> Withdrawal(decimal amount)
        {
                var trx = Guid.NewGuid();
                var deposit = new Withdrawn(trx,amount, DateTime.UtcNow);
                await _streamStore.AppendToStream(_streamId, ExpectedVersion.Any, new NewStreamMessage(trx, "Withdrawn", JsonConvert.SerializeObject(deposit)));
                return trx;
        }
        
        public async Task Transactions()
        {
            decimal balance = 0;
            var endOfStream = false;
            var startVersion = 0;
            while (endOfStream == false)
            {
                var stream = await _streamStore.ReadStreamForwards(_streamId, startVersion, 10);
                endOfStream = stream.IsEnd;
                startVersion = stream.NextStreamVersion;

                foreach (var msg in stream.Messages)
                {
                    switch (msg.Type)
                    {
                        case "Deposited":
                            var depositedJson = await msg.GetJsonData();
                            var deposited = JsonConvert.DeserializeObject<Deposited>(depositedJson);
                            Console.WriteLine($"Deposited: {deposited.Amount:C} @ {deposited.DateTime} ({deposited.TransactionId})");
                            balance += deposited.Amount;
                            break;
                        case "Withdrawn":
                            var withdrawnJson = await msg.GetJsonData();
                            var withdrawn = JsonConvert.DeserializeObject<Withdrawn>(withdrawnJson);
                            Console.WriteLine($"Withdrawn: {withdrawn.Amount:C} @ {withdrawn.DateTime} ({withdrawn.TransactionId})");
                            balance -= withdrawn.Amount;
                            break;
                    }
                }
            }

            Console.WriteLine($"Balance: {balance:C}");
        }
    }
}