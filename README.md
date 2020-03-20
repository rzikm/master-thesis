# Master Thesis - Implementation of QUIC protocol for .NET

This repository contains all necessary code changes needed for managed (C#)
implementation of the QUIC protocol for .NET.

## Building

First, build OpenSSL version with added necessary interface for QUIC. See
extern/akamai-openssl-quic/INSTALL for building instructions.

TODO: built libcrypto.so and libssl.so need to be renamed to libcrypto-quic.so and libssl-quic.so and included in the built sources of System.Net.Http/System.Net.Quic

For building the dotnet runtime, see src/dotnet-runtime/docs/workflow/README.md. 

You can also build the managed QUIC implementation as System.Net.Quic.dll using
the solution in src/System.Net.Quic/System.Net.Quic.sln.

TODO: provide build scripts that do the above steps
