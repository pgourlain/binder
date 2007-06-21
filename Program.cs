using System;
using System.Collections.Generic;
using System.Text;
using GeniusBinding.Core;
using System.Diagnostics;

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

            Console.WriteLine("Enter to exit...");
            Console.ReadLine();

        }

        private static void TestSimplebindingWithconverter()
        {
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
