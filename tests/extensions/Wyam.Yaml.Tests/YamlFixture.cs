﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Util;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using Wyam.Yaml.Dynamic;

namespace Wyam.Yaml.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class YamlFixture : BaseFixture
    {
        public class ExecuteTests : YamlFixture
        {
            [Test]
            public async Task SetsMetadataKey()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument("A: 1");
                Yaml yaml = new Yaml("MyYaml");

                // When
                IList<IDocument> documents = await yaml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(1);
                documents[0].Keys.ShouldBe(new[] { "MyYaml" }, true);
            }

            [Test]
            public async Task GeneratesDynamicObject()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(@"
A: 1
B: true
C: Yes
");
                Yaml yaml = new Yaml("MyYaml");

                // When
                IList<IDocument> documents = await yaml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(1);
                documents[0].Keys.ShouldBe(new[] { "MyYaml" }, true);
                documents[0]["MyYaml"].ShouldBeOfType<DynamicYaml>();
                ((int)((dynamic)documents[0]["MyYaml"]).A).ShouldBe(1);
                ((bool)((dynamic)documents[0]["MyYaml"]).B).ShouldBe(true);
                ((string)((dynamic)documents[0]["MyYaml"]).C).ShouldBe("Yes");
            }

            [Test]
            public async Task FlattensTopLevelScalarNodes()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(@"
A: 1
B: true
C: Yes
");
                Yaml yaml = new Yaml();

                // When
                IList<IDocument> documents = await yaml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(1);
                documents[0].Keys.ShouldBe(new[] { "A", "B", "C" }, true);
                documents[0]["A"].ShouldBe("1");
                documents[0]["B"].ShouldBe("true");
                documents[0]["C"].ShouldBe("Yes");
            }

            [Test]
            public async Task GeneratesDynamicObjectAndFlattens()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(@"
A: 1
B: true
C: Yes
");
                Yaml yaml = new Yaml("MyYaml", true);

                // When
                IList<IDocument> documents = await yaml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(1);
                documents[0].Keys.ShouldBe(new[] { "MyYaml", "A", "B", "C" }, true);
                documents[0]["MyYaml"].ShouldBeOfType<DynamicYaml>();
                ((int)((dynamic)documents[0]["MyYaml"]).A).ShouldBe(1);
                ((bool)((dynamic)documents[0]["MyYaml"]).B).ShouldBe(true);
                ((string)((dynamic)documents[0]["MyYaml"]).C).ShouldBe("Yes");
                documents[0]["A"].ShouldBe("1");
                documents[0]["B"].ShouldBe("true");
                documents[0]["C"].ShouldBe("Yes");
            }

            [Test]
            public async Task ReturnsDocumentIfEmptyInputAndFlatten()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(@"
");
                Yaml yaml = new Yaml();

                // When
                IList<IDocument> documents = await yaml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(1);
                documents[0].Keys.ShouldBeEmpty();
            }

            [Test]
            public async Task EmptyReturnIfEmptyInputAndNotFlatten()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(@"
");
                Yaml yaml = new Yaml("Foo");

                // When
                IList<IDocument> documents = await yaml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(0);
            }

            [Test]
            public async Task UsesDocumentNestingForComplexChildren()
            {
                // Given
                IExecutionContext context = new TestExecutionContext();
                IDocument document = new TestDocument(@"
C:
  - X: 1
    Y: 2
  - X: 4
    Z: 5
");
                Yaml yaml = new Yaml();

                // When
                IList<IDocument> documents = await yaml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                documents.Count.ShouldBe(1);
                documents[0].Keys.ShouldBe(new[] { "C" }, true);
                documents[0]["C"].ShouldBeOfType<IDocument[]>();
                IDocument[] subDocuments = (IDocument[])documents[0]["C"];
                subDocuments.Length.ShouldBe(2);
                subDocuments[0].Keys.ShouldBe(new[] { "X", "Y" }, true);
                subDocuments[0]["X"].ShouldBe("1");
                subDocuments[0]["Y"].ShouldBe("2");
                subDocuments[1].Keys.ShouldBe(new[] { "X", "Z" }, true);
                subDocuments[1]["X"].ShouldBe("4");
                subDocuments[1]["Z"].ShouldBe("5");
            }
        }
    }
}