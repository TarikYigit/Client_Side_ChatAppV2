using System.Windows;
using System.Windows.Controls;

namespace ClientSideChatApp.Helpers
{
    public static class PasswordBoxHelper
    {

        public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxHelper), new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper));

        public static void SetAttach(DependencyObject dp, bool value) => dp.SetValue(AttachProperty, value);
        public static bool GetAttach(DependencyObject dp) => (bool)dp.GetValue(AttachProperty);
        public static string GetBoundPassword(DependencyObject dp) => (string)dp.GetValue(BoundPasswordProperty);
        public static void SetBoundPassword(DependencyObject dp, string value) => dp.SetValue(BoundPasswordProperty, value);
        private static bool GetIsUpdating(DependencyObject dp) => (bool)dp.GetValue(IsUpdatingProperty);
        private static void SetIsUpdating(DependencyObject dp, bool value) => dp.SetValue(IsUpdatingProperty, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {

                passwordBox.PasswordChanged -= PasswordChanged;

                if (!GetIsUpdating(passwordBox))
                {

                    passwordBox.Password = (string)e.NewValue;

                }
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            if (sender is PasswordBox passwordBox)
            {

                if ((bool)e.OldValue) passwordBox.PasswordChanged -= PasswordChanged;

                if ((bool)e.NewValue) passwordBox.PasswordChanged += PasswordChanged;

            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {

            if (sender is PasswordBox passwordBox)
            {

                SetIsUpdating(passwordBox, true);

                SetBoundPassword(passwordBox, passwordBox.Password);

                SetIsUpdating(passwordBox, false);

            }
        }
    }
}