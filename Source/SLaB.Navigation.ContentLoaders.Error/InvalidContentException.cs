﻿#region Using Directives

using System;

#endregion

namespace SLaB.Navigation.ContentLoaders.Error
{
    /// <summary>
    ///   An Exception thrown by the ErrorPageLoader when content from an INavigationContentLoader
    ///   when the content was not a UserControl or a Page.
    /// </summary>
    public class InvalidContentException : Exception
    {

        /// <summary>
        ///   Creates an InvalidContentException.
        /// </summary>
        /// <param name = "content">The invalid content.</param>
        public InvalidContentException(object content)
        {
            this.Content = content;
        }



        /// <summary>
        ///   The invalid content.
        /// </summary>
        public object Content { get; private set; }
    }
}