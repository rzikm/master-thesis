# Master Thesis - Implementation of QUIC protocol for .NET

This repository contains all necessary code changes needed for managed (C#)
implementation of the QUIC protocol for .NET.

## Quickstart

To setup development environment, run first the `setup.sh` or `setup.ps1` scripts.
Note that on windows, you need to run the script from developer powershell prompt, so that `nmake`
used to compile OpenSSL is in path. The scripts will:
- Build the custom openssl branch with QUIC support, leaving the binaries in artifacts/openssl, they
  will be copied from here during the library build.
- Restore nuget packages inside the dotnet runtime repository, these are needed to run unit tests
  properly

After that, you should be able to build and run the managed QUIC library using
`src/System.Net.Quic/System.Net.Quic.sln` solution.

TODO: For technical reasons, only x86 platform is supported on windows.

## Note on repository organisation

The actual source files are located deep inside the dotnet-runtime fork submodule. You can find them
in `src/dotnet-runtime/src/libraries/Common/System/Net/aspnetcore/Quic/Implementations/Managed`, and
the tests in `src/dotnet-runtime/src/libraries/System.Net.Http/tests/UnitTests/Quic/`.

This particular source organization allows building the QUIC as a standalone library (using the
System.Net.Quic.sln), while being versioned in a fork of dotnet runtime. In future the code will be
hopefully fully integrated into dotnet runtime.

## Building as part of dotnet runtime

TODO: The integration is not fully complete yet, build of dotnet runtime will crash. Use
System.Net.Quic.sln for now.
