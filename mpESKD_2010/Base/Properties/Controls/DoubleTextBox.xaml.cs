using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ModPlusAPI.Windows;

namespace mpESKD.Base.Properties.Controls
{
    /// <summary>
    /// Логика взаимодействия для DoubleTextBox.xaml
    /// </summary>
    public partial class DoubleTextBox : UserControl
    {
        /// <summary>
        /// Свойство зависимостей для свойства Value
        /// </summary>
        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register("Value", typeof(double?), typeof(DoubleTextBox),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Числовое значение или null.
        /// Если null - в текстовом окошке выводится ""
        /// </summary>
        public double? Value
        {
            get => (double?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        /// <summary>
        /// Maximum value for the Numeric Up Down control
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(DoubleTextBox), new UIPropertyMetadata(double.MaxValue));

        /// <summary>
        /// Minimum value of the numeric up down conrol.
        /// </summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(DoubleTextBox), new UIPropertyMetadata(0.0));

        public DoubleTextBox()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Cancel || e.Key == Key.Escape)
            {
                try
                {
                    BindingOperations.GetBindingExpression(TextBox, TextBox.TextProperty).UpdateTarget();
                }
                catch (Exception ex)
                {
                    ExceptionBox.Show(ex);
                }
            }
            else if (e.Key == Key.Enter)
            {
                UpdateSourceOrTarget();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (double.TryParse(tb?.Text, out double num))
            {
                if (num < Minimum) Value = Minimum;
                else if (num > Maximum) Value = Maximum;
                else Value = num;
                UpdateSourceOrTarget();
            }

        }

        void UpdateSourceOrTarget()
        {
            try
            {

                BindingExpression bindExpr = BindingOperations.GetBindingExpression
                    (TextBox, TextBox.TextProperty);

                bindExpr.UpdateSource();

                if (bindExpr.HasError)
                    bindExpr.UpdateTarget();
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if(tb != null)
            if (double.TryParse(tb.Text, out double num))
            {
                if (num < Minimum) tb.Text = Minimum.ToString(CultureInfo.InvariantCulture);
                else if (num > Maximum) tb.Text = Maximum.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void TextBox_OnTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var text = tb.Text.Insert(tb.CaretIndex, e.Text);

            //e.Handled = !_numMatch.IsMatch(text);
            e.Handled = !double.TryParse(text, out double num) || num < Minimum;
        }
        
        private void SelectAddress(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            if (tb != null)
            {
                tb.SelectAll();
            }
        }

        private void SelectivelyIgnoreMouseButton(object sender,MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBox);
            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }
    }
}
