using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
