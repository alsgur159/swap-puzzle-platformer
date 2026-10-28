using UnityEditor;
using UnityEngine;

public class UISpriteImporter : AssetPostprocessor
{
    const string Dir = "Assets/Resources/UI/";

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Dir)) return;
        var imp = (TextureImporter)assetImporter;
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = 25;
        imp.filterMode = FilterMode.Point;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.mipmapEnabled = false;
        imp.wrapMode = TextureWrapMode.Clamp;
        imp.alphaIsTransparency = true;

        var settings = new TextureImporterSettings();
        imp.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteBorder = Border(System.IO.Path.GetFileNameWithoutExtension(assetPath));
        imp.SetTextureSettings(settings);
    }

    static Vector4 Border(string name)
    {
        switch (name)
        {
            case "panel": return new Vector4(3, 3, 3, 3);
            case "button": return new Vector4(4, 4, 4, 4);
            case "track":
            case "fill":
            case "pill": return new Vector4(2, 2, 2, 2);
            default: return Vector4.zero;
        }
    }
}
