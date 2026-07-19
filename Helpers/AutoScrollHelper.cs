using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace ClientSideChatApp.Helpers
{
    public static class AutoScrollHelper
    {
        public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollHelper), new PropertyMetadata(false, AutoScrollPropertyChanged));

        public static void SetAutoScroll(DependencyObject obj, bool value) => obj.SetValue(AutoScrollProperty, value);
        public static bool GetAutoScroll(DependencyObject obj) => (bool)obj.GetValue(AutoScrollProperty);

        private static void AutoScrollPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (d is ListBox listBox && (bool)e.NewValue)
            {

                listBox.Loaded += (s, ev) => AttachScroll(listBox);

            }
        }

        private static void AttachScroll(ListBox listBox)
        {

            if (listBox.ItemsSource is INotifyCollectionChanged collection)
            {

                collection.CollectionChanged += (s, e) =>
                {

                    if (e.Action == NotifyCollectionChangedAction.Add && listBox.Items.Count > 0)
                    {

                        listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);

                    }
                };

                if (listBox.Items.Count > 0)

                    listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);
            }
        }
    }
}