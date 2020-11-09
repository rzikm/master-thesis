``` ini

BenchmarkDotNet=v0.12.0, OS=manjaro 
Intel Core i5-6300HQ CPU 2.30GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
.NET Core SDK=3.1.103
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  Job-TUOMAC : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|         Method |        Mean |        Error |       StdDev |       Median |  Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------- |------------:|-------------:|-------------:|-------------:|-------:|--------:|-----------:|------:|------:|-----------:|
|      SslStream |    126.5 us |     19.08 us |     55.04 us |     107.0 us |   1.00 |    0.00 |          - |     - |     - |          - |
| QuicConnection | 87,271.7 us | 22,265.29 us | 65,649.71 us | 123,647.5 us | 789.39 |  675.16 | 11000.0000 |     - |     - | 36288272 B |
