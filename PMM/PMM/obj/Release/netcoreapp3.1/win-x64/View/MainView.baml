   M S B A M L   `   `   `  ���� >  :PMM, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:TosAutoSkillTosAutoSkill  ?$clr-namespace:TosAutoSkill.ViewModelTosAutoSkill.ViewModel      TosAutoSkill.MainView   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Z VDevExpress.Mvvm.UI, Version=19.2.0.0, Culture=neutral, PublicKeyToken=382ad28f6ae31f30W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     ,localclr-namespace:TosAutoSkill     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     3vm$clr-namespace:TosAutoSkill.ViewModel     -     �Name$  
MainWindow�� � Title$ 
PMM v1.0.1��$	��250��$	��175�����#232323 � 
ResizeMode$ NoResize=� � Topmost$
 True�� � WindowStartupLocation$ CenterScreen=� � DataContext *   $TosAutoSkill.ViewModel.MainViewModel  -   
MainVM��.���  � RowDefinitions �� �� �� �� $��25 �.+��� $��0�� �� Orientation$ 
Horizontal=�.+��� $	��5,0q�$��Center=�
�������$��目标进程���� $	��170��$��3,0,5,0q�$��Center=�{��� *
IsStopping+���� *ProcessInfoes+ �� SelectedIndex �� *ProcessIndex+���� .R��� $��2q����� *ProcessName+���� 	 �� 
TargetType"	 ��.��� 
 �� Property
 � �� �� Value �� .R��  �Name$ cb_gd�� � ColumnDefinitions �� $	��150 ��� $��50 �.+��� $��0��$��5q����� *ProcessName+�� $��1��$��5q����� *Pid+ �� Triggers P�  P� Property �@��
IsSelected P� Value$
 True��. ���  �� 
TargetName$ cb_gd��
 � ��
 ������� $ cb_gd��
 � ��
 �����P�  �@��IsHighlighted$
 True��. ��� $ cb_gd��
 � ��
 ��  ��� $ cb_gd��
 � ��
 ������� $��1��$��Center=�$ 
Horizontal=�.+��� $��16(�
�������$�� 快捷键: ���� $��70��$��0,0,6,0q����#333333$��14(�$
��Bold"�
������� �� 
IsReadOnly$
 True��!   TosAutoSkill.FocusExtension   	IsFocused �� *%#HotKey.HotKeyTextBoxFocusable4Skill+{��� *
IsStopping+���� *HotKey.Key4Skill+�� $��16(�
�������$
��CD: ���� $��40�����#333333$��14(�$
��Bold"�
���  ��{��� *
IsStopping+���� *CD+�� $��16(�
�������$
�� 秒���� $��2��$��Center=�$��Center=�$ 
Horizontal=�.+�d� $	��200��$��25��
�������$
��Bold"����� *StartCommand+����  �� RelativeSource �� *Self+���� "	 d�.��� 
 � �� �� $ 
Horizontal=�.+��� $��1q�   svg_play#��[  �� $	��5,0q�$��16(�$��启动�� �� Triggers P�  �@d�	IsChecked$
 trueY�. ��� 
 � �� �� $ 
Horizontal=�.+��� $��1q�  svg_stop#��[ �� $	��5,0q�$��16(�$��停止���� $��25��$��25��$��2,0,0,0q�
�������.���� $��1q�  svg_add#��[ �� $��3��$��2�����#232323
�������$��	0,0.5,0,0q����� .R��  �� $��50 ��� $��* �.@��� $��0��.���� $ 
Horizontal=�.+��� $��5,0,0,0q�$
��Bold"����� *
StatusFlag+ �� StringFormat$ {0} :������ "	 ��.��� 
 � ��
 ����� ��  �� Binding �� *
StatusFlag+ �� Value	 Error.a��� 
 � ��
 �  ���� $��1��.���� 
����������� *
StatusText+