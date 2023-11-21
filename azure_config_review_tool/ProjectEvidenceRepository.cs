using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace azure_administration_tool1
{
    public class ProjectEvidenceRepository
    {
        public ProjectEvidenceRepository(string evidenceFolder)
        {
            try
            {
                if (evidenceFolder != null && evidenceFolder != "")
                {
                    ProjectEvidenceList = new List<ProjectEvidence>();
                    EvidenceFolder = evidenceFolder;
                    FileInfo[] files = new DirectoryInfo(evidenceFolder).GetFiles("*.*", SearchOption.AllDirectories);
                    foreach (FileInfo file in files)
                    {
                        ProjectEvidenceList.Add(new ProjectEvidence(file));
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error");
            }
        }

        public string EvidenceFolder;
        public List<ProjectEvidence> ProjectEvidenceList;

        public List<string> ProjectEvidenceFilenameList
        {
            get
            {
                return ProjectEvidenceList.Select(x => x.EvidenceFile.Name).ToList();
            }
        }

        public List<string> ProjectEvidenceAbsolutePathList
        {
            get
            {
                return ProjectEvidenceList.Select(x => x.EvidenceFile.FullName).ToList();
            }
        }

        public List<string> ProjectEvidenceRelativePathList
        {
            get
            {
                return ProjectEvidenceList.Select(x => x.EvidenceFile.DirectoryName.Replace(EvidenceFolder, "") + @"\" + x.EvidenceFile.Name).ToList();
            }
        }

        public void OpenEvidenceRootFolder()
        {
            try
            {
                if (EvidenceFolder != null && EvidenceFolder != "" && Directory.Exists(EvidenceFolder))
                {
                    System.Diagnostics.Process.Start("explorer.exe", "\"" + EvidenceFolder + "\"");
                }
                else
                {
                    MessageBox.Show("Error. Can not open evidence root path!");
                }
            }
            catch
            {
                MessageBox.Show("Error. Can not open evidence root path!");
            }
        }
    }
}
