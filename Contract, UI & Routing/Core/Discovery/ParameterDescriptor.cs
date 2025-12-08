namespace SoftwareCenter.Core.Discovery
{
    /// <summary>
    /// Describes a single parameter for a capability (e.g., a parameter of a command's constructor).
    /// </summary>
    public class ParameterDescriptor
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the full name of the parameter's .NET type.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the description of the parameter, typically sourced from XML documentation.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDescriptor"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="typeName">The full name of the parameter's .NET type.</param>
        /// <param name="description">The description of the parameter.</param>
        public ParameterDescriptor(string name, string typeName, string description = "")
        {
            Name = name;
            TypeName = typeName;
            Description = description;
        }
    }
}
