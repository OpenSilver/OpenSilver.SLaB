﻿#region Using Directives

using System.Reflection;
using System.Windows.Markup;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

#if !OPENSILVER
[assembly: AssemblyTitle("SLaB.Utilities")]
#endif
#if WINDOWS_PHONE
#else
[assembly: XmlnsDefinition(Constants.XmlNamespace, "SLaB.Utilities")]
#endif