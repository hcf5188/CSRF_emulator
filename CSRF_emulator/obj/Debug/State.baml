   M S B A M L   `   `   `  ����H  DCSRF_emulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:CSRF_emulatorCSRF_emulator  5          CSRF_emulator.State   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:CSRF_emulator     5        N� Title$  State��5	      .��5      � .+�5   
   �� -   5         ��Name$ bt_Start��5      $��启动��6#   $
��Left=�60   $��1089,662,0,0q�6K   $	��Top=�6a   $��77��6y   $��94��6�   $��36(�6�   ���	#FF06FB1D���	#00998F8B���	#FFF7F7736�   �� 5   
   -   5        $ bt_Stop��5      $��停止��6"   $
��Left=�6/   $��1188,662,0,0q�6J   $	��Top=�6`   $��77��6x   $��46��6�   $��24(�6�    J� RenderTransformOrigin$ 	12.597,15&�6�   ���	#00F99494���	#FFF77474���	#FFFB4B426  �� 5   
   -   5         ��Name$ 	tb_Status��5      $
��Left=�6%   $��33��6@   $��	421,6,0,0q�6L    �� TextWrapping$
 Wrap=�6_   $$��让每个卡车跑的更健康��6s   $	��Top=�6�   $	��763��6�   $��22(�6�   ���	#00DEF7CD���	#FFB9F994���	#FFABADB36�   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ grid_saffer��5         DataGridRowColorStyle   RowStyle# [  5         HeadersVisibility$ Column=�5       �� AlternationCount$ 2��5      $	   HorizontalScrollBarVisibility$	 Visible=�5      
   GridLinesVisibility$

 None=�5      $��0q�6)      CanUserAddRows .    
IsReadOnly .    AutoGenerateColumns . $��
0,48,0,113q�5   Q   $��16(�6e   ���	#00DBF7B3��5      �� *	Employees+ �� SelectedItem 5      �� *SelectedEmployee+   Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$	 NO.��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ 70 62   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6=   �� *no+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�  5      $ Id��6%   $	 100 61    6=   �� *id+$ TwoWay=�$ PropertyChanged=�  5      $ 方向��6%   $ 40 61    6<   �� *direc+$ TwoWay=�$ PropertyChanged=�  5      $ 相对时间��6%   $ 80 63    6>   �� *	rela_time+$ TwoWay=�$ PropertyChanged=�  5      $ 长度��6%   $ 40 61    6<   �� *
data_len+$ TwoWay=�$ PropertyChanged=�  5      $ 数据��6%   $ 1* 61    6<   �� *
data_str+$ TwoWay=�$ PropertyChanged=�   ColumnHeaderStyle 5       �� 5!       �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader"  6   .�5"      ��  �� Property � �� �� Value$ Center=�6C   �� 5$       � �� �	#00DBF7B363   �� 5%       � �� �	#FFC3F80F63   �� 5&       � ��$ 1q�68   �� 5'       � �� �#B0B0B064   5(      5+      �� 5,   
   -   5        $ bt_OpenFile��5,      $��打开��6&   $
��Left=�63   $��
1199,6,0,0q�6N   $	��Top=�6b   $��68��6z   $ 0.115,-0.421&�6�   $��33��6�   $��24(�6�   ���	#00DDDDDD���	#FFEEA7A7���	#FF6CF7BE6  �� 5-   
   -   5        $ bt_Clear��5-      $��清空��6#   $
��Left=�60   $��683,693,0,0q�6K   $	��Top=�6`   $��87��6x   $��40��6�   $��24(�6�   $ -0.203,0.479&�6�   $
��Bold"�6�   ���	#00DDDDDD���	#FF71F9BB���	#FFDE92FB6  �� 5.   
   -   5        $ bt_Pause��5.      $��暂停��6#   $
��Left=�60   $��1188,713,0,0q�6K   $	��Top=�6a   $��77��6y   $��43��6�   $��24(�6�   $ 0.377,0.372&�6�   ���	#00C34C4C���	#FF4ED8CC���	#FFEBF3346  �� 5/   
   -   5        $ tb_Curr��5/      $
��Left=�6#   $��98��6>   $��	0,658,0,0q�6J   $
 Wrap=�6]   $(��" 请打开文件并插入设备！��6q   $	��Top=�6�   $	��678��6�   $��14(�6�   
������ ���	#FFEBA2FB6�   �� 50   
   -	   5         ��Name$
 lb_1��50      $$��CSRF 共轨之家 - 模拟器 ��6   $
��Left=�69   $��25,0,0,0q�6T   $	��Top=�6f   $ 
0.343,0.38&�6~   ���	#00E8FCFD$	��298��6�   $��43��6�   ���	#FF06ECF7$��24(�6�   �� 52   
   -
   5        $��sp��52      $y�Hidden=�6   $
��Left=�64   $��74��6O   $��1017,668,0,0q�6[   $	��Top=�6q   $��67��6�   .+�53      	� -   5         	�Name$	 rb0��53       	� 	GroupName$ resCheck��6'   ���	#FFA9EADB���	#FFF0E969$��18(�6l    d� 	IsChecked$
 TrueY�6z   .��6�   ∞ Ω6�   	� 54      -   5        $
 rb60��54      $ resCheck��6(   ���	#FFA9EADB���	#FFF0E969$��18(�6m   .��6{   60Ω6�   	� 55      -   5        $ rb120��55      $ resCheck��6)   ���	#FFA9EADB���	#FFF0E969$��18(�6n   .��6}   120Ω6�   56      �� 57   
   -   5        $ button��57      $��Button��6!   $
��Left=�62   $��1017,729,0,0q�6M   $	��Top=�6c   $��11��6{   6�   �� 58   
   -   5        $ textBox��58      $
��Left=�6#   $��88��6>   $��775,668,0,0q�6J   $
 Wrap=�6_   $	��Top=�6t   $	��237��6�   ���	#00D16262���	#006D8DEC���	#FFF9C9C96�   59      5:      