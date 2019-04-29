﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Documents;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Metadata
{
    [TestFixture]
    [NonParallelizable]
    public class FileNameFixture : BaseFixture
    {
        public class ExecuteTests : FileNameFixture
        {
            [TestCase(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~:/?#[]@!$&'()*+,;=",
                "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz0123456789")]
            [TestCase("Děku.jemeविकीвики-движка", "děku.jemeविकीвикидвижка")]
            [TestCase(
                "this is my title - and some \t\t\t\t\n   clever; (piece) of text here: [ok].",
                "this-is-my-title-and-some-clever-piece-of-text-here-ok")]
            [TestCase(
                "this is my title?!! /r/science/ and #firstworldproblems :* :sadface=true",
                "this-is-my-title-rscience-and-firstworldproblems-sadfacetrue")]
            [TestCase(
                "one-two-three--four--five and a six--seven--eight-nine------ten",
                "onetwothreefourfive-and-a-sixseveneightnineten")]
            public async Task FileNameIsConvertedCorrectly(string input, string output)
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, pipeline, serviceProvider);
                IDocument[] inputs =
                {
                    context.GetDocument(new MetadataItems
                {
                    new MetadataItem(Keys.SourceFileName, input)
                })
                };
                FileName fileName = new FileName();

                // When
                IEnumerable<IDocument> documents = await fileName.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                Assert.AreEqual(output, documents.First().FilePath(Keys.WriteFileName).FullPath);
            }

            [Test]
            public async Task FileNameShouldBeLowercase()
            {
                // Given
                const string input = "FileName With MiXeD CapS";
                const string output = "filename-with-mixed-caps";

                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, pipeline, serviceProvider);
                IDocument[] inputs =
                {
                    context.GetDocument(new MetadataItems
                {
                    new MetadataItem(Keys.SourceFileName, new FilePath(input))
                })
                };
                FileName fileName = new FileName();

                // When
                IEnumerable<IDocument> documents = await fileName.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                Assert.AreEqual(output, documents.First().FilePath(Keys.WriteFileName).FullPath);
            }

            [Test]
            public async Task WithAllowedCharactersDoesNotReplaceProvidedCharacters()
            {
                // Given
                const string input = "this-is-a-.net-tag";
                const string output = "this-is-a-.net-tag";

                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, pipeline, serviceProvider);
                IDocument[] inputs =
                {
                    context.GetDocument(new MetadataItems
                {
                    new MetadataItem(Keys.SourceFileName, new FilePath(input))
                })
                };
                FileName fileName = new FileName();

                // When
                fileName = fileName.WithAllowedCharacters(new string[] { "-" });
                IEnumerable<IDocument> documents = await fileName.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                Assert.AreEqual(output, documents.First().FilePath(Keys.WriteFileName).FullPath);
            }

            [Test]
            public async Task WithAllowedCharactersDoesNotReplaceDotAtEnd()
            {
                // Given
                const string input = "this-is-a-.";
                const string output = "thisisa.";

                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, pipeline, serviceProvider);
                IDocument[] inputs =
                {
                    context.GetDocument(new MetadataItems
                {
                    new MetadataItem(Keys.SourceFileName, new FilePath(input))
                })
                };
                FileName fileName = new FileName();

                // When
                fileName = fileName.WithAllowedCharacters(new string[] { "." });
                IEnumerable<IDocument> documents = await fileName.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                Assert.AreEqual(output, documents.First().FilePath(Keys.WriteFileName).FullPath);
            }

            public static string[] ReservedChars => FileName.ReservedChars;

            [Test]
            [TestCaseSource(nameof(ReservedChars))]
            public async Task FileNameIsConvertedCorrectlyWithReservedChar(string character)
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, pipeline, serviceProvider);
                string manyCharactersWow = new string(character[0], 10);
                IDocument[] inputs =
                {
                    context.GetDocument(new MetadataItems
                {
                    new MetadataItem(
                        Keys.SourceFileName,
                        string.Format("testing {0} some of {0} these {0}", manyCharactersWow))
                })
                };
                FileName fileName = new FileName();

                // When
                IEnumerable<IDocument> documents = await fileName.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                Assert.AreEqual("testing-some-of-these-", documents.First().FilePath(Keys.WriteFileName).FullPath);
            }

            [TestCase(null)]
            [TestCase("")]
            [TestCase(" ")]
            public async Task IgnoresNullOrWhiteSpaceStrings(string input)
            {
                // Given
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, pipeline, serviceProvider);
                IDocument[] inputs =
                {
                    context.GetDocument(new MetadataItems
                {
                    new MetadataItem(Keys.SourceFileName, input)
                })
                };
                FileName fileName = new FileName();

                // When
                IEnumerable<IDocument> documents = await fileName.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                Assert.IsFalse(documents.First().ContainsKey(Keys.WriteFileName));
            }

            [Test]
            public async Task PreservesExtension()
            {
                // Given
                const string input = "myfile.html";
                const string output = "myfile.html";

                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, pipeline, serviceProvider);
                IDocument[] inputs =
                {
                    context.GetDocument(new MetadataItems
                    {
                        new MetadataItem("MyKey", input)
                    })
                };
                FileName fileName = new FileName("MyKey");

                // When
                IEnumerable<IDocument> documents = await fileName.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                Assert.AreEqual(output, documents.First().FilePath(Keys.WriteFileName).FullPath);
            }

            [Test]
            public async Task TrimWhitespace()
            {
                // Given
                const string input = "   myfile.html   ";
                const string output = "myfile.html";

                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                Pipeline pipeline = new Pipeline("Pipeline", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, pipeline, serviceProvider);
                IDocument[] inputs =
                {
                    context.GetDocument(new MetadataItems
                    {
                        new MetadataItem("MyKey", input)
                    })
                };
                FileName fileName = new FileName("MyKey");

                // When
                IEnumerable<IDocument> documents = await fileName.ExecuteAsync(inputs, context).ToListAsync();

                // Then
                Assert.AreEqual(output, documents.First().FilePath(Keys.WriteFileName).FullPath);
            }
        }
    }
}
