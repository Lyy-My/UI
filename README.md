径向渐变算法并不是特别复杂，特别是用于简单的2D渲染和UI效果时。它主要涉及基础的数学运算和插值技术。
核心步骤和数学原理

1.坐标归一化:

对于一个2D纹理，我们需要将每个像素的位置坐标归一化到0到1的范围内。假设纹理大小是N x N，对于任意一个像素坐标 (x, y)，归一化坐标 u 和 v 计算公式如下：

![2@J9CKEE8T1H_)C}4}SYWY9](https://github.com/Lyy-My/UI/assets/58832511/e32bc50e-e1bf-41e4-b626-9fe41c3c15a0)   
归一化坐标 u 和 v 现在在 [0, 1] 范围内。

2.计算与中心点的距离:

需要确定一个中心点 (cx, cy)，通常也在 [0, 1] 范围内。每个像素与中心点之间的距离 dist 可以使用欧几里得距离公式计算：

![image](https://github.com/Lyy-My/UI/assets/58832511/1bddb93e-9fd7-4780-8e26-638a1a34e711)
在代码中，使用 Vector2.Distance(uv, center) 来计算。

3.平滑步进函数:

使用 Mathf.SmoothStep 函数生成平滑的渐变效果。它接受三个参数：开始点、结束点和插值值 x，公式如下：
![image](https://github.com/Lyy-My/UI/assets/58832511/8c3d4e50-253e-4004-b117-ba43b387e0f7) 

a 是 radius，b 是 radius + 0.1f，x 是 dist。

4.线性插值颜色:

根据计算出的平滑步进值，使用线性插值生成渐变颜色：
 
color=lerp(Color.white,gradientColor,gradient)   

lerp 函数线性插值两个颜色值，gradient 是插值因子。
一 、较简单处理方法：
单一径向计算：
二、 稍微复杂一点
1. 多重径向渐变
描述: 多重径向渐变是将多个径向渐变叠加在一起，创建出复杂的渐变效果。这种方法通常用于高质量的图形设计和特殊效果。

实现:

对每个径向渐变进行独立计算，然后将它们叠加在一起。
可以使用加权平均的方法来叠加多个渐变。

示例：

      
    void UpdateMultiGradient(Vector2 center) {  
    Color[] pixels = new Color[textureSize * textureSize];   
    Color[] tempPixels = new Color[textureSize * textureSize];
    // First gradient
    for (int y = 0; y < textureSize; y++)  
    {
        for (int x = 0; x < textureSize; x++)
        {
            float u = (float)x / (textureSize - 1);
            float v = (float)y / (textureSize - 1);
            Vector2 uv = new Vector2(u, v);

            float dist = Vector2.Distance(uv, center);
            float gradient = Mathf.SmoothStep(radius, radius + 0.1f, dist);
            Color color = Color.Lerp(Color.white, gradientColor1, gradient);
            pixels[y * textureSize + x] = color;
        }
    }

    // Second gradient
    for (int y = 0; y < textureSize; y++)
    {
        for (int x = 0; x < textureSize; x++)
        {
            float u = (float)x / (textureSize - 1);
            float v = (float)y / (textureSize - 1);
            Vector2 uv = new Vector2(u, v);

            float dist = Vector2.Distance(uv, center);
            float gradient = Mathf.SmoothStep(radius2, radius2 + 0.1f, dist);
            Color color = Color.Lerp(Color.white, gradientColor2, gradient);
            tempPixels[y * textureSize + x] = color;
        }
    }

    // Combine gradients
    for (int i = 0; i < pixels.Length; i++) 
    {
        pixels[i] = Color.Lerp(pixels[i], tempPixels[i], 0.5f);
    }

    gradientTexture.SetPixels(pixels);
    gradientTexture.Apply();} 

2. 高斯径向模糊
描述: 使用高斯模糊方法生成更平滑的径向渐变效果。这种方法通常用于需要平滑过渡的图形效果，如光晕和阴影。

实现:

使用高斯函数计算每个像素的权重，并对所有像素进行加权平均。
高斯函数公式:

![image](https://github.com/Lyy-My/UI/assets/58832511/e00c1629-7628-4e28-99c0-dab06ea4d66e)

其中， σ 是标准差，决定了模糊的程度。

示例:

    void UpdateGaussianGradient(Vector2 center, float sigma)
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
            float weight = Mathf.Exp(-dist * dist / (2 * sigma * sigma)) / (2 * Mathf.PI * sigma * sigma);
            Color color = Color.Lerp(Color.white, gradientColor, weight);
            pixels[y * textureSize + x] = color;
        }
    }

    gradientTexture.SetPixels(pixels);
    gradientTexture.Apply();}

3. 多层径向渐变（Radial Gradient with Layers）
描述: 使用多层径向渐变，每一层都有不同的参数，如颜色、半径和模糊程度。可以用于复杂的光照和阴影效果。

实现:

定义多层参数，每一层有不同的渐变颜色、半径和模糊参数。
逐层计算每个像素的颜色，并将其叠加到最终的结果上。

示例:


    [System.Serializable] 
    public struct RadialGradientLayer     {
    public Color color;     
    public float radius; 
    public float sigma;}

    public RadialGradientLayer[] layers; 

    void UpdateLayeredGradient(Vector2 center){
    
    Color[] pixels = new Color[textureSize * textureSize];

    for (int y = 0; y < textureSize; y++)
    {
        for (int x = 0; x < textureSize; x++)
        {
            float u = (float)x / (textureSize - 1);
            float v = (float)y / (textureSize - 1);
            Vector2 uv = new Vector2(u, v);
            Color finalColor = Color.white;

            foreach (var layer in layers)
            {
                float dist = Vector2.Distance(uv, center); 
                float weight = Mathf.Exp(-dist * dist / (2 * layer.sigma * layer.sigma)) / (2 * Mathf.PI * layer.sigma * layer.sigma);
                Color layerColor = Color.Lerp(Color.white, layer.color, weight); 
                finalColor = Color.Lerp(finalColor, layerColor, weight);
            }

            pixels[y * textureSize + x] = finalColor;
        }
    }

    gradientTexture.SetPixels(pixels); 
    gradientTexture.Apply();  }
    
优化：
一个256的材质，两个循环，鼠标每动一次就要更新整个材质像素，总共65536个像素点，再算上Gradient就是一次移动更新色彩要运算196608次。
针对这个做了一些处理，shader相关逻辑后面有需求再写吧.最好的优化可能还是需要shader进行

