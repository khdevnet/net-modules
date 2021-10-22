using System.Threading.Tasks;
using VavaCars.TestAutomation.API.E2E.Tests.Core.Extensions;

namespace VavaCars.TestAutomation.API.E2ETests.ScenarioRunner
{
    public class Step
    {
        public string Name { get; }
        public HandleAsync<ScenarioExecutionContext> Handle { get; }

        protected virtual Task HandleInternal(ScenarioExecutionContext context) { return Task.CompletedTask; }

        protected Step(string name)
        {
            Name = name;
            Handle = HandleInternal;
        }

        protected Step()
        {
            Name = this.GetType().Name.ToStepName();
            Handle = HandleInternal;
        }

        private Step(string name, HandleAsync<ScenarioExecutionContext> handle)
        {
            Name = name;
            Handle = handle;
        }

        public static Step Create(string name, HandleAsync<ScenarioExecutionContext> handle) { return new Step(name, handle); }
        public static Step Create(HandleAsync<ScenarioExecutionContext> handle) { return new Step(string.Empty, handle); }
    }
}
