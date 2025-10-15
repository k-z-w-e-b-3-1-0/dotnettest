# dotnettest

## Cyclomatic complexity analyzer

This repository contains a simple utility for measuring the cyclomatic
complexity of legacy VB.NET (.NET Framework 3.5 era) code bases. The script can
scan VB (`.vb`), ASP.NET Web Forms (`.aspx`, `.ascx`, `.master`) and JavaScript
(`.js`) files and reports complexity information per method.

### Usage

```bash
python tools/complexity_analyzer.py <path-to-project-root>
```

By default the tool prints a text report. Pass `--format json` to receive a
JSON payload instead.

The analyzer uses pragmatic heuristics and is not a full parser. Nested
`<script runat="server">` blocks inside ASP.NET files are supported for inline
VB and JavaScript code.
