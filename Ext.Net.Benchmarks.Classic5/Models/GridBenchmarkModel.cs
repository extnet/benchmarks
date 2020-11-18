using Ext.Net.Benchmarks.Common;

namespace Ext.Net.Benchmarks.Classic5.Models
{
    public class GridBenchmarkModel
    {
        public int GridCount { get; set; }

        public object[] GridData => CompaniesDataSet.Data;
    }
}