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
using SilverlightApplicationTestBinding.Model;
using GeniusBinding.Core;
using System.Windows.Data;

namespace SilverlightApplicationTestBinding
{
    public partial class OperatorsBinding : UserControl
    {
        List<Object> _objects = new List<object>();

        public OperatorsBinding()
        {
            InitializeComponent();

            _objects.Add(new SingleValue());
            _objects.Add(new SingleValue());
            _objects.Add(new SingleValue());
            _objects.Add(new BinaryOperator());

            //ui binding
            Binding b = new Binding("Value");
            b.Source = _objects[0];
            b.Mode = BindingMode.TwoWay;
            vLeft.SetBinding(TextBox.TextProperty, b);
            
            b = new Binding("Value");
            b.Source = _objects[1];
            b.Mode = BindingMode.TwoWay;
            vRight.SetBinding(TextBox.TextProperty, b);

            b = new Binding("Value");
            b.Source = _objects[2];
            b.Mode = BindingMode.TwoWay;
            vResult.SetBinding(TextBlock.TextProperty, b);

        }

        public List<object> Objects { get { return _objects; } }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataBinder.AddCompiledBinding(this, "Objects[0].Value", this, "Objects[3].Left");
            DataBinder.AddCompiledBinding(this, "Objects[1].Value", this, "Objects[3].Right");
            DataBinder.AddCompiledBinding(this, "Objects[3].OutValue", this, "Objects[2].Value");
        }
    }
}
