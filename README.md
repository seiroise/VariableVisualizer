![](https://github.com/seiroise/VariableVisualizer/blob/media/gif/vv.gif)

## 内容
- Unity上でシリアライズされている変数(一部の型)の値をグラフに表示するエディタウィンドウ
- float, int, Vector2, Vector3, Vector4, Quaternion, Colorなどに対応

## 使い方
1. Window/Seiro/SerializedPropertyWindowを開く
2. Scene上(またはプレハブ)のGameObjectを設定する
3. コンポーネントと変数を選択する
4. 後は眺めるだけ

## 今後追加したい機能
- 過去の記録と重ね合わせる
- 特定の条件下の時に記録する
- リフレクションでゲッターセッターなどの値も取れるようにする

## 参考
- https://gist.github.com/asus4/b0d43fb119cdf5d3b7825251cadbdd4a
- http://baba-s.hatenablog.com/entry/2015/05/15/162823
- http://baba-s.hatenablog.com/entry/2015/05/21/112930
- https://forum.unity.com/threads/display-array-in-custom-editor-window.380871/

## 動機(使う人は読まなくてもいいやつ)
ゲームのデバッグめんどいから(特に物理エンジン周り)、
パラメータ可視化したらいいんじゃない？そうだやったろ！

と思ったんだけど肝心のRigidbody系は
シリアライズされているパラメータにそこまで価値がないことがわかったので、ちょっと残念。
でもそれがわかっただけでもよし。ということで
