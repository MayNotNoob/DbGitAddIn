using System;
using System.ComponentModel;
using System.Diagnostics;

namespace DbGitAddIn.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            this.VerifyPropertyName(name);

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real, 
            // public, instance property on this object. 
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;
                throw new ArgumentException(msg);

            }
        }
    }
}