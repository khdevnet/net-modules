using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VavaCars.TestAutomation.API.E2ETests.Logging;

namespace VavaCars.TestAutomation.API.E2ETests.ScenarioRunner
{
    public delegate Task HandleAsync<in TRequest>(TRequest request);
    public delegate Task<TResponse> HandleAsync<in TRequest, TResponse>(TRequest request);

    public class AsyncScenarioRunner
    {
        private static ConcurrentQueue<Step> _handlers;
        private static ScenarioExecutionContext _context;
        public string Name { get; set; }

        private AsyncScenarioRunner(ScenarioExecutionContext context, IEnumerable<Step> steps)
        {
            _handlers = new ConcurrentQueue<Step>(steps);
            _context = context;
        }

        public static AsyncScenarioRunner SetName(string name)
        {
            var scenarioRunner = new AsyncScenarioRunner(new ScenarioExecutionContext(new TraceTestOutputLogger()), new ConcurrentQueue<Step>());
            scenarioRunner.Name = name;
            return scenarioRunner;
        }

        public static AsyncScenarioRunner CreateWithSteps(IEnumerable<Step> steps)
        {

            return new AsyncScenarioRunner(new ScenarioExecutionContext(new TraceTestOutputLogger()), new ConcurrentQueue<Step>(steps));
        }

        public AsyncScenarioRunner AddSteps(IEnumerable<Step> steps)
        {
            steps.ToList().ForEach(s => _handlers.Enqueue(s));

            return this;
        }

        public async Task RunAsync()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                _context.Output.Log(this.Name);
            }

            var count = _handlers.Count;

            var steps = _handlers.ToArray();

            for (int index = 0; index < steps.Length; index++)
            {
                var step = steps[index];

                var message = string.IsNullOrEmpty(step.Name) ? "Setup" : step.Name;
                if (!string.IsNullOrEmpty(step.Name))
                {
                    _context.Output.Log($"Step {index + 1}/{count}: {message}");
                }

                await step.Handle(_context).ConfigureAwait(false);
            }
        }
    }
}
