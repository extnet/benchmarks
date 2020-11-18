using Ext.Net.Benchmarks.Classic5.Models;
using Ext.Net.Benchmarks.Common;
using Ext.Net.Core;

using Microsoft.AspNetCore.Mvc;

namespace Ext.Net.Benchmarks.Classic5.Controllers
{
    public class BenchmarkController : Controller
    {
        private static readonly TestRunner _testRunner = new TestRunner();

        public IActionResult Index()
        {
            return this.View();
        }

        public IActionResult Grid(int? count = null, string test = null)
        {
            var total = count ?? 1;

            _testRunner.LogTestRun(test, total);

            var model = new GridBenchmarkModel
            {
                GridCount = total,
                TestName = test ?? string.Empty
            };

            return this.View(model);
        }

        public IActionResult RenderDirectToast(int? count = null, string test = null)
        {
            var total = count ?? 1;

            _testRunner.LogTestRun(test, total);

            var X = this.X();

            for (int i = 0; i < total; i++)
            {
                X.Toast("Test Message " + i);
            }

            return this.Direct();
        }
    }
}
