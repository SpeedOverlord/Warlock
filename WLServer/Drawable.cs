using System.Drawing;

namespace sis
{
    public interface Drawable
    {
        IPair Pos { get; set; }
        int DrawId { get; }
        int DrawArg { get; }
    }
}
