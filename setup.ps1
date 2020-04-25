$opensslRoot = "$PSScriptRoot\extern\akamai-openssl-quic"
$dotnetRuntimeRoot = "$PSScriptRoot\src\dotnet-runtime"

if(!(Get-Command nmake -ErrorAction SilentlyContinue))
{
    echo "Cannot find nmake, are you running from developer shell instance?"
    exit 1;
}

$opensslArtifactRoot = "$PSScriptRoot\artifacts\openssl"

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

.\build.cmd -restore

popd
