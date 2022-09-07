using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FilteredFsoDelete
{
    public class ProducerConsumer<T>
    {
        private readonly Func<int, T> _producer;
        private readonly Func<T, int> _consumer;

        public SettingsEx Settings { get; }

        public ProducerConsumer(SettingsEx settings, Func<int, T> producer, Func<T, int> consumer)
        {
            _producer = producer;
            _consumer = consumer;
            Settings = settings;
        }

        public async Task RunThreadAsync(bool test)
        {
            //CaptureUserInput(out var root, out var dirKeep, out var dirDelete, out var fileKeep, out var fileDelete);

            var root = Settings.TargetDirectory;

            var dirKeep = Settings.DirectoriesDelete.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var dirDelete = Settings.DirectoriesKeep.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var fileKeep = Settings.FilesDelete.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var fileDelete = Settings.FilesKeep.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var buffer = new BufferBlock<T>();
            var pc = new DataflowProducerConsumer<T>();

            var ct = Enumerable.Range(0, 1).Select(x => Task.Run(() => ProduceAsync(buffer, pc))).ToArray();
            var pt = Enumerable.Range(0, 5).Select(x => Task.Run(() => ConsumeAsync(buffer, pc))).ToArray();

            Task.WaitAll(pt);
            Task.WaitAll(ct);

            pc.Stop();

            var ctResults = ct.Select(x => x.Result).ToArray();
            var ptResults = pt.Select(x => x.Result).ToArray();
        }

        private Task<int> ConsumeAsync(BufferBlock<T> buffer, DataflowProducerConsumer<T> pc)
        {
            return pc.ConsumeAsync(buffer, x => _consumer(x));
        }

        private Task<int> ProduceAsync(BufferBlock<T> buffer, DataflowProducerConsumer<T> pc)
        {
            return pc.ProduceAsync(buffer, x => _producer(x));
        }
    }
}
