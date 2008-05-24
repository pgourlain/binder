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
    public class BinaryOperator : NotifyPropertyChangedObject
    {

        protected override void DoNotifyPropertyChanged(string propName)
        {
            base.DoNotifyPropertyChanged(propName);
            if (propName != "OutValue")
            {
                OutValue = Left + Right;
            }
        }

        double _Left;
        public double Left 
        {
            get
            {
                return _Left;
            }
            set
            {
                if (value != _Left)
                {
                    _Left = value;
                    DoNotifyPropertyChanged("Left");
                }
            }
        }

        double _Right;
        public double Right
        {
            get
            {
                return _Right;
            }
            set
            {
                if (value != _Right)
                {
                    _Right = value;
                    DoNotifyPropertyChanged("Right");
                }
            }
        }

        double _OutValue;
        public double OutValue
        {
            get
            {
                return _OutValue;
            }
            set
            {
                if (value != _OutValue)
                {
                    _OutValue = value;
                    DoNotifyPropertyChanged("OutValue");
                }
            }
        }
    }
}
