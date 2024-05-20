using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 实现多层渐变，高斯模糊，并优化批处理像素点的算法
public class RadialGradientEffect4 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    // 图像组件
    public Image image;
    // 渐变颜色数组
    public Color[] gradientColors = new Color[3] { new Color(1, 1, 1, 0.8f), new Color(1, 1, 1, 0.6f), new Color(1, 1, 1, 0.4f) };
    // 半径数组
    public float[] radii = new float[3] { 0.3f, 0.5f, 0.7f };
    // 高斯模糊参数数组
    public float[] sigmas = new float[3] { 0.1f, 0.15f, 0.2f };
    // 纹理大小
    public int textureSize = 256;
    // 更新阈值
    public float updateThreshold = 0.01f;

    // 渐变纹理
    private Texture2D gradientTexture;
    // 纯色纹理
    private Texture2D solidColorTexture;
    // RectTransform组件
    private RectTransform rectTransform;
    // 上次鼠标位置
    private Vector2 lastMousePosition;
    // 指针是否悬停在图像上
    private bool isPointerOver = false;
    // 预计算的颜色数组
    private Color[] precomputedColors;

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

        // 创建纯色纹理
        solidColorTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        solidColorTexture.SetPixel(0, 0, Color.white);
        solidColorTexture.Apply();

        // 设置初始为纯色纹理
        image.sprite = Sprite.Create(solidColorTexture, new Rect(0, 0, solidColorTexture.width, solidColorTexture.height), new Vector2(0.5f, 0.5f));

        // 预计算颜色
        precomputedColors = new Color[textureSize * textureSize];
        PrecomputeColors();
    }

    void Update()
    {
        // 如果指针悬停在图像上
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
                lastMousePosition = normMousePosition;
                // 更新渐变
                UpdateGradient(normMousePosition);
            }
        }
    }

    // 预计算颜色
    void PrecomputeColors()
    {
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float u = (float)x / (textureSize - 1);
                float v = (float)y / (textureSize - 1);
                Vector2 uv = new Vector2(u, v);

                // 初始颜色为白色
                Color finalColor = Color.white;

                // 计算每层渐变
                for (int i = 0; i < gradientColors.Length; i++)
                {
                    float dist = Vector2.Distance(uv, new Vector2(0.5f, 0.5f)); // 以中心为基准计算距离
                    if (dist <= radii[i])
                    {
                        float weight = Mathf.Exp(-dist * dist / (2 * sigmas[i] * sigmas[i])) / (2 * Mathf.PI * sigmas[i] * sigmas[i]);
                        Color layerColor = Color.Lerp(Color.white, gradientColors[i], Mathf.SmoothStep(radii[i], radii[i] + 0.1f, dist) * weight);
                        finalColor = Color.Lerp(finalColor, layerColor, weight);
                    }
                }

                precomputedColors[y * textureSize + x] = finalColor;
            }
        }
    }

    // 更新渐变纹理
    void UpdateGradient(Vector2 center)
    {
        Color[] pixels = new Color[textureSize * textureSize];

        // 计算基于新中心的像素位移
        int centerX = Mathf.RoundToInt(center.x * (textureSize - 1));
        int centerY = Mathf.RoundToInt(center.y * (textureSize - 1));

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                int shiftedX = x - centerX + textureSize / 2;
                int shiftedY = y - centerY + textureSize / 2;

                if (shiftedX >= 0 && shiftedX < textureSize && shiftedY >= 0 && shiftedY < textureSize)
                {
                    pixels[y * textureSize + x] = precomputedColors[shiftedY * textureSize + shiftedX];
                }
                else
                {
                    pixels[y * textureSize + x] = Color.white; // 如果超出边界，则默认背景颜色
                }
            }
        }

        // 应用像素数据并更新纹理
        gradientTexture.SetPixels(pixels);
        gradientTexture.Apply();
        image.sprite = Sprite.Create(gradientTexture, new Rect(0, 0, gradientTexture.width, gradientTexture.height), new Vector2(0.5f, 0.5f));
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
        // 恢复为纯色纹理
        image.sprite = Sprite.Create(solidColorTexture, new Rect(0, 0, solidColorTexture.width, solidColorTexture.height), new Vector2(0.5f, 0.5f));
    }

    // 指针移动事件，不需要处理，因为Update方法已经处理渐变更新
    public void OnPointerMove(PointerEventData eventData)
    {
        // 这里不需要任何东西，Update方法处理梯度更新
    }
}
