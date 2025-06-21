using Il2CppInterop.Runtime.InteropTypes.Arrays;

using UnityEngine;

namespace BaseMod.Core.Utils;
public static class ResourcesUtils {
    public static Sprite? LoadSprite(string folder, string filename) {
        string path = Path.Combine(folder, filename);
        byte[] data = File.ReadAllBytes(path);
        Il2CppStructArray<byte> il2cppData = new(data);
        Texture2D tex = new(2, 2, TextureFormat.RGBA32, false);
        if (tex.LoadImage(il2cppData))
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        return null;
    }
}