using azure_administration_tool1.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace azure_administration_tool1
{
    public class ProjectResourceRepository
    {
        public List<ProjectResource> ProjectResourceList { get; set; }

        public ProjectResourceRepository()
        {
            ProjectResourceList = new List<ProjectResource>();
        }

        public void LoadProjectResources(string csvFileName)
        {
            try
            {
                string[] resourceCsvRows = File.ReadAllText(csvFileName).Replace("\"", "").Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                //1st and 2nd rows are technical rows, start from 3rd row (which is index 2)
                for (int i = 2; i < resourceCsvRows.Length; i++)
                {
                    string resourceId = resourceCsvRows[i].Split(';')[0];
                    string id = resourceCsvRows[i].Split(';')[1];
                    string identity = resourceCsvRows[i].Split(';')[2];
                    string kind = resourceCsvRows[i].Split(';')[3];
                    string location = resourceCsvRows[i].Split(';')[4];
                    string managedBy = resourceCsvRows[i].Split(';')[5];
                    string resourceName = resourceCsvRows[i].Split(';')[6];
                    string name = resourceCsvRows[i].Split(';')[7];
                    string extensionResourceName = resourceCsvRows[i].Split(';')[8];
                    string parentResource = resourceCsvRows[i].Split(';')[9];
                    string plan = resourceCsvRows[i].Split(';')[10];
                    string properties = resourceCsvRows[i].Split(';')[11];
                    string resourceGroupName = resourceCsvRows[i].Split(';')[12];
                    string type = resourceCsvRows[i].Split(';')[13];
                    string resourceType = resourceCsvRows[i].Split(';')[14];
                    string extensionResourceType = resourceCsvRows[i].Split(';')[15];
                    string sku = resourceCsvRows[i].Split(';')[16];
                    string tags = resourceCsvRows[i].Split(';')[17];
                    string subscriptionId = resourceCsvRows[i].Split(';')[18];
                    string createdTime = resourceCsvRows[i].Split(';')[19];
                    string changedTime = resourceCsvRows[i].Split(';')[20];
                    string eTag = resourceCsvRows[i].Split(';')[21];

                    ProjectResource resource = new ProjectResource(resourceId, id, identity, kind, location, managedBy, resourceName,
                        name, extensionResourceName, parentResource, plan, properties, resourceGroupName, type, resourceType,
                        extensionResourceType, sku, tags, subscriptionId, createdTime, changedTime, eTag);
                    ProjectResourceList.Add(resource);
                }
            }
            catch
            {
                //
            }
        }
    }
}
