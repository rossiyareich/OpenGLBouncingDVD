using System.Numerics;
using GLFW;
using OpenGLBouncingDVD.Source.GameLoop;
using OpenGLBouncingDVD.Source.Rendering.Cameras;
using OpenGLBouncingDVD.Source.Rendering.Display;
using OpenGLBouncingDVD.Source.Rendering.Shaders;
using OpenGLBouncingDVD.Source.Rendering.Text;
using OpenGLBouncingDVD.Source.Rendering.Textures;
using static OpenGLBouncingDVD.Library.GL;

namespace OpenGLBouncingDVD.Source.GameLogic
{
    internal class DVDGame : Game
    {
        uint vao;
        uint vbo;

        Shader shader;
        Camera2D camera;

        Texture texture;
        Text text;

        const float zoom = 1f;

        const float margin = 5f;
        const float boxWidthH = 300f / 2;
        const float boxHeightH = 153f / 2;
        float boxPositionX;
        float boxPositionY;
        const float speed = 300f;
        Direction direction = Direction.SE;
        bool hasHitOneEdge;
        bool hasHitTwoEdge;

        int hitEdgeCount = -1;
        int hitCornerCount = 0;
        Vector4 nextColor = new Vector4(1, 1, 1, 1);

        uint textures;

        readonly Vector4[] ColorList = new Vector4[]
        {
            new Vector4(1, 0, 0, 1),    //Red
            new Vector4(1, 0.5f, 0, 1), //Orange
            new Vector4(1, 1, 0, 1),    //Yellow
            new Vector4(0, 1, 0, 1),    //Green
            new Vector4(0, 0, 1, 1),    //Blue
            new Vector4(0.18039f, 0.16863f, 0.37255f, 1),    //Indigo
            new Vector4(0.54510f, 0, 1, 1),    //Violet
            new Vector4(1, 1, 1, 1),    //White
        };

        #region VBOs
        //Interleaved data => X, Y, (0 => inf)TexX, TexY
        float[] vertices =
        {
                -0.5f, 0.5f,   0f, 1f,     //top left
                0.5f, 0.5f,    1f, 1f,     //top right
                -0.5f, -0.5f,  0f, 0f,     //bottom left
                0.5f, -0.5f,   1f, 0f      //bottom right
        };
        #endregion

        #region GLSL Shaders
        //GLSL Shaders to be used by OpenGL PP

        //Position vertex BEFORE anything happens
        string vertexShader = @"#version 330 core
                                    layout (location = 0) in vec2 aPosition;
                                    layout (location = 1) in vec2 aTex;
                                    out vec2 texCoord;
                                    out vec4 ourColor;

                                    uniform mat4 projection;
                                    uniform mat4 model;
                                    uniform vec4 vertexColor;

                                    void main()
                                    {
                                        gl_Position = projection * model * vec4(aPosition.xy, 0, 1.0);
                                        texCoord = aTex;
                                        ourColor = vertexColor;
                                    }";

        //Specify color for each fragment(pixel)
        //Set the color of the fragment(pixel) to the input vertex color
        string fragmentShader = @"#version 330 core
                                        out vec4 FragColor;
                                        in vec2 texCoord;
                                        in vec4 ourColor;

                                        uniform int renderText;

                                        uniform sampler2D tex0;
                                        uniform sampler2D tex1;
                                        
                                        void main()
                                        {
                                            if(renderText <= 0)
                                                FragColor = texture2D(tex0, texCoord) * ourColor;
                                            else
                                                FragColor = texture2D(tex1, texCoord);
                                        }";
        #endregion

        public DVDGame(int initialWindowWidth, int initialWindowHeight, string initialWindowTitle) : base(initialWindowWidth, initialWindowHeight, initialWindowTitle)
        {
        }
        protected override void Initialize()
        {

        }
        protected override unsafe void LoadContent()
        {
            Vector2 squarePosition = DisplayManager.WindowSize / 2;
            boxPositionX = squarePosition.X;
            boxPositionY = squarePosition.Y;

            shader = new Shader(vertexShader, fragmentShader);
            shader.Load();

            fixed (uint* t = &textures)
            {
                glGenTextures(2, t);

                text = new Text(shader, t);

                texture = new Texture(4, "Resources/Texture/DVD_logo.png", shader, t);
                texture.Load();
            }


            //Create VAO and VBO
            vao = glGenVertexArray();
            vbo = glGenBuffer();

            //Bind VAO and VBO(as array buffer)
            glBindVertexArray(vao);
            glBindBuffer(GL_ARRAY_BUFFER, vbo);

            fixed (float* v = &vertices[0])      //Get the addr of vertices as a float*
            {
                //Apply to buffer as array buffer, with the size of float for each vertex on all vertices, with the data as a float* pointing to the vertices array
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);
            }

            //Set the position attributes of the interleaved data => attribute index 0, size of the pair is 2, type is float, data not normalized, 4 floats between pair (two points), first apperance after 0 floats
            glVertexAttribPointer(0, 2, GL_FLOAT, false, sizeof(float) * 4, (void*)0);
            glEnableVertexAttribArray(0);       //Enable attribute index 0
            //Set the texture attributes of the interleaved data => attribute index 1, size of the pair is 2, type is float, data not normalized, 4 floats between pair (two points), first apperance after 2 floats
            glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeof(float) * 4, (void*)(sizeof(float) * 2));
            glEnableVertexAttribArray(1);       //Enable attribute index 1

            //Unbind VAO and VBO once done creating them (VAO & VBO are setup)
            glBindVertexArray(0);
            glBindBuffer(GL_ARRAY_BUFFER, 0);

