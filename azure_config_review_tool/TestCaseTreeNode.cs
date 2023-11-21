using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace azure_administration_tool1
{
    public class TestCaseTreeNode : TreeNode
    {
        public ChildNodeCountType ChildNodeCountType { get; set; }
        public int AggregateChildNodeCountValue { get; set; }
        public TestCase TestCase { get; set; }
        public string TestCaseCategoryLabel { get; set; }

        public TestCaseTreeNode() : base()
        {
            ChildNodeCountType = ChildNodeCountType.None;
            AggregateChildNodeCountValue = 0;
            TestCase = null;
            TestCaseCategoryLabel = null;
        }
    }
}
