#!/bin/bash
# ============================================================
#  ILFix ESP+Aimbot auto-builder  (GitHub Codespaces / Linux)
#  ফোনে করার জন্য: Codespaces browser-এ এসব ফাইল আপলোড করে
#  terminal-এ লিখুন:  bash build.sh
# ============================================================
set -e
cd "$(dirname "$0")"

echo "==> [1/5] mono install হচ্ছে..."
sudo apt-get update -qq
sudo apt-get install -y -qq mono-complete unzip wget

echo "==> [2/5] IFixToolKit check..."
if [ ! -f IFixToolKit/IFix.exe ]; then
    echo "IFix.exe পাওয়া যায়নি। ফোন থেকে এই release ডাউনলোড করে IFixToolKit.zip আপলোড করো:"
    echo "    https://github.com/Tencent/InjectFix/releases"
    echo "তারপর এখানে unzip:  unzip IFixToolKit.zip"
    exit 1
fi

echo "==> [3/5] game DLL check..."
for d in Assembly-CSharp.dll UnityEngine.CoreModule.dll UnityEngine.PhysicsModule.dll UnityEngine.TextRenderingModule.dll UnityEngine.IMGUIModule.dll; do
    if [ ! -f "dlls/$d" ]; then
        echo "dlls/$d MISSING! MT Manager দিয়ে APK থেকে DLL বের করে dlls/ ফোল্ডারে আপলোড করো।"
        exit 1
    fi
done

echo "==> [4/5] compile হচ্ছে..."
mcs -sdk:2 -target:library -out:patched.dll \
    -nowarn:0436,0433,0169,0649 \
    -r:dlls/Assembly-CSharp.dll \
    -r:dlls/UnityEngine.CoreModule.dll \
    -r:dlls/UnityEngine.PhysicsModule.dll \
    -r:dlls/UnityEngine.TextRenderingModule.dll \
    -r:dlls/UnityEngine.IMGUIModule.dll \
    ESP_Aimbot_IFixPatch.cs GameSettingData_patch.cs

echo "==> [5/5] patch bytes বানানো হচ্ছে..."
mono IFixToolKit/IFix.exe -patch \
    IFixToolKit/IFix.Core.dll \
    patched.dll \
    null \
    process_cfg \
    Assembly-CSharp-patch.bytes \
    dlls

if [ -f Assembly-CSharp-patch.bytes ]; then
    echo ""
    echo "============================================"
    echo " SUCCESS!  Assembly-CSharp-patch.bytes ready"
    echo " ফাইলে ডান ক্লিক করে Download করো,"
    echo " তারপর ফোনে: /storage/emulated/0/Android/data/com.dts.freefireth/files/"
    echo "============================================"
else
    echo "Error: patch bytes তৈরি হয়নি। উপরের লেখা আমাকে পাঠাও।"
    exit 1
fi
