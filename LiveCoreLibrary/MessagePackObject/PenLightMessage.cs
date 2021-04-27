using MessagePack;

namespace LiveCoreLibrary
{
    [MessagePackObject]
    public class PenLightMessage
    {
        [Key(0)] public ulong UserId { get; }
        [Key(1)] public PenLightMode mode { get; }
        
        [Key(2)] public ColorRgb _colorRgb { get; }

        public PenLightMessage(ulong userId, PenLightMode mode, ColorRgb colorRgb)
        {
            this.UserId = userId;
            this.mode = mode;
            this._colorRgb = colorRgb;
        }
    }

    public enum PenLightMode
    {
        Pen_None,
        //Pen_Normal,
        //Pen_Swing,
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
    }
}