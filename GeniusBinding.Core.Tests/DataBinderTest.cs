using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

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

        private MyNode<int> GetFeuille(BaseNode destination, int level)
        {
            while (level > 0)
            {
                if (level % 2 != 0)
                    destination = destination.Right;
                else
                    destination = destination.Left;
                level--;
            }
            return (MyNode<int>)destination;
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
            Assert.AreEqual(1235, feuilledest.UserData);
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

        //[Test(Description = "")]
        //public void Test12()
        //{
        //}
        //[Test(Description = "")]
        //public void Test13()
        //{
        //}
        //[Test(Description = "")]
        //public void Test14()
        //{
        //}
        //[Test(Description = "")]
        //public void Test15()
        //{
        //}
        //[Test(Description = "")]
        //public void Test16()
        //{
        //}
        //[Test(Description = "")]
        //public void Test17()
        //{
        //}
        //[Test(Description = "")]
        //public void Test18()
        //{
        //}
        //[Test(Description = "")]
        //public void Test19()
        //{
        //}
        //[Test(Description = "")]
        //public void Test20()
        //{
        //}
    }
}
