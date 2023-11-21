using azure_administration_tool1.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace azure_administration_tool1
{
    public class ProjectResource
    {
        public string ResourceId { get; set; }
        public string Id { get; set; }
        public string Identity { get; set; }
        public string Kind { get; set; }
        public string Location { get; set; }
        public string ManagedBy { get; set; }
        public string ResourceName { get; set; }
        public string Name { get; set; }
        public string ExtensionResourceName { get; set; }
        public string ParentResource { get; set; }
        public string Plan { get; set; }
        public string Properties { get; set; }
        public string ResourceGroupName { get; set; }
        public string Type { get; set; }
        public string ResourceType { get; set; }
        public string ExtensionResourceType { get; set; }
        public string Sku { get; set; }
        public string Tags { get; set; }
        public string SubscriptionId { get; set; }
        public string CreatedTime { get; set; }
        public string ChangedTime { get; set; }
        public string ETag { get; set; }

        public ProjectResource()
        {
            ResourceId = "";
            Id = "";
            Identity = "";
            Kind = "";
            Location = "";
            ManagedBy = "";
            ResourceName = "";
            Name = "";
            ExtensionResourceName = "";
            ParentResource = "";
            Plan = "";
            Properties = "";
            ResourceGroupName = "";
            Type = "";
            ResourceType = "";
            ExtensionResourceType = "";
            Sku = "";
            Tags = "";
            SubscriptionId = "";
            CreatedTime = "";
            ChangedTime = "";
            ETag = "";
        }

        public ProjectResource(string resourceId, string id, string identity, string kind, string location, string managedBy, 
            string resourceName, string name, string extensionResourceName, string parentResource, string plan, string properties, 
            string resourceGroupName, string type, string resourceType, string extensionResourceType, string sku, string tags, 
            string subscriptionId, string createdTime, string changedTime, string eTag)
        {
            ResourceId = resourceId;
            Id = id;
            Identity = identity;
            Kind = kind;
            Location = location;
            ManagedBy = managedBy;
            ResourceName = resourceName;
            Name = name;
            ExtensionResourceName = extensionResourceName;
            ParentResource = parentResource;
            Plan = plan;
            Properties = properties;
            ResourceGroupName = resourceGroupName;
            Type = type;
            ResourceType = resourceType;
            ExtensionResourceType = extensionResourceType;
            Sku = sku;
            Tags = tags;
            SubscriptionId = subscriptionId;
            CreatedTime = createdTime;
            ChangedTime = changedTime;
            ETag = eTag;
        }
    }
}
