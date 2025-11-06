using System.Runtime.InteropServices;
using Microsoft.VisualBasic;



namespace Lager_automation.Models
{
    public class Bricscad
    {

        private dynamic _app;
        private readonly bool _createIfNotExists = true;
        private readonly bool _visible = true;
        private const string PROGID = "BricscadApp.AcadApplication"; // adjust version

        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetActiveObject(ref Guid rclsid, IntPtr reserved,
            [MarshalAs(UnmanagedType.IUnknown)] out object? ppunk);

        private static object? GetActiveComObject(string progId)
        {
            Type? type = Type.GetTypeFromProgID(progId, throwOnError: false);
            if (type == null)
                return null;

            Guid clsid = type.GUID;
            int hr = GetActiveObject(ref clsid, IntPtr.Zero, out object? obj);
            return hr == 0 ? obj : null;
        }

        public dynamic App
        {
            get
            {
                if (_app == null)
                {
                    // 1️⃣ Try to attach to existing BricsCAD process
                    _app = GetActiveComObject(PROGID);

                    // 2️⃣ If not found, start a new one
                    if (_app == null && _createIfNotExists)
                    {
                        Type t = Type.GetTypeFromProgID(PROGID)!;
                        _app = Activator.CreateInstance(t);
                        _app.Visible = _visible;
                    }
                }
                return _app;
            }
        }

        public dynamic Doc
        {
            get
            {
                var app = App;
                try
                {
                    return app.ActiveDocument;
                }
                catch
                {
                    return app.Documents.Add();
                }
            }
        }

        public dynamic Model => Doc.ModelSpace;

        public void DrawLine(double x1, double y1, double x2, double y2)
        {
            var p1 = new double[] { x1, y1, 0 };
            var p2 = new double[] { x2, y2, 0 };
            Model.AddLine(p1, p2);
        }

        public void DrawRect(double x, double y, double width, double height)
        {
            var bl = new double[] { x, y, 0 };
            var br = new double[] { x + width, y, 0 };
            var tr = new double[] { x + width, y + height, 0 };
            var tl = new double[] { x, y + height, 0 };

            DrawLine(bl[0], bl[1], br[0], br[1]);
            DrawLine(br[0], br[1], tr[0], tr[1]);
            DrawLine(tr[0], tr[1], tl[0], tl[1]);
            DrawLine(tl[0], tl[1], bl[0], bl[1]);
        }

        public void AddText(string text, double x, double y, double height = 250)
        {
            var p = new double[] { x, y, 0 };
            Model.AddText(text, p, height);
        }
    }
}
