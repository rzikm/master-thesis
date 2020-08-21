# Master Thesis - Implementation of QUIC protocol for .NET

This repository contains all necessary code changes needed for managed (C#) implementation of the
QUIC protocol for .NET.

## Supported QUIC Protocol Features

This implementation is currently in the prototype stage. The goal was to obtain a minimal working
implementation able to reliably transmit data between client and server. Most unrelated features are
left unimplemented.

- Basic connection establishment
- Encryption and TLS implementation backed by [modified OpenSSL](https://github.com/openssl/openssl/pull/8797)
- Sending data, flow control, loss recovery, congestion control
- Stream/Connection termination
- Coalescing packets into single UDP datagram

## Unsupported QUIC Protocol Features

List of highlights of unimplemented protocol features follows

- Connection migration
- 0-RTT data
- Server Preferred Address

Their implementation is subject for future development.

## Quickstart

Make sure you cloned all git submodules of this repository

    git submodule update --init

Then run the setup script:

    # on Linux
    ./setup.sh

    # on Windows
    setup.cmd

The scripts will:

- Build the native `QuicNative` library using the custom openssl branch of OpenSSL which provides
	necessary APIs for QUIC The native binaries should be placed in `artifacts/native` directory in
	the repository, they will be copied from there when .csproj projects are built. refer to
	OpenSSL's [INSTALL.md](https://github.com/openssl/openssl/blob/master/INSTALL.md) for build
	prerequisites.
- Restore nuget packages inside the dotnet runtime repository, these are needed to build unit tests
  , but not the actual `System.Net.Quic.dll` library.
- you can also pass `-msquic` option to the script to also build the `msquic` library which is used
	in benchmarks to compare the implementation performance. Building msquic requires Powershell
	Core (even on Linux OS). Refer to [msquic README](https://github.com/microsoft/msquic) for all
	prerequisites.

After running the setup script, you should be able to build and run the code using the
`src/System.Net.Quic/System.Net.Quic.sln` solution.

## Usage and API

For usage, see examples in the [Samples](https://github.com/rzikm/master-thesis/tree/master/src/System.Net.Quic/src/Samples) project.

For the list of API methods, see [Quic*.cs files inside the 
repo](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/Common/src/System/Net/Http/aspnetcore/Quic).

## Known Issues

The implementation relies on custom-built version of OpenSSL with added API necessary for quic. This
therefore requires distributing the modified OpenSSL alongside the implementation.

Currently, only `ApplicationProtocols` and `Hostname` from `SslServerAuthenticationOptions` and
`SslClientAuthenticationOptions` are supported. Validation and certificate selection callbacks are
completely unsupported.

Passing `X509Certificate` directly requires that the certificate instance contains private key and
is exportable (it needs to be marshalled to OpenSSL library used).

```csharp
X509Certificate2 cert = new X509Certificate2(cert.pfx, "", X509KeyStorageFlags.Exportable);
```

Preferred way of specifying the certificate to be used are `CertificateFilePath` and
`PrivateKeyFilePath` properties on `QuicListenerOptions`.

## Using the library in your projects

If you wish to use the QUIC library in your project, you have to compile it from source yourself.
There are currently no plans to publish `System.Net.Quic` as a NuGet package.

## Note on repository organisation

I wanted to keep the implementation inside the .NET runtime repository fork to allow future
mergeability, and because that way I can run the functional tests originally made for the MsQuic
based implementation. This repository contains only files which are specific to my master thesis and
do not belong to the .NET runtime repository.

The actual source code of managed QUIC implementation is located in
[src/dotnet-runtime/src/libraries/Common/src/System/Net/Http/aspnetcore/Quic/Implementations/Managed](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/Common/src/System/Net/Http/aspnetcore/Quic/Implementations/Managed).

The code for unit tests for managed QUIC implementation is located at:
[src/dotnet-runtime/src/libraries/System.Net.Http/tests/UnitTests](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/System.Net.Http/tests/UnitTests).

To make the implementation accessible also outside the dotnet runtime, I created a standalone .NET
library project, which includes the above mentioned sources and provides a public wrapper for them.
Referncing this library allows using `QuicConnection` and other classes in .NET applications without
the need for custom-built .NET runtime.

The solution with the wrapper project is located under `src/System.Net.Quic/`. This is also the
solution I use for active development, as the compile times are shorter than building the
`System.Net.Http` assembly containing the internal QUIC sources.

## Building as part of dotnet runtime

Make sure you cloned all submodules in /src/dotnet-runtime, and the guide in [Official .NET repository](https://github.com/rzikm/dotnet-runtime/tree/ac3506ca275af0bf284555d216602a9438c9a662/docs/workflow).

## Comparing with msquic

By default the build from this fork uses the managed QUIC implementation. You can enforce using QUIC
by defining the `USE_MSQUIC` environment variable. The msquic library must be available on the
machine. You can build it as part of the repository by passing `-msquic` to the setup script and
rebuilding the library from the System.Net.Quic.sln solution. Note that running msquic on Windows
currently requires insider build. See [msquic repository](https://github.com/microsoft/msquic).
