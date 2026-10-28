using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildAlphaScene
{
    [MenuItem("Seed/Build first level alpha")]
    public static void Build()
    {
        const string root = "Assets/_Game";
        PrepareSprite(root + "/Art/Placeholder/white.png", 1);
        PrepareSprite(root + "/Art/Character/主角形象草稿.PNG", 1000);
        var square = AssetDatabase.LoadAssetAtPath<Sprite>(root + "/Art/Placeholder/white.png");
        var character = AssetDatabase.LoadAssetAtPath<Sprite>(root + "/Art/Character/主角形象草稿.PNG");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AlphaSceneFactory.Build(square, character);
        var path = root + "/Scenes/Level01_Alpha.unity";
        Directory.CreateDirectory(root + "/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        EditorBuildSettings.scenes = new [] { new EditorBuildSettingsScene(path, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Seed alpha scene built: " + path);
    }
    static void PrepareSprite(string path, float pixelsPerUnit)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (!importer) throw new System.Exception("Missing texture: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        if (pixelsPerUnit > 1) importer.maxTextureSize = 1024;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }
}
