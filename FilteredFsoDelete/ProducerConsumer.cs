using System;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FilteredFsoDelete
{
    public class ProducerConsumer<T, U>
    {
        private Func<int, IProducer<T, U>> _createProducer;
        private Func<int, IConsumer<T, U>> _createConsumer;
        private U? _context;
        private Action<string> _log;
        private BufferBlock<T> _buffer;
        private bool _demoMode;
        private int _maxQueueSize = 100;

        public ProducerConsumer(
            Func<int, IProducer<T, U>> createProducer,
            Func<int, IConsumer<T, U>> createConsumer,
            U context,
            Action<string> log)
        {
            _createConsumer = createConsumer;
            _createProducer = createProducer;
            _context = context;
            _log = log;
        }

        public async Task RunThreadAsync(bool demoMode)
        {
            _demoMode = demoMode;
            _buffer = new BufferBlock<T>();

            // fake async/await
            await Task.CompletedTask;

            var ct = Enumerable.Range(0, 5).Select(x => Task.Run(() => ConsumerTask(x))).ToArray();
            var pt = Enumerable.Range(0, 1).Select(x => Task.Run(() => ProducerTask(x))).ToArray();

            Task.WaitAll(pt);
            Task.WaitAll(ct);

            var ctResults = ct.Select(x => x.Result).ToArray();
            var ptResults = pt.Select(x => x.Result).ToArray();

            var ctr = string.Join(",", ctResults);
            var ptr = string.Join(",", ptResults);

            _buffer = null;

            _log($"PTR Count(s): {ptr}, CTR Count(s): {ctr}");
        }

        public async Task<int> ProducerTask(int instanceIndex)
        {
            // fake async/await
            await Task.CompletedTask;

            using (var producer = _createProducer(instanceIndex))
            {
                producer.Init(_context, _log, _demoMode);

                var count = 0;

                while (true)
                {
                    if (!producer.Produce(count, out var item))
                    {
                        break;
                    }

                    if (!_buffer.Post(item))
                    {
                        break;
                    }

                    count++;

                    while (true)
                    {
                        if (_buffer.Count < _maxQueueSize)
                        {
                            break;
                        }

                        // TODO: replace with managed thread synchronization
                        Thread.Sleep(50);
                    }
                }

                _buffer.Complete();

                return count;
            }
        }

        public async Task<int> ConsumerTask(int instanceIndex)
        {
            using (var consumer = _createConsumer(instanceIndex))
            {
                consumer.Init(_context, _log, _demoMode);

                var count = 0;

                while (await _buffer.OutputAvailableAsync())
                {
                    while (_buffer.TryReceive(out var data))
                    {
                        var result = consumer.Consume(count++, data);

                        if (!result)
                        {
                            Debugger.Break();
                        }
                    }
                }

                //_xxxx.Complete();

                return count;
            }
        }
    }
}
