namespace VSSonarPlugins
{
    using System;

    /// <summary>
    /// The vs sonar extension.
    /// </summary>
    public class VSSonarExtension : Exception
    {
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class ResourceNotSupportedException : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Resource is not supported by any of the installed plugins";
            }
        }        
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class NoFileInViewException : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "No File in View, please open a file in Editor";
            }
        }
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class NoPluginInstalledException : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "No plugin is Installed";
            }
        }
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class MultiLanguageExceptionNotSupported : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Multi Language Is Not Supported in this Version";
            }
        }
    }

    /// <summary>
    /// The plugin exception.
    /// </summary>
    public class ProjectNotAssociatedException : VSSonarExtension
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public override string Message
        {
            get
            {
                return "Project Not Associated";
            }
        }
    }
}