@echo off
setlocal

set "PROJECT_DIR=%~dp0"
for /f "tokens=2" %%i in ('findstr "m_EditorVersion:" "%PROJECT_DIR%ProjectSettings\ProjectVersion.txt"') do set UNITY_VERSION=%%i

set "UNITY_PATH=%PROGRAMFILES%\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe"

if not exist "%UNITY_PATH%" (
    echo Unity %UNITY_VERSION% nao encontrado em %UNITY_PATH%
    pause
    exit /b 1
)

echo Iniciando Unity %UNITY_VERSION%...
"%UNITY_PATH%" -projectPath "%PROJECT_DIR%"
