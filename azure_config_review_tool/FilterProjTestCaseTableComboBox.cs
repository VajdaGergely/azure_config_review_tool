using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace azure_administration_tool1
{
    public class FilterProjTestCaseTableComboBox : ComboBox
    {
        public void Init()
        {
            List<string> items = new List<string>
            {
                "All"
            };
            DataSource = items;
            Refresh();
            SelectedIndex = 0;
        }

        public void Init(List<string> items)
        {
            if (items != null)
            {
                items.Insert(0, "All");
                DataSource = items;
                Refresh();
                SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Error.");
            }
        }

        public void Clear()
        {
            DataSource = null;
            Refresh();
        }
    }
}
