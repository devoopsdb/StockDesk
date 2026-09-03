using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockDesk.Controls;

public partial class QuantityStepper : UserControl
{
    private bool _isInternalTextChange;

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(QuantityStepper),
            new FrameworkPropertyMetadata(
                1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnValueChanged,
                CoerceValue));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(int),
            typeof(QuantityStepper),
            new PropertyMetadata(1, OnMinimumChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(int),
            typeof(QuantityStepper),
            new PropertyMetadata(int.MaxValue, OnMaximumChanged));

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(int),
            typeof(QuantityStepper),
            new PropertyMetadata(1));

    private static readonly DependencyPropertyKey IsDecrementEnabledPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsDecrementEnabled),
            typeof(bool),
            typeof(QuantityStepper),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsDecrementEnabledProperty =
        IsDecrementEnabledPropertyKey.DependencyProperty;

    public bool IsDecrementEnabled
    {
        get => (bool)GetValue(IsDecrementEnabledProperty);
        private set => SetValue(IsDecrementEnabledPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsIncrementEnabledPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsIncrementEnabled),
            typeof(bool),
            typeof(QuantityStepper),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsIncrementEnabledProperty =
        IsIncrementEnabledPropertyKey.DependencyProperty;

    public bool IsIncrementEnabled
    {
        get => (bool)GetValue(IsIncrementEnabledProperty);
        private set => SetValue(IsIncrementEnabledPropertyKey, value);
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public int Step
    {
        get => (int)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public QuantityStepper()
    {
        InitializeComponent();
        DataObject.AddPastingHandler(ValueTextBox, OnPasting);
        UpdateTextBoxText();
        UpdateButtonStates();
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        if (d is QuantityStepper stepper && baseValue is int val)
        {
            int max = Math.Max(stepper.Minimum, stepper.Maximum);
            if (val < stepper.Minimum) return stepper.Minimum;
            if (val > max) return max;
            return val;
        }
        return baseValue;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is QuantityStepper stepper)
        {
            stepper.UpdateTextBoxText();
            stepper.UpdateButtonStates();
        }
    }

    private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is QuantityStepper stepper)
        {
            stepper.CoerceValue(ValueProperty);
            stepper.UpdateButtonStates();
        }
    }

    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is QuantityStepper stepper)
        {
            stepper.CoerceValue(ValueProperty);
            stepper.UpdateButtonStates();
        }
    }

    public void Decrement()
    {
        if (Value > Minimum)
        {
            Value = Math.Max(Minimum, Value - Step);
        }
    }

    public void Increment()
    {
        if (Value < Maximum)
        {
            Value = Math.Min(Maximum, Value + Step);
        }
    }

    private void OnDecrementClick(object sender, RoutedEventArgs e)
    {
        Decrement();
    }

    private void OnIncrementClick(object sender, RoutedEventArgs e)
    {
        Increment();
    }

    private void UpdateTextBoxText()
    {
        if (ValueTextBox == null) return;
        var text = Value.ToString();
        if (ValueTextBox.Text != text)
        {
            _isInternalTextChange = true;
            try
            {
                ValueTextBox.Text = text;
            }
            finally
            {
                _isInternalTextChange = false;
            }
        }
    }

    private void UpdateButtonStates()
    {
        IsDecrementEnabled = Value > Minimum;
        IsIncrementEnabled = Value < Maximum;
    }

    private void OnTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        foreach (char c in e.Text)
        {
            if (!char.IsDigit(c))
            {
                e.Handled = true;
                return;
            }
        }
    }

    private void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            var text = (string)e.DataObject.GetData(DataFormats.Text);
            if (!int.TryParse(text, out _))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInternalTextChange) return;

        if (int.TryParse(ValueTextBox.Text, out int parsed))
        {
            int max = Math.Max(Minimum, Maximum);
            int clamped = Math.Clamp(parsed, Minimum, max);
            if (clamped != Value)
            {
                Value = clamped;
            }
        }
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
        UpdateTextBoxText();
    }

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            Increment();
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            Decrement();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            UpdateTextBoxText();
            e.Handled = true;
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        if (e.Delta > 0)
        {
            Increment();
            e.Handled = true;
        }
        else if (e.Delta < 0)
        {
            Decrement();
            e.Handled = true;
        }
    }
}
