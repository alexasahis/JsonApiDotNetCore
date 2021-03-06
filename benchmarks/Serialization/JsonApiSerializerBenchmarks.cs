using System;
using BenchmarkDotNet.Attributes;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.QueryStrings;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.RequestServices;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Server;
using JsonApiDotNetCore.Serialization.Server.Builders;
using Moq;

namespace Benchmarks.Serialization
{
    [MarkdownExporter]
    public class JsonApiSerializerBenchmarks
    {
        private static readonly BenchmarkResource _content = new BenchmarkResource
        {
            Id = 123,
            Name = Guid.NewGuid().ToString()
        };

        private readonly IJsonApiSerializer _jsonApiSerializer;

        public JsonApiSerializerBenchmarks()
        {
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = DependencyFactory.CreateResourceGraph(options);
            IFieldsToSerialize fieldsToSerialize = CreateFieldsToSerialize(resourceGraph);

            var metaBuilderMock = new Mock<IMetaBuilder<BenchmarkResource>>();
            var linkBuilderMock = new Mock<ILinkBuilder>();
            var includeBuilderMock = new Mock<IIncludedResourceObjectBuilder>();

            var resourceObjectBuilder = new ResourceObjectBuilder(resourceGraph, new ResourceObjectBuilderSettings());

            _jsonApiSerializer = new ResponseSerializer<BenchmarkResource>(metaBuilderMock.Object, linkBuilderMock.Object,
                includeBuilderMock.Object, fieldsToSerialize, resourceObjectBuilder, options);
        }

        private static FieldsToSerialize CreateFieldsToSerialize(IResourceGraph resourceGraph)
        {
            var currentRequest = new CurrentRequest();

            var constraintProviders = new IQueryConstraintProvider[]
            {
                new SparseFieldSetQueryStringParameterReader(currentRequest, resourceGraph)
            };

            var resourceDefinitionProvider = DependencyFactory.CreateResourceDefinitionProvider(resourceGraph);

            return new FieldsToSerialize(resourceGraph, constraintProviders, resourceDefinitionProvider);
        }

        [Benchmark]
        public object SerializeSimpleObject() => _jsonApiSerializer.Serialize(_content);
    }
}
