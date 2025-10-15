#!/usr/bin/env python3
"""Cyclomatic complexity analyzer for VB.NET, ASPX, and JavaScript files.

This tool recursively scans a directory for source files and calculates the
cyclomatic complexity of each method/function that it finds. The implementation
focuses on .NET Framework 3.5 era VB.NET projects, but it also has pragmatic
support for inline VB code contained in ASPX files as well as plain JavaScript
files.

The output can be rendered either as human readable text (default) or JSON.
"""
from __future__ import annotations

import argparse
import json
import os
import re
import sys
from dataclasses import dataclass, field
from typing import Dict, Iterable, List, Optional, Sequence


@dataclass
class MethodComplexity:
    """Represents cyclomatic complexity information for a single method."""

    name: str
    complexity: int
    start_line: int
    end_line: int
    metadata: Dict[str, str] = field(default_factory=dict)


@dataclass
class FileReport:
    """Stores complexity results for a single source file."""

    path: str
    language: str
    methods: List[MethodComplexity]

    @property
    def total_complexity(self) -> int:
        return sum(method.complexity for method in self.methods)


class VBAnalyzer:
    """Heuristic cyclomatic complexity analyzer for VB.NET code."""

    _method_pattern = re.compile(
        r"^\s*(?:Public|Private|Protected|Friend|Shared|Overridable|Overrides|MustOverride|Partial|Static|Default|ReadOnly|WriteOnly|Sealed|New|NotInheritable|Optional|ByVal|ByRef|Async|Iterator|Overloads|MustInherit|Dim|Const|Shadows|Narrowing|Widening|\s)*"
        r"\b(Function|Sub|Property|Get|Set)\s+([A-Za-z0-9_]+)",
        re.IGNORECASE,
    )
    _end_pattern = re.compile(r"^\s*End\s+(Function|Sub|Property|Get|Set)\b", re.IGNORECASE)

    _decision_patterns = [
        re.compile(pattern, re.IGNORECASE)
        for pattern in (
            r"\bElse\s+If\b",
            r"\bIf\b",
            r"\bFor\s+Each\b",
            r"\bFor\b",
            r"\bDo\s+(?:While|Until)\b",
            r"\bLoop\s+(?:While|Until)\b",
            r"\bWhile\b",
            r"\bSelect\s+Case\b",
            r"\bCase\b",
            r"\bCatch\b",
            r"\bAndAlso\b",
            r"\bOrElse\b",
        )
    ]

    def analyze_lines(
        self, lines: Sequence[str], *, offset: int = 0, metadata: Optional[Dict[str, str]] = None
    ) -> List[MethodComplexity]:
        methods: List[MethodComplexity] = []
        current: Optional[MethodComplexity] = None

        for idx, line in enumerate(lines, start=1):
            logical_line = self._strip_comments(line)

            if current is None:
                match = self._method_pattern.search(logical_line)
                if match:
                    current = MethodComplexity(
                        name=match.group(2),
                        complexity=1,
                        start_line=offset + idx,
                        end_line=offset + idx,
                        metadata=dict(metadata or {}),
                    )
                continue

            current.complexity += self._count_decisions(logical_line)
            current.end_line = offset + idx

            if self._end_pattern.search(logical_line):
                methods.append(current)
                current = None

        if current is not None:
            current.end_line = offset + len(lines)
            methods.append(current)

        return methods

    @staticmethod
    def _strip_comments(line: str) -> str:
        comment_index = line.find("'")
        if comment_index >= 0:
            return line[:comment_index]
        return line

    def _count_decisions(self, line: str) -> int:
        count = 0
        for pattern in self._decision_patterns:
            if pattern.pattern == r"\bWhile\b" and re.search(r"\bDo\s+While\b", line, re.IGNORECASE):
                continue
            if pattern.pattern == r"\bCase\b":
                select_case_count = len(re.findall(r"\bSelect\s+Case\b", line, re.IGNORECASE))
                occurrences = len(pattern.findall(line)) - select_case_count
            else:
                occurrences = len(pattern.findall(line))
            count += max(occurrences, 0)
        return count


