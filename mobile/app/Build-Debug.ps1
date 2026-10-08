$ErrorActionPreference = "Stop"
$wrapper = Join-Path $PSScriptRoot 'gradlew.bat'
if (!(Test-Path $wrapper)) {
    $version = '9.4.1'
    $tools = Join-Path $env:LOCALAPPDATA 'FixFlowBuildTools'
    $gradle = Join-Path $tools "gradle-$version\bin\gradle.bat"
    if (!(Test-Path $gradle)) {
        New-Item -ItemType Directory -Path $tools -Force | Out-Null
        $zip = Join-Path $tools "gradle-$version-bin.zip"
        $checksum = (Invoke-RestMethod "https://services.gradle.org/distributions/gradle-$version-bin.zip.sha256").Trim()
        Invoke-WebRequest "https://services.gradle.org/distributions/gradle-$version-bin.zip" -OutFile $zip
        if ((Get-FileHash $zip -Algorithm SHA256).Hash.ToLowerInvariant() -ne $checksum.ToLowerInvariant()) { throw 'Gradle checksum failed.' }
        Expand-Archive $zip -DestinationPath $tools -Force
    }
    $bootstrap = Join-Path $env:TEMP ('fixflow-wrapper-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $bootstrap | Out-Null
    try {
        [IO.File]::WriteAllText((Join-Path $bootstrap 'settings.gradle.kts'),'rootProject.name = "wrapper-bootstrap"')
        & $gradle -p $bootstrap wrapper --gradle-version $version --distribution-type bin
        if ($LASTEXITCODE -ne 0) { throw 'Gradle wrapper creation failed. Check your Java configuration.' }
        Copy-Item (Join-Path $bootstrap 'gradlew.bat') $wrapper
        Copy-Item (Join-Path $bootstrap 'gradlew') (Join-Path $PSScriptRoot 'gradlew')
        $wrapperFolder = Join-Path $PSScriptRoot 'gradle\wrapper'
        New-Item -ItemType Directory -Path $wrapperFolder -Force | Out-Null
        Copy-Item (Join-Path $bootstrap 'gradle\wrapper\*') $wrapperFolder -Force
    } finally { Remove-Item -LiteralPath $bootstrap -Recurse -Force }
}
Push-Location $PSScriptRoot
try {
    & $wrapper testDebugUnitTest assembleDebug
    if ($LASTEXITCODE -ne 0) { throw 'Android build failed. Check the error above.' }
    Write-Host "APK: $PSScriptRoot\app\build\outputs\apk\debug\app-debug.apk" -ForegroundColor Green
} finally { Pop-Location }