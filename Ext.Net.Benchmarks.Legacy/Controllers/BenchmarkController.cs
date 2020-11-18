using System.Web.Mvc;

using Ext.Net.Benchmarks.Common;
using Ext.Net.Benchmarks.Legacy.Models;
using Ext.Net.MVC;

namespace Ext.Net.Benchmarks.Legacy.Controllers
{
    public class BenchmarkController : Controller
    {
        private static readonly TestRunner _testRunner = new TestRunner();

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Grid(int? count = null, string test = null)
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

        public ActionResult RenderDirectToast(int? count = null, string test = null)
        {
            var total = count ?? 1;

            _testRunner.LogTestRun(test, total);

            for (int i = 0; i < total; i++)
            {
                X.Toast("Test Message " + i);
            }

            return this.Direct();
        }
    }
}