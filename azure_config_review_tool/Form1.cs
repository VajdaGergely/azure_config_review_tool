using azure_administration_tool1.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace azure_administration_tool1
{
    public partial class Form1 : Form
    {
        private Settings settings;
        private ProfileSettings profileSettings;
        private Project project;
        private TestCaseRepository testCaseRepository;
        private ProjectResourceRepository projectResourceRepository;
        private ProjectTestCaseRepository projectTestCaseRepository;
        private ProjectFindingRepository projectFindingRepository;
        private ProjectEvidenceRepository projectEvidenceRepository;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitEverything()
        {
            try
            {
                settings = new Settings();
                testCaseRepository = new TestCaseRepository();

                if (settings.YmlRepoLocation != "")
                {
                    testCaseRepository.LoadYmlFiles(settings.YmlRepoLocation);
                }
                else
                {
                    MessageBox.Show("Error. Yml repo location has an invalid value!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //web browser default empty page
                webBrowser1.Url = new Uri("file:///" + Directory.GetCurrentDirectory() + @"\webbrowser_component_index.html");

                //init treeView
                testCaseTreeView1.Init();
                testCaseTreeView1.AddAllTestCaseFromTestCaseRepo(testCaseRepository.testCaseList);
                testCaseTreeView1.CalculateChildCounts((TestCaseTreeNode)testCaseTreeView1.Nodes["all_test_cases"]);
                testCaseTreeView1.ShowChildCounts((TestCaseTreeNode)testCaseTreeView1.Nodes["all_test_cases"]);

                comboBox_StatusChanger.Init();
                textBox_ProjTestCaseComment.SetToDefaultState();
            }
            catch
            {
                MessageBox.Show("Error. Settings file content cant be loaded!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private void ClearTestCaseControls()
        {
            label_TestCaseName.Text = "";
            textBox_ProjTestCaseComment.SetToDefaultState();
            webBrowser1.DocumentText = "";
            textBox_Howto.Text = "";
            textBox_Script.Text = "";
            textBox_Example.Text = "";
            textBox_RawYml.Text = "";
            comboBox_StatusChanger.SelectedIndexChanged -= comboBox_StatusChanger_SelectedIndexChanged;
            comboBox_StatusChanger.SetToDefaultState();
            comboBox_StatusChanger.SelectedIndexChanged += comboBox_StatusChanger_SelectedIndexChanged;
            dataGridView_Evidences.DataSource = null;
            pictureBox1.ImageLocation = @".\no_evidence.png";
        }

        private void ClearControls()
        {
            testCaseTreeView1.Clear();
            ClearTestCaseControls();
        }

        private void ClearBusinessObjects()
        {
            settings = null;
            profileSettings = null;
            testCaseRepository = null;
            project = null;
            testCaseTreeView1.Clear();
        }

        private void FinitEverything()
        {
            ClearControls();
            ClearBusinessObjects();
        }

        private void ResetEverything()
        {
            FinitEverything();
            InitEverything();
        }

        private void SetFormPositionAndSize()
        {
            try
            { 
                profileSettings = new ProfileSettings();
                Location = new Point(profileSettings.WindowLeft, profileSettings.WindowTop);
                Size = new Size(profileSettings.WindowWidth, profileSettings.WindowHeight);
            }
            catch
            {
                //do nothing, profile settings are not a critical part of the application
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitEverything();
            SetFormPositionAndSize();
        }

        private void fillComboBoxFilters()
        {
            //set combobox1
            comboBox_FilterProjTestCaseTable.SelectedIndexChanged -= comboBox_FilterProjTestCaseTable_SelectedIndexChanged;
            comboBox_FilterProjTestCaseTable.Init(projectTestCaseRepository.ProjectTestCasePropertyList);
            comboBox_FilterProjTestCaseTable.SelectedIndexChanged += comboBox_FilterProjTestCaseTable_SelectedIndexChanged;

            //set combobox2
            comboBox_FilterProjTestCaseTable2.SelectedIndexChanged -= comboBox_FilterProjTestCaseTable2_SelectedIndexChanged;
            comboBox_FilterProjTestCaseTable2.Init();
            comboBox_FilterProjTestCaseTable2.SelectedIndexChanged += comboBox_FilterProjTestCaseTable2_SelectedIndexChanged;
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string projName = Microsoft.VisualBasic.Interaction.InputBox("Project name", "Create new project");
                if (projName != null && projName != "")
                {
                    project = new Project(settings.DbPath, projName);
                    if (project.ProjName != null)
                    {
                        var isProjectExists = project.IsProjectExists();
                        if (isProjectExists == false)
                        {
                            if (openFileDialog_PsResourcesCsv.ShowDialog() == DialogResult.OK)
                            {
                                //read ps resource file
                                string projectResourceCsvFile = openFileDialog_PsResourcesCsv.FileName;
                                //string projectResourceCsvFile = @"C:\Users\test\Desktop\resources_min.csv";

                                //set projectResourceRepository
                                projectResourceRepository = new ProjectResourceRepository();
                                projectResourceRepository.LoadProjectResources(projectResourceCsvFile);

                                //set ProjectTestCaseRepository
                                projectTestCaseRepository = new ProjectTestCaseRepository();
                                projectTestCaseRepository.CreateProjectTestCases(projectResourceRepository, testCaseRepository);
                                project.Create(projectTestCaseRepository.ProjectTestCaseList);

                                //set ProjectTestCaseTable DatagridView
                                projectTestCaseTableDataGridView1.DataSource = projectTestCaseRepository.ProjectTestCaseListDataTable;
                                projectTestCaseTableDataGridView1.Refresh();

                                //set ProjectTestCaseTable ComboBoxes
                                fillComboBoxFilters();

                                //set treeView
                                testCaseTreeView1.AddTestCasesFromProjectTestCaseRepo(project.ProjName, projectTestCaseRepository.ProjectTestCaseList,
                                    testCaseRepository.testCaseList);
                                testCaseTreeView1.CalculateChildCounts(testCaseTreeView1.ProjectNode);
                                testCaseTreeView1.ShowChildCounts(testCaseTreeView1.ProjectNode);

                                MessageBox.Show("Project has been created.", "Project created");
                            }
                            else
                            {
                                MessageBox.Show("Csv file not selected. No project created.", "No project created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else if (MessageBox.Show("Project already exists! Would you like to open the project?", "Project already exists",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            //set ProjectTestCaseRepository
                            List<ProjectTestCase> projectTestCaseList = project.Open();
                            projectTestCaseRepository = new ProjectTestCaseRepository(projectTestCaseList);

                            //set ProjectTestCaseTable DatagridView
                            projectTestCaseTableDataGridView1.DataSource = projectTestCaseRepository.ProjectTestCaseListDataTable;
                            projectTestCaseTableDataGridView1.Refresh();

                            //set ProjectTestCaseTable ComboBoxes
                            fillComboBoxFilters();

                            //set treeView
                            testCaseTreeView1.AddTestCasesFromProjectTestCaseRepo(project.ProjName, projectTestCaseRepository.ProjectTestCaseList,
                                testCaseRepository.testCaseList);
                            testCaseTreeView1.CalculateChildCounts(testCaseTreeView1.ProjectNode);
                            testCaseTreeView1.ShowChildCounts(testCaseTreeView1.ProjectNode);

                            MessageBox.Show("Project has been opened.", "Project has been opened");
                        }
                        else
                        {
                            project = null;
                        }
                    }
                    else
                    {
                        project = null;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error. An exception occured!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string projName = Microsoft.VisualBasic.Interaction.InputBox("Project name", "Open project");
                if (projName != null && projName != "")
                {
                    project = new Project(settings.DbPath, projName);
                    if (project.ProjName != null)
                    {
                        var isProjectExists = project.IsProjectExists();
                        if (isProjectExists == true)
                        {
                            //set ProjectTestCaseRepository
                            List<ProjectTestCase> projectTestCaseList = project.Open();
                            projectTestCaseRepository = new ProjectTestCaseRepository(projectTestCaseList);

                            //set ProjectTestCaseTable DatagridView
                            projectTestCaseTableDataGridView1.DataSource = projectTestCaseRepository.ProjectTestCaseListDataTable;
                            projectTestCaseTableDataGridView1.Refresh();

                            //set ProjectTestCaseTable ComboBoxes
                            fillComboBoxFilters();

                            //set treeView
                            testCaseTreeView1.AddTestCasesFromProjectTestCaseRepo(project.ProjName, projectTestCaseRepository.ProjectTestCaseList,
                                testCaseRepository.testCaseList);
                            testCaseTreeView1.CalculateChildCounts(testCaseTreeView1.ProjectNode);
                            testCaseTreeView1.ShowChildCounts(testCaseTreeView1.ProjectNode);

                            MessageBox.Show("Project has been opened.", "Project has been opened");
                        }
                        else
                        {
                            MessageBox.Show("Project is not exists with the name: " + project.ProjName, "Project not opened");
                            project = null;
                        }
                    }
                    else
                    {
                        project = null;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error. An exception occured!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void closeProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (project != null && !project.IsClosed)
            {
                project.Close();
                ClearTestCaseControls();
            }
            testCaseTreeView1.DeleteProjectNode();
            projectTestCaseTableDataGridView1.DataSource = null;
            projectTestCaseTableDataGridView1.Refresh();
            comboBox_FilterProjTestCaseTable.Clear();
            comboBox_FilterProjTestCaseTable2.Clear();
            projectResourceRepository = null;
            projectTestCaseRepository = null;
        }

        private void refreshAllTestCasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetEverything();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //check selected treeview node and get testCaseId
            if (testCaseTreeView1.SelectedNode != null && testCaseTreeView1.SelectedNode.TestCase != null)
            {
                TestCase testCase = testCaseTreeView1.SelectedNode.TestCase;
                //get the filename of the test case yml file by testcaseId
                var filename = testCase.FileNameFull;

                //is yml file exists
                if (File.Exists(filename))
                {
                    //open in notepad++
                    System.Diagnostics.Process.Start(settings.NotepadPlusPlusLocation, "\"" + filename + "\"");
                }
                else
                {
                    MessageBox.Show("Error! Can't open " + filename + " file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView_Evidences_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //get testcase by selected treeview node
            TestCase testCase = testCaseTreeView1.SelectedNode.TestCase;

            //open evidence (the one that has been selected in the datagridView)
            testCase.OpenEvidenceInWindowsGui(e.RowIndex);
        }

        private void dataGridView_Evidences_SelectionChanged(object sender, EventArgs e)
        {
            //get testcase by selected treeview node
            TestCase testCase = testCaseTreeView1.SelectedNode.TestCase;

            if (testCase != null)
            {
                //Load Evidence picture
                if (testCase.EvidenceFileNameList != null && testCase.EvidenceFileNameList.Count == dataGridView_Evidences.Rows.Count)
                {
                    //load selected evidence if it is an image into the pictureBox
                    try
                    {
                        int evidenceIndex = dataGridView_Evidences.SelectedCells[0].RowIndex;
                        if (testCase.isEvidenceAnImageFile(evidenceIndex))
                        {
                            string evidenceFile = testCase.EvidenceFileNameList[evidenceIndex];
                            pictureBox1.ImageLocation = evidenceFile;
                        }
                        else
                        {
                            pictureBox1.ImageLocation = @".\no_evidence.png";
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Exception occured! Can't load evidence file!");
                    }
                }
                else
                {
                    MessageBox.Show("Error! datagridview count and evidence count differs!");
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(MessageBox.Show("Are you sure you quit?", "Exiting", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                //abort exit
                e.Cancel = true;
            }
            else
            {
                //saving profile settings before exit
                try
                {
                    profileSettings.Save(Location.X, Location.Y, Size.Width, Size.Height);
                }
                catch
                {
                    //do nothing, profile settings are not a critical part of the application
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (testCaseTreeView1.SelectedNode != null && testCaseTreeView1.SelectedNode.TestCase != null)
            {
                TestCase testCase = testCaseTreeView1.SelectedNode.TestCase;
                testCase.OpenTestCaseYmlParentFolder();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (testCaseTreeView1.SelectedNode != null && testCaseTreeView1.SelectedNode.TestCase != null)
            {
                TestCase testCase = testCaseTreeView1.SelectedNode.TestCase;
                testCase.OpenTestCaseEvidenceParentFolder();
            }
        }

        private void testCaseTreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //new
            TestCase testCase = ((TestCaseTreeNode)e.Node).TestCase;
            if (testCase != null)
            {
                label_TestCaseName.Text = testCase.TestCaseId;
                if (e.Node.GetType() == typeof(ProjectTestCaseTreeNode) && ((ProjectTestCaseTreeNode)e.Node).ProjectTestCase != null)
                {
                    ProjectTestCase projectTestCase = ((ProjectTestCaseTreeNode)e.Node).ProjectTestCase;
                    comboBox_StatusChanger.SelectedIndexChanged -= comboBox_StatusChanger_SelectedIndexChanged;
                    comboBox_StatusChanger.SelectedIndex = comboBox_StatusChanger.Items.IndexOf(projectTestCase.Status.ToString());
                    comboBox_StatusChanger.SelectedIndexChanged += comboBox_StatusChanger_SelectedIndexChanged;
                    comboBox_StatusChanger.Enabled = true;
                    textBox_ProjTestCaseComment.Text = projectTestCase.Comment;
                    textBox_ProjTestCaseComment.Enabled = true;
                }
                else
                {
                    comboBox_StatusChanger.SelectedIndexChanged -= comboBox_StatusChanger_SelectedIndexChanged;
                    comboBox_StatusChanger.SetToDefaultState();
                    comboBox_StatusChanger.SelectedIndexChanged += comboBox_StatusChanger_SelectedIndexChanged;
                    textBox_ProjTestCaseComment.SetToDefaultState();
                }

                //Set Gergo Guide
                webBrowser1.DocumentText = testCase.GergoHowToHtml;

                //Set FullHowto, Script, Example
                textBox_Howto.Text = testCase.Howto;
                textBox_Script.Text = testCase.Script;
                textBox_Example.Text = testCase.Example;

                //Set Full yml file content
                if (File.Exists(testCase.FileNameFull))
                {
                    textBox_RawYml.Text = File.ReadAllText(testCase.FileNameFull);
                }

                //Set evidences table
                dataGridView_Evidences.SelectionChanged -= dataGridView_Evidences_SelectionChanged;
                dataGridView_Evidences.DataSource = testCase.EvidencesDataTable;
                dataGridView_Evidences.Refresh();
                dataGridView_Evidences.SelectionChanged += dataGridView_Evidences_SelectionChanged;

                //Load Evidence picture
                if (testCase.ImageEvidenceList != null && testCase.ImageEvidenceList.Count > 0)
                {
                    //load the first image evidence in the picture box
                    pictureBox1.ImageLocation = testCase.ImageEvidenceList[0];
                }
                else
                {
                    pictureBox1.ImageLocation = @".\no_evidence.png";
                }
            }
            else
            {
                ClearTestCaseControls();
            }
        }

        private void projectTestCaseTableDataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            int[] enabledColumns = ProjectTestCaseRepository.EnabledColumns;
            projectTestCaseTableDataGridView1.ClipboardKeys(e, enabledColumns);
            projectTestCaseTableDataGridView1.Del(e, enabledColumns);
        }

        private void editMainSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //is settings file exists
            if (File.Exists("settings.yml"))
            {
                //open in notepad++
                System.Diagnostics.Process.Start(settings.NotepadPlusPlusLocation, "settings.yml");
            }
            else
            {
                MessageBox.Show("Error! Can't open settings.yml file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void editResourceTypeConstantsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //is resource type matrix file exists
            if (File.Exists("resource_type_variants_matrix.csv"))
            {
                //open in notepad++
                System.Diagnostics.Process.Start(settings.NotepadPlusPlusLocation, "resource_type_variants_matrix.csv");
            }
            else
            {
                MessageBox.Show("Error! Can't open resource_type_variants_matrix.csv file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void projectTestCaseTableDataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if(e.RowIndex != -1)
                {
                    var row = projectTestCaseTableDataGridView1.Rows[e.RowIndex];
                    string projectTestCaseId = row.Cells[0].Value.ToString();
                    string value = row.Cells[e.ColumnIndex].Value.ToString();
                    if (e.ColumnIndex == 7)
                    {
                        //change status
                        value = value.ToUpper();
                        ChangeStatusFromDataGridView(value, projectTestCaseId);
                        
                        //writeback toUpper version in dataGridView
                        projectTestCaseTableDataGridView1.CellValueChanged -= projectTestCaseTableDataGridView1_CellValueChanged;
                        row.Cells[e.ColumnIndex].Value = value;
                        projectTestCaseTableDataGridView1.CellValueChanged += projectTestCaseTableDataGridView1_CellValueChanged;
                    }
                    else if (e.ColumnIndex == 8)
                    {
                        //change comment
                        ChangeCommentFromDataGridView(value, projectTestCaseId);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error. Can't save status or comment modification.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangeStatusFromDataGridView(string statusValue, string projectTestCaseId)
        {
            //update projectTestCaseRepository (becuase the DataSource DataTable is readonly and not modify the repository)
            projectTestCaseRepository.ProjectTestCaseList.Find(x => x.ProjectTestCaseId == projectTestCaseId).StatusNotSetEmpty = statusValue;
                //(ValidationStatus)Enum.Parse(typeof(ValidationStatus), statusValue == "" ? "NOT_SET" : statusValue);

            //change status value in combobox - only if the selected Node is the one we have been updated
            //if the selected node is not the updated node, then no task to do, because it will be updated automatically by the same repo that we have been modified
            if (testCaseTreeView1.SelectedNode != null && ((ProjectTestCaseTreeNode)testCaseTreeView1.SelectedNode).ProjectTestCase != null &&
                ((ProjectTestCaseTreeNode)testCaseTreeView1.SelectedNode).ProjectTestCase.ProjectTestCaseId == projectTestCaseId)
            {
                comboBox_StatusChanger.SelectedIndexChanged -= comboBox_StatusChanger_SelectedIndexChanged;
                comboBox_StatusChanger.SelectedIndex = comboBox_StatusChanger.Items.IndexOf(statusValue);
                comboBox_StatusChanger.SelectedIndexChanged += comboBox_StatusChanger_SelectedIndexChanged;
            }

            //save new value into db (despite of the selected node)
            project.ModifyStatus(ProjectTestCase.ConvertStatusStringToDbFormat(statusValue), projectTestCaseId);
        }


        private void ChangeStatusFromComboBox()
        {
            if (testCaseTreeView1.SelectedNode != null && ((ProjectTestCaseTreeNode)testCaseTreeView1.SelectedNode).ProjectTestCase != null &&
                    projectTestCaseRepository.ProjectTestCaseListDataTable != null)
            {
                //get status value and projectTestCaseId
                string statusValue = comboBox_StatusChanger.SelectedItem.ToString();
                ProjectTestCase projectTestCase = ((ProjectTestCaseTreeNode)testCaseTreeView1.SelectedNode).ProjectTestCase;

                //Change status value in projectTestCaseTable
                //projectTestCase.Status = (ValidationStatus)Enum.Parse(typeof(ValidationStatus), statusValue);
                projectTestCase.StatusNotSetEmpty = statusValue;
                projectTestCaseTableDataGridView1.CellValueChanged -= projectTestCaseTableDataGridView1_CellValueChanged;
                projectTestCaseTableDataGridView1.DataSource = projectTestCaseRepository.ProjectTestCaseListDataTable;
                projectTestCaseTableDataGridView1.Refresh();
                projectTestCaseTableDataGridView1.CellValueChanged += projectTestCaseTableDataGridView1_CellValueChanged;

                //save new value into db
                project.ModifyStatus(ProjectTestCase.ConvertStatusStringToDbFormat(statusValue), projectTestCase.ProjectTestCaseId);
            }
        }


        private void comboBox_StatusChanger_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeStatusFromComboBox();
        }

        private void ChangeCommentFromDataGridView(string commentValue, string projectTestCaseId)
        {
            //update projectTestCaseRepository (becuase the DataSource DataTable is readonly and not modify the repository)
            projectTestCaseRepository.ProjectTestCaseList.Find(x => x.ProjectTestCaseId == projectTestCaseId).Comment = commentValue;

            //change comment value in textBox - only if the selected Node is the one we have been updated
            //if the selected node is not the updated node, then no task to do, because it will be updated automatically by the same repo that we have been modified
            if (testCaseTreeView1.SelectedNode != null && ((ProjectTestCaseTreeNode)testCaseTreeView1.SelectedNode).ProjectTestCase != null &&
                ((ProjectTestCaseTreeNode)testCaseTreeView1.SelectedNode).ProjectTestCase.ProjectTestCaseId == projectTestCaseId)
            {
                textBox_ProjTestCaseComment.Text = commentValue;
            }

            //save new value into db (despite of the selected node)
            project.ModifyComment(commentValue, projectTestCaseId);
        }

        private void ChangeCommentFromTextBox()
        {
            if (testCaseTreeView1.SelectedNode != null && ((ProjectTestCaseTreeNode)testCaseTreeView1.SelectedNode).ProjectTestCase != null &&
                    projectTestCaseRepository.ProjectTestCaseListDataTable != null)
            {
                //get comment value and projectTestCaseId
                string commentValue = textBox_ProjTestCaseComment.Text;
                ProjectTestCase projectTestCase = ((ProjectTestCaseTreeNode)testCaseTreeView1.SelectedNode).ProjectTestCase;

                //Change comment value in projectTestCaseTable
                projectTestCase.Comment = commentValue;
                projectTestCaseTableDataGridView1.CellValueChanged -= projectTestCaseTableDataGridView1_CellValueChanged;
                projectTestCaseTableDataGridView1.DataSource = projectTestCaseRepository.ProjectTestCaseListDataTable;
                projectTestCaseTableDataGridView1.Refresh();
                projectTestCaseTableDataGridView1.CellValueChanged += projectTestCaseTableDataGridView1_CellValueChanged;

                //save new value into db
                project.ModifyComment(commentValue, projectTestCase.ProjectTestCaseId);
            }
        }

        private void textBox_ProjTestCaseComment_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChangeCommentFromTextBox();
                TestCaseValidationUtils.CommentValueChanged = false;
            }
            else
            {
                TestCaseValidationUtils.CommentValueChanged = true;
            }
        }

        private void textBox_ProjTestCaseComment_Leave(object sender, EventArgs e)
        {
            if (TestCaseValidationUtils.CommentValueChanged)
            {
                    ChangeCommentFromTextBox();
            }
        }

        private void printResourceCsvPSCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Run each of these commands in powershell:" + "\n\n" +
                "Get-AzResource | Export-Csv -Path output.txt -Delimiter ';'" + "\n\n" +
                "Get-AzResource -ResourceGroupName <<resource_group_name>> | Export-Csv -Path output.txt -Delimiter ';'" + "\n\n\n\n" +
                "Press [CTRL + C] to copy (be careful! it will copy everything at once)"
                , "Powershell command");
        }

        private void button_exportToCsv_Click(object sender, EventArgs e)
        {
            if (saveFileDialog_projTestCaseTable_ExportToCsv.ShowDialog() == DialogResult.OK)
            {
                //read export file location
                string exportCsvFile = saveFileDialog_projTestCaseTable_ExportToCsv.FileName;

                //save projectTestCaseTable csv data to file
                try
                {
                    string csvContent = projectTestCaseRepository.ProjectTestCaseListCsv;
                    if(csvContent != null)
                    {
                        File.WriteAllText(exportCsvFile, csvContent);
                        MessageBox.Show("Export was successfull!");
                    }
                    else
                    {
                        MessageBox.Show("Error!");
                    }
                }
                catch
                {
                    MessageBox.Show("Error!");
                }
            }
        }

        private void comboBox_FilterProjTestCaseTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            //get 1st filter value from combobox1
            string filter1 = comboBox_FilterProjTestCaseTable.SelectedItem.ToString();

            if (filter1 != "All")
            {
                //get individual values by 1st combobox selected value
                List<string> filter2 = projectTestCaseRepository.ProjectTestCaseIndividualValues(filter1);
                
                //set combobox2 values
                comboBox_FilterProjTestCaseTable2.Init(filter2);
            }
            else
            {
                comboBox_FilterProjTestCaseTable2.SelectedIndexChanged -= comboBox_FilterProjTestCaseTable2_SelectedIndexChanged;
                comboBox_FilterProjTestCaseTable2.Init();
                comboBox_FilterProjTestCaseTable2.SelectedIndexChanged += comboBox_FilterProjTestCaseTable2_SelectedIndexChanged;
            }
        }

        private void comboBox_FilterProjTestCaseTable2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filter1 = comboBox_FilterProjTestCaseTable.SelectedItem.ToString();
            string filter2 = comboBox_FilterProjTestCaseTable2.SelectedItem.ToString();
            projectTestCaseTableDataGridView1.DataSource = projectTestCaseRepository.GetProjectTestCaseListDataTableFiltered(filter1, filter2);
            projectTestCaseTableDataGridView1.Refresh();
        }

        private void button_CreateFindings_Click(object sender, EventArgs e)
        {
            projectFindingRepository = new ProjectFindingRepository(projectTestCaseRepository.ProjectTestCaseList, testCaseRepository);
            projectFindingRepository.SetResources();
            List<string> findingNames = projectFindingRepository.NokTestCaseList.Select(x => x.TestCaseId).ToList();

            dataGridView_ProjectFindings.Rows.Clear();
            foreach(string findingName in findingNames)
            {
                dataGridView_ProjectFindings.Rows.Add(findingName);
            }
        }

        private void button_GenerateYmlFiles_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK && folderBrowserDialog2.SelectedPath != null && folderBrowserDialog2.SelectedPath != "" &&
                projectFindingRepository != null)
            {
                //set destination folder
                string destYmlFolder = folderBrowserDialog2.SelectedPath;

                //create yml files
                projectFindingRepository.CreateYmlFiles(destYmlFolder, dataGridView_ProjectFindings.FindingEvidenceMatrix, testCaseRepository);
            }
        }

        private void SetEvidenceListBoxDataSource()
        {
            if (projectEvidenceRepository != null)
            {
                if (radioButton1.Checked)
                {
                    listBox_Evidences.DataSource = projectEvidenceRepository.ProjectEvidenceFilenameList;
                }
                else if (radioButton2.Checked)
                {
                    listBox_Evidences.DataSource = projectEvidenceRepository.ProjectEvidenceRelativePathList;
                }
                else if (radioButton3.Checked)
                {
                    listBox_Evidences.DataSource = projectEvidenceRepository.ProjectEvidenceAbsolutePathList;
                }
                listBox_Evidences.Refresh();
            }
        }

        private void button_SetEvidenceFolder_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK && folderBrowserDialog1.SelectedPath != null && folderBrowserDialog1.SelectedPath != "")
            {
                //fill listbox
                projectEvidenceRepository = new ProjectEvidenceRepository(folderBrowserDialog1.SelectedPath);
                SetEvidenceListBoxDataSource();

                //update datagridview evidence column
                ((DataGridViewComboBoxColumn)dataGridView_ProjectFindings.Columns["finding_evidence"]).DataSource = projectEvidenceRepository.ProjectEvidenceRelativePathList;
            }
        }

        private void button_RemoveEvidenceFromList_Click(object sender, EventArgs e)
        {
            try
            {
                //remove from listBox
                projectEvidenceRepository.ProjectEvidenceList.RemoveAt(listBox_Evidences.SelectedIndex);
                SetEvidenceListBoxDataSource();

                //build new list
                List<string> evidenceNameList = projectEvidenceRepository.ProjectEvidenceRelativePathList;
                evidenceNameList.Insert(0, "");

                //reset all instance of removed listbox value
                foreach(DataGridViewRow row in dataGridView_ProjectFindings.Rows)
                {
                    DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)row.Cells["finding_evidence"];
                    if(!evidenceNameList.Contains(cell.Value))
                    {
                        cell.Value = ""; //set to 0th
                    }
                }

                //update datagridview evidence column
                ((DataGridViewComboBoxColumn)dataGridView_ProjectFindings.Columns["finding_evidence"]).DataSource = evidenceNameList;
            }
            catch
            {
                MessageBox.Show("error");
            }
        }

        private void radioButton_grp1_CheckedChanged(object sender, EventArgs e)
        {
            SetEvidenceListBoxDataSource();
        }

        private void button_EditEvidence_Click(object sender, EventArgs e)
        {
            projectEvidenceRepository.ProjectEvidenceList[listBox_Evidences.SelectedIndex].OpenEvidenceInPaintBrush();
        }

        private void button_OpenEvidence_Click(object sender, EventArgs e)
        {
            projectEvidenceRepository.ProjectEvidenceList[listBox_Evidences.SelectedIndex].OpenEvidenceInWindowsGui();
        }

        private void button_OpenEvidenceFolder_Click(object sender, EventArgs e)
        {
            projectEvidenceRepository.OpenEvidenceRootFolder();
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
