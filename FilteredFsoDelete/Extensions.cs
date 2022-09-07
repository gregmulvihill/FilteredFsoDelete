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

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> method)
        {
            return await Task.WhenAll(source.Select(async s => await method(s)));
        }
    }
}
