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
using GeniusBinding.Core;
using System.Reflection.Emit;
using System.Reflection;
using System.Security.Permissions;
using System.Security;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using SilverlightApplicationTestBinding.Model;

namespace SilverlightApplicationTestBinding
{
    [CompilerGenerated]
    [SecurityTreatAsSafeAttribute]
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            MySynchronizationContext ctx = new MySynchronizationContext(this.Dispatcher);
            DataBinder.AddCompiledBinding(tDefault, "Tool.AssemblyPath", t1, "Tool.DefaultPath", ctx);
            DataBinder.AddCompiledBinding(tDefault, "Tool.AssemblyPath", t2, "Tool.DefaultPath", ctx);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyProperty = "tagada tsoin tsoin";
        }

        public string MyProperty { get; set; }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //test binding on another thread
            ToolConfig tc = new ToolConfig();
            MySynchronizationContext ctx = new MySynchronizationContext(this.Dispatcher);
            DataBinder.AddCompiledBinding(tc, "DefaultPath", tDefault, "Tool.AssemblyPath", ctx);

            ThreadPool.QueueUserWorkItem(delegate
            {
                tc.DefaultPath = "my test default";
            });
        }

        #region MySynchronizationContext
        class MySynchronizationContext : SynchronizationContext
        {
            Dispatcher _dispatcher;
            public MySynchronizationContext(Dispatcher d)
            {
                _dispatcher = d;
            }

            public override void Send(SendOrPostCallback fn, object state)
            {
                this._dispatcher.BeginInvoke(fn, state);
            }
        }
        #endregion

        private void OnResetDefault(object sender, RoutedEventArgs e)
        {
            tDefault.Tool.AssemblyPath = null;
        }
    }
}
