using System.IO;
using System.Text.RegularExpressions;

namespace FilteredFsoDelete
{
    /// <summary>
    /// FilteredFsoDeleteLib
    /// </summary>
    public class FilteredFsoDeleteLib
    {
        public static int DeleteDirectories(string root, string[] include, string[] exclude, Action<string> log, bool test)
        {
            var items = Directory.EnumerateDirectories(root, "*.*", SearchOption.AllDirectories);

            return Process(include, exclude, x => MoveToRecycleBin(x, test), items, log);
        }

        public static int DeleteFiles(string root, string[] include, string[] exclude, Action<string> log, bool test)
        {
            var items = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);

            return Process(include, exclude, x => MoveToRecycleBin(x, test), items, log);
        }

        private static void MoveToRecycleBin(string path, bool test)
        {
            if (!test)
            {
                FileOperationAPIWrapper.MoveToRecycleBin(path);
            }
        }

        private static int Process(string[] include, string[] exclude, Action<string> func, IEnumerable<string> items, Action<string> log)
        {
            var filtered = items
                .Select(x => Path.GetFullPath(x))
                .Where(x =>
                    include.Any(y => Regex.IsMatch(x, y, RegexOptions.IgnoreCase)) &&
                    !exclude.Any(y => Regex.IsMatch(x, y, RegexOptions.IgnoreCase)))
                .OrderByDescending(x => x)
                .ToArray();

            var count = 0;

            foreach (var item in filtered)
            {
                try
                {
                    log($"Deleting: {item}");
                    func(item);
                    ++count;
                }
                catch (Exception e)
                {
                    log(e.ToString());
                }
            }

            return count;
        }
    }
}
