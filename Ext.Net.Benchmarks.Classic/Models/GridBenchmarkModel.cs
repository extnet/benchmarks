using Ext.Net.Benchmarks.Common;

namespace Ext.Net.Benchmarks.Classic.Models
{
    public class GridBenchmarkModel
    {
        public int GridCount { get; set; }

        public string TestName { get; set; }

        public object[] GridData => CompaniesDataSet.Data;
    }
}