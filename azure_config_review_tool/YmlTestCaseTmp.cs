using azure_administration_tool1.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace azure_administration_tool1
{
    //poor TestCase object for serialization to yml string
    public class YmlTestCaseTmp
    {
        public YmlTestCaseTmp(TestCase testCase)
        {
            Howto = testCase.Howto;
            Script = testCase.Script;
            Example = testCase.Example;
            Rating = testCase.Rating;
            Name = testCase.Name;
            Observation = testCase.Observation;
            Resources = testCase.Resources;
            Evidences = testCase.Evidences;
            Risk = testCase.Risk;
            Recommendation = testCase.Recommendation;
            Tags = testCase.Tags;
            References = testCase.References;
        }

        
        public string TestProp { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.Literal)] //ez kell ahhoz, hogy a multi line string rendesen keruljon formazasra
        public string Howto { get; set; }
        
        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Script { get; set; }
        
        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Example { get; set; }
        
        public string Rating { get; set; }
        
        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Name { get; set; }

        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Observation { get; set; }
        
        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Resources { get; set; }
        
        public List<Evidence> Evidences { get; set; }
        
        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Risk { get; set; }
        
        [YamlMember(ScalarStyle = ScalarStyle.Literal)]
        public string Recommendation { get; set; }
        public List<Dictionary<string, string>> Tags { get; set; }
        public List<Dictionary<string, string>> References { get; set; }
    }
}
