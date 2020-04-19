using System;
using System.Runtime.InteropServices;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Common.Input;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using PETViewer.Common;
using PETViewer.Common.Model;
using static PETViewer.Common.Camera;

namespace PETViewer.GUI
{
    public class Window : GameWindow
    {
        private Shader _transparentShader;
        private Shader _opaqueShader;
        private Model _petModel;

        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastMousePos;

        private double _time;

        /* for fps counter */
        private double _lastTime = GLFW.GetTime();
        private int _nbFrames = 0;

        private readonly string _applicationTitle;

        public Window(int width, int height, double updateFrequency, string title) : base(
            new GameWindowSettings {UpdateFrequency = updateFrequency},
            new NativeWindowSettings {Size = new Vector2i {X = width, Y = height}, Title = title}
        )
        {
            _applicationTitle = title;
        }

        public void Run(string modelPath)
        {
            _petModel = new Model(modelPath);

            base.Run();
        }

        protected override void OnLoad()
        {
            EnableDebugging();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);

            _transparentShader = new Shader("Shaders/base.vert", "Shaders/transparent.frag");
            _opaqueShader = new Shader("Shaders/base.vert", "Shaders/opaque.frag");

            // initialize the camera 3 units away from the origin and give it the proper aspect ratio
            _camera = new Camera(Vector3.UnitZ * 3)
            {
                AspectRatio = ClientSize.X / (float) ClientSize.Y
            };

            // make the mouse cursor invisible and constrain it to the window
            CursorVisible = false;
            CursorGrabbed = true;

            base.OnLoad();
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _time += 4.0 * e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // var model = Matrix4.Identity * Matrix4.CreateRotationX((float) MathHelper.DegreesToRadians(time));
            var model = Matrix4.Identity
                        * Matrix4.CreateTranslation(new Vector3(0f, -1.75f, 0f))
                        * Matrix4.CreateScale(.2f);

            // first draw all opaque fragments
            _opaqueShader.Use();

            _opaqueShader.SetMat4("projection", _camera.GetProjectionMatrix());
            _opaqueShader.SetMat4("view", _camera.GetViewMatrix());
            _opaqueShader.SetMat4("model", model);

            _petModel.Draw(_opaqueShader);

            // then all transparent ones, this makes opaque fragments behind transparent ones "visible"
            _transparentShader.Use();

            _transparentShader.SetMat4("projection", _camera.GetProjectionMatrix());
            _transparentShader.SetMat4("view", _camera.GetViewMatrix());
            _transparentShader.SetMat4("model", model);

            _petModel.Draw(_transparentShader);

            SwapBuffers();

            base.OnRenderFrame(e);
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // TODO compare commented OnMouseMove
            // if (!IsFocused) // check to see if the window is focused
            // {
            //     return;
            // }

            /* handle keyboard */

            var input = KeyboardState;

            if (input.IsKeyDown(Key.Escape))
            {
                Close();
            }

            if (input.IsKeyDown(Key.W))
                _camera.ProcessKeyboard(CameraMovement.Forward, (float) e.Time);
            if (input.IsKeyDown(Key.S))
                _camera.ProcessKeyboard(CameraMovement.Backward, (float) e.Time);
            if (input.IsKeyDown(Key.A))
                _camera.ProcessKeyboard(CameraMovement.Left, (float) e.Time);
            if (input.IsKeyDown(Key.D))
                _camera.ProcessKeyboard(CameraMovement.Right, (float) e.Time);
            if (input.IsKeyDown(Key.Space))
                _camera.ProcessKeyboard(CameraMovement.Up, (float) e.Time);
            if (input.IsKeyDown(Key.LShift))
                _camera.ProcessKeyboard(CameraMovement.Down, (float) e.Time);

            /* handle mouse */

            var mouse = MouseState;

            if (_firstMove)
            {
                _lastMousePos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // calculate the offset of the mouse position
                var deltaX = mouse.X - _lastMousePos.X;
                var deltaY = mouse.Y - _lastMousePos.Y;
                _lastMousePos = new Vector2(mouse.X, mouse.Y);

                _camera.ProcessMouseMovement(deltaX, deltaY);
            }

            /* measure speed (https://www.opengl-tutorial.org/miscellaneous/an-fps-counter/) */

            double currentTime = GLFW.GetTime();
            _nbFrames++;
            if (currentTime - _lastTime >= 1.0)
            {
                // update title if last update was more than 1 sec ago, and reset timer
                Title = $"{_applicationTitle} | {1000.0 / _nbFrames:0.00} ms/frame ({_nbFrames} fps)";
                _nbFrames = 0;
                _lastTime += 1.0;
            }

            base.OnUpdateFrame(e);
        }


        // This function's main purpose is to set the mouse position back to the center of the window
        // every time the mouse has moved. So the cursor doesn't end up at the edge of the window where it cannot move
        // further out
        // TODO the window doesn't get "focused" when clicking on it, only after changing between windows, but then the mouse is messed up
        // works when settings CursorGrabbed=true and not checking for focus at all even in keyboard
        // protected override void OnMouseMove(MouseMoveEventArgs e)
        // {
        //     if (IsFocused) // check to see if the window is focused
        //     {
        //         MousePosition = new Vector2(ClientSize.X / 2f, ClientSize.Y / 2f);
        //     }
        //
        //     base.OnMouseMove(e);
        // }


        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            _camera.ProcessMouseScroll(e.OffsetY);

            base.OnMouseWheel(e);
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
            _camera.AspectRatio = ClientSize.X / (float) ClientSize.Y;

            base.OnResize(e);
        }


        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            _transparentShader.Dispose();

            base.OnUnload();
        }

        /* Debugging */

        private static void LogDebugMessage(DebugSource debugSource, DebugType type, int id, DebugSeverity severity,
            int len, IntPtr msgPtr, IntPtr customObj)
        {
            var msg = Marshal.PtrToStringAnsi(msgPtr, len);
            Console.WriteLine($"Source: {debugSource}, Type: {type}, id: {id}, Severity: {severity}, msg: '{msg}'");
        }

        private void EnableDebugging()
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            var nullptr = new IntPtr(0);
            GL.Arb.DebugMessageCallback(LogDebugMessage, nullptr);
        }
    }
}
