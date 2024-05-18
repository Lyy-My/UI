using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//径向算法简单版，处理简单渐变效果。
public class RadialGradientEffect1 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public Image image;
    public Color gradientColor = new Color(1, 1, 1, 0.6f);
    public float radius = 0.5f;
    public int textureSize = 256;
    public float updateThreshold = 0.01f;

    private Texture2D gradientTexture;
    private RectTransform rectTransform;
    private Vector2 lastMousePosition;
    private bool isPointerOver = false;

    void Start()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        rectTransform = image.GetComponent<RectTransform>();
        gradientTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        image.sprite = Sprite.Create(gradientTexture, new Rect(0, 0, gradientTexture.width, gradientTexture.height), new Vector2(0.5f, 0.5f));
        UpdateGradient(new Vector2(0.5f, 0.5f)); 
    }

    void Update()
    {
        if (isPointerOver)
        {
            Vector2 localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
            Vector2 normMousePosition = new Vector2(
                Mathf.InverseLerp(-rectTransform.rect.width / 2, rectTransform.rect.width / 2, localMousePosition.x),
                Mathf.InverseLerp(-rectTransform.rect.height / 2, rectTransform.rect.height / 2, localMousePosition.y)
            );

            if (Vector2.Distance(normMousePosition, lastMousePosition) > updateThreshold)
            {
                lastMousePosition = normMousePosition;
                UpdateGradient(normMousePosition);
            }
        }
    }

    void UpdateGradient(Vector2 center)
    {
        Color[] pixels = new Color[textureSize * textureSize];

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float u = (float)x / (textureSize - 1);
                float v = (float)y / (textureSize - 1);
                Vector2 uv = new Vector2(u, v);

                float dist = Vector2.Distance(uv, center);
                float gradient = Mathf.SmoothStep(radius, radius + 0.1f, dist);
                Color color = Color.Lerp(Color.white, gradientColor, gradient);
                pixels[y * textureSize + x] = color;
            }
        }

        gradientTexture.SetPixels(pixels);
        gradientTexture.Apply();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UpdateGradient(new Vector2(0.5f, 0.5f)); 
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        // 
    }
}
