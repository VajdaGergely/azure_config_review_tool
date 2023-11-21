using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace azure_administration_tool1
{
    public enum ValidationStatus
    {
        NOT_SET = 0,
        OK = 1,
        NOK = 2,
        NOT_CHECKED = 3,
        OTHER = 4
    }

    public enum AztsStatus
    {
        NULL = 0,
        PASSED = 1,
        FAILED = 2,
        VERIFY = 2,
        MANUAL = 2
    }

    public class ProjectTestCase
    {
        public string ProjectTestCaseId { get; set; }
        public bool IsBlacklisted { get; set; }
        public string Name { get; set; }
        public string ResourceType { get; set; }
        public string ResourceGroupName { get; set; }
        public string ResourceName { get; set; }
        public string ResourceId { get; set; }
        public ValidationStatus Status
        { 
            get
            {
                return status;
            }

            set
            {
                status = value;
            }
        }
        public string StatusNotSetEmpty
        {
            get
            {
                if (status == ValidationStatus.NOT_SET)
                {
                    return "";
                }
                else
                {
                    return status.ToString();
                }
            }

            set
            {
                if(value == "")
                {
                    status = ValidationStatus.NOT_SET;
                }
                else
                {
                    status = (ValidationStatus)Enum.Parse(typeof(ValidationStatus),value);
                }
            }
        }
        public string Comment { get; set; }
        public AztsStatus AztsStatus { get; set; }

        public ProjectTestCase()
        {

        }

        public ProjectTestCase(string testCaseId, bool isBlacklisted, string name, string resourceType, string resourceGroupName, 
            string resourceName, string resourceId)
        {
            ProjectTestCaseId = testCaseId;
            IsBlacklisted = isBlacklisted;
            Name = name;
            ResourceType = resourceType;
            ResourceGroupName = resourceGroupName;
            ResourceName = resourceName;
            ResourceId = resourceId;
            Status = ValidationStatus.NOT_SET;
            Comment = "";
            AztsStatus = AztsStatus.NULL;
        }

        public ProjectTestCase(string testCaseId, bool isBlacklisted, string name, string resourceType, string resourceGroupName,
            string resourceName, string resourceId, ValidationStatus status, string comment, AztsStatus aztsStatus)
        {
            ProjectTestCaseId = testCaseId;
            IsBlacklisted = isBlacklisted;
            Name = name;
            ResourceType = resourceType;
            ResourceGroupName = resourceGroupName;
            ResourceName = resourceName;
            ResourceId = resourceId;
            Status = status;
            Comment = comment;
            AztsStatus = aztsStatus;
        }

        //ezt mind le kell tarolni majd db-be is!!!
        //blacklistet is taroljuk, mert szopas lenne mindig hozzarakni

        public static string ConvertStatusStringToDbFormat(string statusValue)
        {
            return statusValue == "" ? "NOT_SET" : statusValue;
        }

        private ValidationStatus status;
    }
}
