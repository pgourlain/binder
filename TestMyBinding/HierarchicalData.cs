using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace TestMyBinding
{
    class HierarchicalData : BaseData
    {
        private static int _idcount= 1;
        public static int GetId()
        {
            return _idcount++;
        }

        private string _id;
        public HierarchicalData()
        {
            _id = GetId().ToString();
            _A = new HierarchicalDataA(_id);
        }

        private HierarchicalDataA _A;

        public HierarchicalDataA A
        {
            get { return _A; }
            set { _A = value; DoPropertyChanged("A"); }
        }

    }

    class HierarchicalDataA : BaseData
    {
        private string _id;
        public HierarchicalDataA(string id)
        {
            _id = id;
            _B = new HierarchicalDataB(_id);
        }

        private HierarchicalDataB _B;

        public HierarchicalDataB B
        {
            get { return _B; }
            set { _B = value; DoPropertyChanged("B"); }
        }

    }

    class HierarchicalDataB : BaseData
    {
        private string _id;
        public HierarchicalDataB(string id)
        {
            _id = id;
        }

        private SourceOfData _C = new SourceOfData();

        public SourceOfData C
        {
            get { return _C; }
            set { _C = value; DoPropertyChanged("C"); }
        }

    }
}
