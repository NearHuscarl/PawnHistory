using System.Runtime.CompilerServices;
using UnityEngine;

namespace PawnHistory.Source.Helper;

public static class RectHelper
{
    public static Rect OffsetBy(this Rect rect, Vector2 offset)
    {
        return new Rect(rect.x + offset.x, rect.y + offset.y, rect.width, rect.height);
    }
}

public static class RectExtensions
{
    extension(Rect rect)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect OfSize(float width, float height) => new(0, 0, width, height);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect OfSize(Vector2 size) => new(0, 0, size.x, size.y);
    }
}