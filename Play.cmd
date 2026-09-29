@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Build.ps1" -Run
if errorlevel 1 pause
