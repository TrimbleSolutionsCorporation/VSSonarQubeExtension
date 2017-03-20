using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSonarPlugins.Types
{
    /// <summary>
    /// Field value in settings
    /// </summary>
    public class FieldValue
    {
        public FieldValue()
        {
            this.Values = new List<Tuple<string, string>>();
        }

        /// <summary>
        /// Gets or sets the field values
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public List<Tuple<string, string>> Values { get; set; } 
    }

    /// <summary>
    /// Setting class
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Setting"/> class.
        /// </summary>
        public Setting()
        {
            this.FieldValues = new List<FieldValue>();
            this.Values = new List<string>();
        }

        /// <summary>
        /// Gets or sets the field values.
        /// </summary>
        /// <value>
        /// The field values.
        /// </value>
        public List<FieldValue> FieldValues { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public List<string> Values { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Setting"/> is inherited.
        /// </summary>
        /// <value>
        ///   <c>true</c> if inherited; otherwise, <c>false</c>.
        /// </value>
        public bool Inherited { get; set; }
    }
}