class JavaScriptAnalyzer:
    """Cyclomatic complexity analyzer for JavaScript files."""

    _function_pattern = re.compile(r"\bfunction\s*([A-Za-z0-9_$]*)\s*\(")
    _decision_keywords = {
        "else_if": re.compile(r"\belse\s+if\b", re.IGNORECASE),
        "if": re.compile(r"\bif\b", re.IGNORECASE),
        "for": re.compile(r"\bfor\b", re.IGNORECASE),
        "while": re.compile(r"\bwhile\b", re.IGNORECASE),
        "switch": re.compile(r"\bswitch\b", re.IGNORECASE),
        "case": re.compile(r"\bcase\b", re.IGNORECASE),
        "catch": re.compile(r"\bcatch\b", re.IGNORECASE),
    }

    def analyze_lines(
        self, lines: Sequence[str], *, offset: int = 0, metadata: Optional[Dict[str, str]] = None
    ) -> List[MethodComplexity]:
        methods: List[MethodComplexity] = []
        current: Optional[MethodComplexity] = None
        brace_depth = 0
        body_started = False
        in_block_comment = False

        for idx, raw_line in enumerate(lines, start=1):
            line, in_block_comment = self._strip_comments(raw_line, in_block_comment)
            if not line and not body_started and current is None:
                continue

            if current is None:
                match = self._function_pattern.search(line)
                if match:
                    name = match.group(1) or "<anonymous>"
                    current = MethodComplexity(
                        name=name,
                        complexity=1,
                        start_line=offset + idx,
                        end_line=offset + idx,
                        metadata=dict(metadata or {}),
                    )
                    brace_depth = self._brace_delta(line[match.end() :])
                    body_started = brace_depth > 0
                    current.complexity += self._count_decisions(line[match.end() :])
                continue

            if not body_started:
                brace_delta = self._brace_delta(line)
                if brace_delta > 0:
                    body_started = True
                    brace_depth += brace_delta
                    current.complexity += self._count_decisions(line)
                continue

            current.complexity += self._count_decisions(line)
            brace_depth += self._brace_delta(line)
            current.end_line = offset + idx

            if brace_depth <= 0:
                methods.append(current)
                current = None
                brace_depth = 0
                body_started = False

        if current is not None:
            current.end_line = offset + len(lines)
            methods.append(current)

        return methods

    @staticmethod
    def _brace_delta(line: str) -> int:
        return line.count("{") - line.count("}")

    @staticmethod
    def _strip_comments(line: str, in_block_comment: bool) -> tuple[str, bool]:
        result = []
        i = 0
        length = len(line)
        while i < length:
            if in_block_comment:
                end = line.find("*/", i)
                if end == -1:
                    return "", True
                i = end + 2
                in_block_comment = False
                continue
            if line.startswith("//", i):
                break
            if line.startswith("/*", i):
                in_block_comment = True
                i += 2
                continue
            result.append(line[i])
            i += 1
        return "".join(result), in_block_comment

    def _count_decisions(self, line: str) -> int:
        count = 0
        else_if_matches = len(self._decision_keywords["else_if"].findall(line))
        count += else_if_matches
        if_matches = len(self._decision_keywords["if"].findall(line)) - else_if_matches
        count += max(if_matches, 0)
        for keyword in ("for", "while", "switch", "case", "catch"):
            count += len(self._decision_keywords[keyword].findall(line))
        count += line.count("&&") + line.count("||") + line.count("?")
        return count



