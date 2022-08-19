using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FilteredFsoDelete
{
    internal class Program
    {
        static void Main()
        {
            var args = Environment.GetCommandLineArgs();
            var len = args.Length;

            if (len < 2)
            {
                // C:\Repos\Pinnacle d-\\\.vs$ d-\\bin$ d-\\bin$ d-\\obj$ d+\\lib\\ f-\.user$
                Console.WriteLine($"Use: {Path.GetFileName(Environment.ProcessPath)} target_dir <d-dir_regex_del> <d+d_regex_not_del> <f-file_regex_del> <f+file_regex_not_del>");
                Console.WriteLine($@"Example: {Path.GetFileName(Environment.ProcessPath)} C:\dev\project\ d-\\\.vs$ d-\\bin$ d-\\bin$ d-\\obj$ d+\\lib\\ f-\.user$");
                Environment.Exit(-1);
            }

            var a = args.Skip(1).ToArray();
            var root = a.First();
            var items = a.Skip(1).ToArray();

            var recs = items
                .Select(x => Regex.Match(x, "(?<type>[df][+-])(?<path>.+)"))
                .Where(x => x.Success)
                .Select(x => x.Groups)
                .Select(x => (Type: x["type"].Value, Path: x["path"].Value))
                .ToArray();

            var dirDel = new List<string>();
            var dirKeep = new List<string>();
            var fileDel = new List<string>();
            var fileKeep = new List<string>();

            foreach (var rec in recs)
            {
                switch (rec.Type)
                {
                    case "d-":
                        dirDel.Add(rec.Path);
                        break;
                    case "d+":
                        dirKeep.Add(rec.Path);
                        break;
                    case "f-":
                        fileDel.Add(rec.Path);
                        break;
                    case "f+":
                        fileKeep.Add(rec.Path);
                        break;
                }
            }

            var countDirs = DeleteDirectories(root, dirDel, dirKeep);
            var countFiles = DeleteFiles(root, fileDel, fileKeep);

            if (countDirs > 0 || countFiles > 0)
            {
                Console.WriteLine($"Directories Deleted: {countDirs}");
                Console.WriteLine($"      Files Deleted: {countFiles}");

                Exit(2);
            }
        }

        private static int DeleteDirectories(string root, List<string> include, List<string> exclude)
        {
            var items = Directory.EnumerateDirectories(root, "*.*", SearchOption.AllDirectories);

            return Process(include, exclude, x => Directory.Delete(x, true), items);
        }

        private static int DeleteFiles(string root, List<string> include, List<string> exclude)
        {
            var items = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);

            return Process(include, exclude, x => File.Delete(x), items);
        }

        private static int Process(List<string> include, List<string> exclude, Action<string> func, IEnumerable<string> items)
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
                    Console.WriteLine($"Deleting: {item}");
                    func(item);
                    ++count;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }

            return count;
        }

        private static void Exit(double delay)
        {
            var ts = DateTime.Now.AddSeconds(delay);

            while (ts > DateTime.Now)
            {
                Console.Write($"Exit in: {(ts - DateTime.Now).TotalSeconds:N1}\r");

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.Spacebar)
                    {
                        Console.Write("Press any key to exit.");
                        Console.ReadKey();
                    }

                    return;
                }

                Thread.Sleep(100);
            }
        }
    }
}
