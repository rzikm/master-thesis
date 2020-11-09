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
https://github.com/akamai/openssl/tree/OpenSSL_1_1_1g-quic. This version of the library is expected
to be in the PATH to be loaded by the application.

The need for custom OpenSSL can be avoided by switching to a mock TLS implementation, see
Configuration section later.

The mock implementation can be used for trying the library locally, but interoperation with other
QUIC implementations is possible only with the OpenSSL-based TLS. If you want to make sure your
implementation runs with OpenSSL, define the `DOTNETQUIC_OPENSSL` environment variable. This will
cause the implementation to throw if OpenSSL cannot be loaded.

## Setup and Build

Make sure you cloned all git submodules of this repository

    git submodule update --init

### Building the .NET runtime

Follow the guide in [Official .NET
repository](https://github.com/rzikm/runtimelab/tree/master-managed-quic/docs/workflow). 

### Building custom OpenSSL

The necessary fork of OpenSSL is included as a submodule in `extern/openssl` directory. Follow the
readme instructions in OpenSSL's
[INSTALL.md](https://github.com/openssl/openssl/blob/master/INSTALL.md)

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
- you can also pass `-msquic` option to the script to also build the `MsQuic` library which is used
	in benchmarks to compare the implementation performance. Building msquic requires Powershell
	Core (even on Linux OS). Refer to [msquic README](https://github.com/microsoft/msquic) for all
	prerequisites.

## Usage and API

For usage, see examples in the [Samples](https://github.com/rzikm/master-thesis/tree/master/src/System.Net.Quic/src/Samples) project.

For the list of API methods, see [Quic*.cs files inside the 
repo](https://github.com/rzikm/dotnet-runtime/tree/master-managed-quic/src/libraries/System.Net.Quic/src/System/Net/Quic).

### Tracing and qvis

The implementation can produce traces that can be consumed by https://qvis.edm.uhasselt.be
visualizer. To collect traces, define `DOTNETQUIC_TRACE` environment variable. The traces will be
saved in the working directory of the program.

### Switching the implementation

The internal implementation of `QuicConnection` and related types allows switching the underlying
implementation provider. Currently, there are 3 providers:

- `managed` - (default), managed implementation with TLS backed by modified OpenSSL.
- `managedmocktls` - managed implementation with mocked TLS. This works without additional
  dependencies, but does not interop with other implementations.
- `msquic` - uses the `MsQuic` library. Needs msquic.dll to be in path.

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
