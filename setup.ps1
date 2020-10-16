param(
    [switch]$msquic = $false
)

$dotnetRuntimeRoot = "$PSScriptRoot\src\dotnet-runtime"

$opensslRoot = "$PSScriptRoot\extern\openssl"
$opensslArtifactRoot = "$PSScriptRoot\artifacts\openssl"

$msquicRoot = "$PSScriptRoot\extern\msquic"
$msquicArtifactRoot = "$PSScriptRoot\artifacts\msquic"

if(!(Get-Command nmake -ErrorAction SilentlyContinue))
{
    echo "Cannot find nmake, are you running from developer shell instance?"
    exit 1;
}

if (! [System.IO.File]::Exists("$opensslRoot\Configure"))
{
    echo "Cannot find $opensslRoot\Configure, did you clone all submodules?";
    exit 1
}

if (! [System.IO.File]::Exists("$dotnetRuntimeRoot\build.cmd"))
{
    echo "Cannot find $dotnetRuntimeRoot\build.cmd, did you clone all submodules?";
    exit 1
}

if ($msquic)
{
    echo "Building msquic"
    $null = New-Item -ItemType Directory "$msquicArtifactRoot" -Force
    pushd $msquicRoot

    # msquic references many submodules during build
    git submodule update --init

    pwsh .\scripts\build.ps1 -Tls Schannel -Config Release

    # artifacts directory for msquic build script is not customizable, so we just manually copy the results
    # this command expects there to be a single build in the artifacts directory, otherwise the copy needs
    # to be done manually for the selected build

    cp .\artifacts\*\*\* $msquicArtifactRoot

    popd
}

echo "Checking build prerequisities";
foreach ($command in "perl", "nasm")
{
    if(!(Get-Command $command -ErrorAction SilentlyContinue))
    {
        echo "Cannot find $command, make sure it is in path"
        exit 1;
    }
}


echo "Building custom OpenSSL"
$null = New-Item -ItemType Directory "$opensslArtifactRoot" -Force
pushd $opensslRoot

echo "Checking build prerequisities";
foreach ($command in "perl", "nasm")
{
    if(!(Get-Command $command -ErrorAction SilentlyContinue))
    {
        echo "Cannot find $command, make sure it is in path"
        exit 1;
    }
}

perl "Configure" VC-WIN64A "--prefix=$opensslArtifactRoot"
nmake install_sw
popd

echo "Restoring dotnet-runtime NuGet packages"

pushd $dotnetRuntimeRoot

.\eng\build.ps1 -restore

popd
