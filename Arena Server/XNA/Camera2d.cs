using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Common_Library.XNA
{
    public class Camera2D
    {
        #region Fields

        protected float _zoom;
        protected Matrix _transform;
        protected Matrix _inverseTransform;
        protected Vector2 _pos;
        protected float _rotation;
        protected Viewport _viewport;
        protected MouseState _previewMState;
        protected MouseState _mState;
        protected KeyboardState _keyState;
        protected Int32 _scroll;

        #endregion

        #region Properties

        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; }
        }
        /// <summary>
        /// Camera View Matrix Property
        /// </summary>
        public Matrix Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }
        /// <summary>
        /// Inverse of the view matrix, can be used to get objects screen coordinates
        /// from its object coordinates
        /// </summary>
        public Matrix InverseTransform
        {
            get { return _inverseTransform; }
        }
        public Vector2 Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        #endregion

        #region Constructor

        public Camera2D(Viewport viewport)
        {
            _zoom = 1.0f;
            _scroll = 1;
            _rotation = 0.0f;
            _pos = Vector2.Zero;
            _viewport = viewport;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Update the camera view
        /// </summary>
        public void Update()
        {
            //Call Camera Input
            Input();
            _previewMState = _mState;
            //Clamp zoom value
            _zoom = MathHelper.Clamp(_zoom, 0.0f, 10.0f);
            //Clamp rotation value
            _rotation = ClampAngle(_rotation);
            //Create view matrix
            _transform = Matrix.CreateRotationZ(_rotation) *
                            Matrix.CreateScale(new Vector3(_zoom, _zoom, 1)) *
                            Matrix.CreateTranslation(_pos.X, _pos.Y, 0);
            //Update inverse matrix
            _inverseTransform = Matrix.Invert(_transform);
        }

        /// <summary>
        /// Example Input Method, rotates using cursor keys and zooms using mouse wheel
        /// </summary>
        protected virtual void Input()
        {
            _mState = Mouse.GetState();
            _keyState = Keyboard.GetState();
            //Check zoom
            if (_mState.ScrollWheelValue > _scroll)
            {
                _zoom += 0.1f;
                _scroll = _mState.ScrollWheelValue;
            }
            else if (_mState.ScrollWheelValue < _scroll)
            {
                if(_zoom > 0.1f)
                _zoom -= 0.1f;
                _scroll = _mState.ScrollWheelValue;
            }
            //Check rotation
            
            //Check Move
            if (_mState.LeftButton == ButtonState.Pressed && _previewMState.X > _mState.X)
            {
                _pos.X -= 12.0f;
            }
            if (_mState.LeftButton == ButtonState.Pressed && _previewMState.X < _mState.X)
            {
                _pos.X += 12.0f;
            }
            if (_mState.LeftButton == ButtonState.Pressed && _previewMState.Y < _mState.Y)
            {
                _pos.Y += 12.0f;
            }
            if (_mState.LeftButton == ButtonState.Pressed && _previewMState.Y > _mState.Y)
            {
                _pos.Y -= 12.0f;
            }
            if (_mState.RightButton == ButtonState.Pressed)
            {
                _pos.X = 0.0f;
                _pos.Y = 0.0f;
                _zoom = 0.9f;
            }
        }


        /// <summary>
        /// Clamps a radian value between -pi and pi
        /// </summary>
        /// <param name="radians">angle to be clamped</param>
        /// <returns>clamped angle</returns>
        protected float ClampAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }


        #endregion
    }
}
