using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace VbCyclomaticAnalyzer;

internal static class Program
{
    private static int Main(string[] args)
    {
        var parseResult = CommandLine.Parse(args);
        if (parseResult.ShowHelp)
        {
            CommandLine.WriteUsage();
            if (!string.IsNullOrEmpty(parseResult.Error))
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine(parseResult.Error);
                return 1;
            }

            return 0;
        }

        var options = parseResult.Options!;
        if (!Directory.Exists(options.Root))
        {
            Console.Error.WriteLine($"error: directory not found: {options.Root}");
            return 1;
        }

        var analyzer = new ProjectAnalyzer();
        var reports = analyzer.Analyze(options.Root);

        if (options.Format == OutputFormat.Json)
        {
            WriteJson(reports, options.Threshold);
        }
        else
        {
            WriteText(reports, options.Threshold);
        }

        return 0;
    }

    private static void WriteText(IReadOnlyList<FileReport> reports, int? threshold)
    {
        var alerts = new List<string>();
        foreach (var report in reports)
        {
            Console.WriteLine($"File: {report.Path}");
            if (report.Methods.Count == 0)
            {
                Console.WriteLine("  (no methods found)");
                Console.WriteLine();
                continue;
            }

            foreach (var method in report.Methods)
            {
                var alert = threshold.HasValue && method.Complexity >= threshold.Value;
                if (alert)
                {
                    alerts.Add($"{report.Path}::{method.Name} (complexity {method.Complexity})");
                }

                var line =
                    $"  {method.Name}: complexity={method.Complexity} lines={method.StartLine}-{method.EndLine}";
                if (alert)
                {
                    line += $"  <-- ALERT: complexity exceeds threshold ({threshold})";
                }

                Console.WriteLine(line);
            }

            Console.WriteLine($"  Total complexity: {report.TotalComplexity}");
            Console.WriteLine();
        }

        if (!threshold.HasValue)
        {
            return;
        }

        Console.WriteLine($"Alert summary (threshold={threshold}):");
        if (alerts.Count == 0)
        {
            Console.WriteLine("  No methods exceeded the complexity threshold.");
        }
        else
        {
            foreach (var alert in alerts)
            {
                Console.WriteLine($"  {alert}");
            }
        }
    }

    private static void WriteJson(IReadOnlyList<FileReport> reports, int? threshold)
    {
        var payload = reports.Select(report => new
        {
            path = report.Path,
            totalComplexity = report.TotalComplexity,
            methods = report.Methods.Select(method => new
            {
                name = method.Name,
                complexity = method.Complexity,
                startLine = method.StartLine,
                endLine = method.EndLine,
                exceedsThreshold = threshold.HasValue && method.Complexity >= threshold.Value,
            }),
        });

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        Console.WriteLine(JsonSerializer.Serialize(payload, options));
    }
}

internal enum OutputFormat
{
    Text,
    Json,
}

internal sealed record AnalyzerOptions(string Root, OutputFormat Format, int? Threshold);

internal readonly record struct ParseResult(AnalyzerOptions? Options, bool ShowHelp, string? Error);

internal static class CommandLine
{
    public static ParseResult Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            return new ParseResult(new AnalyzerOptions(".", OutputFormat.Text, null), false, null);
        }

        if (args.Any(static arg => arg is "-h" or "--help"))
        {
            return new ParseResult(null, true, null);
        }

        string? root = null;
        var format = OutputFormat.Text;
        int? threshold = null;

        for (var index = 0; index < args.Count; index++)
        {
            var arg = args[index];
            switch (arg)
            {
                case "--format":
                    if (index + 1 >= args.Count)
                    {
                        return new ParseResult(null, true, "error: --format requires an argument (text or json)");
                    }

                    index++;
                    var formatValue = args[index].ToLowerInvariant();
                    format = formatValue switch
                    {
                        "text" => OutputFormat.Text,
                        "json" => OutputFormat.Json,
                        _ => OutputFormat.Text,
                    };

                    if (formatValue is not ("text" or "json"))
                    {
                        return new ParseResult(
                            null,
                            true,
                            $"error: unsupported format '{args[index]}'. Use 'text' or 'json'.");
                    }

                    break;
                case "--threshold":
                    if (index + 1 >= args.Count)
                    {
                        return new ParseResult(null, true, "error: --threshold requires an integer value.");
                    }

                    index++;
                    if (!int.TryParse(args[index], out var parsedThreshold) || parsedThreshold < 0)
                    {
                        return new ParseResult(null, true, "error: --threshold requires a non-negative integer.");
                    }

                    threshold = parsedThreshold;
                    break;
                default:
                    if (arg.StartsWith("--", StringComparison.Ordinal))
                    {
                        return new ParseResult(null, true, $"error: unknown option '{arg}'.");
                    }

                    if (root is not null)
                    {
                        return new ParseResult(null, true, "error: multiple root paths specified.");
                    }

                    root = arg;
                    break;
            }
        }

        return new ParseResult(new AnalyzerOptions(root ?? ".", format, threshold), false, null);
    }

    public static void WriteUsage()
    {
        Console.WriteLine("VB.NET cyclomatic complexity analyzer");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project tools/VbCyclomaticAnalyzer -- [options] [root]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --format <text|json>     Output format (default: text)");
        Console.WriteLine("  --threshold <number>     Highlight methods whose complexity >= number");
        Console.WriteLine("  -h, --help               Show this message");
    }
}

internal sealed record MethodReport(string Name, int Complexity, int StartLine, int EndLine);

