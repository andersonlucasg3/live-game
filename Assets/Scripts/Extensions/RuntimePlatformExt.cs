using UnityEngine;

public static class RuntimePlatformExt {
    public static bool IsWindowsPlatform(this RuntimePlatform platform) {
        return platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsPlayer;
    }
}