using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace azure_administration_tool1
{
    public class StatusChangerComboBox : ComboBox
    {
        public void Init()
        {
            Items.Add("");
            Items.Add("OK");
            Items.Add("NOK");
            Items.Add("NOT_CHECKED");
            SelectedIndex = 0;
            Enabled = false;
        }

        public void SetToDefaultState()
        {
            SelectedIndex = 0;
            Enabled = false;
        }
    }
}
