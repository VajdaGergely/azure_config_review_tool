using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace azure_administration_tool1
{
    public class ProjectEvidence
    {
        public ProjectEvidence(FileInfo evidenceFile)
        {
            EvidenceFile = evidenceFile;
        }

        public FileInfo EvidenceFile { get; set; }


        public void OpenEvidenceInWindowsGui()
        {
            try
            {
                System.Diagnostics.Process.Start(EvidenceFile.FullName); //open the file with the default program by extension (just like in explorer.exe)
            }
            catch
            {
                MessageBox.Show("Error. Can not open evidence file!");
            }
        }

        public void OpenEvidenceInPaintBrush()
        {
            try
            {
                System.Diagnostics.Process.Start("pbrush.exe", "\"" + EvidenceFile.FullName + "\""); //open the file with paint
            }
            catch
            {
                MessageBox.Show("Error. Can not open evidence file!");
            }
        }
    }
}
