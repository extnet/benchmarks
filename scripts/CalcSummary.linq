<Query Kind="Program">
  <Namespace>System.Globalization</Namespace>
</Query>

void Main()
{
    var rootDir = new DirectoryInfo(@"D:\Andrey\ObjNet\Repos\Ext.NET\benchmarks\results");
    var summaryDir = rootDir.CreateSubdirectory("summary");
    
    var testDirs = rootDir.EnumerateDirectories()
        .Where(x => x.FullName != summaryDir.FullName)
        .ToArray();

    var summary = BuildSummary(testDirs, summaryDir);
    var summaryCsvLines = SummaryToCsvLines(summary);
    
    var summaryFilePath = Path.Combine(summaryDir.FullName, "summary.csv");
    File.WriteAllLines(summaryFilePath, summaryCsvLines);
}

string[] SummaryToCsvLines(Summary summary)
{
    var csvRows = new List<List<string>>();
    
    var isFirst = true;
    
    foreach (var section in summary.Sections)
    {
        if(!isFirst)
        {
            AddNewRow("");
            AddNewRow("");
            AddNewRow("");
        }
        
        AddNewRow(section.Name);
        
        AddNewRow("name / grid count");
        AddCells(section.XLabels);
        
        foreach (var row in section.Rows)
        {
            AddNewRow(row.Label);
            AddCells(row.YValues);
        }
        
        isFirst = false;
    }
    
    PadRows();
    
    return csvRows
        .Select(x => string.Join(';', x))
        .ToArray();

    void AddRow()
    {
        csvRows.Add(new List<string>());
    }

    void AddNewRow(string cell0)
    {
        AddRow();
        AddCell(cell0);
    }

    void AddCell(string cell)
    {
        csvRows.Last().Add(cell);
    }

    void AddCells(IEnumerable<string> cells)
    {
        csvRows.Last().AddRange(cells);
    }
    
    void PadRows()
    {
        var maxRowSize = csvRows
            .Max(x => x.Count);
            
        foreach (var csvRow in csvRows)
        {
            csvRow.AddRange(Enumerable
                .Range(0, maxRowSize - csvRow.Count)
                .Select(_ => ""));
        }
    }
}

Summary BuildSummary(DirectoryInfo[] testDirs, DirectoryInfo summaryDir)
{
    var tests = testDirs.ToDictionary(
        x => x.Name,
        x => ParseTestRuns(x));

    var sections = new[] {
    
        BuildSummarySection(
            "Latency, ms", false,
            tests, x => x.LatencyMs.ToString()),

        BuildSummarySection(
            "Latency, ms (Direct Event)", true,
            tests, x => x.LatencyMs.ToString()),

        BuildSummarySection(
            "Reqs/s", false,
            tests, x => x.RequestsPerSec.ToString()),

        BuildSummarySection(
            "Reqs/s (Direct Event)", true,
            tests, x => x.RequestsPerSec.ToString()),

        BuildSummarySection(
            "Throughput, MB/s", false,
            tests, x => x.ThroughputMBPerSec.ToString()),

        BuildSummarySection(
            "Throughput, MB/s (Direct Event)", true,
            tests, x => x.ThroughputMBPerSec.ToString()),


        BuildSummarySection(
            "Avg RAM, MB", false,
            tests, x => x.Items.Average(i => i.GCAllocatedMb).ToString("F2")),

        BuildSummarySection(
            "Avg RAM, MB (Direct Event)", true,
            tests, x => x.Items.Average(i => i.GCAllocatedMb).ToString("F2")),

        BuildSummarySection(
            "Avg CPU, %", false,
            tests, x => x.Items.Average(i => i.CpuLoad).ToString("F2")),

        BuildSummarySection(
            "Avg CPU, % (Direct Event)", true,
            tests, x => x.Items.Average(i => i.CpuLoad).ToString("F2")),

        BuildSummarySection(
            "Gen0 collection count", false,
            tests, x => x.Items.Last().Gen0Count.ToString()),

        BuildSummarySection(
            "Gen0 collection count (Direct Event)", true,
            tests, x => x.Items.Last().Gen0Count.ToString()),

        BuildSummarySection(
            "Gen1 collection count", false,
            tests, x => x.Items.Last().Gen1Count.ToString()),

        BuildSummarySection(
            "Gen1 collection count (Direct Event)", true,
            tests, x => x.Items.Last().Gen1Count.ToString()),

        BuildSummarySection(
            "Gen2 collection count", false,
            tests, x => x.Items.Last().Gen2Count.ToString()),

        BuildSummarySection(
            "Gen2 collection count (Direct Event)", true,
            tests, x => x.Items.Last().Gen2Count.ToString()),
    };
    
    return new Summary 
    {
        Sections = sections.ToList()
    };
}

SummarySection BuildSummarySection(string name, bool isDirect, Dictionary<string, TestRunInfo[]> tests, Func<TestRunInfo, string> valueFn)
{
    var data = tests
        .SelectMany(t => t
            .Value
            .Where(r => r.IsDirectEvent == isDirect)
            .Select(r => (t.Key, r.GridCount, CellValue: valueFn(r))))
        .ToArray();

    var xLabels = data
        .Select(x => x.GridCount)
        .Distinct()
        .OrderBy(x => x)
        .Select(x => x.ToString())
        .ToList();

    var rows = data
        .ToLookup(x => x.Key)
        .Select(x => new SummaryRow
        {
            Label = x.Key,
            YValues = x
                .OrderBy(x => x.GridCount)
                .Select(y => y.CellValue)
                .ToList()
        })
        .ToList();
        
    return new SummarySection
    {
        Name = name,
        XLabels = xLabels,
        Rows = rows
    };
}

