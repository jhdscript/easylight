   M S B A M L   `   `   `  ����   	EasyLight    EasyLight.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Z VWPFToolkit.Extended, Version=1.8.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f45exttk)http://schemas.xceed.com/wpf/xaml/toolkit    � Title$  
PixelLight��$	��400��$	��400��$	��600��$	��600�� � 
ResizeMode$ CanMinimize=� � Icon$ logo.ico�� � 	Resources    NotifierContextMenu m�Name m� Icon��%>  &	        �� @�m� $ MenuMaximize��-   $��Maximize EasyLight�� �� $��Images/logo.png��$��16��$��16��m� $ MenuWebsite��-   $��Go to EasyLight WebSite�� �� $��Images/internet.png��$��16��$��16��m� $ MenuExit��-   $
��Exit�� �� $��Images/exit.png��$��16��$��16��.���  � RowDefinitions �� $��* ��� $��5 ��� $
��auto ��� $
��auto �.+��� -    ��Name$ TabControlMain��$��0��.@��� -    ��Name$ TabItemCapture��$��Dynamic Mode Settings��.���  �� $
��auto ��� $��5 ��� $��* �.+�� -   	 �Name$	 GridCaptureSettings��$��0��
 � ColumnDefinitions
 �� $
��auto ��� $��3 ��� $��* ��� $��10 ��� $
��auto ��� $��3 ��� $��* ��� $��10 ��� $
��auto ��� $��3 ��� $��* � �� $
��auto ��� $��5 ��� $
��auto �.+��� -    ��Name$ LblProportional��$��0��$��0��$��Right=�$��Center=�$��Proportional���� -    ��Name$ CboProportional��$��2��$��0��@��� -	    ��Name$ CbiProportional1��$��1���� -
   $ CbiProportional2��$��2���� -   $ CbiProportional3��$��3���� -   $ CbiProportional4��$��4���� -   $ CbiProportional5��$��5�� �� 
IsSelected . �� -   $ CbiProportional6��$��6���� -   $ CbiProportional7��$��7���� -   $ CbiProportional8��$��8���� -   $ CbiProportional9��$��9���� -   $ CbiProportional10��$��10���� -   $ LblLayer��$��4��$��0��$��Right=�$��Center=�$��Layers (L x H)���� -   $ CboLayer��$��6��$��0��@��� -   $ CbiLayer4x2��$��4 x 2�� . �� -   $ CbiLayer2x2��$��2 x 2���� -   $ CbiLayer3x2��$��3 x 2���� -   $ CbiLayer3x1��$��3 x 1���� -   $ 	LblMethod��$��8��$��0��$��Right=�$��Center=�$��Method���� -   $ 	CboMethod��$��10��$��0��@��� -   $ CbiMethodMax��$	��Max���� -   $ CbiMethodAverage��$��Average�� . �� -   $ LblHscan��$��0��$��2��$��Right=�$��Center=�$��Hscan���� -   $ CboHscan��$��2��$��2��@��� -   $ 
CbiHscan10��$
��10 %���� -    $ 
CbiHscan20��$
��20 %���� -!   $ 
CbiHscan30��$
��30 %�� . �� -"   $ 
CbiHscan40��$
��40 %���� -#   $ 
CbiHscan50��$
��50 %���� -$   $ 
CbiHscan60��$
��60 %���� -%   $ 
CbiHscan70��$
��70 %���� -&   $ 
CbiHscan80��$
��80 %���� -'   $ 
CbiHscan90��$
��90 %���� -(   $ CbiHscan100��$��100 %���� -)   $ LblVscan��$��4��$��2��$��Right=�$��Center=�$��Vscan���� -*   $ CboVscan��$��6��$��2��@��� -+   $ 
CbiVscan10��$
��10 %���� -,   $ 
CbiVscan20��$
��20 %���� --   $ 
CbiVscan30��$
��30 %�� . �� -.   $ 
CbiVscan40��$
��40 %���� -/   $ 
CbiVscan50��$
��50 %���� -0   $ 
CbiVscan60��$
��60 %���� -1   $ 
CbiVscan70��$
��70 %���� -2   $ 
CbiVscan80��$
��80 %���� -3   $ 
CbiVscan90��$
��90 %���� -4   $ CbiVscan100��$��100 %���� -5    ��Name$ ChkSkipDarkPixels��$��10��$��2��$��Center=�$��Skip Dark Pixels��� -6   $	 GridDraw��$��2��$��Center=�$��Center=�$	��250��$	��200��
��������� -7   $ TabItemStaticMode��$��Static Mode Settings��.��� 
 �� $
��auto ��� $��* ��� $
��auto � �� $
��auto ��� $��* ��� $
��auto �.+�#  Xceed.Wpf.Toolkit.ColorPicker  -8     Name$ ColorPickerTop��$��0��$��1��  -9   $ ColorPickerBottom��$��2��$��1��  -:   $ ColorPickerLeft��$��1��$��0��$��Stretch=�$��Stretch=� � LayoutTransform ��  �� CenterX$ 0Q� �� CenterY$ 0Q� �� Angle$	 -90Q�  -;   $ ColorPickerRight��$��1��$��2��$��Stretch=�$��Stretch=� �� $ 0Q�$ 0Q�$	 -90Q��� -<   $ TabItemArduino��$��Arduino Settings��.���  �� $
��auto ��� $��5 ��� $��* �.+�� -=   $	 GridArduinoSettings��$��0��
 �� $
��auto ��� $��3 ��� $��* ��� $��10 ��� $
��auto ��� $��3 ��� $��* ��� $��10 ��� $
��auto ��� $��3 ��� $��* � �� $
��auto ��� $��5 ��� $
��auto �.+��� ->   $ 
LblPortCom��$��0��$��0��$��Right=�$��Center=�$��Port COM���� -?   $ 
CboPortCom��$��2��$��0���� -@   $ LblBaudRate��$��4��$��0��$��Right=�$��Center=�$��	Baud Rate���� -A   $ CboBaudRate��$��6��$��0��@��� -B   $ CbiBaudRate300��$	��300���� -C   $ CbiBaudRate1200��$
��1200���� -D   $ CbiBaudRate2400��$
��2400���� -E   $ CbiBaudRate4800��$
��4800���� -F   $ CbiBaudRate9600��$
��9600���� -G   $ CbiBaudRate14400��$��14400���� -H   $ CbiBaudRate19200��$��19200���� -I   $ CbiBaudRate28800��$��28800���� -J   $ CbiBaudRate38400��$��38400���� -K   $ CbiBaudRate57600��$��57600���� -L   $ CbiBaudRate115200��$��115200�� . �� -M   $ LblMaxRetries��$��8��$��0��$��Right=�$��Center=�$��Max Retries���� -N   $ CboMaxRetries��$��10��$��0��@��� -O   $ CbiMaxRetries30��$��30�� . �� -P   $ CbiMaxRetries50��$��50���� -Q   $ CbiMaxRetries100��$	��100���� -R   $ CbiMaxRetries250��$	��250���� -S   $ CbiMaxRetries500��$	��500���� -T    ��Name$ ListBoxArduinoLogs��$��2�� � ContextMenu �� .@�m� -U   $ MenuListBoxSave��$��Save log�� �� $��Images/save.png��$��16��$��16���� m� -V   $ MenuListBoxArduinoClear��$��	Clear log�� �� $��Images/delete.png��$��16��$��16��� -W   $	 GridButtons��$��2��
 �� $
��auto ��� $��* ��� $
��auto �.+��� -X    ��Name$ StackPanelMode��$��0�� �� Orientation$ 
Horizontal=�$
��Left=�.+��� -Y   $ LblMode��$��5,0,0,0q�$��Center=�$
��Mode���� -Z   $ CboMode��$��5,0,0,0q�$	��150��@��� -[   $ CbiModeDynamicColor��$��Dynamic Screen Color��
 � Tag$ 0�� . �� -\   $ CbiModeStaticColor��$��Static Ambient Color��$ 1���� -]   $ StackPanelButtons��$��2��$ 
Horizontal=�$��Right=�.+��� -^   $ ChkRunMinimized��$��Center=�$��0,0,5,0q�$��	Minimized�� � ToolTip$ Run Minimized���� -_   $ ChkAutoStart��$��Center=�$��0,0,5,0q�$��	AutoStart��$" AutoStart at Windows startup���� -`    ��Name$ BtSave��$
��Save��$��80��$��0,0,5,0q��� -a   $ BtExit��$
��Exit��$��80��$��0,0,5,0q��� -b    ��Name$ 	BarStatus��$��3��@��� -c   $ LblStatusArduino��$��	Arduino: ���� -d    ��Name$" ImgStatusArduinoNotConnected��$��Images/ledred.png��$��12��$��12��$ Not connected���� -e   $ ImgStatusArduinoPending��$��Images/ledyellow.png��$��12��$��12��$ Connection pending��$y�	Collapsed=��� -f   $ ImgStatusArduinoConnected��$��Images/ledgreen.png��$��12��$��12��$ 	Connected��$y�	Collapsed=��� �� -g   $ LblStatusCpu��$��Cpu: ���� -h   $ TxtStatusCpu��$	��n/a���� �� -i   $ LblStatusMemory��$��Mem: ���� -j   $ TxtStatusMemory��$	��n/a���� �� -k   $ LblStatusNbLoop��$��Nb Loops / sec: ���� -l   $ TxtStatusNbLoop��$�� ��