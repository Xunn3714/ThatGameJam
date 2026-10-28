using UnityEngine;

namespace SeedGame
{
    public sealed class FirstLevelTutorial : MonoBehaviour
    {
        public SeedPlayer player;
        public ResonancePlant firstPlant;
        public float stumpX = 7.5f;
        public float firstPlantX = 12.5f;
        GUIStyle label;
        void OnGUI()
        {
            if (!player || !firstPlant || firstPlant.isActive) return;
            label ??= new GUIStyle(GUI.skin.box) { fontSize = 22, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            label.normal.textColor = Color.white;
            string message;
            float x = player.transform.position.x;
            if (!player.controlsEnabled) message = "醒来...";
            else if (x < stumpX - 1.5f) message = "A / D 或 ← / →   移动";
            else if (x < firstPlantX) message = "SPACE   跳过树桩";
            else message = "E   呼唤卷起的植物";
            GUI.Box(new Rect(Screen.width / 2f - 250, 22, 500, 54), message, label);
        }
    }
}
