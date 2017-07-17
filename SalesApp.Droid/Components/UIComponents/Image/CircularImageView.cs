using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using SalesApp.Core.Logging;

namespace SalesApp.Droid.Components.UIComponents.Image
{
    public class CircularImageView : ImageView
    {
        // Border & Selector configuration variables
        private bool _hasBorder;
        private bool _hasSelector;
        private bool _isSelected;
        private bool _shadowEnabled;
        private int _borderWidth;
        private int _canvasSize;
        private int _selectorStrokeWidth;

        // Objects used for the actual drawing
        private BitmapShader _shader;
        private Bitmap _image;
        private Paint _paint;
        private Paint _paintBorder;
        private Paint _paintSelectorBorder;
        private ColorFilter _selectorFilter;

        private IAttributeSet _attrs;
        private int _defStyle;

        public CircularImageView(IntPtr intPtr, JniHandleOwnership jniHandleOwnership)
            : base(intPtr, jniHandleOwnership)
        {
            Init();
        }

        public CircularImageView(Context context) : base(context, null) { }

        public CircularImageView(Context context, IAttributeSet attrs)
            : this(context, attrs, Resource.Attribute.CircularImageViewStyle)
        {
            this._attrs = attrs;
            Init();
        }

        public CircularImageView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs)
        {
            this._attrs = attrs;
            this._defStyle = defStyle;
            Init();
        }

        /**
         * Initializes paint objects and sets desired attributes.
         * @param Context Context
         * @param _attrs Attributes
         * @param _defStyle Default Style
         */
        private void Init()
        {
            // Initialize paint objects
            _paint = new Paint { AntiAlias = true };
            _paintBorder = new Paint { AntiAlias = true };
            _paintSelectorBorder = new Paint { AntiAlias = true };

            // Disable this view's hardware acceleration on Honeycomb and up (Needed for shadow effect)
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
            {
                SetLayerType(LayerType.Software, _paintBorder);
                SetLayerType(LayerType.Software, _paintSelectorBorder);
            }

            // load the styled attributes and set their properties
            TypedArray attributes = Context.ObtainStyledAttributes(_attrs, Resource.Styleable.CircularImageView, _defStyle, 0);

            // Check if border and/or border is enabled
            _hasBorder = attributes.GetBoolean(Resource.Styleable.CircularImageView_border, false);
            _hasSelector = attributes.GetBoolean(Resource.Styleable.CircularImageView_selector, false);

            // Set border properties if enabled
            if (_hasBorder)
            {
                int defaultBorderSize = (int)(2 * Context.Resources.DisplayMetrics.Density + 0.5f);
                SetBorderWidth(attributes.GetDimensionPixelOffset(Resource.Styleable.CircularImageView_border_width, defaultBorderSize));
                SetBorderColor(attributes.GetColor(Resource.Styleable.CircularImageView_border_color, Color.White));
            }

            // Set selector properties if enabled
            if (_hasSelector)
            {
                int defaultSelectorSize = (int)(2 * Context.Resources.DisplayMetrics.Density + 0.5f);
                SetSelectorColor(attributes.GetColor(
                    Resource.Styleable.CircularImageView_selector_color, Color.Transparent));
                SetSelectorStrokeWidth(attributes.GetDimensionPixelOffset(Resource.Styleable.CircularImageView_selector_stroke_width, defaultSelectorSize));
                SetSelectorStrokeColor(attributes.GetColor(Resource.Styleable.CircularImageView_selector_stroke_color, Color.Blue));
            }

            // Add shadow if enabled
            if (attributes.GetBoolean(Resource.Styleable.CircularImageView_shadow, false))
                SetShadow(attributes);

            // We no longer need our attributes TypedArray, give it back to cache
            attributes.Recycle();
        }

        /**
         * Sets the CircularImageView's border width in pixels.
         * @param borderWidth Width in pixels for the border.
         */
        public void SetBorderWidth(int borderWidth)
        {
            this._borderWidth = borderWidth;
            this.RequestLayout();
            this.Invalidate();
        }

        /**
         * Sets the CircularImageView's basic border color.
         * @param borderColor The new color (including alpha) to set the border.
         */
        public void SetBorderColor(Color borderColor)
        {
            if (_paintBorder != null)
                _paintBorder.Color = borderColor;
            this.Invalidate();
        }

