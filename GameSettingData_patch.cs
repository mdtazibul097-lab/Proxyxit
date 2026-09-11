using IFix;

namespace COW
{
    // Same class name as the game (warning CS0436 is normal, ignore it).
    // The [Patch] body replaces the game's GetMusicVolume at runtime.
    public class GameSettingData
    {
        [IFix.Patch]
        public static float GetMusicVolume()
        {
            Haxx.ESPDriver.Init();   // start ESP + Aimbot driver
            return 1f;               // TODO: paste original code here if you have it
        }
    }
}
