@echo off
setlocal EnableDelayedExpansion

set "OUT=result.txt"
if exist "%OUT%" del "%OUT%"

echo 开始递归查找并读取 .cs 文件...
echo.>"%OUT%"

for /r %%f in (*.cs) do (
    echo ===== %%f =====>>"%OUT%"
    
    rem 使用PowerShell读取UTF-8文件，避免乱码
    powershell -Command "Get-Content '%%f' -Encoding UTF8 | Out-File '%OUT%' -Append -Encoding UTF8"
    
    echo.>>"%OUT%"
)

echo 完成！输出文件：%OUT%
pause