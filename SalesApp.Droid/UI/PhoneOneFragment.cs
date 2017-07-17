using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MK.Solar.Components.UIComponents;
using Fragment = Android.Support.V4.App.Fragment;

namespace MK.Solar.UI
{
    public abstract class PhoneOneFragment : FragmentBase3
    {


        

        public void SetFragment(Fragment fragment, int placeholder, string tag)
        {
            var fragmentTx = ChildFragmentManager.BeginTransaction();
            if (fragment.IsAdded)
            {
                fragmentTx.Remove(fragment);
            }
            fragmentTx.Add(placeholder, fragment, tag);
            fragmentTx.Commit();
        }
    }
}