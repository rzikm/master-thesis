``` ini

BenchmarkDotNet=v0.12.0, OS=manjaro 
Intel Core i5-6300HQ CPU 2.30GHz (Skylake), 1 CPU, 4 logical and 4 physical cores
.NET Core SDK=3.1.103
  [Host]     : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT
  Job-AWMUQU : .NET Core 3.1.3 (CoreCLR 4.700.20.11803, CoreFX 4.700.20.12001), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|         Method |     Mean |     Error |   StdDev | Ratio | RatioSD |     Gen 0 | Gen 1 | Gen 2 |  Allocated |
|--------------- |---------:|----------:|---------:|------:|--------:|----------:|------:|------:|-----------:|
|      SslStream | 6.441 ms | 0.4034 ms | 1.164 ms |  1.00 |    0.00 |         - |     - |     - |   18.66 KB |
| QuicConnection | 6.547 ms | 1.4783 ms | 4.289 ms |  1.06 |    0.72 | 1000.0000 |     - |     - | 4172.26 KB |
