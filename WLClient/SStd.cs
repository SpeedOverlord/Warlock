using System.Windows.Forms;

namespace sis
{
    public class SStd
    {
        public static void AdjustWindowSize(Form form, int desire_width, int desire_height)
        {
            int width_offset = desire_width - form.ClientRectangle.Right;
            int height_offset = desire_height - form.ClientRectangle.Bottom;

            form.Width += width_offset;
            form.Height += height_offset;
        }
    }
}
