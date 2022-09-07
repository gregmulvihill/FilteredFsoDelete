using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FilteredFsoDelete
{
    public class DataflowProducerConsumer<T>
    {
        private Task _producerTask;
        private BufferBlock<T> _buffer = new BufferBlock<T>();

        public DataflowProducerConsumer()
        {
        }

        public async Task<(int Count, int BytesProcessed)> ProduceAsync(ITargetBlock<T> target)
        {
            var count = 0;
            var bytesProcessed = 0;

            using (var sw = new StreamReader($@"C:\ProgramData\Dell\DellDataVault\Log\DDVSVCAPI_Errlog.txt"))
            {
                while (!sw.EndOfStream)
                {
                    var pos = sw.BaseStream.Position;
                    var len = sw.BaseStream.Length;
                    var per = 100.0 * (pos / (double)len);
                    var msg = $"{pos}/{len} {per:N2}%";

                    var line = await sw.ReadLineAsync();
                    var bytes = Encoding.UTF8.GetBytes(line);
                    bytesProcessed += bytes.Length;
                    var genericType = typeof(T);
                    var message = (T)Convert.ChangeType(bytes, genericType);

                    if (!target.Post(message))
                    {
                        var msg0 = Encoding.UTF8.GetString((byte[])(dynamic)message);
                    }

                    count++;
                }
            }

            #region MyRegion

            //var rand = new Random();

            //for (int i = 0; i < 100; ++i)
            //{
            //    byte[] buffer0 = GetNext();

            //    var genericType = typeof(T);
            //    var message = (T)Convert.ChangeType(buffer0, genericType);
            //    target.Post(message);

            //    //var messageHeader = new DataflowMessageHeader();
            //    //var rr = target.OfferMessage(messageHeader, message, _buffer, false);
            //} 
            #endregion

            target.Complete();

            return (count, bytesProcessed); ;
        }

        private byte[] GetNext()
        {
            throw new NotImplementedException();
        }

        public async Task<(int Count, int BytesProcessed)> ConsumeAsync(IReceivableSourceBlock<byte[]> source)
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;

            var count = 0;
            var bytesProcessed = 0;

            while (await source.OutputAvailableAsync())
            {
                while (source.TryReceive(out var data))
                {
                    bytesProcessed += data.Length;
                    var msg = Encoding.UTF8.GetString(data);
                    // Debug.WriteLine($"count: {++count}, msg: {msg}, ThreadId: {threadId}");
                }
            }

            return (count, bytesProcessed);
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
                //logMsg;
            });

            var huh = result;
        }

        internal void SignalDone()
        {
        }

        //internal void StartProducer(Action<object, object> value)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
