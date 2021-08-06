// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace AdtExplorer.Functions.Sdk.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Defines headers for GetById operation.
    /// </summary>
    public partial class DigitalTwinsGetByIdHeaders
    {
        /// <summary>
        /// Initializes a new instance of the DigitalTwinsGetByIdHeaders class.
        /// </summary>
        public DigitalTwinsGetByIdHeaders()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the DigitalTwinsGetByIdHeaders class.
        /// </summary>
        /// <param name="eTag">Weak Etag.</param>
        public DigitalTwinsGetByIdHeaders(string eTag = default(string))
        {
            ETag = eTag;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets weak Etag.
        /// </summary>
        [JsonProperty(PropertyName = "ETag")]
        public string ETag { get; set; }

    }
}
