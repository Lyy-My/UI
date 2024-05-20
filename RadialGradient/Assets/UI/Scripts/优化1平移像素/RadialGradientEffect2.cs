using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//平移像素点
public class RadialGradientEffect2 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
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
    private Color[] precomputedGradient; // 预先计算的渐变颜色数组

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

        // 预先计算梯度
        precomputedGradient = new Color[textureSize * textureSize];
        PrecomputeGradient(new Vector2(0.5f, 0.5f)); // 初始化预计算的梯度
        UpdateGradient(new Vector2(0.5f, 0.5f)); // 用中心初始化
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
                lastMousePosition = normMousePosition;
                // 更新渐变
                UpdateGradient(normMousePosition);
            }
        }
    }

    // 预计算梯度颜色
    void PrecomputeGradient(Vector2 center)
    {
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float u = (float)x / (textureSize - 1);
                float v = (float)y / (textureSize - 1);
                Vector2 uv = new Vector2(u, v);

                float dist = Vector2.Distance(uv, center);
                Color color = Color.Lerp(Color.white, gradientColor, Mathf.SmoothStep(radius, radius + 0.1f, dist));
                precomputedGradient[y * textureSize + x] = color;
            }
        }
    }

    // 更新渐变纹理
    void UpdateGradient(Vector2 center)
    {
        Color[] pixels = new Color[textureSize * textureSize];

        // 计算基于新中心的像素位移
        int offsetX = (int)((center.x - 0.5f) * textureSize);
        int offsetY = (int)((center.y - 0.5f) * textureSize);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                int shiftedX = x - offsetX;
                int shiftedY = y - offsetY;

                if (shiftedX >= 0 && shiftedX < textureSize && shiftedY >= 0 && shiftedY < textureSize)
                {
                    pixels[y * textureSize + x] = precomputedGradient[shiftedY * textureSize + shiftedX];
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
        UpdateGradient(new Vector2(0.5f, 0.5f)); // 当指针退出时重置梯度
    }

    // 指针移动事件，不需要处理，因为Update方法已经处理梯度更新
    public void OnPointerMove(PointerEventData eventData)
    {
        // 这里不需要任何东西，Update方法处理梯度更新
    }
}
