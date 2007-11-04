using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Threading;
using System.Windows.Forms;

namespace GeniusBinding.Core.Tests
{
    [TestFixture]
    public class DataBinderTest
    {
        #region methodes utilitaire pour les tests

        private BaseNode CreateSourceTree(string id, int level)
        {
            MyNode<int> Result = new MyNode<int>(id, 0);
            if (level > 0)
            {
                if (level % 2 != 0)
                    Result.Right = CreateSourceTree(string.Format("{0}.{1}", id, level), level - 1);
                else
                    Result.Left = CreateSourceTree(string.Format("{0}.{1}", id, level), level - 1);
            }
            return Result;
        }

        private MyNode<DataWithBindingList> CreateSourceTreeList(string id, int level)
        {
            MyNode<DataWithBindingList> Result = new MyNode<DataWithBindingList>(id, new DataWithBindingList());
            if (level > 0)
            {
                if (level % 2 != 0)
                    Result.Right = CreateSourceTreeList(string.Format("{0}.{1}", id, level), level - 1);
                else
                    Result.Left = CreateSourceTreeList(string.Format("{0}.{1}", id, level), level - 1);
            }
            return Result;
        }

        private MyNode<int> GetFeuille(BaseNode destination, int level)
        {
            return GetFeuille<int>((MyNode<int>)destination, level) ;
        }

        private MyNode<T> GetFeuille<T>(MyNode<T> destination, int level)
        {
            while (level > 0)
            {
                if (level % 2 != 0)
                    destination = (MyNode<T>)destination.Right;
                else
                    destination = (MyNode<T>)destination.Left;
                level--;
            }
            return (MyNode<T>)destination;
        }

        #endregion

        [Test(Description="Test simple binding")]
        public void Test1()
        {
            SourceOfData source = new SourceOfData();
            SourceOfData destination = new SourceOfData();

            DataBinder.AddCompiledBinding(source, "Prop1", destination, "Prop1");

            source.Prop1 = 25;
            Assert.AreEqual(source.Prop1, destination.Prop1,"simple binding doesn't work !");
            source.Prop1 = 155;
            Assert.AreEqual(source.Prop1, destination.Prop1, "simple binding doesn't work !");
        }

        [Test(Description = "Test binding avec un propertypath")]
        public void Test2()
        {
            BaseNode source = CreateSourceTree("root", 2);
            SourceOfData destination = new SourceOfData();

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Prop1");

            MyNode<int> feuille = GetFeuille(source,2);
            feuille.UserData = 25;
            Assert.AreEqual(feuille.UserData, destination.Prop1, "binding doesn't work !");
            feuille.UserData = 155;
            Assert.AreEqual(feuille.UserData, destination.Prop1, "binding doesn't work !");
        }

        [Test(Description = "binding avec un propertypath en changeant l'arborescence source")]
        public void Test3()
        {
            BaseNode source = CreateSourceTree("root",2);
            SourceOfData destination = new SourceOfData();

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Prop1");

            MyNode<int> feuille = GetFeuille(source,2);
            feuille.UserData = 25;
            Assert.AreEqual(feuille.UserData, destination.Prop1, "binding doesn't work !");
            feuille.UserData = 155;
            Assert.AreEqual(feuille.UserData, destination.Prop1, "binding doesn't work !");
            source.Left = CreateSourceTree("childroot",1);

            MyNode<int> feuille1 = GetFeuille(source, 2);
            feuille1.UserData = 157;
            Assert.AreEqual(feuille1.UserData, destination.Prop1, "binding doesn't work !");
            Assert.AreEqual(feuille1.UserData, 157, "binding doesn't work !");
            Assert.AreEqual(157, destination.Prop1, "binding doesn't work !");
        }
        
