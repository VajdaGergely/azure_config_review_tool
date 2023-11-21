using azure_administration_tool1.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace azure_administration_tool1
{
    public class ResourceTypeVariant
    {
        public string Azts { get; set; }
        public string AzureOfficial { get; set; }
    }

    public class ResourceTypeVariantMatrix
    {
        private const string resourcetypeVariantMatrixCsvFile = @".\resource_type_variants_matrix.csv";
        public List<ResourceTypeVariant> resourceTypeVariantList;

        public void ReadResourceTypeVariants()
        {
            resourceTypeVariantList = new List<ResourceTypeVariant>();
            try
            {
                string[] variants = File.ReadAllText(resourcetypeVariantMatrixCsvFile).Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                //1st and 2nd rows are technical rows, start from 3rd row (which is index 2)
                for (int i = 2; i < variants.Length; i++)
                {
                    ResourceTypeVariant resourceTypeVariant = new ResourceTypeVariant()
                    {
                        Azts = variants[i].Split(';')[0],
                        AzureOfficial = variants[i].Split(';')[1]
                    };
                    resourceTypeVariantList.Add(resourceTypeVariant);
                }
            }
            catch
            {
                resourceTypeVariantList = null;
            }
        }

        public string ConvertResourceTypeFromAztsToAzureOfficial(string aztsType)
        {
            try
            {
                return resourceTypeVariantList.Where(x => x.Azts == aztsType).Single().AzureOfficial;
            }
            catch
            {
                return null;
            }
        }

        public string ConvertResourceTypeFromAzureOfficialToAzts(string azureOfficialType)
        {
            try
            {
                return resourceTypeVariantList.Where(x => x.AzureOfficial == azureOfficialType).Single().Azts;
            }
            catch
            {
                return null;
            }
        }
    }

    public class ProjectTestCaseRepository
    {
        public ProjectTestCaseRepository()
        {
            ProjectTestCaseList = new List<ProjectTestCase>();
            ResourceTypeVariantsMatrix = new ResourceTypeVariantMatrix();
            ResourceTypeVariantsMatrix.ReadResourceTypeVariants();
        }

        public ProjectTestCaseRepository(List<ProjectTestCase> projectTestCaseList)
        {
            ProjectTestCaseList = projectTestCaseList;
            ResourceTypeVariantsMatrix = new ResourceTypeVariantMatrix();
            ResourceTypeVariantsMatrix.ReadResourceTypeVariants();
        }

        public ResourceTypeVariantMatrix ResourceTypeVariantsMatrix { get; set; }
        public List<ProjectTestCase> ProjectTestCaseList { get; set; }

        public static int[] EnabledColumns
        {
            get
            {
                return new int[] { 7, 8 };
            }
        }

        public DataTable ProjectTestCaseListDataTable
        {
            get
            {
                try
                {
                    DataTable resultTable = new DataTable();
                    resultTable.Columns.Add("ProjectTestCaseId", typeof(string));
                    resultTable.Columns.Add("IsBlacklisted", typeof(string));
                    resultTable.Columns.Add("Name", typeof(string));
                    resultTable.Columns.Add("ResourceType", typeof(string));
                    resultTable.Columns.Add("ResourceGroupName", typeof(string));
                    resultTable.Columns.Add("ResourceName", typeof(string));
                    resultTable.Columns.Add("ResourceId", typeof(string));
                    resultTable.Columns.Add("Status", typeof(string));
                    resultTable.Columns.Add("Comment", typeof(string));
                    resultTable.Columns.Add("AztsStatus", typeof(string));
                    resultTable.PrimaryKey = new DataColumn[] { resultTable.Columns["ProjectTestCaseId"] };
                    for (int i = 0; i < ProjectTestCaseList.Count; i++)
                    {
                        DataRow newRow = resultTable.NewRow();
                        newRow[0] = ProjectTestCaseList[i].ProjectTestCaseId;
                        newRow[1] = ProjectTestCaseList[i].IsBlacklisted;
                        newRow[2] = ProjectTestCaseList[i].Name;
                        newRow[3] = ProjectTestCaseList[i].ResourceType;
                        newRow[4] = ProjectTestCaseList[i].ResourceGroupName;
                        newRow[5] = ProjectTestCaseList[i].ResourceName;
                        newRow[6] = ProjectTestCaseList[i].ResourceId;
                        newRow[7] = ProjectTestCaseList[i].StatusNotSetEmpty;
                        newRow[8] = ProjectTestCaseList[i].Comment;
                        newRow[9] = ProjectTestCaseList[i].AztsStatus;
                        resultTable.Rows.Add(newRow);
                    }
                    return resultTable;
                }
                catch
                {
                    return null;
                }
            }
        }

        public string ProjectTestCaseListCsv
        {
            get
            {
                try
                {
                    string resultCsv = "\"ProjectTestCaseId\";\"IsBlacklisted\";\"Name\";\"ResourceType\";\"ResourceGroupName\";\"ResourceName\";\"ResourceId\";\"Status\";\"Comment\";\"AztsStatus\"" + Environment.NewLine;
                    for (int i = 0; i < ProjectTestCaseList.Count; i++)
                    {
                        string row = "";
                        row += "\"" + ProjectTestCaseList[i].ProjectTestCaseId + "\";";
                        row += "\"" + ProjectTestCaseList[i].IsBlacklisted + "\";";
                        row += "\"" + ProjectTestCaseList[i].Name + "\";";
                        row += "\"" + ProjectTestCaseList[i].ResourceType + "\";";
                        row += "\"" + ProjectTestCaseList[i].ResourceGroupName + "\";";
                        row += "\"" + ProjectTestCaseList[i].ResourceName + "\";";
                        row += "\"" + ProjectTestCaseList[i].ResourceId + "\";";
                        ValidationStatus status = ProjectTestCaseList[i].Status;
                        row += "\"" + (status == ValidationStatus.NOT_SET ? "" : status.ToString()) + "\";";
                        row += "\"" + ProjectTestCaseList[i].Comment + "\";";
                        row += "\"" + ProjectTestCaseList[i].AztsStatus + "\""; //no semicolon needed at the end
                        row += Environment.NewLine;
                        resultCsv += row;
                    }
                    return resultCsv.TrimEnd(Environment.NewLine.ToCharArray());
                }
                catch
                {
                    return null;
                }
            }
        }

        public List<string> ProjectTestCasePropertyList
        {
            get
            {
                return  new List<string>() {
                    "ProjectTestCaseId",
                    "IsBlacklisted",
                    "Name",
                    "ResourceType",
                    "ResourceGroupName",
                    "ResourceName",
                    "ResourceId",
                    "Status",
                    "Comment",
                    "AztsStatus"
                };
            }
        }

        public void CreateProjectTestCases(ProjectResourceRepository projectResourceRepository, TestCaseRepository testCaseRepository)
        {
            ProjectTestCaseList = new List<ProjectTestCase>();
            int i = 0;
            foreach (var resource in projectResourceRepository.ProjectResourceList)
            {
                //convert azure official resource type to azts resource type
                string resourceTypeAzts = ResourceTypeVariantsMatrix.ConvertResourceTypeFromAzureOfficialToAzts(resource.ResourceType);
                try
                {
                    //adding all test cases that matches the resource type of the current resource
                    foreach (var testCase in testCaseRepository.testCaseList.Where(x => x.ResourceType == resourceTypeAzts))
                    {
                        string projectTestCaseId = i.ToString();
                        bool isBlacklisted = false;
                        string name = testCase.TestCaseId;
                        string resourceType = testCase.ResourceType;
                        string resourceGroupName = resource.ResourceGroupName;
                        string resourceName = resource.ResourceName;
                        string resourceId = resource.ResourceId;
                        ProjectTestCase projectTestCase = new ProjectTestCase(projectTestCaseId, isBlacklisted, name, resourceType, resourceGroupName, resourceName, resourceId);
                        ProjectTestCaseList.Add(projectTestCase);
                        i++;
                    }
                }
                catch
                {
                    //
                }
            }
        }
        
        public List<string> ProjectTestCaseIndividualValues(string propertyName)
        { 
            try
            {
                List<string> resultList = ProjectTestCaseList.Select(x => x.GetType().GetProperty(propertyName).GetValue(x).ToString()).Distinct().ToList();
                return resultList;
            }
            catch
            {
                return null;
            }
        }

        public DataTable GetProjectTestCaseListDataTableFiltered(string colName, string value)
        {
            DataTable resultTable = ProjectTestCaseListDataTable;
            try
            {
                if (value != "All")
                {
                    resultTable.DefaultView.RowFilter = colName + " = '" + value + "'";
                }
                else
                {
                    //resultTable.DefaultView.RowFilter = "";
                }
                return resultTable;
            }
            catch
            {
                return null;
            }
        }
    }
}