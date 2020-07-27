﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FmodAudioSourceGenerator
{
    [Generator]
    public class PrimarySourceGenerator : ISourceGenerator
    {
        internal static readonly DiagnosticDescriptor FASG02 = new DiagnosticDescriptor(nameof(FASG02), "Ref Unsupported", "ref, out, and in parameters and return type are unsupported", "Source Generator", DiagnosticSeverity.Error, true);
        internal static readonly DiagnosticDescriptor FASG03 = new DiagnosticDescriptor(nameof(FASG03), "Reference Type Parameter found", "Reference type marshalling is not supported", "Source Generator", DiagnosticSeverity.Error, true);
        internal static readonly DiagnosticDescriptor FASG04 = new DiagnosticDescriptor(nameof(FASG04), "Unmanaged Structures only", "Structs that contain references are not supported for marshalling", "", DiagnosticSeverity.Error, true);

        public void Initialize(InitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(SourceGeneratorContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver reciever))
                return;

            var state = new VTableCreationState(context, reciever);

            if (context.CancellationToken.IsCancellationRequested)
                return;

            state.GenerateSources();
        }
    }
}
