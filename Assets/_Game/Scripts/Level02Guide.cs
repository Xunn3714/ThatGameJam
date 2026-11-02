using UnityEngine;

namespace SeedGame
{
    public sealed class Level02Guide : MonoBehaviour
    {
        public SeedPlayer player;
        GUIStyle style;

        void OnGUI()
        {
            if (!player || !player.controlsEnabled) return;
            float x = player.transform.position.x;
            string message;
            if (x < 18f) message = "浅水暗廊：E 点亮蓝藻，或直接踩石块通过";
            else if (x < 31f) message = "双柱：靠近葎草按 E，让叶台长出";
            else if (x < 38f && GearState.Count < 3) message = "高处有第 3 枚齿轮";
            else if (x < 43f && player.transform.position.y < 2f)
                message = "下路：靠近蘑菇按 E，踩上菌伞弹到出口高台";
            else if (x < 43f) message = "高路：E 唤醒酢浆草，空中按住 SPACE 滑翔";
            else if (x < 47f && GearState.Count < 3) message = "蘑菇可弹上高台；出口还需要第 3 枚齿轮";
            else if (x < 47f) message = "E 唤醒蘑菇；在门前连续按 E 三次覆盖四株植物";
            else message = "四株植物同时回应后石门开启";
            style ??= new GUIStyle(GUI.skin.box) { fontSize = 20, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            style.normal.textColor = Color.white;
            GUI.Box(new Rect(Screen.width / 2f - 310, 20, 620, 55), message, style);
        }
    }
}
