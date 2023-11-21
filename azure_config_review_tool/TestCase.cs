using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Core.Tokens;

namespace azure_administration_tool1
{
    public enum RatingType
    {
        XXXX = 0,
        XXX = 0,
        None = 0,
        Informational = 1,
        Info = 1,
        Low = 2,
        Medium = 3,
        High = 4
    }

    public enum ParsedBy
    {
        YamlDotNetLib = 0,
        CustomScripted = 1
    }

    public class Evidence
    {
        public string Path;
        public string Title;
    }

    public class TestCase
    {
        //file nev, file path tartalmazhat egyedi es ertekes informaciot, ezert eltaroljuk
        public FileInfo YmlFileInfo { private get; set; }

        public string FileNameShort { get { return YmlFileInfo.Name; } }
        public string FileNameShortWithoutExtension { get { return YmlFileInfo.Name.Split(new string[] { ".yml", ".yaml" }, StringSplitOptions.RemoveEmptyEntries)[0]; } }
        public string FileNameFull { get { return YmlFileInfo.FullName; } }
        public string PathShort { get { return YmlFileInfo.Directory.Name; } }
        public string PathFull { get { return YmlFileInfo.DirectoryName + @"\"; } }

        //the tipical property to identify the instance
        public string TestCaseId 
        { 
            get 
            {
                try
                {
                    return FileNameShortWithoutExtension != "" ? FileNameShortWithoutExtension : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        //Yml file content
        private string howto;
        public string Howto 
        { 
            get
            {
                if(howto == null)
                {
                    return "";
                }
                else
                {
                    return howto.Replace("\n", "\r\n");
                }
            } 
            
            set
            {
                howto = value;
            }
        }

        public string ExcelHowto
        {
            get
            {
                if (Howto.Contains("FROM EXCEL") && Howto.Contains("----------"))
                {
                    return Howto.Split(new[] { "FROM EXCEL", "----------" }, StringSplitOptions.None)[1];
                }
                else
                {
                    return "";
                }
            }
        }

        public string GergoHowto
        { 
            get
            {
                if(Howto != null && Howto != "" && Howto.Contains("FROM GERGO:") && Howto.Contains("----------"))
                {
                    return Howto.Split(new string[] { "FROM GERGO:" }, StringSplitOptions.None)[1]
                        .Split( new string[] { "----------" }, StringSplitOptions.None)[0];
                }
                else
                {
                    return "";
                }
            } 
        }

        public string GergoHowToHtml
        {
            get
            {
                //html beginning
                string result = @"<html><head>" +
                    "<title>" + FileNameShortWithoutExtension + "</title>" + 
                    "<style>body {font-size: 85%;} p {margin-top: 5; margin-bottom: 5;} " +
                    "h1,h2,h3,h4,h5,h6 {margin-top: 10; margin-bottom: 10;}" +
                    "</style></head><body>";

                result += "<h2>" + FileNameShortWithoutExtension + "</h2>";
                
                //html content
                if(GergoHowto != null && GergoHowto != "")    //valid content
                {
                    foreach(string line in Regex.Split(GergoHowto, "\r\n|\r|\n")) //get lines from HowTo
                    {
                        //converting formatting from md to html
                        string mdHashStr = line.Split(' ')[0];
                        if (Regex.Match(mdHashStr, @"^[#]+$").Success)  //converting heading (h1..h6)
                        {
                            if(mdHashStr.Length <= 6) //max heading level in md and in html is 6
                            {
                                string hNum = mdHashStr.Length.ToString();
                                result += "<h" + hNum + ">" + line.Substring(mdHashStr.Length + 1) +"</h" + hNum + ">";
                            }
                            else
                            {
                                result += "<h6>" + line.Substring(mdHashStr.Length + 1) + "</h6>";
                            }
                        }
                        else //unformatted md lines will be converted to normal p tags
                        {
                            result += "<p>" + line + "</p>";
                        }
                    }
                    return result;
                }
                else    //missing content
                {
                    result += "<h3>No guide has been written for this test case yet!</h3>";
                }
                
                //html end
                result += @"</body></html>";
                return result;
            }
        }

        public string Script { get; set; }
        public string Example { get; set; }

        public string Rating { get; set; }
        public RatingType RatingValue
        { 
            get
            {
                try
                {
                    var result = (RatingType)Enum.Parse(typeof(RatingType), Rating, true);
                    return result;
                }
                catch
                {
                    return RatingType.None;
                }
            }
        }
        public string Name { get; set; }
        public string Observation { get; set; }
        public string Resources { get; set; }

        public List<string> ResourcesList
        {
            get
            {
                return Resources.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            set
            {
                Resources = string.Join(Environment.NewLine, value);
            }
        }

        public List<Evidence> Evidences { get; set; }

        public DataTable EvidencesDataTable
        {
            get
            {
                if (Evidences != null && Evidences.Count > 0)
                {
                    DataTable resultTable = new DataTable();
                    resultTable.Columns.Add("Path", typeof(string));
                    resultTable.Columns.Add("Title", typeof(string));
                    foreach (Evidence evidence in Evidences)
                    {
                        DataRow newRow = resultTable.NewRow();
                        newRow["Path"] = evidence.Path;
                        newRow["Title"] = evidence.Title;
                        resultTable.Rows.Add(newRow);
                    }
                    return resultTable;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<string> EvidenceFileNameList
        {
            get
            {
                if(Evidences != null && Evidences.Count > 0)
                {
                    List<string> result;
                    result = new List<string>();

                    foreach (Evidence evidence in Evidences)
                    {
                        result.Add(PathFull + @"\" + evidence.Path.Replace('/', '\\'));
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public static List<string> imageExtensions = new List<string> { ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", ".ICO", ".TIFF" };
        public static List<string> wordExtensions = new List<string> { ".DOCX", ".DOC" };

        public List<string> ImageEvidenceList
        {
            get
            {
                if (Evidences != null && EvidenceFileNameList != null && EvidenceFileNameList.Count > 0)
                {
                    List<string> result = new List<string>();
                    foreach (string evidenceFile in EvidenceFileNameList)
                    {
                        if (imageExtensions.Contains(Path.GetExtension(evidenceFile).ToUpper()))
                        {
                            result.Add(evidenceFile);
                        }                        
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<string> WordEvidenceList
        {
            get
            {
                if (Evidences != null && EvidenceFileNameList != null && EvidenceFileNameList.Count > 0)
                {
                    List<string> result = new List<string>();
                    foreach (string evidenceFile in EvidenceFileNameList)
                    {
                        if (wordExtensions.Contains(Path.GetExtension(evidenceFile).ToUpper()))
                        {
                            result.Add(evidenceFile);
                        }
                    }
                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool isEvidenceAnImageFile(int evidenceIndex)
        {
            if (imageExtensions.Contains(Path.GetExtension(Evidences[evidenceIndex].Path.ToUpper())))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public bool isEvidenceAWordFile(int evidenceIndex)
        {
            if (wordExtensions.Contains(Path.GetExtension(Evidences[evidenceIndex].Path)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string Risk { get; set; }
        public string Recommendation { get; set; }

        /* A tag-nek a felepitese kerdeses es bizontalan lesz, ezert property-kkel keresztul lehet majd csak elerni*/
        public List<Dictionary<string, string>> Tags { get; set; }

        public string IdFromTag
        {
            get
            {
                return Tags[0]["id"].ToString();
            }
        }

        public string TypeFromTag
        {
            get
            {
                return Tags[0]["type"].ToString();
            }
        }

        public List<Dictionary<string, string>> References { get; set; }

        public string LinkFromReferences
        {
            get
            {
                return References[0]["link"].ToString();
            }
        }

        public string TitleFromReferences
        {
            get
            {
                return References[0]["title"].ToString();
            }
        }

        public string ResourceType
        {
            get
            {
                try
                {
                    return PathShort != "" ? PathShort : null;
                }
                catch
                {
                    return null;
                }
            }
        }
        
        /* Blacklist-elt e */       //tuti nem igy lesz kesobb de valamit beirtunk ide ideiglenesen...
        
        //tags resze
        public bool isBlacklisted
        {
            //ideiglenesen lett kitoltve, a vegso valtozat majd kiderul...
            get
            {
                if (Tags != null && Tags[0] != null && Tags[0].ContainsKey("isBlacklisted") && Tags[0]["isBlacklisted"].ToString() == "true")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public ParsedBy ParsedBy { get; set; }
    
        public void OpenEvidenceInWindowsGui(int evidenceIndex)
        {
            string evidenceFilePath = EvidenceFileNameList[evidenceIndex];
            if (File.Exists(evidenceFilePath))
            {
                System.Diagnostics.Process.Start(evidenceFilePath); //open the file with the default program by extension (just like in explorer.exe)
            }
            else
            {
                MessageBox.Show("The evidence file is not exists with the filename: " + evidenceFilePath);
            }
        }

        public void OpenTestCaseYmlParentFolder()
        {
            if(PathFull != null && PathFull != "" && Directory.Exists(PathFull))
            {
                System.Diagnostics.Process.Start("explorer.exe", "\"" + PathFull + "\"");
            }
            else
            {
                MessageBox.Show("The test case path is not exists with the value: " + PathFull);
            }
        }

        public void OpenTestCaseEvidenceParentFolder()
        {
            if (PathFull != null && PathFull != "" && Directory.Exists(PathFull + @"\evidences"))
            {
                System.Diagnostics.Process.Start("explorer.exe", "\"" + PathFull + "evidences" + "\"");
            }
            else
            {
                MessageBox.Show("The evidence path is not exists with the value: " + PathFull + @"\evidences");
            }
        }
    }
}
