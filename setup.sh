#!/usr/bin/env bash

ROOT="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"

if [ ! -e "$ROOT/extern/akamai-openssl-quic/config" ]; then
    echo "Cannot find extern/akamai-openssl-quic, did you clone all submodules?"
    exit 1
fi

if [ ! -e "$ROOT/src/dotnet-runtime/build.sh" ]; then
    echo "Cannot find src/dotnet-runtime, did you clone all submodules?"
    exit 1
fi

OPENSSL_ARTIFACT_ROOT=$ROOT/artifacts/openssl

echo "Building custom OpenSSL"
mkdir -p "$OPENSSL_ARTIFACT_ROOT"

cd "$ROOT/extern/akamai-openssl-quic"
./config "--prefix=$OPENSSL_ARTIFACT_ROOT"
make install_sw

cd -

echo "Restoring NuGet packages for dotnet runtime"
cd "$ROOT/src/dotnet-runtime"
./build.sh --restore

cd -
