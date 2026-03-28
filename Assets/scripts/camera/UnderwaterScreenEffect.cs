using UnityEngine;
using UnityEngine.UI;

public class UnderwaterScreenEffect : MonoBehaviour
{
    [Header("UI 引用")]
    public Image waterMaskImage;    // 拖入你刚才创建的 UI Image

    [Header("玩家引用")]
    public Player1 player;          // 拖入你的玩家，用于获取当前水体状态

    [Header("遮罩设置")]
    public Color waterColor = new Color(0f, 0.4f, 0.7f, 0.4f); 
    public float fadeSpeed = 5f;   

    private Camera mainCamera;
    private Color currentColor;

    void Start()
    {
        mainCamera = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player1>();
        currentColor = waterColor;
        currentColor.a = 0f;
        if (waterMaskImage != null)
        {
            waterMaskImage.color = currentColor;
        }
    }

    void Update()
    {
        if (waterMaskImage == null || player == null || mainCamera == null) return;
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player1>();
        }
        float targetAlpha = 0f;

        if (player.isInWater && player.currentWater != null)
        {
            float cameraY = mainCamera.transform.position.y;
            float waterSurfaceY = player.currentWater.GetWaterSurfaceY(mainCamera.transform.position.x);

            if (cameraY < waterSurfaceY)
            {
                targetAlpha = waterColor.a;
            }
        }

        // 使用 Lerp (线性插值) 让透明度平滑过渡，不会显得很生硬
        currentColor.a = Mathf.Lerp(currentColor.a, targetAlpha, Time.deltaTime * fadeSpeed);

        // 实时更新图片的颜色
        waterMaskImage.color = currentColor;
    }
}