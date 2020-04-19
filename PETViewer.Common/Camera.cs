using System;
using OpenToolkit.Mathematics;

namespace PETViewer.Common
{
    // This is the camera class as it could be set up after the tutorials on the website
    // It is important to note there are a few ways you could have set up this camera, for example
    // you could have also managed the player input inside the camera class, and a lot of the properties could have
    // been made into functions.

    // TL;DR: This is just one of many ways in which we could have set up the camera
    // Check out the web version if you don't know why we are doing a specific thing or want to know more about the code
    public class Camera
    {
        public enum CameraMovement
        {
            Forward,
            Backward,
            Left,
            Right,
            Up,
            Down
        }

        // We need quite the amount of vectors to define the camera
        // The position is simply the position of the camera
        // the other vectors are directions pointing outwards from the camera to define how it is rotated
        private Vector3 _position = Vector3.Zero;
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        // Pitch is the rotation around the x axis, and it is explained more specifically in the tutorial how we can use this
        private float _pitch = 0f;

        // Yaw is the rotation around the y axis, and it is explained more specifically in the tutorial how we can use this
        private float _yaw = -90f;

        // The speed and the sensitivity are the speeds of respectively,
        // the movement of the camera and the rotation of the camera (mouse sensitivity)
        private float _movementSpeed = 1.5f;
        private float _mouseSensitivity = .2f;

        // The fov (field of view) is how wide the camera is viewing, this has been discussed more in depth in a
        // previous tutorial, but in this tutorial you have also learned how we can use this to simulate a zoom feature.
        private float _fov = 45.0f;

        // This is simply the aspect ratio of the viewport, used for the projection matrix
        public float AspectRatio { get; set; }

        // In the instructor we take in a position
        // We also set the yaw to -90, the code would work without this, but you would be started rotated 90 degrees away from the rectangle
        public Camera(Vector3 position)
        {
            _position = position;
        }

        // Get the view matrix using the amazing LookAt function described more in depth on the web version of the tutorial
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(_position, _position + _front, _up);
        }

        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov), AspectRatio, 0.01f, 100f);
        }

        // Processes input received from any keyboard-like input system. Accepts input parameter in the form of camera defined ENUM (to abstract it from windowing systems)
        public void ProcessKeyboard(CameraMovement direction, float deltaTime)
        {
            float velocity = _movementSpeed * deltaTime;
            switch (direction)
            {
                case CameraMovement.Forward:
                    _position += _front * velocity;
                    break;
                case CameraMovement.Backward:
                    _position -= _front * velocity;
                    break;
                case CameraMovement.Left:
                    _position -= _right * velocity;
                    break;
                case CameraMovement.Right:
                    _position += _right * velocity;
                    break;
                case CameraMovement.Up:
                    _position += _up * velocity;
                    break;
                case CameraMovement.Down:
                    _position -= _up * velocity;
                    break;
            }
        }

        // Processes input received from a mouse input system. Expects the offset value in both the x and y direction.
        public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
        {
            xOffset *= _mouseSensitivity;
            yOffset *= _mouseSensitivity;

            _yaw += xOffset;
            _pitch -= yOffset; // reversed since y-coordinates range from bottom to top

            // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
            // of weird "bugs" when you are using euler angles for rotation. If you want to read more about this you can try researching a topic called gimbal lock
            if (constrainPitch)
            {
                if (_pitch > 89.0f)
                {
                    _pitch = 89.0f;
                }

                if (_pitch < -89.0f)
                {
                    _pitch = -89.0f;
                }
            }

            // Update Front, Right and Up Vectors using the updated Euler angles
            UpdateCameraVectors();
        }

        // Processes input received from a mouse scroll-wheel event. Only requires input on the vertical wheel-axis
        public void ProcessMouseScroll(float yOffset)
        {
            if (_fov >= 1.0f && _fov <= 45.0f)
            {
                _fov -= yOffset;
            }

            if (_fov <= 1.0f)
            {
                _fov = 1.0f;
            }

            if (_fov >= 45.0f)
            {
                _fov = 45.0f;
            }
        }

        // Calculates the front vector from the Camera's (updated) Euler Angles
        private void UpdateCameraVectors()
        {
            // Calculate the new Front vector
            var front = new Vector3
            {
                X = (float) Math.Cos(MathHelper.DegreesToRadians(_yaw))
                    * (float) Math.Cos(MathHelper.DegreesToRadians(_pitch)),
                Y = (float) Math.Sin(MathHelper.DegreesToRadians(_pitch)),
                Z = (float) Math.Sin(MathHelper.DegreesToRadians(_yaw))
                    * (float) Math.Cos(MathHelper.DegreesToRadians(_pitch))
            };
            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results
            _front = Vector3.Normalize(front);

            // Calculate both the right and the up vector using the cross product
            // Note that we are calculating the right from the global up, this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera
            // Normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}
