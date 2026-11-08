using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SeedGame;

public static class BuildAlphaScene
{
    [MenuItem("Seed/Build first level alpha")]
    public static void Build()
    {
        const string root = "Assets/_Game";
        AssetDatabase.Refresh();
        PrepareSprite(root + "/Art/Placeholder/white.png", 1);
        PrepareSprite(root + "/Art/Character/主角形象草稿.PNG", 1000);
        for (int i = 1; i <= 3; i++)
        {
            PrepareSprite(root + "/Resources/Character/Walk" + i + ".png", 200);
            PrepareSprite(root + "/Resources/UI/CelticEnergy" + i + ".png", 200);
        }
        PrepareSprite(root + "/Resources/UI/Gear.png", 200);
        PrepareSprite(root + "/Resources/UI/ResonanceKey.png", 200);
        var square = AssetDatabase.LoadAssetAtPath<Sprite>(root + "/Art/Placeholder/white.png");
        var character = AssetDatabase.LoadAssetAtPath<Sprite>(root + "/Art/Character/主角形象草稿.PNG");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        AlphaSceneFactory.Build(square, character);
        Level01TransitionSetup.Install();
        var path = root + "/Scenes/Level01_Alpha.unity";
        Directory.CreateDirectory(root + "/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
        var secondPath = root + "/Scenes/Level02_Alpha.unity";
        EditorBuildSettings.scenes = File.Exists(secondPath)
            ? new [] { new EditorBuildSettingsScene(path, true), new EditorBuildSettingsScene(secondPath, true) }
            : new [] { new EditorBuildSettingsScene(path, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Seed scene built: " + path);
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
