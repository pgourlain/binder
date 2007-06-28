using System;
using System.Collections.Generic;
using System.Text;
using GeniusBinding.Core;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Reflection;

namespace TestMyBinding
{
    class Program
    {
        static void Main(string[] args)
        {
            TestSimplebinding();
            TestBidirectinnelbinding();
            TestBidirectinnelbindingAndunreferenced();
            TestSimplebindingWithconverter();
            TestRecursiveBinding();

            Console.WriteLine("Enter to exit...");
            Console.ReadLine();
        }

        private static void TestRecursiveBinding()
        {
            Console.WriteLine("test Recursive binding with converter");
            DestinationOfData d = new DestinationOfData();
            SourceOfData s = new SourceOfData();
            DataBinder.AddCompiledBinding(s, "Prop1", d, "Prop1DestDouble", new MyConverter());
            DataBinder.AddCompiledBinding(d, "Prop1DestDouble", s, "Prop1", new MyConverter());

            s.Prop1 = 1234;
        }

        class MyConverter : IBinderConverter<int, int>, IBinderConverter<double, int>
            , IBinderConverter<Point, int>
            , IBinderConverter<int, double>
        {
            #region IBinderConverter<double,int> Members

            public double Convert(int value)
            {
                Console.WriteLine("Converter<double,int> = value+10");
                return value + 10;
            }

            #endregion

            #region IBinderConverter<int,int> Members

            int IBinderConverter<int, int>.Convert(int value)
            {
                Console.WriteLine("Converter<int,int> = value + 1");
                return value + 1;
            }

            #endregion

            #region IBinderConverter<Point,int> Members

            Point IBinderConverter<Point, int>.Convert(int value)
            {
                Console.WriteLine("Converter<Point,int>");
                return new Point(value, value);
            }

            #endregion

            #region IBinderConverter<int,double> Members

            public int Convert(double value)
            {
                return (int)value;
            }

            #endregion
        }

        private static void TestSimplebindingWithconverter()
        {
            Console.WriteLine("test TestSimple binding with converter");
            DestinationOfData d = new DestinationOfData();
            SourceOfData s = new SourceOfData();
            DataBinder.AddCompiledBinding(s, "Prop1", d, "Prop1DestDouble", new MyConverter());
            DataBinder.AddCompiledBinding(s, "Prop1", d, "PropPoint", new MyConverter());

            s.Prop1 = 1234;
        }

        private static void TestBidirectinnelbindingAndunreferenced()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("TestBidirectinnelbindingAndunreferenced");
            DestinationOfData d = new DestinationOfData();
            SourceOfData s = new SourceOfData();

            DataBinder.AddCompiledBinding(s, "Prop1", d, "Prop1Dest");
            DataBinder.AddCompiledBinding(d, "Prop1Dest", s, "Prop1");
            Console.WriteLine("-> try  set 123 on source");
            s.Prop1 = 123;
            Console.WriteLine("-> try  set 456 on dest");
            d.Prop1Dest = 456;
            Console.WriteLine("set destination to null");
            d = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine("-> now, try  set 123 on source");
            s.Prop1 = 123;
        }

        private static void TestBidirectinnelbinding()
        {
            Console.WriteLine("TestBidirectinnelbinding");
            DestinationOfData d = new DestinationOfData();
            SourceOfData s = new SourceOfData();

            DataBinder.AddCompiledBinding(s, "Prop1", d, "Prop1Dest");
            DataBinder.AddCompiledBinding(d, "Prop1Dest", s, "Prop1");
            Console.WriteLine("-> try  set 123 on source");
            s.Prop1 = 123;
            Console.WriteLine("-> try  set 456 on dest");
            d.Prop1Dest = 456;
        }

        private static void TestSimplebinding()
        {
            Console.WriteLine("Test simple binding");
            DestinationOfData d = new DestinationOfData();
            SourceOfData s = new SourceOfData();

            DataBinder.AddCompiledBinding(s, "Prop1", d, "Prop1Dest");
            Console.WriteLine("-> try  set 123 on source");
            s.Prop1 = 123;
        }

        private static void Dump(DestinationOfData d)
        {
            Console.WriteLine("destination DestProp1 :" + d.Prop1Dest);
        }

        private static void Dump(SourceOfData s)
        {
            Console.WriteLine("source Prop1 :" + s.Prop1);
        }
    }
}
