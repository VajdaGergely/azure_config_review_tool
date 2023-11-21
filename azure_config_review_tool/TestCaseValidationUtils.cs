using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Core.Tokens;

namespace azure_administration_tool1
{
    public class TestCaseValidationUtils
    {
        static TestCaseValidationUtils()
        {
            CommentValueChanged = false;
        }

        public static bool CommentValueChanged { get; set; } //prevents double save ('enter' and leave event) and prevent saving unmodified textBox value
    }
}
