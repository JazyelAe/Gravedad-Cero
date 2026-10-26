using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GravedadCero.UI
{
    /// <summary>
    /// Identidad visual "Helios-7": paleta, fuentes, tamaños de texto y sprites generados por
    /// código (paneles con esquinas recortadas, iconos, degradados). Todas las pantallas
    /// los toman de aquí para mantener el mismo estilo.
    /// </summary>
    public static class UITheme
    {
        // ---------- Paleta ----------
        public static readonly Color Deep = new Color(0.027f, 0.05f, 0.086f, 1f);
        public static readonly Color Panel = new Color(0.047f, 0.086f, 0.141f, 0.9f);
        public static readonly Color PanelRaised = new Color(0.075f, 0.137f, 0.212f, 0.95f);
        public static readonly Color Cyan = new Color(0.37f, 0.89f, 1f, 1f);
        public static readonly Color CyanDim = new Color(0.37f, 0.89f, 1f, 0.35f);
        public static readonly Color CyanDeep = new Color(0.06f, 0.42f, 0.55f, 1f);
        public static readonly Color Amber = new Color(1f, 0.71f, 0.28f, 1f);
        public static readonly Color Text = new Color(0.93f, 0.96f, 0.98f, 1f);
        public static readonly Color TextSecondary = new Color(0.66f, 0.74f, 0.82f, 1f);
        public static readonly Color Success = new Color(0.42f, 0.94f, 0.71f, 1f);
        public static readonly Color Danger = new Color(1f, 0.38f, 0.32f, 1f);
        public static readonly Color Locked = new Color(0.40f, 0.46f, 0.53f, 1f);

        // ---------- Tamaños de texto (referencia 1920x1080) ----------
        public const float SizeDisplay = 86f;
        public const float SizeTitle = 56f;
        public const float SizeHeading = 34f;
        public const float SizeBody = 28f;
        public const float SizeSmall = 23f;
        public const float SizeCaption = 20f;

        static TMP_FontAsset titleFont;
        static TMP_FontAsset bodyFont;
        static Material bodyShadowMaterial;
        static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        /// <summary>Orbitron: solo para títulos (no tiene · ‹ › « » ^).</summary>
        public static TMP_FontAsset TitleFont => titleFont != null ? titleFont : (titleFont = Resources.Load<TMP_FontAsset>("Fonts/Orbitron SDF"));

        /// <summary>Exo 2: textos, botones e instrucciones (incluye tildes, ñ, ¿ y ¡).</summary>
        public static TMP_FontAsset BodyFont => bodyFont != null ? bodyFont : (bodyFont = Resources.Load<TMP_FontAsset>("Fonts/Exo2 SDF"));

        /// <summary>Material de Exo 2 con sombra suave, para textos que van sobre la escena.</summary>
        public static Material BodyShadowMaterial
        {
            get
            {
                if (bodyShadowMaterial != null) return bodyShadowMaterial;
                bodyShadowMaterial = new Material(BodyFont.material) { name = "Exo2 SDF Shadow" };
                bodyShadowMaterial.EnableKeyword("UNDERLAY_ON");
                bodyShadowMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.85f));
                bodyShadowMaterial.SetFloat("_UnderlayOffsetX", 0.15f);
                bodyShadowMaterial.SetFloat("_UnderlayOffsetY", -0.2f);
                bodyShadowMaterial.SetFloat("_UnderlayDilate", 0.15f);
                bodyShadowMaterial.SetFloat("_UnderlaySoftness", 0.6f);
                return bodyShadowMaterial;
            }
        }

        // ---------- Sprites generados ----------

        /// <summary>Relleno blanco con esquinas superior izquierda e inferior derecha recortadas (9-slice).</summary>
        public static Sprite PanelFill => Get("fill", () => Chamfer(64, 12, 0));

        /// <summary>Contorno del mismo panel; las esquinas son más brillantes que los lados.</summary>
        public static Sprite PanelLine => Get("line", () => Chamfer(64, 12, 2));

        public static Sprite Dot => Get("dot", () => Make(32, (x, y) => Circle(x, y, 32, 0.42f)));
        public static Sprite Ring => Get("ring", () => Make(64, (x, y) => RingAlpha(x, y, 64, 0.40f, 0.05f)));
        public static Sprite Triangle => Get("triangle", () => Make(32, TriangleAlpha));
        public static Sprite Check => Get("check", () => Make(32, CheckAlpha));
        public static Sprite Lock => Get("lock", () => Make(32, LockAlpha));

        /// <summary>Degradado horizontal de opaco (izquierda) a transparente (derecha).</summary>
        public static Sprite GradientLeft => Get("gradient", () =>
        {
            var tex = NewTexture(256, 4);
            for (int x = 0; x < 256; x++)
                for (int y = 0; y < 4; y++)
                    tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Pow(1f - x / 255f, 1.3f)));
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 256, 4), new Vector2(0.5f, 0.5f), 100f);
        });

        /// <summary>Líneas horizontales finas para un tramado de pantalla muy discreto (mosaico).</summary>
        public static Sprite Scanlines => Get("scanlines", () =>
        {
            var tex = NewTexture(4, 4);
            tex.wrapMode = TextureWrapMode.Repeat;
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    tex.SetPixel(x, y, new Color(1, 1, 1, y == 0 ? 1f : 0f));
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        });

        static Sprite Get(string key, System.Func<Sprite> create)
        {
            if (!sprites.TryGetValue(key, out Sprite sprite) || sprite == null)
            {
                sprite = create();
                sprite.name = "Helios7_" + key;
                sprites[key] = sprite;
            }
            return sprite;
        }

        static Texture2D NewTexture(int w, int h)
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };
        }

        static Sprite Make(int size, System.Func<float, float, float> alpha)
        {
            var tex = NewTexture(size, size);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(alpha(x + 0.5f, y + 0.5f))));
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>Panel 9-slice con dos esquinas en chaflán. lineWidth 0 = relleno; mayor que 0 = contorno.</summary>
        static Sprite Chamfer(int size, int cut, int lineWidth)
        {
            var tex = NewTexture(size, size);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    // Distancia (con signo) a cada borde del polígono; positiva = dentro.
                    float left = px, right = size - px, bottom = py, top = size - py;
                    float topLeft = (px + (size - py) - cut) / 1.41421f;
                    float bottomRight = ((size - px) + py - cut) / 1.41421f;
                    float inside = Mathf.Min(Mathf.Min(left, right), Mathf.Min(Mathf.Min(bottom, top), Mathf.Min(topLeft, bottomRight)));
                    float a;
                    if (lineWidth <= 0) a = Mathf.Clamp01(inside + 0.5f);
                    else
                    {
                        float edge = Mathf.Clamp01(inside + 0.5f) * Mathf.Clamp01(lineWidth - inside + 0.5f);
                        bool nearCorner = Mathf.Min(px, size - px) < 20 && Mathf.Min(py, size - py) < 20;
                        a = edge * (nearCorner ? 1f : 0.45f);
                    }
                    tex.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            tex.Apply();
            float b = cut + 4;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }

        static float Circle(float x, float y, float size, float radius)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f)) / size;
            return (radius - d) * size + 0.5f;
        }

        static float RingAlpha(float x, float y, float size, float radius, float thickness)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f)) / size;
            return (thickness - Mathf.Abs(d - radius)) * size + 0.5f;
        }

        static float TriangleAlpha(float x, float y)
        {
            // Triángulo que apunta hacia arriba: vértice en (16, 27), base de (5, 6) a (27, 6).
            float halfWidth = (27f - y) / 21f * 11f;
            float inside = Mathf.Min(Mathf.Min(y - 6f, 27f - y), halfWidth - Mathf.Abs(x - 16f));
            return inside + 0.5f;
        }

        static float CheckAlpha(float x, float y)
        {
            var p = new Vector2(x, y);
            float d = Mathf.Min(SegmentDistance(p, new Vector2(6, 16), new Vector2(13, 9)), SegmentDistance(p, new Vector2(13, 9), new Vector2(26, 23)));
            return 2.6f - d + 0.5f;
        }

        static float LockAlpha(float x, float y)
        {
            // Cuerpo rectangular y arco superior.
            float body = Mathf.Min(Mathf.Min(x - 7f, 25f - x), Mathf.Min(y - 4f, 17f - y));
            float arc = Mathf.Abs(Vector2.Distance(new Vector2(x, y), new Vector2(16, 18)) - 6.5f);
            float shackle = y > 17f ? 2f - arc : -1f;
            return Mathf.Max(body, shackle) + 0.5f;
        }

        static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }
    }
}
