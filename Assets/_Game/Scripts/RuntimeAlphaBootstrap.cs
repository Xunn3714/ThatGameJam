using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class RuntimeAlphaBootstrap : MonoBehaviour
{
    void Start()
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root == gameObject) continue;
            root.SetActive(false);
            Destroy(root);
        }
        var whiteTexture = Resources.Load<Texture2D>("Alpha/white");
        var heroTexture = Resources.Load<Texture2D>("Alpha/character");
        if (!whiteTexture || !heroTexture)
        {
            Debug.LogError("Alpha art resources are missing.");
            return;
        }
        var white = Sprite.Create(whiteTexture, new Rect(0,0,whiteTexture.width,whiteTexture.height),
            new Vector2(.5f,.5f), 1);
        var hero = Sprite.Create(heroTexture, new Rect(0,0,heroTexture.width,heroTexture.height),
            new Vector2(.5f,.5f), 1000);
        AlphaSceneFactory.Build(white, hero);
    }
}
