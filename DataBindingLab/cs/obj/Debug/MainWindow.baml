   M S B A M L   `   `   `  ����I  EDataBindingLab, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:DataBindingLabDataBindingLab  5          DataBindingLab.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      ,srcclr-namespace:DataBindingLab     5        � Title$  List of Products��5       � SizeToContent$ WidthAndHeight=�5       � 
ResizeMode$ NoResize=�5       � 	Resources 5	       �� 
TargetType �� Triggers P� Property P� Value �� Property	 �� Value   groupingHeaderTemplate
 �� Path  checkBoxStyle  listingDataView �� Source �� Current �� Source��%�  '��       &	  �     &	     &	 w    �� 5
   
   " ��6    5      P� 5       �@��
IsSelected . . �5      ��  � ��
	 �����63   5      5      �� 5   
   .R�5      �� 
����  �$
��Bold"�5   *   $��12(�6<   ��5      �� $

 Name�5   <   5      �� 5   
   " ��6&   .�5      ��  � ��	 �#3333336+   �� 5       � ��$
	 Bold"�6+   5      �� 5   
    5      �� # Z $
 AuctionItems�5      5      .��5       �  � RowDefinitions 5!      �� 5"   
   $
��Auto �6   �� 5#   
   $
��Auto �6   �� 5$   
   $	��300 �6   �� 5%   
   $
��Auto �6   �� 5&   
   $
��Auto �6    � ColumnDefinitions 5)      �� 5*   
   �� 5+   
   �� 5,   
   .+�5/   
   �� $��0��6   $��3��6!     
titleStyle#��[ 50      $��8,20,8,8q�68   List of items for sale:6c   �� 51   
   -   5        $��Grouping��51      $��1��6#   $��0��60   $��8q�53      #��[ 6   .��6?   Group by category6R   �� 54   
   -   5        $��	Filtering��54      $��1��6$   $��1��61   $��8q�56      #��[ 6   .��6=   Show only bargains6Q   �� 57   
   -   5        $��Sorting��57      $��1��6"   $��3��6/   $��8q�59      #��[ 6   .��6?   Sort by category and date6Z   �� 5:   
   -   5        $��Master��5:      $��2��6    $��3��6-   $��8q�6A   ��5;      �� # [  �� 
GroupStyle 5<      (  "System.Windows.Controls.GroupStyle  5=         HeaderTemplate# [  5>      5A      �� 5C      -   5        $��Detail��5C      $��3��6#   $��3��60    ! detailsProductListingTemplate#��[ 5E      $��9,0,0,0q�5F      ��5D      �� # [ 5F      �� 5G   
   -   5        $��OpenAddProduct��5G      $��4��6'   $��1��64   $��Add Product��6D   $��Center=�6Z   $��8q�6w   5H      5I      5J      