using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 实现更高效的径向渐变效果，只更新必要的像素
public class RadialGradientEffect3 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public Image image; // UI中的Image组件
    public Color gradientColor = new Color(1, 1, 1, 0.6f); // 渐变颜色
    public float radius = 0.5f; // 渐变半径
    public int textureSize = 256; // 纹理大小
    public float updateThreshold = 0.01f; // 更新阈值

    private Texture2D gradientTexture; // 渐变纹理
    private RectTransform rectTransform; // RectTransform组件
    private Vector2 lastMousePosition; // 上次鼠标位置
    private bool isPointerOver = false; // 指针是否悬停在图像上

    void Start()
    {
        // 初始化image组件
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        // 获取RectTransform组件
        rectTransform = image.GetComponent<RectTransform>();

        // 创建渐变纹理
        gradientTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        image.sprite = Sprite.Create(gradientTexture, new Rect(0, 0, gradientTexture.width, gradientTexture.height), new Vector2(0.5f, 0.5f));

        //UpdateGradient(new Vector2(0.5f, 0.5f)); // 用中心初始化
    }

    void Update()
    {
        if (isPointerOver)
        {
            // 将鼠标位置转换为局部坐标
            Vector2 localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
            // 归一化鼠标位置
            Vector2 normMousePosition = new Vector2(
                Mathf.InverseLerp(-rectTransform.rect.width / 2, rectTransform.rect.width / 2, localMousePosition.x),
                Mathf.InverseLerp(-rectTransform.rect.height / 2, rectTransform.rect.height / 2, localMousePosition.y)
            );

            // 判断鼠标位置是否变化超过阈值
            if (Vector2.Distance(normMousePosition, lastMousePosition) > updateThreshold)
            {
                // 记录上次的鼠标位置
                Vector2 prevMousePosition = lastMousePosition;
                lastMousePosition = normMousePosition;

                // 更新梯度，只更新必要的像素
                UpdateGradientPartially(prevMousePosition, normMousePosition);
            }
        }
    }

    void UpdateGradientPartially(Vector2 prevCenter, Vector2 newCenter)
    {
        // 计算每个圆心之间的像素范围变化
        int prevCenterX = Mathf.RoundToInt(prevCenter.x * (textureSize - 1));
        int prevCenterY = Mathf.RoundToInt(prevCenter.y * (textureSize - 1));
        int newCenterX = Mathf.RoundToInt(newCenter.x * (textureSize - 1));
        int newCenterY = Mathf.RoundToInt(newCenter.y * (textureSize - 1));

        int minX = Mathf.Min(prevCenterX, newCenterX) - Mathf.CeilToInt(radius * textureSize) - 1;
        int maxX = Mathf.Max(prevCenterX, newCenterX) + Mathf.CeilToInt(radius * textureSize) + 1;
        int minY = Mathf.Min(prevCenterY, newCenterY) - Mathf.CeilToInt(radius * textureSize) - 1;
        int maxY = Mathf.Max(prevCenterY, newCenterY) + Mathf.CeilToInt(radius * textureSize) + 1;

        minX = Mathf.Clamp(minX, 0, textureSize - 1);
        maxX = Mathf.Clamp(maxX, 0, textureSize - 1);
        minY = Mathf.Clamp(minY, 0, textureSize - 1);
        maxY = Mathf.Clamp(maxY, 0, textureSize - 1);

        // 更新必要像素
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float u = (float)x / (textureSize - 1);
                float v = (float)y / (textureSize - 1);
                Vector2 uv = new Vector2(u, v);

                float prevDist = Vector2.Distance(uv, prevCenter);
                float newDist = Vector2.Distance(uv, newCenter);

                if (prevDist > radius + 0.1f || newDist <= radius + 0.1f)
                {
                    Color color = Color.Lerp(Color.white, gradientColor, Mathf.SmoothStep(radius, radius + 0.1f, newDist));
                    gradientTexture.SetPixel(x, y, color);
                }
            }
        }

        gradientTexture.Apply();
    }

    // 当指针进入图像时
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        lastMousePosition = Vector2.positiveInfinity; // 强制更新第一次进入时的渐变
    }

    // 当指针离开图像时
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        //UpdateGradient(new Vector2(0.5f, 0.5f)); // 当指针退出时重置梯度
    }

    // 指针移动事件，不需要处理，因为Update方法已经处理梯度更新
    public void OnPointerMove(PointerEventData eventData)
    {
        // 这里不需要任何东西，Update方法处理梯度更新
    }
}
