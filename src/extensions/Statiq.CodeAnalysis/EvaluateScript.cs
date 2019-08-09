﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.CodeAnalysis.Scripting;
using Statiq.Common;

namespace Statiq.CodeAnalysis
{
    /// <summary>
    /// Evaluates a C# based script contained in document content.
    /// </summary>
    /// <category>Extensibility</category>
    public class EvaluateScript : ParallelModule
    {
        protected override async IAsyncEnumerable<IDocument> ExecuteAsync(IDocument input, IExecutionContext context)
        {
            // Get the assembly
            byte[] assembly = input.Bool(CompileScript.CompiledKey)
                ? await input.GetBytesAsync()
                : ScriptHelper.Compile(await input.GetStringAsync(), input, context);

            // Evaluate the script
            object value = await ScriptHelper.EvaluateAsync(assembly, input, context);
            await foreach (IDocument result in context.CloneOrCreateDocuments(input, value))
            {
                yield return result;
            }
        }
    }
}
