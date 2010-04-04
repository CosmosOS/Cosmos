QEMU on Windows

　QEMU は、多種のCPUをエミュレートするプログラムです。Windows版は
アルファバージョン（開発初期のバージョン）です。

　ダブルハイフン"--"は、必要なくなりました。すべてのオプションにシングルハイフンを
使って下さい。

1.インストール
　zipファイルを展開してください。インストーラーを使う必要はありません。

2. qemu の実行
　実行するのには、２つ方法があります。

 2.1 バッチファイルを使う方法
　qemu-win.batをダブルクリックすると、デスクトップ上にlinuxが起動します。

コマンドプロンプト（MS-DOSプロンプト）上では次のようにします。

	qemu.exe -L . -hda linux.img

　-L が、biosの位置を、-hda がハードディスクのイメージファイルを
指定するオプションです。

　マウスカーソルが消えたときは、CtrlとAltキーを同時に押してください。WindowsMeでは、
AltとTabを使ってください。

　linuxを終了するには、Ctrl-Alt-2のキーを同時に押します。(qemu)プロンプトがでたら、
quitとタイプします。

        (qemu) quit

 2.2 ショートカットを使う方法
　qemu.exeのショートカットを作ります。右クリックしてプロパティを表示し、リンク先
に、-L . -hda linux.img を付け足します。ショートカットをダブルクリックすれば
実行されます。

3. 確認
　同梱されているlinux.imgには、nbenchというベンチマークが含まれています。
起動するには、次のようにします。

	sh-2.05b# cd nbench
	sh-2.05b# ./nbench

　INTEGER INDEX と FLOATING-POINT INDEX が Pentium 90MHz との比較を表します。

4. x86_64のエミュレーション

　qemu-x86_64.batをダブルクリックしても、デスクトップにLinuxが起動します。
３２ビットと６４ビットのOSを動かすことができます。

5. ハードディスクイメージ
　qemu-img.exeを使って、ハードディスクのイメージファイルを作ることができます。
１０Mバイトのハードディスクイメージを作るには、次のようにします。

　qemu-img.exe create harddisk.img 10M

6. フロッピーとCD-ROM
　QEMUモニターを使って、フロッピーやCD-ROMイメージを替えることができます。
　QEMUモニターを表示するには、Ctrl,Alt,2のキーを同時に押してください。Ctrl, Alt, 1
を同時に押すと、ゲストOSの画面にもどります。

　フロッピーとCD-ROMを使うには、イメージファイルに変換する必要があります。

　フロッピーのイメージ化には、DiskExploreなど好みのものを使ってください。

　CD-ROMは、iso形式のイメージに変換する必要があります。CD-Rライティングソフトがあれば、
それが使えると思います。なければ、cdrtoolsというフリーのソフトウェアもあります。

　これらを同時に使うには、次のようにします。

　qemu.exe -L . -m 128 -boot a -fda floppy.img -hda harddisk.img -cdrom cdimage.iso

  -L : bios の位置
  -m : メモリサイズ（Mバイト）
  -boot : 起動するデバイス　フロッピー(a)、ハードディスク(c)、CD-ROM(d)
  -fda : フロッピーイメージ
  -hda : ハードディスクイメージ
  -cdrom : CD-ROMイメージ

フロッピーとCD-ROMイメージを取り替えたいときは、QEMUモニターで次のようにします。
(qemu) change fda filename.img
もしくは、
(qemu) change cdrom cdimage.iso

7. アンインストール
　展開したフォルダを削除してください。インストーラを使った場合、レジストリを使用しています。


8. 注意
　１つのハードディスクイメージで、２つＱＥＭＵを同時に動かさないでください。ディスク
イメージが壊れます。

9. ライセンス
　仮想CPUコアライブラリおよびPCシステムエミュレータはLGPLライセンスとなっています。
Licenseフォルダにあるファイルをご覧ください。
　なお、本プログラムの使用による問題について保証は出来かねますのでご了承ください。

10. リンク
  QEMU
	http://fabrice.bellard.free.fr/qemu/
  Bochs BIOS
	http://bochs.sourceforge.net/ 
  VGA BIOS
	http://www.nongnu.org/vgabios/
  MinGW
	http://www.mingw.org/
  SDL Library
	http://www.libsdl.org/

kazu