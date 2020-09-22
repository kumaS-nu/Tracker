# Tracker

Unity向けのWebカメラ・動画ファイルからフェイストラッキング・全身トラッキングを行うアセット。中に何か変なのも入っているけど勝手に使ってもよい。

[booth](https://kumas.booth.pm/items/1657599)で配布、販売しています。

# 導入

このアセットを使用するだけならboothの方にunitypackageとしてまとまってるのでそちらを使用した方が楽。

コントリビュートとかこれをそのまま入れたいときは、unityのAssetフォルダで、

```shell
git init
git remote add origin https://github.com/kumachan0210/Tracker.git
git pull origin master
```

としてください。最初はクローズな環境を想定してリポジトリを作っていたので、公開用のこっちがその一つ下のフォルダで作るしかなかったためこんな変な構成になっています。

# 依存ライブラリ

[shimat/opencvsharp](https://github.com/shimat/opencvsharp)

[takuya-takeuchi/DlibDotNet](https://github.com/takuya-takeuchi/DlibDotNet)


# コントリビュート

* Issueを立てる。（Issueが立ってなかったり、pull requestと関連付けられていなかったら見ません。）
* forkし、pull requestを出す。（マージ先はdevelopで。）
* 私が見てよかったらマージする。

# ライセンス

私が作った分のライセンスは MIT License です。
