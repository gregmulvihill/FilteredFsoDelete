using Microsoft.VisualBasic.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using System.Windows;

namespace FilteredFsoDelete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _settingsFilePath = @".\settings.json";

        private string _defaultTargetDirectory = @"C:\Repos";
        private IEnumerable<string> _defaultFileKeep = new string[] { };
        private IEnumerable<string> _defaultDirectoriesKeep = new[] { @"\\lib\\", };
        private IEnumerable<string> _defaultFilesDelete = new[] { @"\.user$", };
        private IEnumerable<string> _defaultDirectoriesDelete = new[] { @"\\\.vs$", @"\\bin$", @"\\obj$", };

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void SaveSettings()
        {
            var settings = new SettingsEx
            {
                TargetDirectory = TargetDirectory.Text,

                DirectoriesKeep = DirectoriesKeep.Text,
                FilesKeep = FilesKeep.Text,

                DirectoriesDelete = DirectoriesDelete.Text,
                FilesDelete = FilesDelete.Text,
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }

        private void LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                TargetDirectory.Text = _defaultTargetDirectory;
                FilesKeep.Text = String.Join(Environment.NewLine, _defaultFileKeep);
                DirectoriesKeep.Text = String.Join(Environment.NewLine, _defaultDirectoriesKeep);
                FilesDelete.Text = String.Join(Environment.NewLine, _defaultFilesDelete);
                DirectoriesDelete.Text = String.Join(Environment.NewLine, _defaultDirectoriesDelete);

                SaveSettings();
            }

            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<SettingsEx>(json);

            TargetDirectory.Text = settings.TargetDirectory;
            FilesKeep.Text = String.Join(Environment.NewLine, settings.FilesKeep);
            DirectoriesKeep.Text = String.Join(Environment.NewLine, settings.DirectoriesKeep);
            FilesDelete.Text = String.Join(Environment.NewLine, settings.FilesDelete);
            DirectoriesDelete.Text = String.Join(Environment.NewLine, settings.DirectoriesDelete);
        }

        private void Button_Click_Run(object sender, RoutedEventArgs e)
        {
            Log.Items.Clear();

            Task.Run(async () => await RunThreadAsync(false));
        }

        private void Button_Click_Test(object sender, RoutedEventArgs e)
        {
            Log.Items.Clear();

            Task.Run(async () => await RunThreadAsync(true));
        }

        private void Log_(string msg)
        {
            Log.Dispatcher.Invoke(() =>
            {
                Log.Items.Add(msg);

                //SelectedItem
                //SelectedIndex
                //SelectedValue
                //SelectedValuePath

                Log.SelectedIndex = Log.Items.Count - 1;
                Log.ScrollIntoView(Log.SelectedItem);
            });
        }

        private async Task RunThreadAsync(bool test)
        {
            //CaptureUserInput(out var root, out var dirKeep, out var dirDelete, out var fileKeep, out var fileDelete);

            var root = TargetDirectory.ViaDispatcher(x => x.Text);

            var dirKeep = DirectoriesDelete.ViaDispatcher(x => x.Text).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var dirDelete = DirectoriesKeep.ViaDispatcher(x => x.Text).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var fileKeep = FilesDelete.ViaDispatcher(x => x.Text).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var fileDelete = FilesKeep.ViaDispatcher(x => x.Text).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var buffer = new BufferBlock<byte[]>();
            var pc = new DataflowProducerConsumer<byte[]>();

            var ct = Enumerable.Range(0, 1).Select(x => Task.Run(() => ProduceAsync(buffer, pc))).ToArray();
            var pt = Enumerable.Range(0, 5).Select(x => Task.Run(() => ConsumeAsync(buffer, pc))).ToArray();

            Task.WaitAll(pt);
            Task.WaitAll(ct);

            pc.SignalDone();

            var ctResults = ct.Select(x => x.Result).ToArray();
            var ptResults = pt.Select(x => x.Result).ToArray();
        }

        private static Task<(int, int)> ConsumeAsync(BufferBlock<byte[]> buffer, DataflowProducerConsumer<byte[]> pc)
        {
            return pc.ConsumeAsync(buffer);
        }

        private static Task<(int, int)> ProduceAsync(BufferBlock<byte[]> buffer, DataflowProducerConsumer<byte[]> pc)
        {
            return pc.ProduceAsync(buffer);
        }

        private Task<Action<int, string>> NewMethod(
            bool test,
            Action<string> ff,
            string root,
            string[] dirKeep,
            string[] dirDelete,
            string[] fileKeep,
            string[] fileDelete)
        {
            // TODO: move into ProduceAsync, ConsumeAsync
            Log_("### Delete Directories...");
            var countDirs = FilteredFsoDeleteLib.DeleteDirectories(root, dirKeep, dirDelete, ff /*x => Log_(x)*/, test);
            Log_($"### Deleted {countDirs} Directories");

            Log_("### Delete Files...");
            var countFiles = FilteredFsoDeleteLib.DeleteFiles(root, fileKeep, fileDelete, ff /*x => Log_(x)*/, test);
            Log_($"### Deleted {countFiles} Files");

            return default;
        }

        private void CaptureUserInput(
            out string root,
            out string[] dirKeep,
            out string[] dirDelete,
            out string[] fileKeep,
            out string[] fileDelete)
        {
            string root_ = string.Empty;

            var dirKeep_ = Array.Empty<string>();
            var dirDelete_ = Array.Empty<string>();

            var fileKeep_ = Array.Empty<string>();
            var fileDelete_ = Array.Empty<string>();

            Dispatcher.Invoke(() =>
            {
                root_ = TargetDirectory.Text;

                dirKeep_ = DirectoriesDelete.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                dirDelete_ = DirectoriesKeep.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                fileKeep_ = FilesDelete.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                fileDelete_ = FilesKeep.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            });

            root = root_;

            dirKeep = dirKeep_;
            dirDelete = dirDelete_;

            fileKeep = fileKeep_;
            fileDelete = fileDelete_;
        }

        private void Button_Click_FolderBrowser(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var targetFolder = System.IO.Path.GetFullPath(TargetDirectory.Text);

                dialog.InitialDirectory = System.IO.Path.GetDirectoryName(targetFolder);
                dialog.Description = "My Description";
                //dialog.AutoUpgradeEnabled = true;
                dialog.RootFolder = Environment.SpecialFolder.LocalApplicationData;
                //dialog.SelectedPath = targetFolder;
                dialog.ShowNewFolderButton = false;
                dialog.UseDescriptionForTitle = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TargetDirectory.Text = dialog.SelectedPath;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
    }
}
