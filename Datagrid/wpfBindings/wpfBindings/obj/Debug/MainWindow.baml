   M S B A M L   `   `   `  ����  wpfBindings    wpfBindings.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  
MainWindow��5   	   $	��350��6   $	��525��6)    � 	Resources 5      ��%    5	      .��5
      �  5   
   ��%    5       � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6    � RowDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ 	Datagrid1��5   3   $��30q�6   $��2��6   $��2��6�   ��6G   �� *
PersonList+ 5         RowHeaderTemplate �� Orientation ��Name)  #System.Windows.Controls.DataGridRow �� 
TargetType �� Triggers	 �� Binding
 �� Value �� Property �� Value��%�  &	        '  �     7   �� 5      .R�5      �� $ 
Horizontal=�6!   .+�5      �� $
 lock��6    $%��D:\Program Files\thCAS0A72K.gif��6.   $��16��6W   6d   5      5      �� 5      "  6    5       �� 5!      	 6&   �� *Member+
 True6A   .a�5#      ��  �@�Tag$ ReadOnly��64   �� 5$       �@ HeaderTemplate8   6m   5%      5(      5)      5,      56      58      