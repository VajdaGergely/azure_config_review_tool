using azure_administration_tool1.Properties;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Core.Tokens;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;

namespace azure_administration_tool1
{
    public class Project
    {
        private string dbPath;
        private string projName;

        public Project(string dbPath, string projName)
        {
            if (File.Exists(dbPath))
            {
                this.dbPath = dbPath;
            }
            else
            {
                this.dbPath = null;
                MessageBox.Show("Error. Azure db file is not exists with path: " + dbPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Regex matchAlphaNumUnderscore = new Regex(@"^[a-zA-Z0-9_]+$");
            if (matchAlphaNumUnderscore.IsMatch(projName))
            {
                this.projName = projName;
            }
            else
            {
                this.projName = null;
                MessageBox.Show("Invalid project name! Project name can only contains: [a-z][A-Z][0-9][_] characters.", "Invalid project name",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            IsClosed = true;
        }

        public string DbPath 
        {
            get
            {
                if(dbPath == null)
                {
                    MessageBox.Show("Warning! Azure db file has null value.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return dbPath;
            }

            set
            {
                dbPath = value;
            }
        }

        public string ProjName 
        { 
            get
            {
                if (projName == null)
                {
                    MessageBox.Show("Warning! Project name has null value.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return projName;
            }

            set
            {
                projName = value;
            }
        }

        public bool IsClosed { get; set; }

        private string ProjectTableName
        {
            get
            {
                if (ProjName != null)
                {
                    return ProjName + "_project_testcase_table";
                }
                else
                {
                    return null;
                }
            }
        }

        public bool? IsProjectExists()
        {
            bool? result = null;
            SQLiteConnection con = null;
            try
            {
                con = new SQLiteConnection("Data Source=" + DbPath);
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(null, con);
                cmd.CommandText = "SELECT COUNT(*) FROM projects WHERE proj_name=@project_name;";
                SQLiteParameter projectNameParam = new SQLiteParameter("@project_name", DbType.String);
                projectNameParam.Value = ProjName;
                cmd.Parameters.Add(projectNameParam);
                cmd.Prepare();
                SQLiteDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows && reader.Read() && reader.FieldCount == 1)
                {
                    int queryResult = int.Parse(reader[0].ToString());
                    reader.Close();

                    switch (queryResult)
                    {
                        case 1:
                            result = true;
                            break;
                        case 0:
                            result = false;
                            break;
                        default:
                            result = null;
                            break;
                    }
                }
                con.Close();
            }
            catch
            {
                MessageBox.Show("Error. Can't read from database!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                result = null;
            }
            return result;
        }

        public void Create(List<ProjectTestCase> projectTestCaseList)
        {
            try
            {
                SQLiteConnection con = new SQLiteConnection("Data Source=" + DbPath);
                con.Open();

                //insert new table name in projects table
                SQLiteCommand cmd1 = new SQLiteCommand(null, con);
                cmd1.CommandText = "INSERT INTO projects VALUES (@proj_name);";
                SQLiteParameter projNameParam = new SQLiteParameter("@proj_name", DbType.String);
                projNameParam.Value = ProjName;
                cmd1.Parameters.Add(projNameParam);
                cmd1.Prepare();
                cmd1.ExecuteNonQuery();

                //create 'projectTestCaseTable' sql table in database
                SQLiteCommand cmd2 = new SQLiteCommand(null, con);
                cmd2.CommandText = "CREATE TABLE IF NOT EXISTS '" + ProjectTableName + "' (" +
                    "'ProjectTestCaseId' TEXT, " +
                    "'IsBlacklisted' TEXT, " +
                    "'Name' TEXT, " +
                    "'ResourceType' TEXT, " +
                    "'ResourceGroupName' TEXT, " +
                    "'ResourceName' TEXT, " +
                    "'ResourceId' TEXT, " +
                    "'Status' TEXT, " +
                    "'Comment' TEXT, " +
                    "'AztsStatus' TEXT" +
                    ");";
                cmd2.ExecuteNonQuery();

                //insert projectTestCase entries into the database
                SQLiteCommand cmd3 = new SQLiteCommand(null, con);
                foreach (ProjectTestCase projectTestCase in projectTestCaseList)
                {
                    cmd3.CommandText = "INSERT INTO " + ProjectTableName + " VALUES (@projectTestCaseId, @isBlacklisted, @name, @resourceType, " +
                        "@resourceGroupName, @resourceName, @resourceId, @status, @comment, @aztsStatus);";
                    SQLiteParameter ProjectTestCaseIdParam = new SQLiteParameter("@projectTestCaseId", DbType.String);
                    SQLiteParameter IsBlacklistedParam = new SQLiteParameter("@isBlacklisted", DbType.String);
                    SQLiteParameter NameParam = new SQLiteParameter("@name", DbType.String);
                    SQLiteParameter ResourceTypeParam = new SQLiteParameter("@resourceType", DbType.String);
                    SQLiteParameter ResourceGroupNameParam = new SQLiteParameter("@resourceGroupName", DbType.String);
                    SQLiteParameter ResourceNameParam = new SQLiteParameter("@resourceName", DbType.String);
                    SQLiteParameter ResourceIdParam = new SQLiteParameter("@resourceId", DbType.String);
                    SQLiteParameter StatusParam = new SQLiteParameter("@status", DbType.String);
                    SQLiteParameter CommentParam = new SQLiteParameter("@comment", DbType.String);
                    SQLiteParameter AztsStatusParam = new SQLiteParameter("@aztsStatus", DbType.String);
                    ProjectTestCaseIdParam.Value = projectTestCase.ProjectTestCaseId;
                    IsBlacklistedParam.Value = projectTestCase.IsBlacklisted;
                    NameParam.Value = projectTestCase.Name;
                    ResourceTypeParam.Value = projectTestCase.ResourceType;
                    ResourceGroupNameParam.Value = projectTestCase.ResourceGroupName;
                    ResourceNameParam.Value = projectTestCase.ResourceName;
                    ResourceIdParam.Value = projectTestCase.ResourceId;
                    StatusParam.Value = projectTestCase.Status;
                    CommentParam.Value = projectTestCase.Comment;
                    AztsStatusParam.Value = projectTestCase.AztsStatus;
                    cmd3.Parameters.Add(ProjectTestCaseIdParam);
                    cmd3.Parameters.Add(IsBlacklistedParam);
                    cmd3.Parameters.Add(NameParam);
                    cmd3.Parameters.Add(ResourceTypeParam);
                    cmd3.Parameters.Add(ResourceGroupNameParam);
                    cmd3.Parameters.Add(ResourceNameParam);
                    cmd3.Parameters.Add(ResourceIdParam);
                    cmd3.Parameters.Add(StatusParam);
                    cmd3.Parameters.Add(CommentParam);
                    cmd3.Parameters.Add(AztsStatusParam);
                    cmd3.Prepare();
                    cmd3.ExecuteNonQuery();
                }
                con.Close();
                IsClosed = false;
            }
            catch
            {
                MessageBox.Show("Error. Can not create tables for project!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public List<ProjectTestCase> Open()
        {
            List<ProjectTestCase> projectTestCaseList = new List<ProjectTestCase>();
            try
            {
                SQLiteConnection con = new SQLiteConnection("Data Source=" + DbPath);
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(null, con);
                cmd.CommandText = "SELECT * FROM " + ProjectTableName + ";";
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ProjectTestCase projectTestCase = new ProjectTestCase()
                    {
                        ProjectTestCaseId = reader[0].ToString(),
                        IsBlacklisted = bool.Parse(reader[1].ToString()),
                        Name = reader[2].ToString(),
                        ResourceType = reader[3].ToString(),
                        ResourceGroupName = reader[4].ToString(),
                        ResourceName = reader[5].ToString(),
                        ResourceId = reader[6].ToString(),
                        Status = (ValidationStatus)Enum.Parse(typeof(ValidationStatus), reader[7].ToString(), true),
                        Comment = reader[8].ToString(),
                        AztsStatus = (AztsStatus)Enum.Parse(typeof(AztsStatus), reader[9].ToString(), true)
                    };
                    projectTestCaseList.Add(projectTestCase);
                }
                con.Close();
                IsClosed = false;
                return projectTestCaseList;
            }
            catch
            {
                MessageBox.Show("Error. Can not read from database!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public void Close()
        {
            this.projName = null;
            IsClosed = true;
        }

        public void ModifyStatus(string statusValue, string projectTestCaseId)
        {
            try
            {
                SQLiteConnection con = new SQLiteConnection("Data Source=" + DbPath);
                con.Open();

                //insert new table name in projects table
                SQLiteCommand cmd1 = new SQLiteCommand(null, con);
                cmd1.CommandText = "UPDATE " + ProjectTableName + " SET status=@status WHERE projectTestCaseId=@projectTestCaseId;";
                SQLiteParameter statusParam = new SQLiteParameter("@status", DbType.String);
                SQLiteParameter projectTestCaseIdParam = new SQLiteParameter("@projectTestCaseId", DbType.String);
                statusParam.Value = statusValue;
                projectTestCaseIdParam.Value = projectTestCaseId;
                cmd1.Parameters.Add(statusParam);
                cmd1.Parameters.Add(projectTestCaseIdParam);
                cmd1.Prepare();
                cmd1.ExecuteNonQuery();
                con.Close();
            }
            catch
            {
                MessageBox.Show("Error. Can not save status modification in database!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ModifyComment(string commentValue, string projectTestCaseId)
        {
            try
            {
                SQLiteConnection con = new SQLiteConnection("Data Source=" + DbPath);
                con.Open();

                //insert new table name in projects table
                SQLiteCommand cmd1 = new SQLiteCommand(null, con);
                cmd1.CommandText = "UPDATE " + ProjectTableName + " SET comment=@comment WHERE projectTestCaseId=@projectTestCaseId;";
                SQLiteParameter commentParam = new SQLiteParameter("@comment", DbType.String);
                SQLiteParameter projectTestCaseIdParam = new SQLiteParameter("@projectTestCaseId", DbType.String);
                commentParam.Value = commentValue;
                projectTestCaseIdParam.Value = projectTestCaseId;
                cmd1.Parameters.Add(commentParam);
                cmd1.Parameters.Add(projectTestCaseIdParam);
                cmd1.Prepare();
                cmd1.ExecuteNonQuery();
                con.Close();
            }
            catch
            {
                MessageBox.Show("Error. Can not save status modification in database!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
