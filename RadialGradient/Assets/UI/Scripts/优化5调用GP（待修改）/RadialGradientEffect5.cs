using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//暂时不可用
public class RadialGradientEffect5 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public Image image;
    public Color[] gradientColors = new Color[3] { new Color(1, 1, 1, 0.8f), new Color(1, 1, 1, 0.6f), new Color(1, 1, 1, 0.4f) };
    public float[] radii = new float[3] { 0.3f, 0.5f, 0.7f };
    public float[] sigmas = new float[3] { 0.1f, 0.15f, 0.2f };
    public int textureSize = 256;
    public float updateThreshold = 0.01f;

    private Texture2D gradientTexture;
    private RectTransform rectTransform;
    private Vector2 lastMousePosition;
    private bool isPointerOver = false;

    public ComputeShader computeShader;
    private RenderTexture renderTexture;

    void Start()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        rectTransform = image.GetComponent<RectTransform>();

        gradientTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;

        renderTexture = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

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
        int kernel = computeShader.FindKernel("CSMain");

        computeShader.SetTexture(kernel, "Result", renderTexture);
        computeShader.SetVector("center", new Vector2(center.x, center.y));
        computeShader.SetVector("gradientColor1", gradientColors[0]);
        computeShader.SetVector("gradientColor2", gradientColors[1]);
        computeShader.SetVector("gradientColor3", gradientColors[2]);
        computeShader.SetFloat("radius1", radii[0]);
        computeShader.SetFloat("radius2", radii[1]);
        computeShader.SetFloat("radius3", radii[2]);
        computeShader.SetFloat("sigma1", sigmas[0]);
        computeShader.SetFloat("sigma2", sigmas[1]);
        computeShader.SetFloat("sigma3", sigmas[2]);

        computeShader.Dispatch(kernel, textureSize / 8, textureSize / 8, 1);

        RenderTexture.active = renderTexture;
        gradientTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        gradientTexture.Apply();

        image.sprite = Sprite.Create(gradientTexture, new Rect(0, 0, gradientTexture.width, gradientTexture.height), new Vector2(0.5f, 0.5f));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        lastMousePosition = Vector2.positiveInfinity;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UpdateGradient(new Vector2(0.5f, 0.5f));
    }

    public void OnPointerMove(PointerEventData eventData)
    {
    }
}
