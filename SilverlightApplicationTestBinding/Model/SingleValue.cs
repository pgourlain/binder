using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightApplicationTestBinding.Model
{
    public class SingleValue : NotifyPropertyChangedObject
    {
        double _Value;
        public double Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (value != _Value)
                {
                    _Value = value;
                    DoNotifyPropertyChanged("Value");
                }
            }
        }
    }
}
