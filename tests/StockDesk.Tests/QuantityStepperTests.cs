using System;
using System.Threading;
using System.Windows.Data;
using StockDesk.Controls;
using Xunit;

namespace StockDesk.Tests;

public class QuantityStepperTests
{
    private void RunInSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            throw new AggregateException("STA thread exception", exception);
        }
    }

    [Fact]
    public void QuantityStepper_Defaults_AreConfiguredProperly()
    {
        RunInSta(() =>
        {
            var stepper = new QuantityStepper();

            Assert.Equal(1, stepper.Value);
            Assert.Equal(1, stepper.Minimum);
            Assert.Equal(int.MaxValue, stepper.Maximum);
            Assert.Equal(1, stepper.Step);
            Assert.False(stepper.IsDecrementEnabled); // Value == Minimum (1)
            Assert.True(stepper.IsIncrementEnabled);
        });
    }

    [Fact]
    public void QuantityStepper_IncrementAndDecrement_UpdatesValueAndButtonStates()
    {
        RunInSta(() =>
        {
            var stepper = new QuantityStepper
            {
                Minimum = 1,
                Maximum = 3,
                Value = 1
            };

            Assert.False(stepper.IsDecrementEnabled);
            Assert.True(stepper.IsIncrementEnabled);

            stepper.Increment();
            Assert.Equal(2, stepper.Value);
            Assert.True(stepper.IsDecrementEnabled);
            Assert.True(stepper.IsIncrementEnabled);

            stepper.Increment();
            Assert.Equal(3, stepper.Value);
            Assert.True(stepper.IsDecrementEnabled);
            Assert.False(stepper.IsIncrementEnabled); // reached maximum

            // Further increment should be blocked
            stepper.Increment();
            Assert.Equal(3, stepper.Value);

            // Decrement back
            stepper.Decrement();
            Assert.Equal(2, stepper.Value);
            Assert.True(stepper.IsDecrementEnabled);
            Assert.True(stepper.IsIncrementEnabled);

            stepper.Decrement();
            Assert.Equal(1, stepper.Value);
            Assert.False(stepper.IsDecrementEnabled);
            Assert.True(stepper.IsIncrementEnabled);
        });
    }

    [Fact]
    public void QuantityStepper_ValueSetting_ClampsToBounds()
    {
        RunInSta(() =>
        {
            var stepper = new QuantityStepper
            {
                Minimum = 1,
                Maximum = 5
            };

            stepper.Value = 10;
            Assert.Equal(5, stepper.Value);

            stepper.Value = -2;
            Assert.Equal(1, stepper.Value);
        });
    }

    [Fact]
    public void QuantityStepper_TwoWayBinding_SynchronizesWithSource()
    {
        RunInSta(() =>
        {
            var source = new TestSourceModel { Count = 5 };
            var stepper = new QuantityStepper
            {
                Minimum = 1,
                Maximum = 10
            };

            var binding = new Binding(nameof(TestSourceModel.Count))
            {
                Source = source,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(stepper, QuantityStepper.ValueProperty, binding);

            Assert.Equal(5, stepper.Value);

            stepper.Increment();
            Assert.Equal(6, source.Count);

            stepper.Decrement();
            Assert.Equal(5, source.Count);

            source.Count = 9;
            Assert.Equal(9, stepper.Value);
        });
    }

    private class TestSourceModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        private int _count;
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }
    }
}
