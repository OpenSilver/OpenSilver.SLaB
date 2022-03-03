using SLaB.Navigation.ContentLoaders.Utilities;
using System;
using System.Windows;
using System.Windows.Navigation;

namespace ScratchContent.OpenSilver.Navigation
{
    public class NonLinearNavigationContentLoader : ContentLoaderBase
    {

        #region Declarations

        static readonly INavigationContentLoader _defaultLoader = new PageResourceContentLoader();
        static readonly INonLinearNavigationActivePages _defaultActivePages = new NonLinearNavigationActivePages();
        INonLinearNavigationActivePages _activePages;

        #endregion //Declarations

        #region Properties

        public INonLinearNavigationActivePages ActivePages
        {
            get { return _activePages ?? _defaultActivePages; }
            set { _activePages = value; }
        }

        static public INonLinearNavigationActivePages Current
        {
            get { return _defaultActivePages; }
        }

        static public string CurrentPage;
        #endregion //Properties

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the INavigationContentLoader being wrapped by the NonLinearNavigationContentLoader
        /// </summary>
        /// <value>The INavigationContentLoader</value>
        public INavigationContentLoader ContentLoader
        {
            get { return (INavigationContentLoader)GetValue(ContentLoaderProperty); }
            set { SetValue(ContentLoaderProperty, value); }
        }

        public static readonly DependencyProperty ContentLoaderProperty =
            DependencyProperty.Register("ContentLoader", typeof(INavigationContentLoader), typeof(NonLinearNavigationContentLoader),
                new PropertyMetadata(
#if OPENSILVER
                    new SLaB.Navigation.ContentLoaders.PageResource.PageResourceContentLoader()
#else
                    null
#endif
                    ));


        public static NavigateKey GetNavigateKey(DependencyObject obj)
        {
            return (NavigateKey)obj.GetValue(NavigateKeyProperty);
        }

        public static void SetNavigateKey(DependencyObject obj, NavigateKey value)
        {
            obj.SetValue(NavigateKeyProperty, value);
        }

        public static readonly DependencyProperty NavigateKeyProperty =
            DependencyProperty.RegisterAttached("NavigateKey", typeof(NavigateKey), typeof(NonLinearNavigationContentLoader), new PropertyMetadata(null));

        #endregion //Dependency Properties

        #region Constructor

        public NonLinearNavigationContentLoader() { }

        #endregion //Constructor

        #region Methods

        /// <summary>
        /// Gets a value that indicates whether the specified URI can be loaded.
        /// </summary>
        /// <param name="targetUri">The URI to test.</param>
        /// <param name="currentUri">The URI that is currently loaded.</param>
        /// <returns>true if the URI can be loaded; otherwise, false.</returns>
        public override bool CanLoad(Uri targetUri, Uri currentUri)
        {
            return (this.ContentLoader ?? _defaultLoader).CanLoad(targetUri, currentUri);
        }

        /// <summary>
        /// Creates an instance of a LoaderBase that will be used to handle loading.
        /// </summary>
        /// <returns>An instance of a LoaderBase.</returns>
        protected override LoaderBase CreateLoader()
        {
            return new NonLinearNavigationLoader(this);
        }

        #endregion //Methods

        #region Nested NonLinearNavigationLoader Class

        class NonLinearNavigationLoader : LoaderBase
        {
            NonLinearNavigationContentLoader _parent;
            IAsyncResult _result;

            INavigationContentLoader Loader
            {
                get { return _parent.ContentLoader ?? _defaultLoader; }
            }

            INonLinearNavigationActivePages ActivePages
            {
                get { return _parent.ActivePages; }
            }

            public NonLinearNavigationLoader(NonLinearNavigationContentLoader parent)
            {
                _parent = parent;
            }

