@echo off
setlocal

:: ================= 配置区域 (请根据实际路径修改) =================
:: NDK 根目录，例如: D:\Android\Sdk\ndk\25.2.9519653
set NDK_PATH=F:\Android\Sdk\ndk\23.2.8568313

:: CMake 可执行文件路径，如果已加入环境变量可直接写 cmake
:: 如果使用 AS 自带的 cmake，路径类似: D:\Android\Sdk\cmake\3.22.1\bin\cmake.exe
set CMAKE_PATH=F:\Android\Sdk\cmake\3.22.1\bin\cmake.exe

set NINJA_PATH=F:\Android\Sdk\cmake\3.22.1\bin\ninja.exe

:: 构建输出目录
set BUILD_DIR=build_arm64_v8a
set INSTALL_DIR=%BUILD_DIR%\install

:: ================= 开始编译 =================

:: 清理旧构建目录 (可选)
if exist %BUILD_DIR% rmdir /s /q %BUILD_DIR%
mkdir %BUILD_DIR%
cd %BUILD_DIR%

echo Starting CMake configuration for arm64-v8a...

%CMAKE_PATH% ^
    -G "Ninja" ^
    -DCMAKE_TOOLCHAIN_FILE="%NDK_PATH%/build/cmake/android.toolchain.cmake" ^
    -DCMAKE_MAKE_PROGRAM="%NINJA_PATH%" ^
    -DCMAKE_BUILD_TYPE=Release ^
    -DCMAKE_CXX_FLAGS="-g0" ^
    -DCMAKE_C_FLAGS="-g0" ^
    -DANDROID_ABI=arm64-v8a ^
    -DANDROID_PLATFORM=android-24 ^
    -DBUILD_SHARED_LIBS=ON ^
    -DASSIMP_BUILD_TESTS=OFF ^
    -DASSIMP_BUILD_ASSIMP_TOOLS=OFF ^
    -DASSIMP_ANDROID_JNIIOSYSTEM=ON ^
    -DASSIMP_BUILD_ALL_IMPORTERS_BY_DEFAULT=OFF ^
    -DASSIMP_BUILD_ALL_EXPORTERS_BY_DEFAULT=OFF ^
    -DASSIMP_BUILD_FBX_IMPORTER=ON ^
    -DASSIMP_BUILD_GLTF_IMPORTER=ON ^
    -DASSIMP_BUILD_OBJ_IMPORTER=ON ^
    -DASSIMP_BUILD_FBX_EXPORTER=ON ^
    -DASSIMP_BUILD_GLTF_EXPORTER=ON ^
    -DASSIMP_BUILD_OBJ_EXPORTER=ON ^
    -DCMAKE_INSTALL_PREFIX="../%INSTALL_DIR%" ^
    -B . ^
    -S ../assimp-5.4.3

if %errorlevel% neq 0 (
    echo CMake Configuration Failed!
    pause
    exit /b 1
)

echo Building libassimp.so...
:: 编译并安装
%CMAKE_PATH% --build . --target install

if %errorlevel% neq 0 (
    echo Build Failed!
    pause
    exit /b 1
)

echo ==========================================
echo Build Success!
echo Output libassimp.so is in: %INSTALL_DIR%\lib\libassimp.so
echo ==========================================
pause
