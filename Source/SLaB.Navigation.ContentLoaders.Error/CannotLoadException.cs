﻿#region Using Directives

using System;
using System.Windows.Navigation;

#endregion

namespace SLaB.Navigation.ContentLoaders.Error
{
    /// <summary>
    ///   An Exception thrown by the ErrorPageLoader when its INavigationContentLoader's CanLoad method returns false.
    /// </summary>
    public class CannotLoadException : Exception
    {

        /// <summary>
        ///   Constructs a CannotLoadException.
        /// </summary>
        /// <param name = "loader">The loader whose CanLoad method returned false.</param>
        /// <param name = "targetUri">The targetUri passed into CanLoad.</param>
        /// <param name = "currentUri">The currentUri passed into CanLoad.</param>
        public CannotLoadException(INavigationContentLoader loader, Uri targetUri, Uri currentUri)
        {
            this.Loader = loader;
            this.TargetUri = targetUri;
            this.CurrentUri = currentUri;
        }



        /// <summary>
        ///   The currentUri passed into CanLoad.
        /// </summary>
        public Uri CurrentUri { get; private set; }

        /// <summary>
        ///   The loader whose CanLoad method returned false.
        /// </summary>
        public INavigationContentLoader Loader { get; private set; }

        /// <summary>
        ///   The targetUri passed into CanLoad.
        /// </summary>
        public Uri TargetUri { get; private set; }
    }
}