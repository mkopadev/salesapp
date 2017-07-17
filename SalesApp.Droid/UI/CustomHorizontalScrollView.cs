using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace SalesApp.Droid.UI
{

    public class PeriodHorizontalScrollView : HorizontalScrollView, View.IOnTouchListener, GestureDetector.IOnGestureListener
    {

        private const int SwipeMinDistance = 300;
        private const int SwipeThresholdVelocity = 300;
        private const int SwipePageOnFactor = 10;


        private GestureDetector _gestureDetector;
        private int _scrollTo = 0;
        private int _maxItem = 0;
        private int _activeItem = 0;
        private float _prevScrollX = 0;
        private bool _start = true;
        private int _itemWidth = 0;
        private float _currentScrollX;
        private bool _flingDisable = true;
        private int _prevActiveItem = 0;

        public event EventHandler<ItemSelectedChangeEventHandlerArgs> ItemSelectedChangeEventHandler;


        protected virtual void OnItemSelectedChangeEventHandler(ItemSelectedChangeEventHandlerArgs e)
        {
            EventHandler<ItemSelectedChangeEventHandlerArgs> handler = ItemSelectedChangeEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public PeriodHorizontalScrollView(Context context)
            : base(context)
        {
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        }

        public PeriodHorizontalScrollView(Context context, int maxItem, int itemWidth)
            : base(context)
        {
            _maxItem = maxItem;
            _itemWidth = itemWidth;
            _gestureDetector = new GestureDetector(this);
            SetOnTouchListener(this);
        }

        

        public bool OnTouch(View v, MotionEvent e)
        {
            if (_gestureDetector.OnTouchEvent(e))
            {
                return true;
            }

            var returnValue = _gestureDetector.OnTouchEvent(e);
            var x = (int)e.RawX;

            switch (e.Action)
            {
                case MotionEventActions.Move:
                    if (_start)
                    {
                        _prevScrollX = x;
                        _start = false;
                    }
                    break;
                case MotionEventActions.Up:
                    _start = true;
                    _currentScrollX = x;
                    int minFactor = _itemWidth / SwipePageOnFactor;

                    if ((_prevScrollX - _currentScrollX) > minFactor)
                    {
                        if (_activeItem < _maxItem - 1)
                            _activeItem = _activeItem + 1;

                    }
                    else if ((_currentScrollX - _prevScrollX) > minFactor)
                    {
                        if (_activeItem > 0)
                            _activeItem = _activeItem - 1;
                    }

                    //_scrollTo = _activeItem * _itemWidth;
                    //SmoothScrollTo(_scrollTo, 0);
                    if (_activeItem != _prevActiveItem)
                    {
                        SetCenter(_activeItem);
                        ItemSelectedChangeEventHandlerArgs args = new ItemSelectedChangeEventHandlerArgs
                        {
                            itemSelected = _activeItem
                        };
                        OnItemSelectedChangeEventHandler(args);
                    }
                    
                    returnValue = true;
                    break;
            }

            

            return returnValue;

        }

        public void SetCenter(int index)
        {

            ViewGroup parent = (ViewGroup)GetChildAt(0);

            var preView = parent.GetChildAt(_prevActiveItem);
            preView.SetBackgroundColor(Resources.GetColor(Resource.Color.white));
            var lp = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent ,
                    ViewGroup.LayoutParams.WrapContent);
            lp.SetMargins(5, 5, 5, 5);
            preView.LayoutParameters = lp;

            var view = parent.GetChildAt(index);
            view.SetBackgroundColor(Resources.GetColor(Resource.Color.gray1));

            var screenWidth = ((Activity)Context).WindowManager
                    .DefaultDisplay.Width;

           // var scrollX = (view.Left - (screenWidth / 2)) + (view.Width / 2);
            _scrollTo = _activeItem * _itemWidth;
            SmoothScrollTo(_scrollTo, 0);
            _prevActiveItem = index;
        }

        public bool OnDown(MotionEvent e)
        {
            return false;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            if (_flingDisable)
                return false;
            bool returnValue = false;
            float ptx1 = 0, ptx2 = 0;
            if (e1 == null || e2 == null)
                return false;
            ptx1 = e1.GetX();
            ptx2 = e2.GetX();
            // right to left

            if (ptx1 - ptx2 > SwipeMinDistance
              && Math.Abs(velocityX) > SwipeThresholdVelocity)
            {
                if (_activeItem < _maxItem - 1)
                    _activeItem = _activeItem + 1;

                returnValue = true;

            }
            else if (ptx2 - ptx1 > SwipeMinDistance
            && Math.Abs(velocityX) > SwipeThresholdVelocity)
            {
                if (_activeItem > 0)
                    _activeItem = _activeItem - 1;

                returnValue = true;
            }
            _scrollTo = _activeItem * _itemWidth;
            SmoothScrollTo(0, _scrollTo);
            return returnValue;
        }

        public void OnLongPress(MotionEvent e)
        {

        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return false;
        }

        public void OnShowPress(MotionEvent e)
        {

        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }
    }

    public class ItemSelectedChangeEventHandlerArgs : EventArgs
    {
        public int itemSelected { get; set; }
    }
}