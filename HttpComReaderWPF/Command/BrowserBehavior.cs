using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HttpComReaderWPF.Command
{
    public class BrowserBehavior
    {
        // "HtmlString" attached property for a WebView
        public static readonly DependencyProperty HtmlStringProperty =
           DependencyProperty.RegisterAttached("HtmlString", typeof(string), typeof(BrowserBehavior), new PropertyMetadata("", OnHtmlStringChanged));

        // Getter and Setter
        public static string GetHtmlString(DependencyObject obj) { return (string)obj.GetValue(HtmlStringProperty); }
        public static void SetHtmlString(DependencyObject obj, string value) { obj.SetValue(HtmlStringProperty, value); }

        // Handler for property changes in the DataContext : set the WebView
        private static void OnHtmlStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser wv = d as WebBrowser;
            if (wv != null)
            {
                wv.NavigateToString((string)e.NewValue);
            }
        }
        
    }
}
