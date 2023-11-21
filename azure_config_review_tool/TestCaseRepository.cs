using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.Serialization.NamingConventions;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;
using azure_administration_tool1.Properties;
using System.Xml.Linq;
using System.Security.Policy;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace azure_administration_tool1
{
    public class TestCaseRepository
    {
        public List<TestCase> testCaseList;

        public TestCaseRepository()
        {
            testCaseList = new List<TestCase>();
        }

        public void LoadYmlFiles(string ymlPath) //??rename
        {

            //load yml files
            var allYmlFiles = Directory.GetFiles(ymlPath, "*.y*ml", SearchOption.AllDirectories);
            foreach (var ymlFile in allYmlFiles)
            {
                string shortFileName = ymlFile.Split('\\').Last();
                if (shortFileName != "EMPTY.yml" && shortFileName != "EMPTY.yaml" && !shortFileName.Contains("EMPTY - Copy"))
                {
                    testCaseList.Add(ParseYmlFile(ymlFile));
                }
            }
        }

        //universal search functions, search test case(s) by any property
        public TestCase SearchSingle(string paraName, string value)
        {
            //lehet hogy SingleOrDefault-ot kene hasznalni mert ez igy exception-t dob hogyha nincs talalat
            //de egyelore csak kezeljuk az exceptiont es mi magunk adunk vissza null-t helyette
            try
            {
                return testCaseList.Single(x => x.GetType().GetProperty(paraName).GetValue(x).ToString() == value);
            }
            catch
            {
                return null;
            }
        }

        public TestCase SearchFirst(string paraName, string value)
        {
            try
            {
                return testCaseList.Find(x => x.GetType().GetProperty(paraName).GetValue(x).ToString() == value);
            }
            catch
            {
                return null;
            }
        }

        public List<TestCase> SearchAll(string paraName, string value)
        {
            try
            {
                return testCaseList.FindAll(x => x.GetType().GetProperty(paraName).GetValue(x).ToString() == value);
            }
            catch
            {
                return null;
            }
        }

        public TestCase SearchByTestCaseId(string testCaseId)
        {
            return SearchSingle("TestCaseId", testCaseId);
        }

        private string SanitizeYmlString(string ymlString)
        {
            string result = "";

            //remove lines that only contain whitespace or has been commented out
            foreach (string line in Regex.Split(ymlString, "\r\n|\r|\n")) //get lines from ymlString
            {
                var matchBlankLine = Regex.Match(line, @"^\s+$");                   //line contains only white space (blank line)
                var matchMdString = Regex.Match(line.Trim(), @"^#{1,6}[ ].*");     //md h1-h6 format
                var matchCommentLine = Regex.Match(line, @"^([\s]+[#].*)|^([#].*)");  //commented out lines (begins with hashmark or whitespace + hashmark)

                if (!matchBlankLine.Success && (matchMdString.Success || !matchCommentLine.Success))
                {
                    result += line + "\r\n";
                }
            }
            
            return result;
        }

        private TestCase ParseYmlFile(string ymlFile)
        {
            var ymlString = File.ReadAllText(ymlFile);
            ymlString = SanitizeYmlString(ymlString);
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            TestCase testCase = null;
            
            //parse yml file with YamlDotNet library
            try
            {
                testCase = deserializer.Deserialize<TestCase>(ymlString);
                if(testCase != null)
                {
                    testCase.ParsedBy = ParsedBy.YamlDotNetLib;
                }
            }
            catch
            {
                //MessageBox.Show("Error when parsing yml file '" + ymlFile + "'", "Error. Exception occured.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //if parsing was unsuccessfull we try to read yml fields with plain string functions
            if (testCase == null)
            {
                /*testCase = TryToReadYmlFields(ymlString);
                if (testCase != null)
                {
                    testCase.ParsedBy = ParsedBy.CustomScripted;
                }*/
                //MessageBox.Show("Error when parsing yml file '" + ymlFile + "'", "Error. Test case is null", MessageBoxButtons.OK, MessageBoxIcon.Error);
                testCase = new TestCase();
            }
            testCase.YmlFileInfo = new System.IO.FileInfo(ymlFile);
            return testCase;
        }

        /* Try to read yml fields one by one with string functions */
        private TestCase TryToReadYmlFields(string ymlString)
        {
            TestCase testCase = new TestCase();
            try
            {
                /* Get fields and corresponding contents (based on identation) into dictionary */
                Dictionary<string, string> ymlContentDict = new Dictionary<string, string>();
                string currentField = "";
                //iterating through lines
                foreach (string line in Regex.Split(ymlString, "\r\n|\r|\n"))
                {
                    var match = Regex.Match(line, @"^[^\s].*"); //with ident => content of a field      without ident => (field name) or (field name + value)
                    if (match.Success)
                    {
                        /* field */
                        currentField = match.Value.Split(':')[0]; //field name is everything before the ':' character
                        string remainderChars = match.Value.Split(':')[1]; //maybe stores value maybe not
                        if (remainderChars.Contains("\"") || remainderChars.Contains("\'"))
                        {
                            //field row contains the field and the value as well like this =>       field_name : "field_value"      or      field_name : 'field_value'
                            ymlContentDict.Add(currentField, remainderChars.Split(new string[] { "\"", "\'" }, StringSplitOptions.None)[1].Trim());
                        }
                        else
                        {
                            //field row only contains name of the field and useless characters
                            ymlContentDict.Add(currentField, "");
                        }
                    }
                    else
                    {
                        /* content */
                        //add new line to the corresponding field - without identation - and with line break
                        ymlContentDict[currentField] += match.Value.Trim() + "\r\n";
                    }
                }

                /* Try to load TestCase fields from dictionary one by one (with the hope of partially loaded fields instead of nothing) */

                /* simple string fields */
                testCase.Howto = ymlContentDict["howto"];
                testCase.Script = ymlContentDict["script"];
                testCase.Example = ymlContentDict["example"];
                testCase.Rating = ymlContentDict["rating"];
                testCase.Name = ymlContentDict["name"];
                testCase.Observation = ymlContentDict["observation"];
                testCase.Resources = ymlContentDict["resources"];
                testCase.Risk = ymlContentDict["risk"];
                testCase.Recommendation = ymlContentDict["recommendation"];

                /* List<> type fields */
                //Evidences
                testCase.Evidences = new List<Evidence>();

                string[] lines = Regex.Split(ymlContentDict["evidences"], "\r\n|\r|\n");
                string pathPattern = @"(^-path)|(^- path)";
                string titlePattern = @"^title";
                int i = 0;
                Evidence evidence = null;
                while (i < lines.Length)
                {
                    string val = "";
                    if (lines[i].Contains(":"))
                    {
                        val = lines[i].Split(':')[1].Trim().Trim(new char[] { '\'', '\"' });

                        if (Regex.Match(lines[i], pathPattern).Success)
                        {
                            //path record + new evidence creation
                            evidence = new Evidence();
                            evidence.Path = val;
                        }
                        else if(Regex.Match(lines[i], titlePattern).Success)
                        {
                            //title is presented too, so we set title
                            evidence.Title = val;
                        }

                        //checking end of record to do saving
                        if ((i + 1) == lines.Length || Regex.Match(lines[i + 1], pathPattern).Success)
                        {
                            //next line is path so title is missing, we can save the record
                            testCase.Evidences.Add(evidence);
                        }
                    }
                    i++;
                }


                //Tags
                testCase.Tags = new List<Dictionary<string, string>>();

                lines = Regex.Split(ymlContentDict["tags"], "\r\n|\r|\n");
                string idPattern = @"(^-id)|(^- id)";
                string typePattern = @"^type";
                i = 0;
                Dictionary<string, string> tag = null;
                while (i < lines.Length)
                {
                    string val = "";
                    if (lines[i].Contains(":"))
                    {
                        val = lines[i].Split(':')[1].Trim().Trim(new char[] { '\'', '\"' });

                        if (Regex.Match(lines[i], idPattern).Success)
                        {
                            //path record + new evidence creation
                            tag = new Dictionary<string, string>();
                            tag["id"] = val;
                        }
                        else if (Regex.Match(lines[i], typePattern).Success)
                        {
                            //title is presented too, so we set title
                            tag["type"] = val;
                        }

                        //checking end of record to do saving
                        if ((i + 1) == lines.Length || Regex.Match(lines[i + 1], pathPattern).Success)
                        {
                            //next line is path so title is missing, we can save the record
                            testCase.Tags.Add(tag);
                        }
                    }
                    i++;
                }


                //References
                testCase.References = new List<Dictionary<string, string>>();

                lines = Regex.Split(ymlContentDict["references"], "\r\n|\r|\n");
                string linkPattern = @"(^-link)|(^- link)";
                titlePattern = @"^type";
                i = 0;
                Dictionary<string, string> reference = null;
                while (i < lines.Length)
                {
                    string val = "";
                    if (lines[i].Contains(":"))
                    {
                        val = lines[i].Split(':')[1].Trim().Trim(new char[] { '\'', '\"' });

                        if (Regex.Match(lines[i], linkPattern).Success)
                        {
                            //path record + new evidence creation
                            reference = new Dictionary<string, string>();
                            reference["link"] = val;
                        }
                        else if (Regex.Match(lines[i], titlePattern).Success)
                        {
                            //title is presented too, so we set title
                            reference["title"] = val;
                        }

                        //checking end of record to do saving
                        if ((i + 1) == lines.Length || Regex.Match(lines[i + 1], pathPattern).Success)
                        {
                            //next line is path so title is missing, we can save the record
                            testCase.References.Add(reference);
                        }
                    }
                    i++;
                }
            }
            catch
            {
                //...
            }

            return testCase;
        }

        public void SaveToYmlFile(string destinationFolder, TestCase testCase)
        {
            try
            {
                YmlTestCaseTmp ymlTestCase = new YmlTestCaseTmp(testCase);
                var serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                string ymlString = serializer.Serialize(ymlTestCase);
                string ymlFilePath = destinationFolder + @"\" + testCase.TestCaseId + ".yml";
                File.WriteAllText(ymlFilePath, ymlString);
            }
            catch
            {
                MessageBox.Show("error");
            }
        }
    }
}
