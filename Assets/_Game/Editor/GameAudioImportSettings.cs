using UnityEditor;
using UnityEngine;

namespace SeedGame.Editor
{
    // Decode plant audio for short cuts; stream the long music track.
    public sealed class GameAudioImportSettings : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            bool plant = assetPath == "Assets/_Game/Resources/Audio/PlantsMovement.mp3";
            bool music = assetPath == "Assets/_Game/Resources/Audio/GameMusic.mp3";
            if (!plant && !music) return;
            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings;
            settings.loadType = plant ? AudioClipLoadType.DecompressOnLoad : AudioClipLoadType.Streaming;
            importer.defaultSampleSettings = settings;
        }
    }
}
