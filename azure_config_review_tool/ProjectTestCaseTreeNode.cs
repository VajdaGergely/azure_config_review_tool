using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azure_administration_tool1
{
    public class ProjectTestCaseTreeNode : TestCaseTreeNode
    {
        public ProjectTestCase ProjectTestCase { get; set; }

        public ProjectTestCaseTreeNode() : base()
        {
            ProjectTestCase = null;
        }
    }
}
