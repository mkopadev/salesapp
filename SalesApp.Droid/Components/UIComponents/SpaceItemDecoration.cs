using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace SalesApp.Droid.Components.UIComponents
{
    public class SpaceItemDecoration: RecyclerView.ItemDecoration
{
	private int mSpace;
	private bool mShowFirstDivider = false;
	private bool mShowLastDivider = false;
	int mOrientation = -1;

	public SpaceItemDecoration(Context context, IAttributeSet attrs) {
		mSpace = 0;
	}
	public SpaceItemDecoration(Context context, IAttributeSet attrs, bool showFirstDivider,
		bool showLastDivider) {
        mSpace = 0;
		mShowFirstDivider = showFirstDivider;
		mShowLastDivider = showLastDivider;
	}

	public SpaceItemDecoration(int spaceInPx)
	{
		mSpace = spaceInPx;
	}

	public SpaceItemDecoration(int spaceInPx, bool showFirstDivider,
		bool showLastDivider)
	{
        mSpace = spaceInPx;
		mShowFirstDivider = showFirstDivider;
		mShowLastDivider = showLastDivider;
	}

	public SpaceItemDecoration(Context ctx, int resId)
	{
		mSpace = ctx.Resources.GetDimensionPixelSize(resId);
	}
	public SpaceItemDecoration(Context ctx, int resId, bool showFirstDivider,
		bool showLastDivider)
	{
        mSpace = ctx.Resources.GetDimensionPixelSize(resId);
		mShowFirstDivider = showFirstDivider;
		mShowLastDivider = showLastDivider;
	}

	public override  void GetItemOffsets(Rect outRect, View view, RecyclerView parent,
		RecyclerView.State state)
	{
		if (mSpace == 0) {
			return;
		}

		if (mOrientation == -1)
			getOrientation(parent);

		int position = parent.GetChildAdapterPosition(view);
		if (position == RecyclerView.NoPosition || (position == 0 && !mShowFirstDivider)) {
			return;
		}

		if (mOrientation == LinearLayoutManager.Vertical) {
			outRect.Top = mSpace;
			if (mShowLastDivider && position == (state.ItemCount - 1)) {
				outRect.Bottom = outRect.Top;
			}
		} else {
			outRect.Left = mSpace;
			if (mShowLastDivider && position == (state.ItemCount - 1)) {
				outRect.Right = outRect.Left;
			}
		}
	}

	private int getOrientation(RecyclerView parent) {
		if (mOrientation == -1) {
			if (parent.GetLayoutManager().GetType() == typeof(LinearLayoutManager)) {
				LinearLayoutManager layoutManager = (LinearLayoutManager) parent.GetLayoutManager();
				mOrientation = layoutManager.Orientation;
			} else {
				throw new IllegalStateException(
					"DividerItemDecoration can only be used with a LinearLayoutManager.");
			}
		}
		return mOrientation;
	}
}
}