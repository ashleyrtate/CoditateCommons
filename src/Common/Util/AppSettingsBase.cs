using System;
using System.Collections.Specialized;

namespace Coditate.Common.Util
{
    /// <summary>
    /// Based class for creating custom wrappers around .NET application settings.
    /// </summary>
    public abstract class AppSettingsBase
    {
        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public NameValueCollection Settings { get; protected set; }

        /// <summary>
        /// Gets a typed setting value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The setting key.</param>
        /// <returns></returns>
        public virtual T Get<T>(string key)
        {
            string valueStr = Settings[key];
            if (string.IsNullOrEmpty(valueStr))
            {
                string message = string.Format("No setting value stored for key '{0}'", key);
                throw new InvalidOperationException(message);
            }
            object value;
            if (typeof(T) == typeof(Guid))
            {
                value = new Guid(valueStr);
            }
            else
            {
                value = Convert.ChangeType(valueStr, typeof (T));
            }
            return (T) value;
        }

        /// <summary>
        /// Sets the specified setting.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public virtual void Set(string key, object value)
        {
            Settings[key] = value.ToString();
        }
    }
}