        [Test(Description = "changement de Left dans le noeud racine, vérification qu'aucune modification n'est excercer sur la destination lors de la modification de l'ancienne valeur de root.Left")]
        public void Test4()
        {
            BaseNode source = CreateSourceTree("root", 2);
            SourceOfData destination = new SourceOfData();

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Prop1");

            MyNode<int> feuille = GetFeuille(source, 2);
            feuille.UserData = 25;
            Assert.AreEqual(feuille.UserData, destination.Prop1, "binding doesn't work !");
            feuille.UserData = 155;
            Assert.AreEqual(feuille.UserData, destination.Prop1, "binding doesn't work !");
            source.Left = CreateSourceTree("childroot", 1);

            MyNode<int> feuille1 = GetFeuille(source, 2);
            feuille1.UserData = 157;
            Assert.AreEqual(feuille1.UserData, destination.Prop1, "binding doesn't work !");
            Assert.AreEqual(feuille1.UserData, 157, "binding doesn't work !");
            Assert.AreEqual(157, destination.Prop1, "binding doesn't work !");

            //changement de l'ancienne valeur
            feuille.UserData = 456;
            Assert.AreEqual(157, destination.Prop1, "binding doesn't work !");
        }

        [Test(Description = "vérification que le binding ne garde pas de références sur les objets, les empêchant ainsi d'être collecté")]
        public void Test5()
        {
            SourceOfData source = new SourceOfData();
            SourceOfData destination = new SourceOfData();
            DataBinder.AddCompiledBinding(source, "Prop1", destination, "Prop1");
            source.Prop1 = 123;
            Assert.AreEqual(123, destination.Prop1, "binding doesn't work !");
            Assert.AreEqual(source.Prop1, destination.Prop1, "binding doesn't work !");
            bool finalized = false;
            destination.OnFinalized += delegate(object sender, EventArgs e)
            {
                finalized = true;
            };
            destination = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.IsTrue(finalized);

            source = new SourceOfData();
            destination = new SourceOfData();
            DataBinder.AddCompiledBinding(source, "Prop1", destination, "Prop1");
            source.Prop1 = 123;
            Assert.AreEqual(123, destination.Prop1, "binding doesn't work !");
            Assert.AreEqual(source.Prop1, destination.Prop1, "binding doesn't work !");
            finalized = false;
            source.OnFinalized += delegate(object sender, EventArgs e)
            {
                finalized = true;
            };
            source = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Assert.IsTrue(finalized);
        }

        [Test(Description = "binding sur un propertyPath en destination")]
        public void Test6()
        {
            BaseNode destination = CreateSourceTree("root", 2);
            SourceOfData source = new SourceOfData();

            DataBinder.AddCompiledBinding(source, "Prop1", destination, "Left.Right.UserData");
            source.Prop1 = 25;
            MyNode<int> feuille = GetFeuille(destination, 2);
            Assert.AreEqual(25, feuille.UserData, "binding doesn't work !");
        }

        [Test(Description = "binding incorrecte")]
        [ExpectedException(typeof(CompiledBindingException))]
        public void Test7()
        {
            BaseNode source = CreateSourceTree("root", 2);
            SourceOfData destination = new SourceOfData();

            DataBinder.AddCompiledBinding(source, "A.B.Z", destination, "Prop1");
        }

        [Test(Description = "binding incorrecte 2")]
        [ExpectedException(typeof(CompiledBindingException))]
        public void Test8()
        {
            BaseNode source = CreateSourceTree("root", 2);
            SourceOfData destination = new SourceOfData();

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "tagada");
        }

