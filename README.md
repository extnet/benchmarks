# benchmarks
Ext.NET benchmarks and performance testing

# Process

The current process involves:
- running a Win/Linux container 
- restarting the container before each test (each bombardier run)
- warming up the container by clicking the links on the main page
- running [bombardier](https://github.com/codesenberg/bombardier) for **65** seconds
- saving bombardier stats into the generated CSV file

## Build an image

```
docker build -t extnet-benchmark-classic -f ".\Ext.Net.Benchmarks.Classic\Dockerfile" .
```

## Run a container

When a page `~/Benchmark/Grid` is invoked with `test=<name>` parameter, it starts logging performance metrics.
Metrics are saved to a file named by the `test` parameter after requests are no longer generated.

The result file is written to a directory mount to a local `d:\_results` path.

### Windows

```
docker run -p 5000:80 --cpus 4 -v "d:\_results":"c:\app\results" --name ext-bench-classic extnet-benchmark-classic
```

### Linux

```
docker run -p 5000:80 --cpus 4 -v "d:\_results":/app/results" --name ext-bench-classic extnet-benchmark-classic
```

## Stop a container

```
docker rm -f ext-bench-classic
```

## Run bombardier

Parameter `count` controls number of grids on a page

```
bombardier -l -c 100 -d 65s -t 65s "http://localhost:5000/Benchmark/Grid?test=classic-71&count=0"

bombardier -l -c 100 -d 65s -t 65s "http://localhost:5000/Benchmark/Grid?test=classic-71&count=4"

bombardier -l -c 100 -d 65s -t 65s "http://localhost:5000/Benchmark/Grid?test=classic-71&count=8"

bombardier -l -c 100 -d 65s -t 65s "http://localhost:5000/Benchmark/Grid?test=classic-71&count=12"

bombardier -l -c 100 -d 65s -t 65s "http://localhost:5000/Benchmark/Grid?test=classic-71&count=16"

bombardier -l -c 100 -d 65s -t 65s "http://localhost:5000/Benchmark/Grid?test=classic-71&count=20"
```

### Direct Event

```
bombardier -l -c 100 -d 65s -t 65s -m POST -b "test=classic-71-directevent&count=0" "http://localhost:5000/Benchmark/RenderDirectToast"

bombardier -l -c 100 -d 65s -t 65s -m POST -b "test=classic-71-directevent&count=4" "http://localhost:5000/Benchmark/RenderDirectToast"

bombardier -l -c 100 -d 65s -t 65s -m POST -b "test=classic-71-directevent&count=8" "http://localhost:5000/Benchmark/RenderDirectToast"

bombardier -l -c 100 -d 65s -t 65s -m POST -b "test=classic-71-directevent&count=12" "http://localhost:5000/Benchmark/RenderDirectToast"

bombardier -l -c 100 -d 65s -t 65s -m POST -b "test=classic-71-directevent&count=16" "http://localhost:5000/Benchmark/RenderDirectToast"

bombardier -l -c 100 -d 65s -t 65s -m POST -b "test=classic-71-directevent&count=20" "http://localhost:5000/Benchmark/RenderDirectToast"

```