        /**
         * Sets the color of the selector to be draw over the
         * CircularImageView. Be sure to provide some opacity.
         * @param selectorColor The color (including alpha) to set for the selector overlay.
         */
        public void SetSelectorColor(Color selectorColor)
        {
            this._selectorFilter = new PorterDuffColorFilter(selectorColor, PorterDuff.Mode.SrcAtop);
            this.Invalidate();
        }

        /**
         * Sets the stroke width to be drawn around the CircularImageView
         * during click events when the selector is enabled.
         * @param selectorStrokeWidth Width in pixels for the selector stroke.
         */
        public void SetSelectorStrokeWidth(int selectorStrokeWidth)
        {
            this._selectorStrokeWidth = selectorStrokeWidth;
            this.RequestLayout();
            this.Invalidate();
        }

        /**
         * Sets the stroke color to be drawn around the CircularImageView
         * during click events when the selector is enabled.
         * @param selectorStrokeColor The color (including alpha) to set for the selector stroke.
         */
        public void SetSelectorStrokeColor(Color selectorStrokeColor)
        {
            if (_paintSelectorBorder != null)
                _paintSelectorBorder.Color = selectorStrokeColor;
            this.Invalidate();
        }

        /**
         * Enables a dark shadow for this CircularImageView.
         * @param shadowEnabled Set to true to render a shadow or false to disable it.
         */
        public void SetShadow(TypedArray attributes)
        {
            this._shadowEnabled = true;
            Color shadowColor = attributes.GetColor(Resource.Styleable.CircularImageView_shadow_color, Color.Black);
            float shadowRadius = attributes.GetFloat(Resource.Styleable.CircularImageView_shadow_width, 0.0f);

            _paintBorder.SetShadowLayer(shadowRadius, 0.0f, 0.0f, shadowColor);
            _paintSelectorBorder.SetShadowLayer(shadowRadius, 0.0f, 0.0f, shadowColor);
        }

        /**
         * Enables a dark shadow for this CircularImageView.
         * If the radius is set to 0, the shadow is removed.
         * @param radius
         * @param dx
         * @param dy
         * @param color
         */
        public void SetShadow(float radius, float dx, float dy, int color)
        {
            // TODO
        }

        protected override void OnDraw(Canvas canvas)
        {
            // Don't draw anything without an image
            if (_image == null)
                return;

            // Nothing to draw (Empty bounds)
            if (_image.Height == 0 || _image.Width == 0)
                return;

            // We'll need this later
            int oldCanvasSize = _canvasSize;

            // Compare canvas sizes
            _canvasSize = canvas.Width;
            if (canvas.Height < _canvasSize)
                _canvasSize = canvas.Height;

            // Reinitialize shader, if necessary
            if (oldCanvasSize != _canvasSize)
                RefreshBitmapShader();

            // Apply shader to paint
            _paint.SetShader(_shader);

            // Keep track of selectorStroke/border width
            int outerWidth = 0;

            // Get the exact X/Y axis of the view
            int center = _canvasSize / 2;


            if (_hasSelector && _isSelected)
            { // Draw the selector stroke & apply the selector filter, if applicable
                outerWidth = _selectorStrokeWidth;
                center = (_canvasSize - (outerWidth * 2)) / 2;

                _paint.SetColorFilter(_selectorFilter);
                canvas.DrawCircle(center + outerWidth, center + outerWidth, ((_canvasSize - (outerWidth * 2)) / 2) + outerWidth - 4.0f, _paintSelectorBorder);
            }
            else if (_hasBorder)
            { // If no selector was drawn, draw a border and clear the filter instead... if enabled
                outerWidth = _borderWidth;
                center = (_canvasSize - (outerWidth * 2)) / 2;

                _paint.SetColorFilter(null);
                canvas.DrawCircle(center + outerWidth, center + outerWidth, ((_canvasSize - (outerWidth * 2)) / 2) + outerWidth - 4.0f, _paintBorder);
            }
            else // Clear the color filter if no selector nor border were drawn
                _paint.SetColorFilter(null);

            // Draw the circular image itself
            canvas.DrawCircle(center + outerWidth, center + outerWidth, ((_canvasSize - (outerWidth * 2)) / 2.0f) - 4.0f, _paint);
        }

