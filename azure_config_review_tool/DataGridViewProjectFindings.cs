using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace azure_administration_tool1
{
    public class DataGridViewProjectFindings : DataGridView
    {
        public Dictionary<string, string> FindingEvidenceMatrix
        {
            get
            {
                if(Rows != null && Rows.Count > 0 && Columns != null && Columns.Count > 1 && Columns["finding_evidence"] != null)
                {
                    Dictionary<string, string> resultDict = new Dictionary<string, string>();
                    foreach(DataGridViewRow row in Rows)
                    {
                        string k = row.Cells["finding_name"].Value.ToString();
                        string v = row.Cells["finding_evidence"].Value.ToString();
                        resultDict.Add(k, v);
                    }
                    return resultDict;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