        [Test(Description = "changement de Left dans la racine source, et changement de Left dans la racine destination")]
        public void Test9()
        {
            BaseNode source = CreateSourceTree("root", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Left.Right.UserData");

            MyNode<int> feuilleSource = GetFeuille(source, 2);
            MyNode<int> feuilledest = GetFeuille(destination, 2);

            feuilleSource.UserData = 1235;
            Assert.AreEqual(feuilleSource.UserData, feuilledest.UserData);
            Assert.AreEqual(1235, feuilledest.UserData);

            source.Left = CreateSourceTree("root1",1);
            destination.Left = CreateSourceTree("destroot1", 1);
            
            feuilleSource.UserData = 7894;
            //Destination reprend la valeur par défaut
            Assert.AreEqual(0, feuilledest.UserData);
            MyNode<int> feuilledest1 = GetFeuille(destination, 2);

            Assert.AreEqual(0, feuilledest1.UserData);
            MyNode<int> feuilleSource1 = GetFeuille(source, 2);
            feuilleSource1.UserData = 123;
            Assert.AreEqual(123, feuilledest1.UserData);
        }

        [Test(Description = "test ajout/suppression du binding")]
        public void Test10()
        {
            int actual = DataBinder.Bindings.Count;

            BaseNode source = CreateSourceTree("root", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Left.Right.UserData");

            Assert.AreEqual(actual+1, DataBinder.Bindings.Count, "add/remove doesn't work !");
            DataBinder.RemoveCompiledBinding(source, "Left.Right.UserData", destination, "Left.Right.UserData");
            Assert.AreEqual(actual, DataBinder.Bindings.Count, "add/remove doesn't work !");
        }

        [Test(Description = "test Unbind()")]
        public void Test11()
        {
            int actual = DataBinder.Bindings.Count;

            BaseNode source = CreateSourceTree("root", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Left.Right.UserData");

            Assert.AreEqual(actual + 1, DataBinder.Bindings.Count, "add/remove doesn't work !");
            DataBinder.Bindings[0].UnBind();
            Assert.AreEqual(actual, DataBinder.Bindings.Count, "add/remove doesn't work !");
        }

        [Test(Description = "test Enabled/Disabled")]
        public void Test12()
        {
            SourceOfData source = new SourceOfData();
            SourceOfData destination = new SourceOfData();
            DataBinder.AddCompiledBinding(source, "Prop1", destination, "Prop1");
            source.Prop1 = 123;
            Assert.AreEqual(123, destination.Prop1, "binding doesn't work !");
            Assert.AreEqual(source.Prop1, destination.Prop1, "binding doesn't work !");
            Console.WriteLine("binding count : {0}", DataBinder.Bindings.Count);
            DataBinder.Bindings[DataBinder.Bindings.Count-1].Enabled = false;
            source.Prop1 = 456;
            Assert.AreEqual(456, source.Prop1, "binding doesn't work !");
            Assert.AreEqual(123, destination.Prop1, "binding doesn't work !");
            DataBinder.Bindings[DataBinder.Bindings.Count - 1].Enabled = true;
            source.Prop1 = 789;
            Assert.AreEqual(789, destination.Prop1, "binding doesn't work !");
            Assert.AreEqual(source.Prop1, destination.Prop1, "binding doesn't work !");
        }

        [Test(Description = "test Enabled/Disabled by DataBinder.EnableDisableBinding")]
        public void Test13()
        {
            SourceOfData source = new SourceOfData();
            SourceOfData destination = new SourceOfData();
            DataBinder.AddCompiledBinding(source, "Prop1", destination, "Prop1");
            source.Prop1 = 123;
            Assert.AreEqual(123, destination.Prop1, "binding doesn't work !");
            Assert.AreEqual(source.Prop1, destination.Prop1, "binding doesn't work !");
            Console.WriteLine("binding count : {0}", DataBinder.Bindings.Count);
            DataBinder.EnableDisableBinding(source, "Prop1", false);
            source.Prop1 = 456;
            Assert.AreEqual(456, source.Prop1, "binding doesn't work !");
            Assert.AreEqual(123, destination.Prop1, "binding doesn't work !");
            DataBinder.EnableDisableBinding(source, "Prop1", true);
            source.Prop1 = 789;
            Assert.AreEqual(789, destination.Prop1, "binding doesn't work !");
            Assert.AreEqual(source.Prop1, destination.Prop1, "binding doesn't work !");
        }

        [Test(Description = "test binding with null reference in middle of path of source")]
        public void Test14()
        {
            BaseNode source = CreateSourceTree("root", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Left.Right.UserData");

            MyNode<int> feuilleSource = GetFeuille(source, 2);
            MyNode<int> feuilledest = GetFeuille(destination, 2);

            feuilleSource.UserData = 1235;
            Assert.AreEqual(feuilleSource.UserData, feuilledest.UserData);
            Assert.AreEqual(1235, feuilledest.UserData);
            source.Left.Right = null;
            feuilleSource.UserData = 1235;
            Assert.AreEqual(1235, feuilledest.UserData);
        }

        [Test(Description = "test binding with null reference in middle of path destination")]
        public void Test15()
        {
            BaseNode source = CreateSourceTree("root", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Left.Right.UserData");

            MyNode<int> feuilleSource = GetFeuille(source, 2);
            MyNode<int> feuilledest = GetFeuille(destination, 2);

            feuilleSource.UserData = 1235;
            Assert.AreEqual(feuilleSource.UserData, feuilledest.UserData);
            Assert.AreEqual(1235, feuilledest.UserData);
            destination.Left.Right = null;
            feuilleSource.UserData = 1235;
            Assert.AreEqual(1235, feuilledest.UserData);
        }

        [Test(Description = "test binding with null reference in middle of paths before binding")]
        public void Test16()
        {
            BaseNode source = CreateSourceTree("root", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            MyNode<int> feuilleSource = GetFeuille(source, 2);
            MyNode<int> feuilledest = GetFeuille(destination, 2);
            destination.Left.Right = null;
            source.Left.Right = null;
            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Left.Right.UserData");


            feuilleSource.UserData = 1235;
            Assert.AreEqual(0, feuilledest.UserData);
        }

        [Test(Description = "set reference to null before binding and set to valid reference after binding")]
        public void Test17()
        {
            BaseNode source = CreateSourceTree("root", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            MyNode<int> feuilleSource = GetFeuille(source, 2);
            MyNode<int> feuilledest = GetFeuille(destination, 2);
            destination.Left.Right = null;
            source.Left.Right = null;
            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Left.Right.UserData");


            feuilleSource.UserData = 1235;
            Assert.AreEqual(0, feuilledest.UserData);
            
            source.Left.Right = feuilleSource;
            Assert.AreEqual(0, feuilledest.UserData);
            destination.Left.Right = feuilledest;
            Assert.AreEqual(1235, feuilledest.UserData);
        }

        [Test(Description = "test binding with array as destination")]
        public void Test18()
        {
            BaseNode source = CreateSourceTree("root", 2);
            MyNode<DataWithBindingList> destination = CreateSourceTreeList("root dest", 2);

            MyNode<int> feuilleSource = GetFeuille(source, 2);
            feuilleSource.UserData = 789;
            MyNode<DataWithBindingList> feuilledest = GetFeuille<DataWithBindingList>(destination, 2);
            feuilledest.UserData.IntList.Add(123);
            DataBinder.AddCompiledBinding(source, "Left.Right.UserData", destination, "Left.Right.UserData.IntList[0]");

            Assert.AreEqual(789, feuilledest.UserData.IntList[0]);

        }

        [Test(Description = "test binding with an array on source")]
        public void Test19()
        {
            MyNode<DataWithBindingList> source = CreateSourceTreeList("root source", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            MyNode<int> destinationSource = GetFeuille(destination, 2);
            destinationSource.UserData = 789;
            MyNode<DataWithBindingList> feuilleSource = GetFeuille<DataWithBindingList>(source, 2);
            //feuilleSource.UserData.IntList.Add(354);
            DataBinder.AddCompiledBinding(source, "Left.Right.UserData.IntList[0]", destination, "Left.Right.UserData");
            feuilleSource.UserData.IntList.Add(354);

            Assert.AreEqual(354, destinationSource.UserData);

            feuilleSource.UserData.IntList.Add(123);

            Assert.AreEqual(354, destinationSource.UserData);

            feuilleSource.UserData.IntList[0] = 789;

            Assert.AreEqual(789, destinationSource.UserData);
        }

        [Test(Description = "test binding with an array on source, array is already filled")]
        public void Test20()
        {
            MyNode<DataWithBindingList> source = CreateSourceTreeList("root source", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            MyNode<int> destinationSource = GetFeuille(destination, 2);
            destinationSource.UserData = 789;
            MyNode<DataWithBindingList> feuilleSource = GetFeuille<DataWithBindingList>(source, 2);
            feuilleSource.UserData.IntList.Add(354);

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData.IntList[0]", destination, "Left.Right.UserData");

            Assert.AreEqual(354, destinationSource.UserData);
        }

        [Test(Description = "test binding with an array on source, replace instance of array with another, check that old array is disposed")]
        public void Test21()
        {
            bool listfinalized = false;
            MyNode<DataWithBindingList> source = CreateSourceTreeList("root source", 2);
            BaseNode destination = CreateSourceTree("root dest", 2);

            MyNode<int> destinationSource = GetFeuille(destination, 2);
            destinationSource.UserData = 789;
            MyNode<DataWithBindingList> feuilleSource = GetFeuille<DataWithBindingList>(source, 2);
            feuilleSource.UserData.MyIntList.OnFinalized += delegate
            {
                listfinalized = true;
            };
            feuilleSource.UserData.MyIntList.Add(354);

            DataBinder.AddCompiledBinding(source, "Left.Right.UserData.MyIntList[0]", destination, "Left.Right.UserData");

            Assert.AreEqual(354, destinationSource.UserData);
            feuilleSource.UserData.MyIntList = new MyBindingList();
            feuilleSource.UserData.MyIntList.Add(555);
            Assert.AreEqual(555, destinationSource.UserData);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            //checks that Array is really finalized
            Assert.IsTrue(listfinalized);
        }

        class SourceDataThrowException : BaseData
        {
            int _PropGetThrowException;
            public int PropGetThrowException
            {
                get
                {
                    throw new NotImplementedException("PropGetThrowException");
                }
                set
                {
                    if (_PropGetThrowException != value)
                    {
                        _PropGetThrowException = value;
                        DoPropertyChanged("PropGetThrowException");
                    }
                }
            }
        }

        [Test(Description = "Test exception on get")]
        [ExpectedException(typeof(CompiledBindingException))]
        public void Test22()
        {
            SourceDataThrowException source = new SourceDataThrowException();
            SourceOfData destination = new SourceOfData();

            DataBinder.AddCompiledBinding(source, "PropGetThrowException", destination, "Prop1");
        }

        class SourceDataNotifyOnPropertyChanged
        {
            public event EventHandler IntPropertyChanged;
            int _IntProperty;
            public int IntProperty
            {
                get
                {
                    return _IntProperty;
                }
                set
                {
                    if (_IntProperty != value)
                    {
                        _IntProperty = value;
                        if (IntPropertyChanged != null)
                            IntPropertyChanged(this, EventArgs.Empty);
                    }
                }
            }
        }

        [Test(Description = "test notification by 'PropertyName'Changed event")]
        public void Test23()
        {
            SourceDataNotifyOnPropertyChanged source = new SourceDataNotifyOnPropertyChanged();
            source.IntProperty = 123456;
            SourceOfData destination = new SourceOfData();

            DataBinder.AddCompiledBinding(source, "IntProperty", destination, "Prop1");
            Assert.AreEqual(123456, destination.Prop1);
            source.IntProperty = 456789;
            Assert.AreEqual(456789, destination.Prop1);
        }

        class DataToTestSynchronizationContext : BaseData
        {
            int _IntProperty;
            public int IntProperty
            {
                get
                {
                    if (OnGet != null)
                        OnGet(this, EventArgs.Empty);
                    return _IntProperty;
                }
                set
                {
                    if (_IntProperty != value)
                    {
                        if (OnSet != null)
                            OnSet(this, EventArgs.Empty);
                        _IntProperty = value;
                        DoPropertyChanged("IntProperty");
                    }
                }
            }

            public event EventHandler OnGet;
            public event EventHandler OnSet;
        }

        [Test(Description = "test binding with synchronization context")]
        public void Test24()
        {
            int currentThId = -1;
            int getThId = 0, setThId = 0;
            ManualResetEvent waitevent = new ManualResetEvent(false);
            DataToTestSynchronizationContext source = new DataToTestSynchronizationContext();
            source.IntProperty = 123456;
            DataToTestSynchronizationContext destination = new DataToTestSynchronizationContext();

            new Thread((ThreadStart)delegate
            {
                //Create a synchronizationcontext
                Button b = new Button();
                bool propHasSet = false;
                currentThId = Thread.CurrentThread.ManagedThreadId;
                ExecutionContext exCtx = Thread.CurrentThread.ExecutionContext;

                DataBinder.AddCompiledBinding(source, "IntProperty", destination, "IntProperty", SynchronizationContext.Current);
                Assert.AreEqual(123456, destination.IntProperty);

                source.OnGet += delegate
                {
                    getThId = Thread.CurrentThread.ManagedThreadId;
                };

                destination.OnSet += delegate
                {
                    setThId = Thread.CurrentThread.ManagedThreadId;
                    propHasSet = true;
                };

                Thread th = new Thread((ThreadStart)delegate
                {
                    source.IntProperty = 456789;

                });
                th.Start();
                while (!propHasSet)
                {
                    Application.DoEvents();
                }
                waitevent.Set();
            }).Start();
            waitevent.WaitOne();
            Assert.AreEqual(456789, destination.IntProperty);
            Assert.AreEqual(currentThId, getThId);
            Assert.AreEqual(currentThId, setThId);

        }

    }
}
