   M S B A M L   `   `   `  ���� H  DCSRF_emulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:CSRF_emulatorCSRF_emulator      CSRF_emulator.State   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     -localclr-namespace:CSRF_emulator     -     N� Title$  State��.��� .+��� -    ��Name$ bt_Start��$��启动��$
��Left=�$��1089,662,0,0q�$	��Top=�$��77��$��94��$��36(����	#FF06FB1D���	#00998F8B���	#FFF7F773�� -   $ bt_Stop��$��停止��$
��Left=�$��1188,662,0,0q�$	��Top=�$��77��$��46��$��24(� J� RenderTransformOrigin$ 	12.597,15&����	#00F99494���	#FFF77474���	#FFFB4B42�� -    ��Name$ 	tb_Status��$
��Left=�$��33��$��	421,6,0,0q� �� TextWrapping$
 Wrap=�$$��让每个卡车跑的更健康��$	��Top=�$	��763��$��22(����	#00DEF7CD���	#FFB9F994���	#FFABADB3&   System.Windows.Controls.DataGrid  -     Name$ grid_saffer��   DataGridRowColorStyle   RowStyle# [     HeadersVisibility$ Column=� �� AlternationCount$ 2��$	   HorizontalScrollBarVisibility$	 Visible=�
   GridLinesVisibility$

 None=�$��0q�   CanUserAddRows .    
IsReadOnly .    AutoGenerateColumns . $��
0,48,0,113q�$��16(����	#FFFEFFFE���	#FF7FBBF7���	#FFC55252���� *	Employees+ �� SelectedItem �� *SelectedEmployee+   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$	 NO.��   Width5  /System.Windows.Controls.DataGridLengthConverter$ 70 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *no+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�  $ Id��$	 100  �� *id+$ TwoWay=�$ PropertyChanged=�  $ 方向��$ 40  �� *direc+$ TwoWay=�$ PropertyChanged=�  $ 相对时间��$ 80  �� *	rela_time+$ TwoWay=�$ PropertyChanged=�  $ 长度��$ 40  �� *
data_len+$ TwoWay=�$ PropertyChanged=�  $ 数据��$ 1*  �� *
data_str+$ TwoWay=�$ PropertyChanged=�   ColumnHeaderStyle ��  �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader"  .���  �� Property � �� �� Value$ Center=���  � �� �	#00DBF7B3��  � �� �	#FF0000FF��  � ��$ 1q���  � �� �#B0B0B0�� -   $ bt_OpenFile��$��打开��$
��Left=�$��
1199,6,0,0q�$	��Top=�$��68��$ 0.115,-0.421&�$��33��$��24(����	#00DDDDDD���	#FFEEA7A7���	#FF6CF7BE�� -   $ bt_Clear��$��清空��$
��Left=�$��683,693,0,0q�$	��Top=�$��87��$��40��$��24(�$ -0.203,0.479&�$
��Bold"����	#00DDDDDD���	#FF71F9BB���	#FFDE92FB�� -   $ bt_Pause��$��暂停��$
��Left=�$��1188,713,0,0q�$	��Top=�$��77��$��43��$��24(�$ 0.377,0.372&����	#00C34C4C���	#FF4ED8CC���	#FFEBF334�� -	   $ tb_Curr��$
��Left=�$��98��$��	0,658,0,0q�$
 Wrap=�$(��" 请打开文件并插入设备！��$	��Top=�$	��678��$��14(�
������ ���	#FFEBA2FB�� -
    ��Name$
 lb_1��$$��CSRF 共轨之家 - 模拟器 ��$
��Left=�$��25,0,0,0q�$	��Top=�$ 
0.343,0.38&����	#00E8FCFD$	��298��$��43�����	#FF06ECF7$��24(��� -   $��sp��$y�Hidden=�$
��Left=�$��74��$��1017,668,0,0q�$	��Top=�$��67��.+�	� -    	�Name$	 rb0�� 	� 	GroupName$ resCheck�����	#FFA9EADB���	#FFF0E969$��18(� d� 	IsChecked$
 TrueY�.��∞ Ω	� -   $
 rb60��$ resCheck�����	#FFA9EADB���	#FFF0E969$��18(�.��60Ω	� -   $ rb120��$ resCheck�����	#FFA9EADB���	#FFF0E969$��18(�.��120Ω�� -   $ button��$��Button��$
��Left=�$��1017,729,0,0q�$	��Top=�$��11���� -   $ textBox��$
��Left=�$��88��$��775,668,0,0q�$
 Wrap=�$	��Top=�$	��237�����	#00D16262���	#006D8DEC���	#FFF9C9C9