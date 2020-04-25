# Master Thesis - Implementation of QUIC protocol for .NET

This repository contains all necessary code changes needed for managed (C#)
implementation of the QUIC protocol for .NET.

## Quickstart

To setup development environment, run first the `setup.sh` or `setup.cmd` scripts.
Note that on windows, you need to run the script from developer shell prompt (x64), so that `nmake`
used to compile OpenSSL is in path. The scripts will:
- Build the custom openssl branch with QUIC support, leaving the binaries in artifacts/openssl, they
  will be copied from here during the library build.
- Restore nuget packages inside the dotnet runtime repository, these are needed to run unit tests
  properly

After that, you should be able to build and run the managed QUIC library using
`src/System.Net.Quic/System.Net.Quic.sln` solution.

TODO: For technical reasons, only 64-bit runtime supported on windows.

## Note on repository organisation

I wanted to keep the implementation inside the .NET runtime repository fork because that way I
can run the functional tests made for msquic based implementation against my implementation, and
also try my implementation as backend for HTTP3. My fork of the dotnet runtime is referenced as a
submodule at `src/dotnet-runtime`.

The actual source code of managed QUIC implementation is located in
[src/dotnet-runtime/src/libraries/Common/src/System/Net/Http/aspnetcore/Quic/Implementations/Managed](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/Common/src/System/Net/Http/aspnetcore/Quic/Implementations/Managed).

The code for unit tests for managed QUIC implementation is located at:
[src/dotnet-runtime/src/libraries/System.Net.Http/tests/UnitTests](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/System.Net.Http/tests/UnitTests).

To make the implementation accessible also outside the dotnet runtime, I created a standalone .NET
Core 3.1 library project, which includes the above mentioned sources and provides a public wrapper
for them. Referncing this library allows using `QuicConnection` and other classes in regular .NET
Core application.

The solution with the wrapper project is located under `src/System.Net.Quic/`. This is also the the
solution I use for active development, as the compile times are shorter than building the
`System.Net.Http` assembly containing the internal QUIC sources.

## Building as part of dotnet runtime

TODO: The integration is not fully complete yet, build of dotnet runtime will crash. Use
System.Net.Quic.sln for now.
