using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace azure_administration_tool1
{
    public enum ChildNodeCountType
    {
        None = 0,
        Normal = 1,
        Aggregate = 2
    }

    public class TestCaseTreeView : TreeView
    {
        public TestCaseTreeView()
        {
            ProjectNode = null;
        }

        public TestCaseTreeNode ProjectNode { get; set; }

        public new TestCaseTreeNode SelectedNode
        {
            get
            {
                return (TestCaseTreeNode)base.SelectedNode;
            }

            set
            {
                base.SelectedNode = value;
            }
        }

        public void CalculateChildCounts(TestCaseTreeNode treeNode)
        {
            //deal with only those nodes that have child nodes
            if (treeNode.Nodes.Count > 0)
            {
                //stepping one level lower in the structure to deal with child nodes
                foreach (TestCaseTreeNode childNode in treeNode.Nodes)
                {
                    CalculateChildCounts(childNode);
                }

                /* do work related to the actual node */
                //set aggregate child node count
                int lastLevelElementCount = 0;
                foreach (TestCaseTreeNode tempChildNode in treeNode.Nodes)
                {
                    if (tempChildNode.AggregateChildNodeCountValue == 0)
                    {
                        //last level elements (only testCase elements matters, testCaseCategory elements are not
                        if (tempChildNode.TestCase != null)
                        {
                            lastLevelElementCount++;
                        }
                    }
                    else
                    {
                        //aggregate child node count has already been set
                        treeNode.AggregateChildNodeCountValue += tempChildNode.AggregateChildNodeCountValue;
                    }
                }

                treeNode.AggregateChildNodeCountValue += lastLevelElementCount;
            }
            else
            {
                treeNode.AggregateChildNodeCountValue = 0;
            }
        }

        public void ShowChildCounts(TestCaseTreeNode treeNode)
        {
            //recursion exit condition
            if (treeNode.Nodes.Count > 0)
            {
                //recursion loop
                foreach (TestCaseTreeNode childNode in treeNode.Nodes)
                {
                    ShowChildCounts(childNode);
                }
            }

            //task to do on every item that is testCaseCategory item (and not testCase item)
            if (treeNode.TestCaseCategoryLabel != null)
            {
                switch (treeNode.ChildNodeCountType)
                {
                    case ChildNodeCountType.Normal:
                        treeNode.Text = treeNode.TestCaseCategoryLabel + " [" + treeNode.Nodes.Count.ToString() + "]";
                        break;
                    case ChildNodeCountType.Aggregate:
                        treeNode.Text = treeNode.TestCaseCategoryLabel + " [" + treeNode.AggregateChildNodeCountValue.ToString() + "]";
                        break;
                    case ChildNodeCountType.None:
                        treeNode.Text = treeNode.TestCaseCategoryLabel;
                        break;
                }
            }
        }

        public void HideChildCounts(TestCaseTreeNode treeNode)
        {
            //recursion exit condition
            if (treeNode.Nodes.Count > 0)
            {
                //recursion loop
                foreach (TestCaseTreeNode childNode in treeNode.Nodes)
                {
                    HideChildCounts(childNode);
                }

                //task to do on every item
                if (treeNode.TestCaseCategoryLabel != null)
                {
                    treeNode.Text = treeNode.TestCaseCategoryLabel;
                }
            }
        }

        public void Init()
        {
            TestCaseTreeNode newNode = new TestCaseTreeNode()
            {
                Name = "all_test_cases",
                Text = "All test cases",
                ChildNodeCountType = ChildNodeCountType.None,
                TestCaseCategoryLabel = "all_test_cases"
            };
            this.Nodes.Add(newNode);

            TestCaseTreeNode newNode2 = new TestCaseTreeNode()
            {
                Name = "by_resource_type",
                Text = "By resource type",
                ChildNodeCountType = ChildNodeCountType.Aggregate,
                TestCaseCategoryLabel = "by_resource_type"
            };
            this.Nodes["all_test_cases"].Nodes.Add(newNode2);

            TestCaseTreeNode newNode3 = new TestCaseTreeNode()
            {
                Name = "by_test_case_category",
                Text = "By test case category",
                ChildNodeCountType = ChildNodeCountType.Aggregate,
                TestCaseCategoryLabel = "by_test_case_category"
            };
            this.Nodes["all_test_cases"].Nodes.Add(newNode3);

            TestCaseTreeNode newNode4 = new TestCaseTreeNode()
            {
                Name = "in_scope",
                Text = "in scope",
                ChildNodeCountType = ChildNodeCountType.Normal,
                TestCaseCategoryLabel = "in_scope"
            };
            this.Nodes["all_test_cases"].Nodes["by_test_case_category"].Nodes.Add(newNode4);

            TestCaseTreeNode newNode5 = new TestCaseTreeNode()
            {
                Name = "not_in_scope",
                Text = "not in scope",
                ChildNodeCountType = ChildNodeCountType.Normal,
                TestCaseCategoryLabel = "not_in_scope"
            };
            this.Nodes["all_test_cases"].Nodes["by_test_case_category"].Nodes.Add(newNode5);

            TestCaseTreeNode newNode6 = new TestCaseTreeNode()
            {
                Name = "project_test_cases",
                Text = "Project test cases",
                ChildNodeCountType = ChildNodeCountType.Aggregate,
                TestCaseCategoryLabel = "project_test_cases"
            };
            this.Nodes.Add(newNode6);

            //this.Nodes["project_test_cases"].Nodes.Add((TreeNode)newNode4.Clone());
            //this.Nodes["project_test_cases"].Nodes.Add((TreeNode)newNode5.Clone());

            this.Nodes["all_test_cases"].ExpandAll();
            this.Nodes["project_test_cases"].ExpandAll();
        }

        public void AddAllTestCaseFromTestCaseRepo(List<TestCase> testCaseList)
        {
            foreach (TestCase testCase in testCaseList)
            {
                /* add to 'by_resource_type' node */

                //create resource type node if it is not already exists
                string resourceTypeNodeName = "all_test_cases_by_resource_type_" + testCase.ResourceType;
                if (!this.Nodes["all_test_cases"].Nodes["by_resource_type"].Nodes.ContainsKey(resourceTypeNodeName))
                {
                    TestCaseTreeNode newNode = new TestCaseTreeNode()
                    {
                        Name = resourceTypeNodeName,
                        Text = testCase.ResourceType,
                        ChildNodeCountType = ChildNodeCountType.Normal,
                        TestCaseCategoryLabel = testCase.ResourceType
                    };
                    this.Nodes["all_test_cases"].Nodes["by_resource_type"].Nodes.Add(newNode);
                }

                //add test case node to the corresponding resourceType node
                TestCaseTreeNode newNode2 = new TestCaseTreeNode()
                {
                    Name = "all_test_cases_by_resource_type_" + testCase.Name,
                    Text = testCase.TestCaseId,
                    ChildNodeCountType = ChildNodeCountType.None,
                    TestCase = testCase
                };
                this.Nodes["all_test_cases"].Nodes["by_resource_type"].Nodes[resourceTypeNodeName].Nodes.Add(newNode2);


                /* add to 'by_test_case_category' node */

                //add test case node to the corresponding test case category node
                TestCaseTreeNode newNode3 = new TestCaseTreeNode()
                {
                    Name = "all_test_cases_by_test_case_category" + testCase.Name,
                    Text = testCase.TestCaseId,
                    ChildNodeCountType = ChildNodeCountType.None,
                    TestCase = testCase
                };
                if (!testCase.isBlacklisted)
                {
                    this.Nodes["all_test_cases"].Nodes["by_test_case_category"].Nodes["in_scope"].Nodes.Add(newNode3);
                }
                else
                {
                    this.Nodes["all_test_cases"].Nodes["by_test_case_category"].Nodes["not_in_scope"].Nodes.Add(newNode3);
                }
            }
        }

        public void AddTestCasesFromProjectTestCaseRepo(string projName, List<ProjectTestCase> projectTestCaseList, List<TestCase> testCaseList)
        {
            //add project root node
            TestCaseTreeNode newNode = new TestCaseTreeNode()
            {
                Name = "project_" + projName,
                Text = projName,
                ChildNodeCountType = ChildNodeCountType.Aggregate,
                TestCaseCategoryLabel = projName
            };
            this.Nodes["project_test_cases"].Nodes.Add(newNode);
            ProjectNode = newNode;

            //add in_scope node
            TestCaseTreeNode newNode2 = new TestCaseTreeNode()
            {
                Name = newNode.Name + "_in_scope",
                Text = "In scope",
                ChildNodeCountType = ChildNodeCountType.Normal,
                TestCaseCategoryLabel = "in_scope"
            };
            newNode.Nodes.Add(newNode2);

            //add not_in_scope node
            TestCaseTreeNode newNode3 = new TestCaseTreeNode()
            {
                Name = newNode.Name + "not_in_scope",
                Text = "Not in scope",
                ChildNodeCountType = ChildNodeCountType.Normal,
                TestCaseCategoryLabel = "not_in_scope"
            };
            newNode.Nodes.Add(newNode3);

            //add project test case nodes
            foreach (ProjectTestCase projectTestCase in projectTestCaseList)
            {
                ProjectTestCaseTreeNode newNode4 = new ProjectTestCaseTreeNode()
                {
                    Name = newNode.Name + "_" + projectTestCase.Name,
                    Text = projectTestCase.Name,
                    ChildNodeCountType = ChildNodeCountType.None,
                    TestCase = testCaseList.Where(x => x.TestCaseId == projectTestCase.Name).Single(),

                    //attach the projectTestCase entry, cant use the variable in foreach because it is local var, and not a valid pointer to the global entry
                    //thats why we need to use this ugly code below
                    ProjectTestCase = projectTestCaseList.Where(x => x.ProjectTestCaseId == projectTestCase.ProjectTestCaseId).Single()
                };
                if (!projectTestCase.IsBlacklisted)
                {
                    newNode2.Nodes.Add(newNode4);
                }
                else
                {
                    newNode3.Nodes.Add(newNode4);
                }
            }
            this.Nodes["project_test_cases"].Expand();
            ProjectNode.Expand();
        }

        public void DeleteProjectNode()
        {
            this.Nodes["project_test_cases"].Nodes.Clear();
            ProjectNode = null;
        }

        public void Clear()
        {
            this.Nodes.Clear();
        }
    }
}
