#!/usr/bin/env bash
ROOT="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"

MSQUIC_ARTIFACT_ROOT=$ROOT/artifacts/msquic
OPENSSL_ARTIFACT_ROOT=$ROOT/artifacts/openssl

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

echo "Building custom OpenSSL"
mkdir -p "$OPENSSL_ARTIFACT_ROOT"

cd "$ROOT/extern/openssl"
./config "--prefix=$OPENSSL_ARTIFACT_ROOT"
make install_sw

cd -


echo "Restoring NuGet packages for dotnet runtime"
pushd "$ROOT/src/dotnet-runtime"
./build.sh --restore
popd
