# サンプルプロジェクトでのレポート出力例

このチュートリアルでは、リポジトリに含まれている `samples/` ディレクトリに対して
サイクロマティック複雑度アナライザーを実行し、テキストレポートを取得する手順を紹介します。

## 1. コマンドの実行

以下のコマンドを実行して、`samples/` 配下の VB、ASP.NET、JavaScript ファイルを解析します。

```bash
python tools/complexity_analyzer.py samples
```

## 2. 出力結果

コマンド実行後に表示される出力の例を以下に示します。

```text
File: samples/master/Admin.master (aspx)
  Page_Load: complexity=3 lines=20-25 [container=aspx-script language=vb]
  Total complexity: 3
File: samples/master/Site.master (aspx)
  Page_Load: complexity=5 lines=20-27 [container=aspx-script language=vb]
  Total complexity: 5
File: samples/aspx/Reports.aspx (aspx)
  GetStatusColor: complexity=6 lines=18-29 [container=aspx-script language=vb]
  Total complexity: 6
File: samples/aspx/Default.aspx (aspx)
  BtnSubmit_Click: complexity=3 lines=23-34 [container=aspx-script language=vb]
  Total complexity: 3
File: samples/vb/OrderProcessor.vb (vb)
  CalculateDiscount: complexity=8 lines=2-22 [language=vb]
  NotifyCustomer: complexity=4 lines=24-32 [language=vb]
  SendEmail: complexity=1 lines=34-35 [language=vb]
  SendStandardEmail: complexity=1 lines=37-38 [language=vb]
  LogMissingEmail: complexity=1 lines=40-41 [language=vb]
  Total complexity: 15
File: samples/vb/Calculator.vb (vb)
  Add: complexity=1 lines=2-4 [language=vb]
  Max: complexity=5 lines=6-15 [language=vb]
  Fibonacci: complexity=4 lines=17-29 [language=vb]
  Total complexity: 10
File: samples/js/app.js (js)
  initializeDashboard: complexity=4 lines=1-12 [language=js]
  renderWidget: complexity=4 lines=14-25 [language=js]
  Total complexity: 8
File: samples/js/utils.js (js)
  filterActiveItems: complexity=4 lines=1-9 [language=js]
  computeScore: complexity=4 lines=11-24 [language=js]
  Total complexity: 8
File: samples/ascx/Header.ascx (aspx)
  Page_Load: complexity=3 lines=15-19 [container=aspx-script language=vb]
  Total complexity: 3
File: samples/ascx/Footer.ascx (aspx)
  Page_Load: complexity=3 lines=7-14 [container=aspx-script language=vb]
  Total complexity: 3
```

## 3. 次のステップ

- `--format json` オプションを指定すると、同じ解析結果を JSON 形式で取得できます。
- サンプルファイルを編集したり、新しいファイルを追加して、複雑度計測の変化を確認してみてください。
