using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace azure_administration_tool1
{
    public class old_ProjectTestCaseTable
    {
        private string dbPath; //lehet hogy ezt kesobb majd nem itt rogzitjuk
        private string projName;
        private DataTable dataSource;
        private bool initDone;
        private DataGridView dgv;
        private ComboBox comboBox;

        public string ProjName
        {
            get
            {
                return projName;
            }

            set
            {
                this.projName = value;
            }
        }

        public old_ProjectTestCaseTable(DataGridView dataGridView_X, ComboBox comboBox_X, string db_path)
        {
            //init ProjectTestCaseTable  DataTable (the dataSource behind the ProjectTestCaseTable DataGridView)
            this.dataSource = new DataTable();
            this.dataSource.Columns.Add("testCaseId", typeof(int));
            this.dataSource.Columns.Add("testCaseCategory", typeof(string));
            this.dataSource.Columns.Add("controlId", typeof(string));
            this.dataSource.Columns.Add("resourceGroupName", typeof(string));
            this.dataSource.Columns.Add("resourceName", typeof(string));
            this.dataSource.Columns.Add("resourceId", typeof(string));
            this.dataSource.Columns.Add("status", typeof(string));
            this.dataSource.Columns.Add("comment", typeof(string));
            this.dataSource.PrimaryKey = new DataColumn[] { this.dataSource.Columns["testCaseId"] };

            //buster for CellChanged event (dont want to trigger it when we fill it at start)
            this.initDone = false;

            //dependecy injection from constructor
            this.dgv = dataGridView_X;
            this.comboBox = comboBox_X;

            //linking dataGridView and dataTable
            this.dgv.DataSource = this.dataSource;
            this.dgv.Columns["testCaseId"].ReadOnly = true;
            this.dgv.Columns["testCaseCategory"].ReadOnly = true;
            this.dgv.Columns["controlId"].ReadOnly = true;
            this.dgv.Columns["resourceGroupName"].ReadOnly = true;
            this.dgv.Columns["resourceName"].ReadOnly = true;
            this.dgv.Columns["resourceId"].ReadOnly = true;
            this.dgv.Columns["status"].ReadOnly = false;
            this.dgv.Columns["comment"].ReadOnly = false;

            //inject dbPath
            this.dbPath = db_path;

            //init misc things
            //init Combobox FilterProjTestCaseTable
            comboBox.Items.Clear();
            comboBox.Items.Add("All");

            //init projName
            this.projName = "";
        }

        private void AddTestCase(string id, string category, string name, string resourceGroupName, string resourceName, string resourceId, string status, string comment)
        {
            //add new entry to dataTable (behind of dataGridView)
            DataRow new_row = this.dataSource.NewRow();
            new_row[0] = id;
            new_row[1] = category;
            new_row[2] = name;
            new_row[3] = resourceGroupName;
            new_row[4] = resourceName;
            new_row[5] = resourceId;
            new_row[6] = status;
            new_row[7] = comment;
            this.dataSource.Rows.Add(new_row);
        }

        private DataTable LoadStatusAndCommentFromDb()
        {
            //open database
            SQLiteConnection con = new SQLiteConnection("Data Source=" + this.dbPath);
            con.Open();

            //-->read test case validation results
            SQLiteCommand cmd = new SQLiteCommand(null, con);
            string table_name = this.projName + "_testcase_validation_results";
            cmd.CommandText = "SELECT * FROM " + table_name + ";";
            SQLiteDataReader reader = cmd.ExecuteReader();

            DataTable tempTable = new DataTable();

            //get header information
            if (reader.Read())
            {
                //init azsk DataTable
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    tempTable.Columns.Add(reader.GetName(i), typeof(string));
                }
                tempTable.PrimaryKey = new DataColumn[] { tempTable.Columns["testCaseId"] };

                //add first row to azskTestCases, because we already read one row...
                DataRow new_row = tempTable.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    new_row[i] = reader[i].ToString();
                }
                tempTable.Rows.Add(new_row);
            }

            //get data rows
            while (reader.Read())
            {
                //add new row to azskTestCases
                DataRow new_row = tempTable.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    new_row[i] = reader[i].ToString();
                }
                tempTable.Rows.Add(new_row);
            }

            //close reader
            reader.Close();

            //close connection
            con.Close();

            return tempTable;
        }

        public bool LoadStatusAndComment()
        {
            DataTable tempTable = LoadStatusAndCommentFromDb();

            foreach (DataRow row in dataSource.Rows)
            {
                row["status"] = tempTable.Rows.Find(row["testCaseId"])["Status"].ToString();
                row["comment"] = tempTable.Rows.Find(row["testCaseId"])["Comment"].ToString();
            }

            return true;
        }

        public void AddTestCasesFromAzskTestCases(DataTable azskTestCases, DataTable testCaseCategories)
        {
            for (int i = 0; i < azskTestCases.Rows.Count; i++)
            {
                string testCaseId = azskTestCases.Rows[i][0].ToString();
                string testCaseCategory = testCaseCategories.Rows.Find(azskTestCases.Rows[i][1])["main_category"].ToString();
                if (azskTestCases.Rows.Find(testCaseId)["Status"].ToString() != "Passed") //skipping 'passed' test cases in project test case table
                {
                    string testCaseName = azskTestCases.Rows[i][1].ToString();
                    string resourceGroupName = azskTestCases.Rows[i][4].ToString();
                    string resourceName = azskTestCases.Rows[i][5].ToString();
                    string resourceId = azskTestCases.Rows[i][12].ToString();
                    AddTestCase(testCaseId, testCaseCategory, testCaseName, resourceGroupName, resourceName, resourceId, "", "");

                    //add new item to 'filter combobox' if the item not exists yet
                    if (!comboBox.Items.Contains(testCaseCategory))
                    {
                        comboBox.Items.Add(testCaseCategory);
                    }
                }
            }

            //now cellChanged event can work
            this.initDone = true;

            //set comboBox selected filter value to 'All'
            comboBox.SelectedIndex = comboBox.Items.IndexOf("All");
        }

        public void SaveTestCaseStatusOrCommentToDb(int rowIndex, int colIndex, DataGridView dataGridView_X)
        {
            try
            {
                if (rowIndex != -1 && this.initDone == true)
                {
                    //init vars
                    string tableName = this.projName + "_testcase_validation_results";

                    //open database
                    SQLiteConnection con = new SQLiteConnection("Data Source=" + this.dbPath);
                    con.Open();

                    //-->build query

                    SQLiteCommand cmd = new SQLiteCommand(null, con);
                    if (colIndex == 6)
                    {
                        //status field changed
                        cmd.CommandText = "UPDATE " + tableName + " SET status=@status WHERE testCaseId=@testCaseId;";
                        SQLiteParameter statusParam = new SQLiteParameter("@status", DbType.String);
                        statusParam.Value = dataGridView_X.Rows[rowIndex].Cells[colIndex].Value.ToString().ToUpper(); //get status value
                        cmd.Parameters.Add(statusParam);
                    }
                    else if (colIndex == 7)
                    {
                        //comment field changed
                        cmd.CommandText = "UPDATE " + tableName + " SET comment=@comment WHERE testCaseId=@testCaseId;";
                        SQLiteParameter commentParam = new SQLiteParameter("@comment", DbType.String);
                        commentParam.Value = dataGridView_X.Rows[rowIndex].Cells[colIndex].Value.ToString(); //get comment value
                        cmd.Parameters.Add(commentParam);
                    }

                    //set testCaseId parameter
                    SQLiteParameter testCaseIdParam = new SQLiteParameter("@testCaseId", DbType.String);
                    testCaseIdParam.Value = dataGridView_X.Rows[rowIndex].Cells[0].Value.ToString(); //get testCaseId of entry of modified cell
                    cmd.Parameters.Add(testCaseIdParam);

                    //run sql query
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    //MessageBox.Show("Test case record has been updated in the database!");
                }
            }
            catch
            {
                MessageBox.Show("Error. Test case record info is not saved in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Filter(string value)
        {
            if (value == "All")
            {
                this.dataSource.DefaultView.RowFilter = "";
            }
            else
            {
                this.dataSource.DefaultView.RowFilter = "testCaseCategory = '" + value + "'";
            }
        }

        public void Clear()
        {
            this.dataSource.Clear();
            this.comboBox.Items.Clear();
            this.initDone = false;
            this.projName = "";
        }
    }
}
