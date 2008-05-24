using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace SilverlightApplicationTestBinding.Model
{
    public class ToolConfig : INotifyPropertyChanged
    {

        private string _defaultPath;

        public string DefaultPath
        {
            get
            {
                return _defaultPath;
            }
            set
            {
                if (_defaultPath != value)
                {
                    _defaultPath = value;
                    DoPropertyChanged("DefaultPath");
                }
            }
        }

        private string _AssemblyPath;
        public string AssemblyPath 
        {
            get { return _AssemblyPath; }
            set
            {
                if (value != _AssemblyPath)
                {
                    _AssemblyPath = value;
                    DoPropertyChanged("AssemblyPath");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void DoPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion
    }
}
