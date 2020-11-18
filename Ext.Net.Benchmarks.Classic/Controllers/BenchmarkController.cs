﻿using Ext.Net.Benchmarks.Classic.Models;
using Ext.Net.Benchmarks.Common;
using Ext.Net.Core;

using Microsoft.AspNetCore.Mvc;

namespace Ext.Net.Benchmarks.Classic.Controllers
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
            _testRunner.LogTestRun(test);

            var model = new GridBenchmarkModel
            {
                GridCount = count ?? 1
            };

            return this.View(model);
        }

        public IActionResult RenderDirectToast(int? count)
        {
            var total = count ?? 1;
            var X = this.X();

            for (int i = 0; i < total; i++)
            {
                X.Toast("Test Message " + i);
            }

            return this.Direct();
        }
    }
}
