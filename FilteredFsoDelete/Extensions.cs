using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FilteredFsoDelete
{
    public static class Extensions
    {
        public static U ViaDispatcher<T, U>(this T frameworkElement, Func<T, U> propertyGetter)
            where T : FrameworkElement
        {
            var v = default(U);
            frameworkElement.Dispatcher.Invoke(() => { v = propertyGetter.Invoke(frameworkElement); });

            return v;
        }
    }
}
