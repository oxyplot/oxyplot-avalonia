using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace AvaloniaExamples
{
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Compat
        protected void SetValue<T>(ref T prop, T value, Expression<Func<object>> ignored,
            [CallerMemberName] string propertyName = null)
            => SetValue(ref prop, value, propertyName);

        protected void RaisePropertyChanged(Expression<Func<object>> ignored, [CallerMemberName] string propertyName = null)
            => OnPropertyChanged(propertyName);
        
        protected void SetValue<T>(ref T prop, T value, [CallerMemberName] string propertyName = null)
        {
            prop = value;
            OnPropertyChanged(propertyName);
        }
    }
}