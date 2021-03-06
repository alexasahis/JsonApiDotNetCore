using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Responsible for converting resources into <see cref="ResourceObject"/>s
    /// given a list of attributes and relationships.
    /// </summary> 
    public interface IResourceObjectBuilder
    {
        /// <summary>
        /// Converts <paramref name="resource"/> into a <see cref="ResourceObject"/>.
        /// Adds the attributes and relationships that are enlisted in <paramref name="attributes"/> and <paramref name="relationships"/>
        /// </summary>
        /// <param name="resource">Resource to build a Resource Object for</param>
        /// <param name="attributes">Attributes to include in the building process</param>
        /// <param name="relationships">Relationships to include in the building process</param>
        /// <returns>The resource object that was built</returns>
        ResourceObject Build(IIdentifiable resource, IEnumerable<AttrAttribute> attributes, IEnumerable<RelationshipAttribute> relationships);
    }
}
