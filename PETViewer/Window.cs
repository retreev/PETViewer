using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using PETViewer.Common;

namespace PETViewer
{
    public class Window : GameWindow
    {
        private readonly Vertex[] _vertices;

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private Shader _shader;
        private Texture _texture;

        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;

        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title)
        {
            _vertices = Util.LoadPet();
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * Vertex.Stride, _vertices,
                BufferUsageHint.StaticDraw);

            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            _texture = new Texture(Util.GetTexturePath());
            _texture.Use();

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // bind buffers to the VAO
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            int vertexLocation = _shader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, Vertex.Stride, 0);

            int textureLocation = _shader.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(textureLocation);
            GL.VertexAttribPointer(textureLocation, 2, VertexAttribPointerType.Float, false, Vertex.Stride, Vector3.SizeInBytes);
            
            int normalLocation = _shader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, Vertex.Stride, Vector3.SizeInBytes + Vector2.SizeInBytes);
            
            _camera = new Camera(Vector3.UnitZ * 3)
            {
                AspectRatio = Width / (float) Height
            };

            CursorVisible = false;

            base.OnLoad(e);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.DepthBufferBit);

            _texture.Use(TextureUnit.Texture0);
            _shader.Use();
            
            _shader.SetMatrix4("model", Matrix4.Identity);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
            
            _shader.SetInt("texture0", 0);

//            _shader.SetVector3("viewPos", _camera.Position);


            GL.BindVertexArray(_vertexArrayObject);

            // Then replace your call to DrawTriangles with one to DrawElements
            // Arguments:
            //   Primitive type to draw. Triangles in this case.
            //   How many indices should be drawn. Six in this case.
            //   Data type of the indices. The indices are an unsigned int, so we want that here too.
            //   Offset in the EBO. Set this to 0 because we want to draw the whole thing.
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length);

            SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused)
            {
                return;
            }

            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            if (input.IsKeyDown(Key.W))
            {
                _camera.Position += _camera.Front * _camera.Speed * (float) e.Time;
            }

            if (input.IsKeyDown(Key.S))
            {
                _camera.Position -= _camera.Front * _camera.Speed * (float) e.Time;
            }

            if (input.IsKeyDown(Key.A))
            {
                _camera.Position -= _camera.Right * _camera.Speed * (float) e.Time;
            }

            if (input.IsKeyDown(Key.D))
            {
                _camera.Position += _camera.Right * _camera.Speed * (float) e.Time;
            }

            if (input.IsKeyDown(Key.Q))
            {
                _camera.Position += _camera.Up * _camera.Speed * (float) e.Time;
            }

            if (input.IsKeyDown(Key.E))
            {
                _camera.Position -= _camera.Up * _camera.Speed * (float) e.Time;
            }

            MouseState mouse = Mouse.GetState();

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                float deltaX = mouse.X - _lastPos.X;
                float deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * _camera.Sensitivity;
                _camera.Pitch -= deltaY * _camera.Sensitivity; // reversed since y-coordinates range from bottom to top
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Focused)
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _camera.Fov -= e.DeltaPrecise;
            base.OnMouseWheel(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            _camera.AspectRatio = Width / (float) Height;
            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteProgram(_shader.Handle);
            GL.DeleteTexture(_texture.Handle);

            base.OnUnload(e);
        }
    }
}
