``` ini

BenchmarkDotNet=v0.12.0, OS=manjaro 
Intel Core i5-6300HQ CPU 2.30GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
.NET Core SDK=3.1.103
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  Job-CWZBMU : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|         Method |       Mean |      Error |     StdDev |     Median | Ratio | RatioSD |     Gen 0 |     Gen 1 | Gen 2 |  Allocated |
|--------------- |-----------:|-----------:|-----------:|-----------:|------:|--------:|----------:|----------:|------:|-----------:|
|      SslStream |   5.529 ms |  0.1972 ms |  0.5497 ms |   5.440 ms |  1.00 |    0.00 |         - |         - |     - |   18.91 KB |
| QuicConnection | 106.344 ms | 21.5032 ms | 63.4028 ms | 126.804 ms | 19.36 |   11.99 | 1000.0000 | 1000.0000 |     - | 3841.66 KB |
