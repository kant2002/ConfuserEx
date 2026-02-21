param(
    [string]$Configuration = "Release",
    [string]$SolutionPath = "Confuser2.slnx"
)

$ErrorActionPreference = 'Stop'
msbuild $SolutionPath /t:Restore,Build /p:Configuration=$Configuration /p:Platform=x64 /v:m
#vstest.console Tests/*/bin/Release/net*/*.dll /TestAdapterPath:Tests/*/bin/Release/net* /Logger:trx