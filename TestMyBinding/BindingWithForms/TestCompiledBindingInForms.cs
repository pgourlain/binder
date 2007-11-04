using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GeniusBinding.Core;
using System.Threading;

namespace TestMyBinding.BindingWithForms
{
    public partial class TestCompiledBindingInForms : Form
    {
        public TestCompiledBindingInForms()
        {
            InitializeComponent();
        }

        class MyData : BaseData
        {
            private string _prop1;

            public string TextPropertySample
            {
                get { return _prop1; }
                set 
                {
                    if (_prop1 != value)
                    {
                        _prop1 = value;
                        this.DoPropertyChanged("TextPropertySample");
                    }
                }
            }

        }

        MyData _CurrentData = new MyData();

        class StringToBoolean : IBinderConverter<bool, string>
        {

            #region IBinderConverter<bool,string> Members

            public bool Convert(string value)
            {
                return !string.IsNullOrEmpty(value);
            }

            #endregion
        }

        private void TestCompiledBindingInForms_Load(object sender, EventArgs e)
        {
            DataBinder.AddCompiledBinding(textBox1, "Text", _CurrentData, "TextPropertySample");
            DataBinder.AddCompiledBinding(_CurrentData, "TextPropertySample", textBox1, "Text");
            DataBinder.AddCompiledBinding(_CurrentData, "TextPropertySample", lblTextboxCopy, "Text");

            //DataBinder.AddCompiledBinding(_CurrentData, "TextPropertySample", button1, "Enabled", new StringToBoolean());
            DataBinder.AddCompiledBinding(_CurrentData, "TextPropertySample", button1, "Enabled", DataBinder.CreateConverter<bool, string>(delegate(string value)
            {
                if (value == "exception")
                    throw new Exception("value cannot be converted");
                return !string.IsNullOrEmpty(value);
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MyData data = new MyData();
            DataBinder.AddCompiledBinding(data, "TextPropertySample", textBox2, "Text", SynchronizationContext.Current);
            ThreadPool.QueueUserWorkItem((WaitCallback)delegate
            {
                for (int i = 0; i < 1000; i++)
                {
                    data.TextPropertySample = i.ToString();
                    Thread.Sleep(5);
                }
            });


        }
    }
}