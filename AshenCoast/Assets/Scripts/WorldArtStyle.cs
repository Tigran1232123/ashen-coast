using UnityEngine;
namespace AshenCoast
{
    public static class WorldArtStyle
    {
        public const float PixelsPerUnit=32;
        public static float SamplingStep(Sprite sprite,float scale)
            =>Mathf.Max(1,Mathf.Round(sprite.pixelsPerUnit/(Mathf.Max(.001f,Mathf.Abs(scale))*PixelsPerUnit)));
    }
}
