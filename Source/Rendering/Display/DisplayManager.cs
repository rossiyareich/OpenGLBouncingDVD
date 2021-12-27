using System.Drawing;
using System.Numerics;
using GLFW;
using static OpenGLBouncingDVD.Library.GL;
using Monitor = GLFW.Monitor;

namespace OpenGLBouncingDVD.Source.Rendering.Display
{
    static class DisplayManager
    {
        public static Window Window { get; set; }
        public static Vector2 WindowSize { get; set; }

        public static void CreateWindow(int width, int height, string title)
        {
            WindowSize = new Vector2(width, height);

            Glfw.Init();
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);       //OpenGL 3.x
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);       //OpenGL 3.3
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);  //OpenGL 3.3 Core Profile

            Glfw.WindowHint(Hint.Focused, true);                //Focused on start
            Glfw.WindowHint(Hint.Resizable, false);             //Fixed window size

            //Monitor.None => Windowed mode; Window.None => Don't share OpenGL Window context w/ other apps;
            //Returns a HWND (Window.None if error)
            Window = Glfw.CreateWindow(width, height, title, Monitor.None, Window.None);

            if (Window == Window.None)                           //No window created
            {
                return;
            }

            //Set window position to center
            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;    //Size of monitor
            int x = (screen.Width - width) / 2;
            int y = (screen.Height - height) / 2;
            Glfw.SetWindowPosition(Window, x, y);

            Glfw.MakeContextCurrent(Window);
            Import(Glfw.GetProcAddress);

            glViewport(0, 0, width, height);                    //Set viewport size
            Glfw.SwapInterval(0);                               //0 => VSync off; 1 => VSync on;
        }

        public static void CloseWindow()
        {
            Glfw.DestroyWindow(Window);
            Glfw.Terminate();
        }

    }
}
