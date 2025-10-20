# dotnettest

## サイクロマティック複雑度アナライザー

このリポジトリには、レガシーな VB.NET (.NET Framework 3.5 世代) のコードベースを対象とした
サイクロマティック複雑度の計測ユーティリティが含まれています。最新のツールは .NET API
(`Microsoft.CodeAnalysis.VisualBasic`) を用いたコンソールアプリケーションで、VB (`.vb`)
ファイルの構文木を解析してメソッド/アクセサー単位の複雑度をレポートします。

Python 製の `tools/complexity_analyzer.py` も引き続き同梱されており、ASP.NET Web Forms
(`.aspx`、`.ascx`、`.master`) や JavaScript (`.js`) を含む混在プロジェクトをヒューリスティックに
解析したい場合に利用できます。

### 使い方

#### .NET コンソールアプリケーション

1. .NET 6 SDK がインストールされていることを確認します。
2. リポジトリをクローンし、作業ディレクトリを移動します。

    ```bash
    git clone https://github.com/your-org/dotnettest.git
    cd dotnettest
    ```

3. 複雑度を計測したいプロジェクトのルートパスを指定してアプリケーションを実行します。

    ```bash
    dotnet run --project tools/VbCyclomaticAnalyzer -- <path-to-project-root>
    ```

4. `--format json` オプションで JSON 形式、`--threshold <number>` で閾値アラートを有効化できます。

#### Python スクリプト

1. Python 3.9 以降がインストールされていることを確認します。
2. `tools/complexity_analyzer.py` を使用すると VB に加えて ASP.NET/JavaScript を解析できます。

    ```bash
    python tools/complexity_analyzer.py <path-to-project-root>
    ```

3. JSON 形式で取得したい場合は `--format json` オプションを指定します。

    ```bash
    python tools/complexity_analyzer.py <path-to-project-root> --format json > report.json
    ```

   JSON 出力はファイルにリダイレクトすることで後から再利用しやすくなります。

Python スクリプトは完全なパーサーではなく、実践的なヒューリスティックに基づいています。
ASP.NET ファイル内の `<script runat="server">` ブロックに含まれる VB や JavaScript の
インラインコードにも対応しています。

### サンプルデータ

`samples/` ディレクトリには、各主要拡張子ごとに 2 つずつ用意したサンプルファイルが格納されています。
ツールの挙動を確認する際に活用してください。

### チュートリアル出力

`tutorials/complexity_report.md` には、サンプルデータに対してツールを実行した際のコマンドと
出力例をチュートリアル形式で保存しています。