            public override void Load(Uri targetUri, Uri currentUri)
            {
                //tamper protection 
                Boolean isMatchRequired = targetUri.OriginalString.Contains("addNew") && targetUri.OriginalString.Contains("trackingKey");

                try
                {
                    //tamper protection 
                    if (isMatchRequired && !this.ActivePages.Pages.ContainsKey(targetUri.OriginalString))
                    {
                        throw new InvalidOperationException("Uri missing from cache");
                    }

                    var containsKey = ContainsURI(targetUri);

                    if (containsKey == true)
                    {
                        //Return the 'cached page'
                        string __cachedURI = targetUri.OriginalString;

                        Object result = GetURI(targetUri, out __cachedURI);

                        Log(string.Format("Returning cached page targetUri.OriginalString: {0}", __cachedURI));

                        CurrentPage = result.ToString();
                        if (result is Uri)
                            base.Complete((Uri)result);
                        else
                            base.Complete(result);
                    }
                    else
                    {
                        _result = this.Loader.BeginLoad(targetUri, currentUri, (res) =>
                        {
                            try
                            {
                                LoadResult loadResult = this.Loader.EndLoad(res);
                                if (loadResult.RedirectUri != null)
                                    base.Complete(loadResult.RedirectUri);
                                else
                                {
                                    DependencyObject content = loadResult.LoadedContent as DependencyObject;
                                    CurrentPage = content.GetType().ToString();
                                    if (content != null)
                                    {
                                        String currentOriginalString = null;
                                        if (currentUri == null || String.IsNullOrWhiteSpace(currentUri.OriginalString))
                                        {
                                            currentOriginalString = "/Home";
                                        }
                                        else
                                        {
                                            currentOriginalString = currentUri.OriginalString;
                                        }

                                        String targetUriString = targetUri.OriginalString;
                                        String trackingKey = String.Empty;
                                        if (targetUri.OriginalString.EndsWith("action=addNew"))
                                        {
                                            trackingKey = Guid.NewGuid().ToString();
                                            targetUriString = String.Concat(targetUriString.Replace("action=addNew", String.Empty), "trackingKey=", trackingKey);
                                        }

                                        content.SetValue(NonLinearNavigationContentLoader.NavigateKeyProperty, new NavigateKey(content.GetType(), targetUriString, currentOriginalString, this.ActivePages, trackingKey));

                                        Log(string.Format("Adding to ActivePages: targetUriString: {0}", targetUriString));

                                        SetURI(content, targetUriString);

                                        //Return a newly 'cached page'
                                        base.Complete(content);
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException("LoadedContent was null or not a DependencyObject");
                                    }
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                base.Error(e);
                                return;
                            }
                        }, null);
                    }
                }
                catch (Exception e)
                {
                    base.Error(e);
                }
            }

            private void SetURI(DependencyObject content, String targetUriString)
            {
                this.ActivePages.Pages.Add(targetUriString, content);
            }

            private object GetURI(Uri targetUri, out string cachedURIString)
            {
                cachedURIString = targetUri.OriginalString;
                Object result = null;

                //var containsKey = this.ActivePages.Pages.ContainsKey(targetUri.OriginalString);

                //if (containsKey == false && targetUri.OriginalString.Contains("DynamicForm.xaml?patient=") == true)
                //{
                //    //Examples:
                //    //  "/Virtuoso.Home;component/Views/DynamicForm.xaml?patient=3637&admission=1978&form=1893&service=1610&task=1801&encounter=-1"
                //    //  "/Virtuoso.Home;component/Views/DynamicForm.xaml?patient=3637&admission=1978&form=1893&service=1610&task=1801&encounter=1030"

                //    //Attempt to find a re-keyed DynamicForm
                //    var cachedKey = this.ActivePages.Pages.Keys.Where(k =>
                //    {
                //        var len = k.IndexOf("&encounter=");
                //        if (len > 0)
                //        {
                //            if (k.Substring(0, len) == targetUri.OriginalString.Substring(0, len))
                //                return true;
                //        }
                //        return false;
                //    });

                //    containsKey = (cachedKey.Any() == true);
                //    if (containsKey == true)
                //    {
                //        cachedURIString = cachedKey.First();
                //        result = this.ActivePages.Pages[cachedKey.First()];
                //    }
                //    else
                //        result = this.ActivePages.GetPage(targetUri.OriginalString);
                //}
                //else
                //    result = this.ActivePages.GetPage(targetUri.OriginalString);

                result = this.ActivePages.GetPage(targetUri.OriginalString);

                return result;
            }

            private bool ContainsURI(Uri targetUri)
            {
                var containsKey = this.ActivePages.Pages.ContainsKey(targetUri.OriginalString);

                //if (containsKey == false && targetUri.OriginalString.Contains("DynamicForm.xaml?patient=") == true)
                //{
                //    //Examples:
                //    //  "/Virtuoso.Home;component/Views/DynamicForm.xaml?patient=3637&admission=1978&form=1893&service=1610&task=1801&encounter=-1"
                //    //  "/Virtuoso.Home;component/Views/DynamicForm.xaml?patient=3637&admission=1978&form=1893&service=1610&task=1801&encounter=1030"

                //    //Attempt to find a re-keyed DynamicForm
                //    var possibleMatches = this.ActivePages.Pages.Keys.Where(k =>
                //    {
                //        var len = k.IndexOf("&encounter=");
                //        if (len > 0)
                //        {
                //            if (k.Substring(0, len) == targetUri.OriginalString.Substring(0, len))
                //                return true;
                //        }
                //        return false;
                //    });

                //    containsKey = (possibleMatches.Any() == true);

                //}
                return containsKey;
            }

            public override void Cancel()
            {
                this.Loader.CancelLoad(_result);
            }

            private void Log(String msg)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
                System.Diagnostics.Debug.WriteLine(msg);
                System.Diagnostics.Debug.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
#endif
            }
        }

        #endregion //Nested NonLinearNavigationLoader Class
    }
}