        public override bool DispatchTouchEvent(MotionEvent motionEvent)
        {
            // Check for clickable state and do nothing if disabled
            if (!this.Clickable)
            {
                this._isSelected = false;
                return OnTouchEvent(motionEvent);
            }

            // Set selected state based on Motion Event
            switch (motionEvent.Action)
            {
                case MotionEventActions.Down:
                    this._isSelected = true;
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Scroll:
                case MotionEventActions.Outside:
                case MotionEventActions.Cancel:
                    this._isSelected = false;
                    break;
            }

            // Redraw image and return super type
            this.Invalidate();
            return base.DispatchTouchEvent(motionEvent);
        }

        public override void Invalidate(Rect dirty)
        {
            base.Invalidate(dirty);

            // Don't do anything without a valid drawable
            if (Drawable == null)
                return;

            // Extract a Bitmap out of the drawable & set it as the main shader
            _image = DrawableToBitmap(Drawable);
            if (_shader != null || _canvasSize > 0)
                RefreshBitmapShader();
        }

        public override void Invalidate(int l, int t, int r, int b)
        {
            base.Invalidate(l, t, r, b);

            // Don't do anything without a valid drawable
            if (Drawable == null)
                return;

            // Extract a Bitmap out of the drawable & set it as the main shader
            _image = DrawableToBitmap(Drawable);
            if (_shader != null || _canvasSize > 0)
                RefreshBitmapShader();
        }

        public override void Invalidate()
        {
            base.Invalidate();

            // Don't do anything without a valid drawable
            if (Drawable == null)
                return;

            // Extract a Bitmap out of the drawable & set it as the main shader
            _image = DrawableToBitmap(Drawable);
            if (_shader != null || _canvasSize > 0)
                RefreshBitmapShader();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int width = MeasureWidth(widthMeasureSpec);
            int height = MeasureHeight(heightMeasureSpec);
            SetMeasuredDimension(width, height);
        }

        private int MeasureWidth(int measureSpec)
        {
            int result;
            MeasureSpecMode specMode = MeasureSpec.GetMode(measureSpec);
            int specSize = MeasureSpec.GetSize(measureSpec);

            if (specMode == MeasureSpecMode.Exactly)
            {
                // The parent has determined an exact size for the child.
                result = specSize;
            }
            else if (specMode == MeasureSpecMode.AtMost)
            {
                // The child can be as large as it wants up to the specified size.
                result = specSize;
            }
            else
            {
                // The parent has not imposed any constraint on the child.
                result = _canvasSize;
            }

            return result;
        }

        private int MeasureHeight(int measureSpecHeight)
        {
            int result;
            MeasureSpecMode specMode = MeasureSpec.GetMode(measureSpecHeight);
            int specSize = MeasureSpec.GetSize(measureSpecHeight);

            if (specMode == MeasureSpecMode.Exactly)
            {
                // We were told how big to be
                result = specSize;
            }
            else if (specMode == MeasureSpecMode.AtMost)
            {
                // The child can be as large as it wants up to the specified size.
                result = specSize;
            }
            else
            {
                // Measure the text (beware: ascent is a negative number)
                result = _canvasSize;
            }

            return (result + 2);
        }

        /**
         * Convert a drawable object into a Bitmap.
         * @param drawable Drawable to extract a Bitmap from.
         * @return A Bitmap created from the drawable parameter.
         */
        public Bitmap DrawableToBitmap(Drawable drawable)
        {
            if (drawable == null)   // Don't do anything without a proper drawable
                return null;
            else if (drawable.GetType() == typeof(BitmapDrawable))    // Use the getBitmap() method instead if BitmapDrawable
                return ((BitmapDrawable)drawable).Bitmap;

            // Create Bitmap object out of the drawable
            Bitmap bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(bitmap);
            drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
            drawable.Draw(canvas);

            // Return the created Bitmap
            return bitmap;
        }

        /**
         * Reinitializes the shader texture used to fill in
         * the Circle upon drawing.
         */
        public void RefreshBitmapShader()
        {
            try
            {
                if (_shader != null)
                {
                    return;
                }

                Bitmap scaledBitmap = Bitmap.CreateScaledBitmap(_image, _canvasSize, _canvasSize, false);
                _shader = new BitmapShader(scaledBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);
            }
            catch (ArrayIndexOutOfBoundsException ex)
            {
                var logger = LogManager.Get(typeof(CircularImageView));
                logger.Error(ex);
            }
        }
    }
}