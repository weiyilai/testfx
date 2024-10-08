﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Testing.Extensions.Diagnostics.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ExtensionResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ExtensionResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Testing.Extensions.Diagnostics.Resources.ExtensionResources", typeof(ExtensionResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Creating dump file &apos;{0}&apos;.
        /// </summary>
        internal static string CreatingDumpFile {
            get {
                return ResourceManager.GetString("CreatingDumpFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The hang dump file.
        /// </summary>
        internal static string HangDumpArtifactDescription {
            get {
                return ResourceManager.GetString("HangDumpArtifactDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hang dump file.
        /// </summary>
        internal static string HangDumpArtifactDisplayName {
            get {
                return ResourceManager.GetString("HangDumpArtifactDisplayName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Environment variable &apos;{0}&apos; is set to &apos;{1}&apos; instead of &apos;{2}&apos;.
        /// </summary>
        internal static string HangDumpEnvironmentVariableInvalidValueErrorMessage {
            get {
                return ResourceManager.GetString("HangDumpEnvironmentVariableInvalidValueErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Environment variable &apos;{0}&apos; is not set.
        /// </summary>
        internal static string HangDumpEnvironmentVariableIsMissingErrorMessage {
            get {
                return ResourceManager.GetString("HangDumpEnvironmentVariableIsMissingErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Produce hang dump files when a test execution exceed a given time..
        /// </summary>
        internal static string HangDumpExtensionDescription {
            get {
                return ResourceManager.GetString("HangDumpExtensionDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hang dump.
        /// </summary>
        internal static string HangDumpExtensionDisplayName {
            get {
                return ResourceManager.GetString("HangDumpExtensionDisplayName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specify the name of the dump file.
        /// </summary>
        internal static string HangDumpFileNameOptionDescription {
            get {
                return ResourceManager.GetString("HangDumpFileNameOptionDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generate a dump file if the test process hangs.
        /// </summary>
        internal static string HangDumpOptionDescription {
            get {
                return ResourceManager.GetString("HangDumpOptionDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hang dump timeout of &apos;{0}&apos; expired.
        /// </summary>
        internal static string HangDumpTimeoutExpired {
            get {
                return ResourceManager.GetString("HangDumpTimeoutExpired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specify the timeout after which the dump will be generated.
        ///The timeout value is specified in one of the following formats:
        ///    1.5h, 1.5hour, 1.5hours,
        ///    90m, 90min, 90minute, 90minutes,
        ///    5400s, 5400sec, 5400second, 5400seconds.
        ///    Default is 30m..
        /// </summary>
        internal static string HangDumpTimeoutOptionDescription {
            get {
                return ResourceManager.GetString("HangDumpTimeoutOptionDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;--hangdump-timeout&apos; expects a single timeout argument.
        /// </summary>
        internal static string HangDumpTimeoutOptionInvalidArgument {
            get {
                return ResourceManager.GetString("HangDumpTimeoutOptionInvalidArgument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specify the type of the dump.
        ///Valid values are &apos;Mini&apos;, &apos;Heap&apos;, &apos;Triage&apos; (only available in .NET 6+) or &apos;Full&apos;.
        ///Default type is &apos;Full&apos;.
        /// </summary>
        internal static string HangDumpTypeOptionDescription {
            get {
                return ResourceManager.GetString("HangDumpTypeOptionDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; is not a valid dump type.
        ///Valid options are &apos;Mini&apos;, &apos;Heap&apos;, &apos;Triage&apos; (only available in .NET 6+) and &apos;Full&apos;.
        /// </summary>
        internal static string HangDumpTypeOptionInvalidType {
            get {
                return ResourceManager.GetString("HangDumpTypeOptionInvalidType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Request of type &apos;{0}&apos; is not supported.
        /// </summary>
        internal static string HangDumpUnsupportedRequestTypeErrorMessage {
            get {
                return ResourceManager.GetString("HangDumpUnsupportedRequestTypeErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The list of tests that were running at the time of the hang.
        /// </summary>
        internal static string HangTestListArtifactDescription {
            get {
                return ResourceManager.GetString("HangTestListArtifactDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hang test list.
        /// </summary>
        internal static string HangTestListArtifactDisplayName {
            get {
                return ResourceManager.GetString("HangTestListArtifactDisplayName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You specified one or more hang dump parameters but did not enable it, add --hangdump to the command line.
        /// </summary>
        internal static string MissingHangDumpMainOption {
            get {
                return ResourceManager.GetString("MissingHangDumpMainOption", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot find mutex &apos;{0}&apos;.
        /// </summary>
        internal static string MutexDoesNotExistErrorMessage {
            get {
                return ResourceManager.GetString("MutexDoesNotExistErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mutex name wasn&apos;t received after &apos;{0}&apos; seconds.
        /// </summary>
        internal static string MutexNameReceptionTimeoutErrorMessage {
            get {
                return ResourceManager.GetString("MutexNameReceptionTimeoutErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The following tests were still running when dump was taken (format: [&lt;time-elapsed-since-start&gt;] &lt;name&gt;):.
        /// </summary>
        internal static string RunningTestsWhileDumping {
            get {
                return ResourceManager.GetString("RunningTestsWhileDumping", resourceCulture);
            }
        }
    }
}
