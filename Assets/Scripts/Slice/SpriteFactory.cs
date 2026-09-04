// SpriteFactory.cs — 아트 에셋 없이 단색 스프라이트 런타임 생성 (슬라이스 전용 플레이스홀더).
using UnityEngine;
using BroDungeon.Data;

namespace BroDungeon.Slice
{
    public static class SpriteFactory
    {
        /// 단색 스프라이트 생성. 픽셀 크기 = 캐릭터 규격(문서 06-2), PPU=16.
        public static Sprite Solid(Color c, int wpx, int hpx)
        {
            var tex = new Texture2D(wpx, hpx, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color[wpx * hpx];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, wpx, hpx),
                new Vector2(0.5f, 0.5f), Constants.PIXELS_PER_UNIT);
        }

        public static SpriteRenderer AttachSprite(GameObject go, Color c, int wpx, int hpx, int order = 0)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Solid(c, wpx, hpx);
            sr.sortingOrder = order;
            return sr;
        }
    }
}
