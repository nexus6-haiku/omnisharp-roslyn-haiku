using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Models;
using OmniSharp.Models.Diagnostics;

namespace OmniSharp.LanguageServerProtocol
{
    public static class Helpers
    {
        public static Diagnostic ToDiagnostic(this DiagnosticLocation location)
        {
            return new Diagnostic()
            {
                // We don't have a code at the moment
                // Code = quickFix.,
                Message = location.Text,
                Range = location.ToRange(),
                Severity = ToDiagnosticSeverity(location.LogLevel),
                Code = location.Id,
                // TODO: We need to forward this type though if we add something like Vb Support
                Source = "csharp",
            };
        }

        public static Range ToRange(this QuickFix location)
        {
            return new Range()
            {
                Start = new Position()
                {
                    Character = location.Column,
                    Line = location.Line
                },
                End = new Position()
                {
                    Character = location.EndColumn,
                    Line = location.EndLine
                },
            };
        }

        public static DiagnosticSeverity ToDiagnosticSeverity(string logLevel)
        {
            // We stringify this value and pass to clients
            // should probably use the enum at somepoint
            if (Enum.TryParse<Microsoft.CodeAnalysis.DiagnosticSeverity>(logLevel, out var severity))
            {
                switch (severity)
                {
                    case Microsoft.CodeAnalysis.DiagnosticSeverity.Error:
                        return DiagnosticSeverity.Error;
                    case Microsoft.CodeAnalysis.DiagnosticSeverity.Hidden:
                        return DiagnosticSeverity.Hint;
                    case Microsoft.CodeAnalysis.DiagnosticSeverity.Info:
                        return DiagnosticSeverity.Information;
                    case Microsoft.CodeAnalysis.DiagnosticSeverity.Warning:
                        return DiagnosticSeverity.Warning;
                }
            }

            return DiagnosticSeverity.Information;
        }

        public static Uri ToUri(string fileName)
        {
            fileName = fileName.Replace(":", "%3A").Replace("\\", "/");
            if (!fileName.StartsWith("/")) return new Uri($"file:///{fileName}");
            return new Uri($"file://{fileName}");
        }

        public static string FromUri(Uri uri)
        {
            if (uri.Segments.Length > 1)
            {
                // On windows of the Uri contains %3a local path
                // doesn't come out as a proper windows path
                if (uri.Segments[1].IndexOf("%3a", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return FromUri(new Uri(uri.AbsoluteUri.Replace("%3a", ":").Replace("%3A", ":")));
                }
            }
            return uri.LocalPath;
        }

        public static Range ToRange((int column, int line) location)
        {
            return new Range()
            {
                Start = ToPosition(location),
                End = ToPosition(location)
            };
        }

        public static Position ToPosition((int column, int line) location)
        {
            return new Position(location.line, location.column);
        }

        public static Position ToPosition(OmniSharp.Models.V2.Point point)
        {
            return new Position(point.Line, point.Column);
        }

        public static Range ToRange((int column, int line) start, (int column, int line) end)
        {
            return new Range()
            {
                Start = new Position(start.line, start.column),
                End = new Position(end.line, end.column)
            };
        }

        public static Range ToRange(OmniSharp.Models.V2.Range range)
        {
            return new Range()
            {
                Start = ToPosition(range.Start),
                End = ToPosition(range.End)
            };
        }

        public static string EscapeMarkdown(string markdown)
        {
            if (markdown == null)
                return null;
            return Regex.Replace(markdown, @"([\\`\*_\{\}\[\]\(\)#+\-\.!])", @"\$1");
        }

        private static readonly IDictionary<string, SymbolKind> Kinds = new Dictionary<string, SymbolKind>
        {
            { OmniSharp.Models.V2.SymbolKinds.Class, SymbolKind.Class },
            { OmniSharp.Models.V2.SymbolKinds.Delegate, SymbolKind.Class },
            { OmniSharp.Models.V2.SymbolKinds.Enum, SymbolKind.Enum },
            { OmniSharp.Models.V2.SymbolKinds.Interface, SymbolKind.Interface },
            { OmniSharp.Models.V2.SymbolKinds.Struct, SymbolKind.Struct },
            { OmniSharp.Models.V2.SymbolKinds.Constant, SymbolKind.Constant },
            { OmniSharp.Models.V2.SymbolKinds.Destructor, SymbolKind.Method },
            { OmniSharp.Models.V2.SymbolKinds.EnumMember, SymbolKind.EnumMember },
            { OmniSharp.Models.V2.SymbolKinds.Event, SymbolKind.Event },
            { OmniSharp.Models.V2.SymbolKinds.Field, SymbolKind.Field },
            { OmniSharp.Models.V2.SymbolKinds.Indexer, SymbolKind.Property },
            { OmniSharp.Models.V2.SymbolKinds.Method, SymbolKind.Method },
            { OmniSharp.Models.V2.SymbolKinds.Operator, SymbolKind.Operator },
            { OmniSharp.Models.V2.SymbolKinds.Property, SymbolKind.Property },
            { OmniSharp.Models.V2.SymbolKinds.Namespace, SymbolKind.Namespace },
            { OmniSharp.Models.V2.SymbolKinds.Unknown, SymbolKind.Class },
        };

        public static SymbolKind ToSymbolKind(string omnisharpKind)
        {
            return Kinds.TryGetValue(omnisharpKind.ToLowerInvariant(), out var symbolKind) ? symbolKind : SymbolKind.Class;
        }
    }
}
