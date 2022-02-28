﻿#if OPENSILVER // https://github.com/OpenSilver/OpenSilver/blob/develop/src/Compiler/Shared/XamlResourcesHelper.cs
using System;
using System.Text.RegularExpressions;

namespace SLaB.Navigation.ContentLoaders.PageResource
{
    internal static class XamlResourcesHelper
    {
        /// <summary>
        /// Generates a class name from the uri for the class that will allow us to 
        /// instantiate the Page defined in the file at the given <see cref="Uri"/>.
        /// This class is used in <b>Frame</b> to create an instance of the page 
        /// (for example when using MyFrame.Source = new Uri("/InnerPage.xaml", UriKind.Relative);).
        /// </summary>
        /// <param name="componentUri">The Uri from which to generate the class name.</param>
        /// <returns>The newly generated class name.</returns>
        internal static string GenerateClassNameFromComponentUri(string componentUri)
        {
            // Convert to TitleCase (so that when we remove the spaces, it is easily readable):
            string className = MakeTitleCase(componentUri);

            // If file name contains invalid chars, remove them:
            className = Regex.Replace(className, @"\W", "ǀǀ"); //Note: this is not a pipe (the thing we get with ctrl+alt+6), it is U+01C0

            // If class name doesn't begin with a letter, insert an underscore:
            if (char.IsDigit(className, 0))
            {
                className = className.Insert(0, "_");
            }

            // Remove white space:
            className = className.Replace(" ", string.Empty);

            className += "ǀǀFactory"; //Note: this is not a pipe (the thing we get with ctrl+alt+6), it is U+01C0

            return className;
        }

        internal static string MakeTitleCase(string str)
        {
            string result = "";
            string lowerStr = str.ToLower();
            int length = str.Length;
            bool makeUpper = true;
            int lastCopiedIndex = -1;
            //****************************
            //HOW THIS WORKS:
            //
            //  We go through all the characters of the string.
            //  If any is not an alphanumerical character, we make the next alphanumerical character uppercase.
            //  To do so, we copy the string (on which we call toLower) bit by bit into a new variable,
            //  each bit being the part between two uppercase characters, and while inserting the uppercase version of the character between each bit.
            //  then we add the end of the string.
            //****************************

            for (int i = 0; i < length; ++i)
            {
                char ch = lowerStr[i];
                if (ch >= 'a' && ch <= 'z' || ch >= '0' && ch <= '0')
                {
                    if (makeUpper && ch >= 'a' && ch <= 'z') //if we have a letter, we make it uppercase. otherwise, it is a number so we let it as is.
                    {
                        if (!(lastCopiedIndex == -1 && i == 0)) //except this very specific case, we should never have makeUpper at true while i = lastCopiedindex + 1 (since we made lowerStr[lastCopiedindex] into an uppercase letter.
                        {
                            result += lowerStr.Substring(lastCopiedIndex + 1, i - lastCopiedIndex - 1); //i - lastCopied - 1 because we do not want to copy the current index since we want to make it uppercase:
                        }
                        result += (char)(ch - 32); //32 is the difference between the lower case and the upper case, meaning that (char)('a' - 32) --> 'A'.
                        lastCopiedIndex = i;
                    }
                    makeUpper = false;
                }
                else
                {
                    makeUpper = true;
                }
            }
            //we copy the rest of the string:
            if (lastCopiedIndex < length - 1)
            {
                result += str.Substring(lastCopiedIndex + 1);
            }
            return result;
        }
    }
}
#endif
