# Master Thesis - Implementation of QUIC protocol for .NET

This repository contains all necessary code needed for managed (C#) implementation of the QUIC
protocol for .NET.

## Blog posts

I have documented some of my development progress on [my blog site](https://rzikm.github.io).

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

## OpenSSL integration

To function correctly, the implementation requires a custom branch of OpenSSL from Akamai
https://github.com/akamai/openssl/tree/OpenSSL_1_1_1g-quic. The implementation tries to decect that
a QUIC-supporting OpenSSL is present in the path. If not a mock implementation is used instead.

The mock implementation can be used for trying the library locally, but interoperation with other
QUIC implementations is possible only with the OpenSSL-based TLS. If you want to make sure your
implementation runs with OpenSSL, define the `DOTNETQUIC_OPENSSL` environment variable. This will
cause the implementation to throw if OpenSSL cannot be loaded.

## Setup and Build

Make sure you cloned all git submodules of this repository

    git submodule update --init

### Building the Library

You should be able to build and run the code using the `src/System.Net.Quic/System.Net.Quic.sln`
solution. This should produce a standalone `System.Net.Quic.dll` you can reference form a normal
.NET project. Note that compiling the source requires preview version of .NET 5 SDK.

Alternatively, you can build entire dotnet runtime from source from the `src/dotnet-runtime`
subdirectory.

### Building as part of dotnet runtime

Follow the guide in [Official .NET
repository](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/docs/workflow). Note
that the code running against this build of runtime still requires custom build of OpenSSL in the
path.

### Building the Unit tests

You can build and run unit tests using the `src/System.Net.Quic/System.Net.Quic.sln` solution, but
you first need to perform nuget restore in the `src/dotnet-runtime` subdirectory. Use commands:

    # on Linux
    ./build.sh -restore

    # on Windows
    setup.cmd -restore

### Building custom OpenSSL

The necessary fork of OpenSSL is included as a submodule in `extern/openssl` directory. Follow the
readme instructions in OpenSSL's
[INSTALL.md](https://github.com/openssl/openssl/blob/master/INSTALL.md)

### Quick Setup Script

Alternatively, the necessary steps above can be performed via a helper scripts. Make sure all
submodules are cloned

    git submodule update --init --recursive

Then run the setup script:

    # on Linux
    ./setup.sh

    # on Windows
    setup.cmd

The scripts will:

- Build the modified `openssl` library using the custom branch of OpenSSL which provides necessary
	APIs for QUIC The native binaries should be placed in `artifacts/openssl` directory in the
	repository. You then need to add the directory to the path so that the libraries are loaded by
	.NET at runtime. refer to OpenSSL's
	[INSTALL.md](https://github.com/openssl/openssl/blob/master/INSTALL.md) for build prerequisites.
- Restore nuget packages inside the dotnet runtime repository, these are needed to build unit tests,
  but not the actual `System.Net.Quic.dll` library.
- you can also pass `-msquic` option to the script to also build the `msquic` library which is used
	in benchmarks to compare the implementation performance. Building msquic requires Powershell
	Core (even on Linux OS). Refer to [msquic README](https://github.com/microsoft/msquic) for all
	prerequisites.

## Usage and API

For usage, see examples in the [Samples](https://github.com/rzikm/master-thesis/tree/master/src/System.Net.Quic/src/Samples) project.

For the list of API methods, see [Quic*.cs files inside the 
repo](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/System.Net.Quic/src/System/Net/Quic).

## Tracing and qvis

The implementation can produce traces that can be consumed by https://qvis.edm.uhasselt.be
visualizer. To collect traces, define `DOTNETQUIC_TRACE` environment variable. The traces will be
saved in the working directory of the program.

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

The actual source code of managed QUIC implementation is located in the dotnet runtime fork in
[src/dotnet-runtime/src/libraries/System.Net.Quic/src/System/Net/Quic/Implementations/Managed](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/System.Net.Quic/src/System/Net/Quic/Implementations/Managed).

The code for unit tests for managed QUIC implementation is located at:
[src/dotnet-runtime/src/libraries/System.Net.Quic/src/System/Net/Quic/Implementations/Managed](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/System.Net.Quic/src/System/Net/Quic/Implementations/Managed).
[src/dotnet-runtime/src/libraries/System.Net.Quic/tests/UnitTests](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/System.Net.Quic/tests/UnitTests).

To make the implementation accessible also without having to build the entire dotnet runtime, I
created a standalone .NET library project, which includes the above mentioned sources. Referncing
this library allows using `QuicConnection` and other classes without the need to use entire
custom-built .NET runtime.

The solution with the wrapper project is located under `src/System.Net.Quic/`. This is also the
solution I use for active development, as the compile times are shorter than building the
`System.Net.Http` assembly containing the internal QUIC sources.

## Comparing with msquic

By default the build from this fork uses the managed QUIC implementation. You can enforce using QUIC
by defining the `USE_MSQUIC` environment variable. The msquic library must be available on the
machine. You can build it as part of the repository by passing `-msquic` to the setup script and
rebuilding the library from the System.Net.Quic.sln solution. Note that running msquic on Windows
currently requires insider build. See [msquic repository](https://github.com/microsoft/msquic).

Preliminary performance comparisons can be found on [my blog](https://rzikm.github.io/quic-in-net-comparison-with-msquic/).
