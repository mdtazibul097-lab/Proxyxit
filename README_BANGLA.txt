============================================================
  ESP BOX + AIMBOT — ILFix Patch Build Package
  (PC ছাড়া, শুধু ফোন + GitHub Codespaces)
============================================================

তোমার যা যা করতে হবে (একবারই):

STEP 1 — GitHub Codespace বানাও (ফোন browser-এ)
  1. github.com-এ login করো
  2. github.com/codespaces → "New codespace" → blank repository

STEP 2 — ফাইল আপলোড করো (Codespaces-এ drag & drop)
  এই package-এর সব ফাইল:
    ESP_Aimbot_IFixPatch.cs
    GameSettingData_patch.cs
    process_cfg
    build.sh

  আর game-এর DLL গুলো (MT Manager দিয়ে APK থেকে বের করো):
    dlls/ ফোল্ডার বানিয়ে তার ভেতর:
    Assembly-CSharp.dll
    UnityEngine.CoreModule.dll
    UnityEngine.PhysicsModule.dll
    UnityEngine.TextRenderingModule.dll
    UnityEngine.IMGUIModule.dll

  আর IFixToolKit:
    https://github.com/Tencent/InjectFix/releases থেকে
    IFixToolKit.zip ডাউনলোড করে আপলোড করে unzip করো
    (Codespaces terminal-এ: unzip IFixToolKit.zip)

STEP 3 — Build চালাও
  Codespaces terminal-এ লিখো:
    bash build.sh

  সফল হলে Assembly-CSharp-patch.bytes তৈরি হবে।

STEP 4 — Download + Phone
  Assembly-CSharp-patch.bytes-এ ডান ক্লিক → Download
  ফোনে রাখো:
    /storage/emulated/0/Android/data/com.dts.freefireth/files/
  Game চালাও → "MOD MENU" বাটন → ESP/Aimbot ON

============================================================
Error আসলে:
  - terminal-এর লেখা (screen shot বা copy) আমাকে পাঠাও
  - compile error হলে বলবে কোন line-এ
============================================================
