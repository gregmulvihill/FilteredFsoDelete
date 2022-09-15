using System.IO;
using System.Text.Json;
using System.Windows;
using FilteredFsoDelete.ProducerConsumer;

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
        private ProducerConsumer<string, AppSettings> _producerConsumer;
        private AppSettings? _settings;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();

            _producerConsumer = new ProducerConsumer<string, AppSettings>(x => new MyProducer(), x => new MyConsumer(), _settings, LogWriteLine);
        }

        private void LogWriteLine(string msg)
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

        private void SaveSettings()
        {
            _settings = new AppSettings
            {
                TargetDirectory = TargetDirectory.Text,

                DirectoriesKeep = DirectoriesKeep.Items.Cast<MyType>().ToArray(),
                FilesKeep = FilesKeep.Items.Cast<MyType>().ToArray(),

                DirectoriesDelete = DirectoriesDelete.Items.Cast<MyType>().ToArray(),
                FilesDelete = FilesDelete.Items.Cast<MyType>().ToArray(),
            };

            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }

        private void LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                TargetDirectory.Text = _defaultTargetDirectory;

                foreach (var item in _defaultFileKeep) FilesKeep.Items.Add(new MyType(true, item));
                foreach (var item in _defaultDirectoriesKeep) DirectoriesKeep.Items.Add(new MyType(true, item));
                foreach (var item in _defaultFilesDelete) FilesDelete.Items.Add(new MyType(true, item));
                foreach (var item in _defaultDirectoriesDelete) DirectoriesDelete.Items.Add(new MyType(true, item));

                SaveSettings();
            }

            var json = File.ReadAllText(_settingsFilePath);
            _settings = JsonSerializer.Deserialize<AppSettings>(json);

            TargetDirectory.Text = _settings.TargetDirectory;

            FilesKeep.Items.Clear();
            DirectoriesKeep.Items.Clear();
            FilesDelete.Items.Clear();
            DirectoriesDelete.Items.Clear();

            foreach (var item in _settings.FilesKeep) FilesKeep.Items.Add(item);
            foreach (var item in _settings.DirectoriesKeep) DirectoriesKeep.Items.Add(item);
            foreach (var item in _settings.FilesDelete) FilesDelete.Items.Add(item);
            foreach (var item in _settings.DirectoriesDelete) DirectoriesDelete.Items.Add(item);
        }

        private void Button_Click_Run(object sender, RoutedEventArgs e)
        {
            SaveSettings();

            Log.Items.Clear();

            Task.Run(async () => await _producerConsumer.RunThreadAsync(false));
        }

        private void Button_Click_Test(object sender, RoutedEventArgs e)
        {
            Log.Items.Clear();

            Task.Run(async () => await _producerConsumer.RunThreadAsync(true));
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
