using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Markup;

namespace FilteredFsoDelete
{
    public class DataflowProducerConsumer<T>
    {
        private Func<int, T> _getNewItem;

        private Task _producerTask;
        // private BufferBlock<T> _buffer = new BufferBlock<T>();

        public async Task<int> ProduceAsync(ITargetBlock<T> target, Func<int, T> getNewItem)
        {
            _getNewItem = getNewItem;

            // fake async/await
            await Task.CompletedTask;

            var threadId = Thread.CurrentThread.ManagedThreadId;
            var count = 0;

            var len = 100;

            for (var pos = 0; pos < len; pos++)
            {
                T item = _getNewItem(pos);

                var per = 100.0 * ((double)(pos + 1) / (double)(len));
                var msg = $"{pos + 1}/{len} {per:N2}%";
                Debug.WriteLine(msg);
                Debug.WriteLine($"Producer Thread - ThreadId: {threadId}, count: {++count}, msg: {item}");

                if (!target.Post(item))
                {
                    Debugger.Break();
                    //var msg0 = Encoding.UTF8.GetString((MyType)(dynamic)message);
                }

                Thread.Sleep(10);

                count++;
            }

            target.Complete();

            return count;
        }

        public async Task<int> ConsumeAsync(IReceivableSourceBlock<T> source, Func<T, int> consumer)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var count = 0;

            while (await source.OutputAvailableAsync())
            {
                while (source.TryReceive(out var data))
                {
                    var result = consumer(data);
                    Debug.WriteLine($"Consumer Thread - ThreadId: {threadId}, count: {++count}, msg: {data}");
                    Thread.Sleep(125);
                }
            }

            return count;
        }

        //public void Produce(ITargetBlock<T> target)
        //{
        //    var rand = new Random();

        //    for (int i = 0; i < 100; ++i)
        //    {
        //        //var buffer = new byte[1024];
        //        //rand.NextBytes(buffer);

        //        T item = default;
        //        target.Post(item);
        //    }

        //    target.Complete();
        //}

        //public async Task<T> ConsumeAsync(IReceivableSourceBlock<T> source)
        //{
        //    T item = default;

        //    while (await source.OutputAvailableAsync())
        //    {
        //        while (source.TryReceive<T>(out var data))
        //        {
        //            //bytesProcessed += data.Length;
        //        }
        //    }

        //    return item;
        //}

        //public async Task<T> ConsumeAsync(ISourceBlock<T> source)
        //{
        //    while (await source.OutputAvailableAsync())
        //    {
        //        var data = await source.ReceiveAsync();

        //        return data;
        //    }

        //    return default;
        //}

        //public async Task Demo()
        //{
        //    var buffer = new BufferBlock<string>();
        //    var consumerTask = ConsumeAsync(buffer);
        //    Produce(buffer);

        //    var bytesProcessed = await consumerTask;

        //    Console.WriteLine($"Processed {bytesProcessed:#,#} bytes.");
        //}

        public void StartProducer(Func<object, Action<string>, object> f)
        {
            _producerTask = Task.Run(() => ProducerTask(f));
        }

        private void ProducerTask(Func<object, Action<string>, object> f)
        {
            var result = f("arg1", logMsg =>
            {
            });

            var huh = result;
        }

        public void Stop()
        {
        }
    }
}
