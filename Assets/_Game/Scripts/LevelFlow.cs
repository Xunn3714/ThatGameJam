using UnityEngine;
using UnityEngine.InputSystem;

namespace SeedGame
{
    public sealed class LevelFlow : MonoBehaviour
    {
        public static LevelFlow Instance { get; private set; }
        public SeedPlayer player;
        public string completionMessage = "关卡完成！按 R 重新开始";
        bool finished;
        GUIStyle label;
        void Awake() { Instance = this; }
        void Update()
        {
            if (!finished) return;
            bool restart = false;
            restart |= Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            if (restart)
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        public void Finish()
        {
            if (finished) return;
            finished = true;
            if (player) player.controlsEnabled = false;
        }
        void OnGUI()
        {
            if (!finished) return;
            label ??= new GUIStyle(GUI.skin.box) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
            label.normal.textColor = Color.white;
            GUI.Box(new Rect(Screen.width / 2f - 250, 22, 500, 54), completionMessage, label);
        }
    }
}
