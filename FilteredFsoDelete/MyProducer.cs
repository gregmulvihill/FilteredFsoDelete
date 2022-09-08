﻿using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace FilteredFsoDelete
{
    public class MyProducer : IProducer<string, SettingsEx>, IDisposable
    {
        private SettingsEx _settings;
        private Action<string> _log;
        private bool _demoMode;
        private IEnumerable<string> _filteredPaths;
        private IEnumerator<string> _filteredPathsEnumerator;

        public int Count { get; private set; }

        public void Init(SettingsEx context, Action<string> log, bool demoMode)
        {
            _settings = context;
            _log = log;
            _demoMode = demoMode;

            Count = 0;

            _filteredPaths = GetPaths();
            _filteredPathsEnumerator = _filteredPaths.GetEnumerator();

            _log($"Producer {Thread.CurrentThread.ManagedThreadId:N0} Init");
        }

        public void Dispose()
        {
            _filteredPathsEnumerator?.Dispose();
            _filteredPathsEnumerator = null;

            _log($"Producer {Thread.CurrentThread.ManagedThreadId:N0} Done");
        }

        public bool Produce(int instanceIndex, out string value)
        {
            var success = _filteredPathsEnumerator.MoveNext();

            if (!success)
            {
                value = default;
                return false;
            }

            value = _filteredPathsEnumerator.Current;

            _log($"Producer {Thread.CurrentThread.ManagedThreadId:N0} {value}");

            return true;
        }

        private IEnumerable<string> GetPaths()
        {
            var dirEnum = GetDirEnumerator();

            foreach (var item in dirEnum)
            {
                yield return item;
            }

            var fileEnum = GetFileEnumerator();

            foreach (var item in fileEnum)
            {
                yield return item;
            }

            yield break;
        }

        private IEnumerable<string> GetFileEnumerator()
        {
            var files = Directory.EnumerateFiles(_settings.TargetDirectory, "*.*", SearchOption.AllDirectories);
            var filesInclude = _settings.FilesKeep.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var filesExclude = _settings.FilesDelete.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            return GetFiltered(files, filesInclude, filesExclude);
        }

        private IEnumerable<string> GetDirEnumerator()
        {
            var dirs = Directory.EnumerateDirectories(_settings.TargetDirectory, "*.*", SearchOption.AllDirectories);
            var dirInclude = _settings.DirectoriesKeep.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x.Length);
            var dirExclude = _settings.DirectoriesDelete.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).OrderBy(x => x.Length);

            return GetFiltered(dirs, dirInclude, dirExclude);
        }

        private IEnumerable<string> GetFiltered(IEnumerable<string> items, IEnumerable<string> include, IEnumerable<string> exclude)
        {
            var filteredItems = items
                .Select(x => Path.GetFullPath(x))
                .Where(x =>
                    !include.Any(y => Regex.IsMatch(x, y, RegexOptions.IgnoreCase)) &&
                    exclude.Any(y => Regex.IsMatch(x, y, RegexOptions.IgnoreCase)))
                .OrderByDescending(x => x).ToArray();

            return filteredItems;
        }
    }
}