class ASPXAnalyzer:
    """Extracts and analyzes inline script blocks within ASPX files."""

    def __init__(self, vb_analyzer: VBAnalyzer, js_analyzer: JavaScriptAnalyzer) -> None:
        self._vb_analyzer = vb_analyzer
        self._js_analyzer = js_analyzer

    def analyze(self, content: str) -> List[MethodComplexity]:
        methods: List[MethodComplexity] = []
        lower_content = content.lower()
        position = 0
        closing_tag = '</script>'

        while True:
            start = lower_content.find('<script', position)
            if start == -1:
                break
            tag_close = lower_content.find('>', start)
            if tag_close == -1:
                break
            tag_text = content[start : tag_close + 1]
            if 'runat="server"' not in tag_text.lower():
                position = tag_close + 1
                continue
            end = lower_content.find(closing_tag, tag_close + 1)
            if end == -1:
                break
            block = content[tag_close + 1 : end]
            language = self._detect_language(tag_text)
            block_offset = content[: tag_close + 1].count('\n')
            lines = block.splitlines()
            metadata = {"container": "aspx-script", "language": language}
            if language == 'vb':
                methods.extend(
                    self._vb_analyzer.analyze_lines(lines, offset=block_offset + 1, metadata=metadata)
                )
            elif language in {'js', 'javascript', 'ecmascript'}:
                methods.extend(
                    self._js_analyzer.analyze_lines(lines, offset=block_offset + 1, metadata=metadata)
                )
            position = end + len(closing_tag)

        return methods

    @staticmethod
    def _detect_language(tag_text: str) -> str:
        lower_tag = tag_text.lower()
        marker = 'language='
        index = lower_tag.find(marker)
        if index == -1:
            return 'vb'
        quote_start = lower_tag.find('"', index + len(marker))
        single_start = lower_tag.find("'", index + len(marker))
        if quote_start == -1 or (single_start != -1 and single_start < quote_start):
            quote_start = single_start
            quote_char = "'"
        else:
            quote_char = '"'
        if quote_start == -1:
            return 'vb'
        quote_end = lower_tag.find(quote_char, quote_start + 1)
        if quote_end == -1:
            return 'vb'
        return tag_text[quote_start + 1 : quote_end].strip().lower()



def detect_language_from_extension(path: str) -> Optional[str]:
    extension = os.path.splitext(path)[1].lower()
    if extension == ".vb":
        return "vb"
    if extension == ".js":
        return "js"
    if extension in {".aspx", ".ascx", ".master"}:
        return "aspx"
    return None


def iter_source_files(root: str) -> Iterable[str]:
    for dirpath, _, filenames in os.walk(root):
        for name in filenames:
            path = os.path.join(dirpath, name)
            if detect_language_from_extension(path):
                yield path


def analyze_path(root: str) -> List[FileReport]:
    vb_analyzer = VBAnalyzer()
    js_analyzer = JavaScriptAnalyzer()
    aspx_analyzer = ASPXAnalyzer(vb_analyzer, js_analyzer)
    reports: List[FileReport] = []

    for path in iter_source_files(root):
        language = detect_language_from_extension(path)
        if language is None:
            continue
        with open(path, "r", encoding="utf-8", errors="ignore") as handle:
            content = handle.read()
        if language == "vb":
            methods = vb_analyzer.analyze_lines(content.splitlines(), metadata={"language": "vb"})
        elif language == "js":
            methods = js_analyzer.analyze_lines(content.splitlines(), metadata={"language": "js"})
        else:
            methods = aspx_analyzer.analyze(content)
        reports.append(FileReport(path=path, language=language, methods=methods))

    return reports


def render_text(reports: List[FileReport]) -> str:
    lines: List[str] = []
    for report in reports:
        lines.append(f"File: {report.path} ({report.language})")
        if not report.methods:
            lines.append("  (no methods found)")
            continue
        for method in report.methods:
            metadata = " ".join(f"{key}={value}" for key, value in sorted(method.metadata.items()))
            lines.append(
                f"  {method.name}: complexity={method.complexity} lines={method.start_line}-{method.end_line}"
                + (f" [{metadata}]" if metadata else "")
            )
        lines.append(f"  Total complexity: {report.total_complexity}")
    return "\n".join(lines)


def main(argv: Optional[Sequence[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="Measure cyclomatic complexity for VB.NET-era projects.")
    parser.add_argument("root", nargs="?", default=".", help="Root directory to scan")
    parser.add_argument("--format", choices={"text", "json"}, default="text", help="Output format")
    args = parser.parse_args(argv)

    reports = analyze_path(args.root)

    if args.format == "json":
        payload = [
            {
                "path": report.path,
                "language": report.language,
                "total_complexity": report.total_complexity,
                "methods": [
                    {
                        "name": method.name,
                        "complexity": method.complexity,
                        "start_line": method.start_line,
                        "end_line": method.end_line,
                        "metadata": method.metadata,
                    }
                    for method in report.methods
                ],
            }
            for report in reports
        ]
        json.dump(payload, fp=sys.stdout, indent=2, ensure_ascii=False)
        sys.stdout.write("\n")
    else:
        print(render_text(reports))

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
