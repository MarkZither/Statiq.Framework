﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Modules;
using Wyam.Common.Util;

namespace Wyam.CodeAnalysis.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class AnalyzeCSharpSyntaxFixture : AnalyzeCSharpBaseFixture
    {
        public class ExecuteTests : AnalyzeCSharpSyntaxFixture
        {
            [Test]
            public async Task ImplicitClassAccessibility()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal class Green");
            }

            [Test]
            public async Task ImplicitMemberAccessibility()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            void Blue()
                            {
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe("private void Blue()");
            }

            [Test]
            public async Task ExplicitClassAccessibility()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("public class Green");
            }

            [Test]
            public async Task ExplicitMemberAccessibility()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            internal void Blue()
                            {
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe("internal void Blue()");
            }

            [Test]
            public async Task ClassAttributes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        [  Foo]
                        [Bar ,Foo ]
                        class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ToString().ShouldBe(
                    @"[Foo]
[Bar, Foo]
internal class Green",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task MethodAttributes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            [  Foo]
                            [Bar  ]
                            int Blue()
                            {
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ToString().ShouldBe(
                    @"[Foo]
[Bar]
private int Blue()",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ClassComments()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        // asfd
                        [  Foo /* asdf */]
                        [Bar( /* asdf */ 5)  ] // asdf
                        /* asfd */
                        class /* asfd */ Green // asdf 
                            /* asdf */ : Blue  // asfd
                        {
                            // asdf
                        }

                        class Blue
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ToString().ShouldBe(
                    @"[Foo]
[Bar(5)]
internal class Green : Blue",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AbstractClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        abstract class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal abstract class Green");
            }

            [Test]
            public async Task SealedClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        sealed class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal sealed class Green");
            }

            [Test]
            public async Task StaticClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        static class Green
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal static class Green");
            }

            [Test]
            public async Task StaticMethod()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            static void Blue()
                            {
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe("private static void Blue()");
            }

            [Test]
            public async Task ClassWithGenericTypeParameters()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green<out TKey, TValue>
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal class Green<out TKey, TValue>");
            }

            [Test]
            public async Task ClassWithGenericTypeParametersAndConstraints()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green<out TKey, TValue> where TValue : class
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal class Green<out TKey, TValue> where TValue : class");
            }

            [Test]
            public async Task ClassWithGenericTypeParametersAndBaseAndConstraints()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green<out TKey, TValue> : Blue, 


                            IFoo 

                    where TValue : class
                        {
                        }

                        class Blue
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal class Green<out TKey, TValue> : Blue, IFoo where TValue : class");
            }

            [Test]
            public async Task MethodWithGenericParameters()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag)
                            {
                                return value;
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe("public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag)");
            }

            [Test]
            public async Task MethodWithGenericParametersAndConstraints()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag) where TKey : class
                            {
                                return value;
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe("public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag) where TKey : class");
            }

            [Test]
            public async Task Enum()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        enum Green
                        {
                            Foo = 3,
                            Bar = 5
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal enum Green");
            }

            [Test]
            public async Task EnumWithBase()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        enum Green : long
                        {
                            Foo = 3,
                            Bar = 5
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal enum Green");
            }

            [Test]
            public async Task ExplicitProperty()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public int Blue
			                {
				                get { return 1 ; }
			                }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe("public int Blue { get; }");
            }

            [Test]
            public async Task AutoProperty()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public int Blue { get; set; }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ShouldBe("public int Blue { get; set; }");
            }

            [Test]
            public async Task WrapsForLongMethodSignature()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag, int something, int somethingElse, int anotherThing) where TKey : class
                            {
                                return value;
                            }
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetMember(results, "Green", "Blue")["Syntax"].ToString().ShouldBe(
                    @"public TValue Blue<TKey, TValue>(TKey key, TValue value, bool flag, int something, int somethingElse, int anotherThing) 
    where TKey : class",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task WrapsForLongClassSignature()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green<TKey, TValue> : IReallyLongInterface, INameToForceWrapping, IFoo, IBar, IFooBar, ICircle, ISquare, IRectangle where TKey : class
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ToString().ShouldBe(
                    @"internal class Green<TKey, TValue> : IReallyLongInterface, INameToForceWrapping, IFoo, IBar, 
    IFooBar, ICircle, ISquare, IRectangle
    where TKey : class",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ClassWithInterfaces()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green : IFoo, IBar, IFooBar
                        {
                        }
                    }
                ";
                IDocument document = GetDocument(code);
                IExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                List<IDocument> results = await module.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                GetResult(results, "Green")["Syntax"].ShouldBe("internal class Green : IFoo, IBar, IFooBar");
            }
        }
    }
}