TestRunInfo[] ParseTestRuns(DirectoryInfo testDir)
{
    var runs = new List<TestRunInfo>();
    
    foreach (var file in testDir.EnumerateFiles())
    {
        var run = ParseTestRun(file);
        runs.Add(run);
    }
    
    return runs.ToArray();
}

TestRunInfo ParseTestRun(FileInfo file)
{
    try
    {
        var data = File.ReadAllText(file.FullName);
        return ParseTestResult(data, file.Name);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Failed to parse: '{file.Name}'", ex);
    }

    TestRunInfo ParseTestResult(string data, string name)
    {
        var gridCountRgx = new Regex(@"_(?<value>\d+)\.csv$");
        var osRgx = new Regex(@"^OS Version:\s+(?<value>.*?);", RegexOptions.Multiline);
        var clrRgx = new Regex(@"^CLR Version:\s+(?<value>.*?);", RegexOptions.Multiline);
        var procRxg = new Regex(@"^Processor Count:\s+(?<value>.*?);", RegexOptions.Multiline);

        var reqRgx = new Regex(@"^\s+Reqs/sec\s+(?<value>[0-9\.]+)\s", RegexOptions.Multiline);
        var latencyRgx = new Regex(@"^\s+Latency\s+(?<value>[0-9\.]+)(?<scale>m?s)\s", RegexOptions.Multiline);
        var throughputRgx = new Regex(@"^\s+Throughput:\s+(?<value>[0-9\.]+)MB", RegexOptions.Multiline);

        var info = new TestRunInfo
        {
            GridCount = GetValueMatchInt(gridCountRgx, name),
            IsDirectEvent = name.Contains("directevent"),
            
            OS = GetValueMatchStr(osRgx, data),
            CLR = GetValueMatchStr(clrRgx, data),
            ProcessorCount = GetValueMatchInt(procRxg, data),
            RequestsPerSec = GetValueMatchDecimal(reqRgx, data),
            LatencyMs = GetValueMatchDecimalScaled(latencyRgx, data, s => s == "s" ? 1000 : 1),
            ThroughputMBPerSec = GetValueMatchDecimal(throughputRgx, data),
        };

        var isFound = false;
        var cpuLoad = 0;
        var ts = 0;

        foreach (var line in data.Split('\n'))
        {
            if (!isFound)
            {
                if (line.StartsWith("TS (ms);CPU time (ms)"))
                {
                    isFound = true;
                }
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(';')
                .Select(x => x.Trim())
                .Select(int.Parse)
                .ToArray();

            var newTs = Math.Max(0, parts[0]);
            var newCpuLoad = Math.Max(0, parts[1]);
            
            var cpuLoadNorm = (newTs - ts == 0) 
                ? 0 
                : 100m * Math.Max(0, newCpuLoad - cpuLoad) / ((newTs - ts) * info.ProcessorCount);

            ts = newTs;
            cpuLoad = newCpuLoad;

            info.Items.Add(new TestResultItem
            {
                TimestampMs = newTs,
                CpuLoad = cpuLoadNorm,
                PrivateMemMb = (decimal)parts[2] / (1024 * 1024),
                WorkingSetMb = (decimal)parts[3] / (1024 * 1024),
                GCAllocatedMb = (decimal)parts[4] / (1024 * 1024),
                Gen0Count = parts[5],
                Gen1Count = parts[6],
                Gen2Count = parts[7],
                ThreadPoolSize = parts[8]
            });
        }

        return info;

        string GetValueMatchStr(Regex rgx, string str)
        {
            var m = rgx.Match(str);
            if (!m.Success)
            {
                throw new InvalidOperationException($"Regex '{rgx}' is not matched.");
            }
            return m.Groups["value"].Value;
        }

        int GetValueMatchInt(Regex rgx, string str)
        {
            var m = GetValueMatchStr(rgx, str);
            return int.Parse(m);
        }

        decimal GetValueMatchDecimal(Regex rgx, string str)
        {
            var m = GetValueMatchStr(rgx, str);
            return decimal.Parse(m, CultureInfo.InvariantCulture);
        }

        decimal GetValueMatchDecimalScaled(Regex rgx, string str, Func<string, decimal> scaleFn)
        {
            var m = rgx.Match(str);
            if (!m.Success)
            {
                throw new InvalidOperationException($"Regex '{rgx}' is not matched.");
            }

            var v = m.Groups["value"].Value;
            var s = m.Groups["scale"].Value;

            return decimal.Parse(v, CultureInfo.InvariantCulture)
                * scaleFn(s);
        }
    }
}

class TestRunInfo
{
    public int GridCount { get; set; }
    public bool IsDirectEvent { get; set;}

    public string OS { get; set; }
    public string CLR { get; set; }
    public int ProcessorCount { get; set; }

    public decimal RequestsPerSec { get; set; }
    public decimal LatencyMs { get; set; }
    public decimal ThroughputMBPerSec { get; set; }

    public List<TestResultItem> Items = new List<TestResultItem>();
}

class TestResultItem
{
    public int TimestampMs { get; set; }
    public decimal CpuLoad { get; set; }

    public decimal PrivateMemMb { get; set; }
    public decimal WorkingSetMb { get; set; }
    public decimal GCAllocatedMb { get; set; }

    public int Gen0Count { get; set; }
    public int Gen1Count { get; set; }
    public int Gen2Count { get; set; }

    public int ThreadPoolSize { get; set; }
}

class Summary
{
    public List<SummarySection> Sections { get; set; } = new List<SummarySection>();
}

class SummarySection
{
    public string Name { get; set; }

    public List<string> XLabels { get; set; } = new List<string>();

    public List<SummaryRow> Rows { get; set; }
}

class SummaryRow
{
    public string Label { get; set; }
    
    public List<string> YValues { get; set; } = new List<string>();
}
