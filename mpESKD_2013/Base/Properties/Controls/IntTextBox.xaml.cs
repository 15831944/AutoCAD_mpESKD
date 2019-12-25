namespace mpESKD.Base.Properties.Controls
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using ModPlusAPI.Windows;

    public partial class IntTextBox
    {
        /// <summary>
        /// Свойство зависимостей для свойства Value
        /// </summary>
        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register("Value", typeof(int?), typeof(IntTextBox),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Числовое значение или null.
        /// Если null - в текстовом окошке выводится ""
        /// </summary>
        public int? Value
        {
            get => (int?)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Maximum value for the Numeric Up Down control
        /// </summary>
        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(IntTextBox), new UIPropertyMetadata(int.MaxValue));

        /// <summary>
        /// Minimum value of the numeric up down conrol.
        /// </summary>
        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(IntTextBox), new UIPropertyMetadata(0));

        public IntTextBox()
        {
            InitializeComponent();
        }

        private void TextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Cancel || e.Key == Key.Escape)
            {
                try
                {
                    BindingOperations.GetBindingExpression(TextBox, TextBox.TextProperty)?.UpdateTarget();
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

        private void TextBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UpdateSourceOrTarget();
        }

        void UpdateSourceOrTarget()
        {
            try
            {
                BindingExpression bindExpr = BindingOperations.GetBindingExpression(
                    TextBox, TextBox.TextProperty);

                bindExpr?.UpdateSource();

                if (bindExpr != null && bindExpr.HasError)
                {
                    bindExpr.UpdateTarget();
                }
            }
            catch (Exception ex)
            {
                ExceptionBox.Show(ex);
            }
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            if (Value < Maximum)
            {
                Value++;
                RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
            }
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            if (Value > Minimum)
            {
                Value--;
                RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
            }
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (int.TryParse(tb.Text, out int num))
                {
                    if (num < Minimum)
                    {
                        tb.Text = Minimum.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (num > Maximum)
                    {
                        tb.Text = Maximum.ToString(CultureInfo.InvariantCulture);
                    }

                    // if (num < Minimum) Value = Minimum;
                    // else if (num > Maximum) Value = Maximum;
                    // else Value = num;
                }
            }
        }

        // Increase button clicked
        private static readonly RoutedEvent IncreaseClickedEvent =
            EventManager.RegisterRoutedEvent("IncreaseClicked", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(IntTextBox));

        /// <summary>The IncreaseClicked event is called when the Increase button clicked</summary>
        public event RoutedEventHandler IncreaseClicked
        {
            add => AddHandler(IncreaseClickedEvent, value);
            remove => RemoveHandler(IncreaseClickedEvent, value);
        }

        // Increase button clicked
        private static readonly RoutedEvent DecreaseClickedEvent =
            EventManager.RegisterRoutedEvent("DecreaseClicked", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(IntTextBox));

        /// <summary>The DecreaseClicked event is called when the Decrease button clicked</summary>
        public event RoutedEventHandler DecreaseClicked
        {
            add => AddHandler(DecreaseClickedEvent, value);
            remove => RemoveHandler(DecreaseClickedEvent, value);
        }

        private void TextBox_OnTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var text = tb.Text.Insert(tb.CaretIndex, e.Text);

            // e.Handled = !_numMatch.IsMatch(text);
            e.Handled = !int.TryParse(text, out int num);
        }

        private void SelectAddress(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb?.SelectAll();
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (TextBox)sender;
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
