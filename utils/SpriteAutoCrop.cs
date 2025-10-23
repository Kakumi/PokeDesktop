using Godot;

namespace PokeDesktop.utils
{
    public static class SpriteAutoCrop
    {
        public static Rect2I ComputeOpaqueRect(Texture2D tex, float alphaThreshold = 0.05f)
        {
            var img = tex.GetImage();

            int w = img.GetWidth();
            int h = img.GetHeight();

            int minX = w, minY = h, maxX = -1, maxY = -1;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float a = img.GetPixel(x, y).A;
                    if (a > alphaThreshold)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < 0)
                return new Rect2I(0, 0, 0, 0);

            return new Rect2I(minX, minY, (maxX - minX + 1), (maxY - minY + 1));
        }

        private static Rect2I ComputeOpaqueRectForAnimated(AnimatedTexture animated, float alphaThreshold = 0.05f)
        {
            int frameCount = animated.Frames;
            if (frameCount <= 0)
                return new Rect2I(0, 0, 0, 0);

            bool anyOpaque = false;

            int combinedMinX = int.MaxValue;
            int combinedMinY = int.MaxValue;
            int combinedMaxX = int.MinValue;
            int combinedMaxY = int.MinValue;

            for (int i = 0; i < frameCount; i++)
            {
                Texture2D frameTex = animated.GetFrameTexture(i);
                if (frameTex == null)
                    continue;

                Rect2I r = ComputeOpaqueRect(frameTex, alphaThreshold);

                if (r.Size.X == 0 || r.Size.Y == 0)
                    continue;

                anyOpaque = true;

                if (r.Position.X < combinedMinX) combinedMinX = r.Position.X;
                if (r.Position.Y < combinedMinY) combinedMinY = r.Position.Y;

                int rMaxX = r.Position.X + r.Size.X - 1;
                int rMaxY = r.Position.Y + r.Size.Y - 1;

                if (rMaxX > combinedMaxX) combinedMaxX = rMaxX;
                if (rMaxY > combinedMaxY) combinedMaxY = rMaxY;
            }

            if (!anyOpaque)
                return new Rect2I(0, 0, 0, 0);

            return new Rect2I(
                combinedMinX,
                combinedMinY,
                (combinedMaxX - combinedMinX + 1),
                (combinedMaxY - combinedMinY + 1)
            );
        }

        public static void FitBottomTo(Sprite2D sprite, float alphaThreshold = 0.05f)
        {
            Rect2I cropRect;
            if (sprite.Texture is AnimatedTexture animatedTexture)
            {
                cropRect = ComputeOpaqueRectForAnimated(animatedTexture, alphaThreshold);
            }
            else
            {
                cropRect = ComputeOpaqueRect(sprite.Texture, alphaThreshold);
            }

            sprite.RegionEnabled = true;
            sprite.RegionRect = cropRect;

            sprite.Centered = false;

            var pos = sprite.Position;
            pos.Y = sprite.Texture.GetSize().Y - cropRect.Size.Y;
            sprite.Position = pos;
        }

        public static int GetBottomTransparentRows(Texture2D tex, float alphaThreshold = 0.05f)
        {
            if (tex == null) return 0;
            var rect = ComputeOpaqueRect(tex, alphaThreshold);
            if (rect.Size.X == 0 || rect.Size.Y == 0) return 0;

            int h = tex.GetHeight();
            int bottomTransparent = h - (rect.Position.Y + rect.Size.Y);
            return bottomTransparent < 0 ? 0 : bottomTransparent;
        }

        public static void ApplyCrop(TextureRect tr, float alphaThreshold = 0.05f, bool alignBottom = true)
        {
            if (tr == null || tr.Texture == null) return;

            Texture2D baseTex = tr.Texture;

            Rect2I cropRect = (baseTex is AnimatedTexture at)
                ? ComputeOpaqueRectForAnimated(at, alphaThreshold)
                : ComputeOpaqueRect(baseTex, alphaThreshold);

            if (cropRect.Size.X <= 0 || cropRect.Size.Y <= 0) return;

            var atlas = new AtlasTexture
            {
                Atlas = baseTex,
                Region = cropRect
            };
            tr.Texture = atlas;

            tr.CustomMinimumSize = cropRect.Size;
            tr.Size = cropRect.Size;

            if (alignBottom)
            {
                int dy = GetBottomTransparentRows(baseTex, alphaThreshold);
                tr.Position -= new Vector2(0, dy);
                //tr.OffsetRight = cropRect.Position.X;
                //tr.OffsetTop = cropRect.Position.Y;
            }
        }
    }
}
