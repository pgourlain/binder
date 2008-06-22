using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using SilverlightApplicationTestBinding.Model;
using System.Windows.Data;

namespace SilverlightApplicationTestBinding
{
    public partial class ToolConfigUC : UserControl, INotifyPropertyChanged
    {
        ToolConfig _tool;

        public ToolConfigUC()
        {
            _tool = new ToolConfig();
            InitializeComponent();
            Binding b = new Binding("Tool.AssemblyPath");
            b.Source = this;
            b.Mode = BindingMode.TwoWay;
            wt.SetBinding(TextBox.TextProperty, b);

            //b = new Binding("Tool.DefaultPath");
            //b.Source = this;
            //b.Mode = BindingMode.TwoWay;
            //wt.SetBinding(WatermarkedTextBox.WatermarkProperty, b);
        }

        public ToolConfig Tool
        {
            get
            {
                return _tool;
            }
            set
            {
                _tool = value;
                DoPropertyChanged("Tool");
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
