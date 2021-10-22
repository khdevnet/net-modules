using System.Collections.Generic;
using VavaCars.TestAutomation.API.E2ETests.Logging;

namespace VavaCars.TestAutomation.API.E2ETests.ScenarioRunner
{
    public class ScenarioExecutionContext
    {
        public ITestOutputLogger Output { get; set; }

        public ScenarioExecutionContext(ITestOutputLogger output)
        {
            Output = output;
            Data = new Dictionary<string, object>();
        }

        public Dictionary<string, object> Data { get; }
    }
}
