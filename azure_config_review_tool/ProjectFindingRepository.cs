using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azure_administration_tool1
{
    public class ProjectFindingRepository
    {
        public ProjectFindingRepository(List<ProjectTestCase> projectTestCaseList, TestCaseRepository testCaseRepository)
        {
            try
            {
                //get all projectTestCase that's status is nok
                NokProjectTestCaseList = projectTestCaseList.Where(x => x.Status == ValidationStatus.NOK).ToList();

                //get the distinct test case names from the nok project test case list
                List<string> nokTestCaseNames = NokProjectTestCaseList.Select(x => x.Name).Distinct().ToList();

                //get the test cases by distinct nok test case names
                NokTestCaseList = testCaseRepository.testCaseList.Where(x => nokTestCaseNames.Contains(x.TestCaseId)).ToList();
            }
            catch
            {
                NokProjectTestCaseList = null;
                NokTestCaseList = null;
            }
        }

        public List<ProjectTestCase> NokProjectTestCaseList;
        public List<TestCase> NokTestCaseList;

        public void SetResources()
        {
            foreach(TestCase nokTestCase in NokTestCaseList)
            {
                nokTestCase.ResourcesList = NokProjectTestCaseList.Where(x => x.Name == nokTestCase.TestCaseId).Select(x => x.ResourceId).ToList();
            }
        }

        public void CreateYmlFiles(string destFolder, Dictionary<string, string> testCaseEvidenceMatrix, TestCaseRepository testCaseRepository)
        {
            if (NokTestCaseList != null && NokProjectTestCaseList != null)
            {
                //iterating through nok finding list
                foreach (TestCase testCase in NokTestCaseList)
                {
                    //set resources
                    testCase.ResourcesList = NokProjectTestCaseList.Where(x => x.Name == testCase.TestCaseId).Select(x => x.ResourceId).ToList();

                    //set evidence
                    string evidencePathName = testCaseEvidenceMatrix[testCase.TestCaseId];
                    testCase.Evidences = new List<Evidence>() { new Evidence() { Path = evidencePathName, Title = "" } }; //title can be empty

                    //write into yml file
                    testCaseRepository.SaveToYmlFile(destFolder, testCase);
                }
            }
        }
    }
}
