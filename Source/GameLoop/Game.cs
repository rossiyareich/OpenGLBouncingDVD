using GLFW;
using OpenGLBouncingDVD.Source.Rendering.Display;

namespace OpenGLBouncingDVD.Source.GameLoop
{
    abstract class Game
    {
        protected int InitialWindowWidth { get; set; }
        protected int InitialWindowHeight { get; set; }
        protected string InitialWindowTitle { get; set; }

        public Game(int initialWindowWidth, int initialWindowHeight, string initialWindowTitle)
        {
            InitialWindowWidth = initialWindowWidth;
            InitialWindowHeight = initialWindowHeight;
            InitialWindowTitle = initialWindowTitle;
        }

        public void Run()
        {
            Initialize();
            DisplayManager.CreateWindow(InitialWindowWidth, InitialWindowHeight, InitialWindowTitle);
            LoadContent();
            while (!Glfw.WindowShouldClose(DisplayManager.Window))
            {
                GameTime.DeltaTime = Glfw.Time - GameTime.TotalElapsedSeconds;
                GameTime.TotalElapsedSeconds = Glfw.Time;

                Update();
                Glfw.PollEvents();              //Make sure window is still responding
                Render();
            }
            Close();
            DisplayManager.CloseWindow();
        }

        protected abstract void Initialize();    //Before Window, OpenGL Inits
        protected abstract void LoadContent();  //Load stuff and hook to OpenGL
        protected abstract void Update();
        protected abstract void Render();
        protected abstract void Close();

    }
}
