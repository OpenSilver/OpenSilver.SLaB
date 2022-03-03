namespace ScratchContent.OpenSilver.Navigation
{
    using SLaB.Navigation.ContentLoaders.Utilities;
    using System;
    using System.Windows;
    using System.Windows.Markup;
    using System.Windows.Navigation;

    public interface IConfigurationAuthorizer
    {
        /// <summary>
        ///   Checks whether the principal has sufficient authorization to access the Uri being loaded by the AuthContentLoader.
        ///   If the principal is authorized, this method should simply return.  Otherwise, it should throw.
        /// </summary>
        /// <param name = "principal">The user credentials against which to check.</param>
        /// <param name = "targetUri">The Uri being loaded.</param>
        /// <param name = "currentUri">The current Uri from which the new Uri is being loaded.</param>
        void CheckConfiguration();
    }

    /// <summary>
    ///   An INavigationContentLoader that throws an NotConfiguredException if the application has not been initialized
    /// </summary>
    [ContentProperty("Authorizer")]
    public class InitContentLoader : ContentLoaderBase
    {
        /// <summary>
        ///   The Authorizer that will be used to authorize the Principal.
        /// </summary>
        public static readonly DependencyProperty AuthorizerProperty =
            DependencyProperty.Register("Authorizer",
                                        typeof(IConfigurationAuthorizer),
                                        typeof(InitContentLoader),
                                        new PropertyMetadata(null));

        /// <summary>
        ///   The INavigationContentLoader being wrapped by the InitContentLoader.
        /// </summary>
        public static readonly DependencyProperty ContentLoaderProperty =
            DependencyProperty.Register("ContentLoader",
                                        typeof(INavigationContentLoader),
                                        typeof(InitContentLoader),
                                        new PropertyMetadata(null));

        private static readonly INavigationContentLoader DefaultLoader = new PageResourceContentLoader();

        /// <summary>
        ///   The Authorizer that will be used to check that the configuration has finished.
        /// </summary>
        public IConfigurationAuthorizer Authorizer
        {
            get { return (IConfigurationAuthorizer)this.GetValue(AuthorizerProperty); }
            set { this.SetValue(AuthorizerProperty, value); }
        }

        /// <summary>
        ///   The INavigationContentLoader being wrapped by the InitContentLoader.
        /// </summary>
        public INavigationContentLoader ContentLoader
        {
            get { return (INavigationContentLoader)this.GetValue(ContentLoaderProperty); }
            set { this.SetValue(ContentLoaderProperty, value); }
        }

        /// <summary>
        ///   Gets a value that indicates whether the specified URI can be loaded.
        /// </summary>
        /// <param name = "targetUri">The URI to test.</param>
        /// <param name = "currentUri">The URI that is currently loaded.</param>
        /// <returns>true if the URI can be loaded; otherwise, false.</returns>
        public override bool CanLoad(Uri targetUri, Uri currentUri)
        {
            return (this.ContentLoader ?? DefaultLoader).CanLoad(targetUri, currentUri);
        }

        /// <summary>
        ///   Creates an instance of a LoaderBase that will be used to handle loading.
        /// </summary>
        /// <returns>An instance of a LoaderBase.</returns>
        protected override LoaderBase CreateLoader()
        {
            return new InitLoader(this);
        }

        private class InitLoader : LoaderBase
        {
            #region Fields (3)

            //[Import(typeof(VirtuosoApplicationConfiguration))]
            //public VirtuosoApplicationConfiguration Configuration { get; set; }

            private INavigationContentLoader _Loader;
            private readonly InitContentLoader _Parent;
            private IAsyncResult _Result;

            #endregion Fields

            #region Constructors (1)

            public InitLoader(InitContentLoader parent)
            {
                this._Parent = parent;
            }

            #endregion Constructors

            #region Methods (2)

            // Public Methods (2) 

            public override void Cancel()
            {
                this._Loader.CancelLoad(this._Result);
            }

            public override void Load(Uri targetUri, Uri currentUri)
            {
                this._Loader = this._Parent.ContentLoader ?? DefaultLoader;

                bool is_logout = targetUri.OriginalString.EndsWith("Logout.xaml");

                if (
                    (this._Parent.Authorizer != null) &&
                    (is_logout == false)  //not authorizing the Logout page...
                   )
                {
                    try
                    {
                        this._Parent.Authorizer.CheckConfiguration();
                    }
                    catch (Exception e)
                    {
                        this.Error(e);
                        return;
                    }
                }
                try
                {
                    this._Result = this._Loader.BeginLoad(targetUri,
                                                          currentUri,
                                                          res =>
                                                          {
                                                              try
                                                              {
                                                                  var loadResult = this._Loader.EndLoad(res);
                                                                  if (loadResult.RedirectUri != null)
                                                                      Complete(loadResult.RedirectUri);
                                                                  else
                                                                      Complete(loadResult.LoadedContent);
                                                                  return;
                                                              }
                                                              catch (Exception e)
                                                              {
                                                                  this.Error(e);
                                                                  return;
                                                              }
                                                          },
                                                          null);
                }
                catch (Exception e)
                {
                    this.Error(e);
                }
            }

            #endregion Methods
        }
    }
}
