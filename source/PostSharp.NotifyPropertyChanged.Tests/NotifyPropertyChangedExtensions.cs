using System.Collections.Generic;
using System.ComponentModel;

namespace PostSharp.NotifyPropertyChanged.Tests
{
    public static class NotifyPropertyChangedExtensions
    {
        public static List<string> ObservePropertyChanges(this object @this)
        {
            var propertyChanges = new List<string>();
            var viewModel = @this as INotifyPropertyChanged;
            if (viewModel != null)
                viewModel.PropertyChanged += (sender, args) => propertyChanges.Add(args.PropertyName);
            return propertyChanges;
        }
    }
}