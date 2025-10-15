# dotnettest

## サイクロマティック複雑度アナライザー

このリポジトリには、レガシーな VB.NET (.NET Framework 3.5 世代) のコードベースを対象とした
サイクロマティック複雑度の計測ユーティリティが含まれています。スクリプトは VB (`.vb`)、
ASP.NET Web Forms (`.aspx`、`.ascx`、`.master`)、JavaScript (`.js`) ファイルを走査し、
メソッド単位の複雑度をレポートします。

### 使い方

1. Python 3.9 以降がインストールされていることを確認します。
2. リポジトリをクローンし、作業ディレクトリを移動します。

    ```bash
    git clone https://github.com/your-org/dotnettest.git
    cd dotnettest
    ```

3. 複雑度を計測したいプロジェクトのルートパスを指定してスクリプトを実行します。

    ```bash
    python tools/complexity_analyzer.py <path-to-project-root>
    ```

4. 既定ではテキスト形式のレポートが表示されます。JSON 形式で取得したい場合は `--format json`
   オプションを指定します。

    ```bash
    python tools/complexity_analyzer.py <path-to-project-root> --format json > report.json
    ```

   JSON 出力はファイルにリダイレクトすることで後から再利用しやすくなります。

アナライザーは完全なパーサーではなく、実践的なヒューリスティックに基づいています。
ASP.NET ファイル内の `<script runat="server">` ブロックに含まれる VB や JavaScript の
インラインコードにも対応しています。

### サンプルデータ

`samples/` ディレクトリには、各主要拡張子ごとに 2 つずつ用意したサンプルファイルが格納されています。
ツールの挙動を確認する際に活用してください。

### チュートリアル出力

`tutorials/complexity_report.md` には、サンプルデータに対してツールを実行した際のコマンドと
出力例をチュートリアル形式で保存しています。
