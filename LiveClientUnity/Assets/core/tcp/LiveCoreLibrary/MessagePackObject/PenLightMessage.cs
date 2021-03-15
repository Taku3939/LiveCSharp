// 
// 
// 

using MessagePack;
using UnityEngine;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class PenLightMessage
    {
        [Key(0)] public PenLightMode mode { get; }
        
        [Key(1)] public ColorRgb _colorRgb;

        public PenLightMessage(PenLightMode mode, ColorRgb colorRgb)
        {
            this.mode = mode;
            this._colorRgb = colorRgb;
        }
    }

    public enum PenLightMode
    {
        Pen_None,
        Pen_Normal,
        Pen_Swing,
        Pen_Swing1,
        Pen_Swing2,
        Pen_Swing3,
        Pen_Swing4
    }

    [MessagePackObject]
    public struct ColorRgb
    {
        [Key(0)] public float r;
        [Key(1)] public float g;
        [Key(2)] public float b;

        public ColorRgb(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
        public ColorRgb(Color color)
        {
            this.r = color.r;
            this.g = color.g;
            this.b = color.b;
        }
        
        public Color ToUnityColor() => new Color(this.r, this.g, this.b);
    }
}