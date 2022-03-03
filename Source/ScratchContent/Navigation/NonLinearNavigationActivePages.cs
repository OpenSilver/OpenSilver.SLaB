using System;
using System.Collections.Generic;
using System.Windows;

namespace ScratchContent.OpenSilver.Navigation
{
    public class NonLinearNavigationActivePages : INonLinearNavigationActivePages
    {
        #region Declarations

        IDictionary<String, DependencyObject> _pages = new Dictionary<String, DependencyObject>();

        #endregion //Declarations

        #region Properties

        public IDictionary<string, DependencyObject> Pages
        {
            get { return _pages; }
        }

        #endregion //Properties

        #region Constructor

        public NonLinearNavigationActivePages()
        {
        }

        #endregion //Constructor

        #region Methods

        public void RekeyPage(string currentURIString, string newURIString)
        {
            if (this.Pages.ContainsKey(currentURIString))
            {
                var content = this.Pages[currentURIString];

                Log(string.Format("[RekeyPage] Removing from Pages: currentURIString: {0}", currentURIString));

                ////////////////////////////////////////////////////////////////////

                this.Pages.Remove(currentURIString);  //TODO: shouldn't the 'content' of this Pages[uri] be 'cleaned' up?

                ////////////////////////////////////////////////////////////////////

                Log(string.Format("[RekeyPage] Adding to Pages: newURIString: {0}", newURIString));

                if (this.Pages.ContainsKey(newURIString) == false) this.Pages.Add(newURIString, content);
            }
        }

        private void Log(string msg)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
            System.Diagnostics.Debug.WriteLine(msg);
            System.Diagnostics.Debug.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
#endif
        }

        public void RemovePage(string uriString)
        {
            if (this.Pages.ContainsKey(uriString) == false)
                return;
            var target = this.Pages[uriString];
            //var pb = target as Virtuoso.Core.View.PageBase;
            //if (pb != null)
            //{
            //    bool OKToRemove = pb.Cleanup();
            //    if (OKToRemove)
            //    {
            //        System.Diagnostics.Debug.WriteLine(string.Format("[RemovePage] Removing from Pages: uriString: {0}", uriString));

            //        this.Pages.Remove(uriString);
            //        pb = null;
            //    }
            //}
            //else
            {
                this.Pages.Remove(uriString);  //remove this page from the navigation cache
            }
        }

        public String GetCurrentSource(String parentUriOriginalString)
        {
            if (this.Pages.ContainsKey(parentUriOriginalString))
            {
                var nk = this.Pages[parentUriOriginalString].GetValue(NonLinearNavigationContentLoader.NavigateKeyProperty) as NavigateKey;
                if (nk != null)
                    return nk.CurrentSource;
            }
            return String.Empty;
        }

        public Object GetPage(String targetUriOriginalString)
        {
            if (this.Pages.ContainsKey(targetUriOriginalString))
            {
                return this.GetLastChainedView(targetUriOriginalString, targetUriOriginalString);
            }
            return null;
        }

        public Int32 GetActiveCountForApplicaitonSuite(String applicationSuite)
        {
            Int32 count = 0;
            foreach (var item in this.Pages.Values)
            {
                var nk = item.GetValue(NonLinearNavigationContentLoader.NavigateKeyProperty) as NavigateKey;
                if (nk != null && nk.IsChainable && nk.ApplicationSuite != null && nk.ApplicationSuite.Equals(applicationSuite))
                {
                    count += 1;
                }
            }
            return count;
        }

        public Int32 GetActiveCountForView(Type view)
        {
            Int32 count = 0;
            foreach (var item in this.Pages.Values)
            {
                var nk = item.GetValue(NonLinearNavigationContentLoader.NavigateKeyProperty) as NavigateKey;
                if (nk != null && nk.ViewType == view)
                {
                    count += 1;
                }
            }
            return count;
        }


        //Delta version...
        //Object GetLastChainedView(String targetUriOriginalString, String baseOriginalString)
        //{
        //    var target = this.Pages[targetUriOriginalString];
        //    var nk = target.GetValue(NonLinearNavigationContentLoader.NavigateKeyProperty) as NavigateKey;
        //    if (nk != null)
        //    {
        //        if (nk.IsChainable)
        //        {
        //            foreach (var contentPage in this.Pages.Values)
        //            {
        //                var contentNavigateKey = contentPage.GetValue(NonLinearNavigationContentLoader.NavigateKeyProperty) as NavigateKey;
        //                if (contentNavigateKey != null && contentNavigateKey.ParentUriOriginalString.Equals(nk.UriString))
        //                {
        //                    return GetLastChainedView(contentNavigateKey.UriString, baseOriginalString);
        //                }
        //            }
        //        }

        //        if (targetUriOriginalString.Equals(baseOriginalString))
        //        {
        //            return target;
        //        }
        //        else
        //        {
        //            try
        //            {
        //                return new Uri(nk.CurrentSource, UriKind.Absolute);
        //            }
        //            catch (Exception)
        //            {
        //                return new Uri(nk.CurrentSource, UriKind.Relative);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return target;
        //    }
        //}

        Object GetLastChainedView(String targetUriOriginalString, String baseOriginalString)
        {
            var target = this.Pages[targetUriOriginalString];
            var nk = target.GetValue(NonLinearNavigationContentLoader.NavigateKeyProperty) as NavigateKey;
            if (nk != null)
            {
                if (nk.IsChainable)
                {
                    foreach (var contentPage in this.Pages.Values)
                    {
                        var contentNavigateKey = contentPage.GetValue(NonLinearNavigationContentLoader.NavigateKeyProperty) as NavigateKey;
                        if (contentNavigateKey != null && contentNavigateKey.ParentUriOriginalString.Equals(nk.UriString))
                        {
                            return GetLastChainedView(contentNavigateKey.UriString, baseOriginalString);
                        }
                    }
                }

                if (targetUriOriginalString.Equals(baseOriginalString))
                {
                    return target;
                }
                else
                {
                    try
                    {
                        return new Uri(nk.CurrentSource, UriKind.Absolute);
                    }
                    catch (Exception)
                    {
                        return new Uri(nk.CurrentSource, UriKind.Relative);
                    }
                }
            }
            else
            {
                return target;
            }
        }

        #endregion //Methods
    }
}
