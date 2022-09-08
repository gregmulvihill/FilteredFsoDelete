using System.Windows.Threading;

namespace FilteredFsoDelete
{
    public class Tools
    {
        public static void DoEvents()
        {
            App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
    }
}
