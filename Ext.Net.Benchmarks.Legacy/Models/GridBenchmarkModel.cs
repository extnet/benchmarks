using Ext.Net.Benchmarks.Common;

namespace Ext.Net.Benchmarks.Legacy.Models
{
    public class GridBenchmarkModel
    {
        public int GridCount { get; set; }

        public object[] GridData => CompaniesDataSet.Data;
    }
}