internal sealed record FileReport(string Path, IReadOnlyList<MethodReport> Methods)
{
    public int TotalComplexity => Methods.Sum(method => method.Complexity);
}

internal sealed class ProjectAnalyzer
{
    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        ".git",
        "node_modules",
    };

    public IReadOnlyList<FileReport> Analyze(string root)
    {
        var results = new List<FileReport>();
        var rootPath = Path.GetFullPath(root);

        foreach (var file in EnumerateVbFiles(rootPath))
        {
            var relativePath = Path.GetRelativePath(rootPath, file);
            var methods = AnalyzeFile(file);
            results.Add(new FileReport(relativePath, methods));
        }

        return results;
    }

    private static IEnumerable<string> EnumerateVbFiles(string root)
    {
        var directories = new Stack<string>();
        directories.Push(root);

        while (directories.Count > 0)
        {
            var current = directories.Pop();
            foreach (var directory in Directory.EnumerateDirectories(current))
            {
                var name = Path.GetFileName(directory);
                if (IgnoredDirectories.Contains(name))
                {
                    continue;
                }

                directories.Push(directory);
            }

            foreach (var file in Directory.EnumerateFiles(current, "*.vb"))
            {
                yield return file;
            }
        }
    }

    private static IReadOnlyList<MethodReport> AnalyzeFile(string path)
    {
        var source = File.ReadAllText(path);
        var tree = VisualBasicSyntaxTree.ParseText(source);
        var root = tree.GetCompilationUnitRoot();
        var collector = new MethodCollector(tree);
        collector.Visit(root);
        return collector.Methods;
    }
}

internal sealed class MethodCollector : VisualBasicSyntaxWalker
{
    private readonly SyntaxTree _tree;
    private readonly List<MethodReport> _methods = new();

    public MethodCollector(SyntaxTree tree)
        : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        _tree = tree;
    }

    public IReadOnlyList<MethodReport> Methods => _methods;

    public override void VisitMethodBlock(MethodBlockSyntax node)
    {
        var name = node.SubOrFunctionStatement.Identifier.Text;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = node.SubOrFunctionStatement.SubOrFunctionStatementKeyword.Text;
        }

        AddMethod(name, node);
        base.VisitMethodBlock(node);
    }

    public override void VisitConstructorBlock(ConstructorBlockSyntax node)
    {
        var name = "New";
        if (node.SubNewStatement?.ParameterList is { } parameters)
        {
            name += parameters.ToString();
        }

        AddMethod(name, node);
        base.VisitConstructorBlock(node);
    }

    public override void VisitAccessorBlock(AccessorBlockSyntax node)
    {
        var accessorName = node.AccessorStatement.Kind() switch
        {
            SyntaxKind.GetAccessorStatement => "Get",
            SyntaxKind.SetAccessorStatement => "Set",
            SyntaxKind.AddHandlerAccessorStatement => "AddHandler",
            SyntaxKind.RemoveHandlerAccessorStatement => "RemoveHandler",
            SyntaxKind.RaiseEventAccessorStatement => "RaiseEvent",
            _ => node.AccessorStatement.Keyword.Text,
        };

        var propertyBlock = node.Ancestors().OfType<PropertyBlockSyntax>().FirstOrDefault();
        var propertyName = propertyBlock?.PropertyStatement.Identifier.Text ?? "<accessor>";
        AddMethod($"{propertyName}.{accessorName}", node);
        base.VisitAccessorBlock(node);
    }

    private void AddMethod(string name, SyntaxNode node)
    {
        var walker = new CyclomaticComplexityWalker();
        walker.Visit(node);
        var span = _tree.GetLineSpan(node.Span);
        _methods.Add(
            new MethodReport(
                name,
                walker.Complexity,
                span.StartLinePosition.Line + 1,
                span.EndLinePosition.Line + 1));
    }
}

internal sealed class CyclomaticComplexityWalker : VisualBasicSyntaxWalker
{
    public int Complexity { get; private set; } = 1;

    public CyclomaticComplexityWalker()
        : base(SyntaxWalkerDepth.StructuredTrivia)
    {
    }

    public override void Visit(SyntaxNode? node)
    {
        if (node is null)
        {
            return;
        }

        switch (node.Kind())
        {
            case SyntaxKind.MultiLineIfBlock:
            case SyntaxKind.SingleLineIfStatement:
                Complexity++;
                break;
            case SyntaxKind.ElseIfBlock:
                Complexity++;
                break;
            case SyntaxKind.ForBlock:
            case SyntaxKind.ForEachBlock:
            case SyntaxKind.WhileBlock:
            case SyntaxKind.DoLoopBlock:
            case SyntaxKind.DoWhileLoopBlock:
            case SyntaxKind.DoUntilLoopBlock:
            case SyntaxKind.DoLoopWhileBlock:
            case SyntaxKind.DoLoopUntilBlock:
                Complexity++;
                break;
            case SyntaxKind.SelectBlock:
                Complexity++;
                break;
            case SyntaxKind.CaseBlock:
                Complexity++;
                break;
            case SyntaxKind.CaseElseBlock:
                break;
            case SyntaxKind.CatchBlock:
                Complexity++;
                break;
            case SyntaxKind.BinaryConditionalExpression:
                Complexity++;
                break;
        }

        base.Visit(node);
    }

    public override void VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        if (node.IsKind(SyntaxKind.AndAlsoExpression) || node.IsKind(SyntaxKind.OrElseExpression))
        {
            Complexity++;
        }

        base.VisitBinaryExpression(node);
    }
}
