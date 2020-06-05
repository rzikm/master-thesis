#!/usr/bin/env bash
ROOT="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"

MSQUIC_ARTIFACT_ROOT=$ROOT/artifacts/msquic
NATIVE_ARTIFACT_ROOT=$ROOT/artifacts/native
QUIC_NATIVE_SOURCE=$ROOT/src/dotnet-runtime/src/libraries/Native/AnyOS/QuicNative

# parse args
while [[ $# > 0 ]]; do
  opt="$(echo "${1/#--/-}" | awk '{print tolower($0)}')"

  if [[ $firstArgumentChecked -eq 0 && $opt =~ ^[a-zA-Z.+]+$ ]]; then
    shift 1
    continue
  fi

  firstArgumentChecked=1

  case "$opt" in
     -msquic)
       msquic=1
       shift 1
      ;;
  esac
done

if [ ! -e "$ROOT/extern/akamai-openssl-quic/config" ]; then
    echo "Cannot find extern/akamai-openssl-quic, did you clone all submodules?"
    exit 1
fi

if [ ! -e "$ROOT/src/dotnet-runtime/build.sh" ]; then
    echo "Cannot find src/dotnet-runtime, did you clone all submodules?"
    exit 1
fi

if [ ! -z "$msquic" ]; then
  echo "building msquic"

  mkdir -p "$MSQUIC_ARTIFACT_ROOT"

  cd "$ROOT/extern/msquic"
  git submodule update --init

  pwsh scripts/build.ps1 -Tls openssl -Config Release

  # the script above is expected to create a single release in the /extern/msquic/artfiacts
  # directory, so we just copy the contents to our artifacts dir
  cp `find artifacts -type f` "$MSQUIC_ARTIFACT_ROOT"

  cd -
fi

echo "Restoring NuGet packages for dotnet runtime"
pushd "$ROOT/src/dotnet-runtime"
./build.sh --restore
popd

echo "Building QuicNative"
QUIC_NATIVE_BUILD_DIR="$ROOT/obj/QuicNative/"
mkdir -p "$QUIC_NATIVE_BUILD_DIR"
pushd "$QUIC_NATIVE_BUILD_DIR"

cmake "$QUIC_NATIVE_SOURCE" "-DCMAKE_INSTALL_PREFIX=$NATIVE_ARTIFACT_ROOT/linux86_64"
cmake --build . --parallel 3 --config Release --target install

popd