            camera = new Camera2D(DisplayManager.WindowSize / 2, zoom);
        }
        protected override void Update()
        {
        }
        protected override void Render()
        {
            //Do DVD movement
            var newDirection = direction;
            if (boxPositionX - boxWidthH <= 0) //Left wall
            {
                newDirection = direction switch
                {
                    Direction.NW => Direction.NE,
                    Direction.SW => Direction.SE,
                    _ => direction
                };

                hasHitOneEdge = (newDirection != direction);
            }
            else if (boxPositionX + boxWidthH >= DisplayManager.WindowSize.X) //Right wall
            {
                newDirection = direction switch
                {
                    Direction.NE => Direction.NW,
                    Direction.SE => Direction.SW,
                    _ => direction
                };
                hasHitOneEdge = (newDirection != direction);
            }

            if (boxPositionY - boxHeightH <= 0) //Top wall
            {
                newDirection = direction switch
                {
                    Direction.NE => Direction.SE,
                    Direction.NW => Direction.SW,
                    _ => direction
                };

                if (!hasHitOneEdge)
                {
                    hasHitOneEdge = (newDirection != direction);
                }
            }
            else if (boxPositionY + boxHeightH >= DisplayManager.WindowSize.Y) //Bottom wall
            {
                newDirection = direction switch
                {
                    Direction.SE => Direction.NE,
                    Direction.SW => Direction.NW,
                    _ => direction
                };

                if (!hasHitOneEdge)
                {
                    hasHitOneEdge = (newDirection != direction);
                }
            }

            if (hasHitOneEdge && !hasHitTwoEdge &&
                ((boxPositionX - boxWidthH <= margin && boxPositionY - boxHeightH <= margin) ||
                (boxPositionX + boxWidthH >= DisplayManager.WindowSize.X - margin && boxPositionY - boxHeightH <= margin) ||
                (boxPositionX - boxWidthH <= margin && boxPositionY + boxHeightH >= DisplayManager.WindowSize.Y - margin) ||
                (boxPositionX + boxWidthH >= DisplayManager.WindowSize.X - margin && boxPositionY + boxHeightH >= DisplayManager.WindowSize.Y - margin)))
            {
                hasHitTwoEdge = true;
            }

            if (hasHitOneEdge)
            {
                hitEdgeCount++;
            }

            if (hasHitTwoEdge)
            {
                hitCornerCount++;
                nextColor = NextColor();
            }
            else if (hasHitOneEdge)
            {
                nextColor = NextColor();
            }

            direction = newDirection;

            (float deltaMovementX, float deltaMovementY) = direction switch
            {
                Direction.NE => (1, -1),
                Direction.NW => (-1, -1),
                Direction.SE => (1, 1),
                Direction.SW => (-1, 1),
                _ => (0, 0)
            };

            boxPositionX += deltaMovementX * speed * GameTime.DeltaTimeF;
            boxPositionY += deltaMovementY * speed * GameTime.DeltaTimeF;

            //Clear to black
            glClearColor(0, 0, 0, 1);
            glClear(GL_COLOR_BUFFER_BIT);       //Perform clear

            //Want square to be 300x153px
            Vector2 scale = new Vector2(300, 153);

            //Set the transform(model) matrix
            Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scale.X, scale.Y, 1);
            Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationZ(0);

            {
                Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(boxPositionX, boxPositionY, 0);
                shader.SetMatrix4x4("model", scaleMatrix * rotationMatrix * translationMatrix);

                shader.SetVector4("vertexColor", nextColor.X, nextColor.Y, nextColor.Z, nextColor.W);

                texture.Use();
                shader.Use();

                //Set shader uniform (for coordinate mapping) only after the shader is used
                shader.SetMatrix4x4("projection", camera.GetProjectionMatrix());

                glBindVertexArray(vao);             //Bind VAO once used (tell OpenGL to use our VAO)
                glDrawArrays(GL_TRIANGLE_STRIP, 0, vertices.Length / 4);
                texture.Unuse();
            }
            {
                if (hasHitTwoEdge)
                {
                    hasHitTwoEdge = false;
                    hasHitOneEdge = false;
                    text.UpdateText(@$"Edges hit: {hitEdgeCount}
Corners hit: {hitCornerCount}");
                    text.Load();
                }
                if (hasHitOneEdge)
                {
                    hasHitOneEdge = false;
                    text.UpdateText(@$"Edges hit: {hitEdgeCount}
Corners hit: {hitCornerCount}");
                    text.Load();
                }

                Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(200f, DisplayManager.WindowSize.Y - 40f, 0);
                shader.SetMatrix4x4("model", scaleMatrix * rotationMatrix * translationMatrix);

                shader.SetVector4("vertexColor", nextColor.X, nextColor.Y, nextColor.Z, nextColor.W);

                text.Use();
                shader.Use();

                //Set shader uniform (for coordinate mapping) only after the shader is used
                shader.SetMatrix4x4("projection", camera.GetProjectionMatrix());

                glDrawArrays(GL_TRIANGLE_STRIP, 0, vertices.Length / 4);
                text.Unuse();
            }

            glBindVertexArray(0);               //Unbind VAO once done

            Glfw.SwapBuffers(DisplayManager.Window);
        }
        protected override void Close()
        {
            glDeleteVertexArray(vao);
            glDeleteBuffer(vbo);
            glDeleteShader(shader.ProgramID);

            unsafe
            {
                fixed (uint* t = &textures)
                {
                    glDeleteTextures(2, t);
                }
            }
        }
        Vector4 NextColor()
        {
            Random random = new Random();
            var color = ColorList[random.Next(ColorList.Length)];
            if (color == nextColor)
            {
                return NextColor();
            }
            else
            {
                return color;
            }
        }
    }
}
