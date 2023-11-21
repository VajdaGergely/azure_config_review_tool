using azure_administration_tool1.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azure_administration_tool1
{
    public class AztsRecord
    {
        public string Id;
        public string SubscriptionId;
        public string ControlName;
        public string ResourceId;
        public string ResourceType;
        public string ResourceName;
        public string ResourceGroupName;
        public string ScannedOn;
        public string StatusReason;
        public string VerificationResult;
        public string RemediationSteps;
        public string ControlSpecification;
        public string Severity;
        public string EvaluatedUsingMDC;
    }
}
