   M S B A M L   `   `   `  ����  ControlTemplateExamples(    "ControlTemplateExamples.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$  ControlTemplate Examples��5   	    � WindowState$ 	Maximized=�5   	    � 	Resources 5         dataGridData � XPath��%6  &	        � 5      $ Books��5	      .��5
      ��<Books xmlns="">
          <Book ISBN="0-7356-0562-9" InStock="True" Number="9">
            <Title>XML in Action</Title>
            <Summary>XML Web Technology</Summary>
          </Book>
          <Book ISBN="0-7356-1370-2" InStock="True" Number="8">
            <Title>Programming Microsoft Windows With C#</Title>
            <Summary>C# Programming using the .NET Framework</Summary>
          </Book>
          <Book ISBN="0-7356-1288-9" InStock="False" Number="0">
            <Title>Inside C#</Title>
            <Summary>C# Language Programming</Summary>
          </Book>
          <Book ISBN="0-7356-1377-X" InStock="True" Number="5">
            <Title>Introducing Microsoft .NET</Title>
            <Summary>Overview of .NET Technology</Summary>
          </Book>
          <Book ISBN="0-7356-1448-2" InStock="False" Number="0">
            <Title>Microsoft C# Language Specifications</Title>
            <Summary>The C# language definition</Summary>
          </Book>
        </Books>        5,      5-      ��5.      �� 5/        WindowColor �� Color# �  6   .��51      �� .��52      � .+�54      ��   VerticalHIC#��[ 6   $��Button��55      .@�56   
   �� $��8q�6    �� 	IsDefault . $��Default��58      �� 59   
   $��8q�6   $��Normal��5:      �� 5;   
   $��8q�6   {�.  $��Disabled��5=      5>   	   �� 5A      #��[ 6   $��CheckBox��5B      .@�5C   
   �� $��8q�6   $��Normal��5D      �� 5E   
   $��8q�6    d� 	IsChecked$
 trueY�5F      $��Checked��5G      �� 5H   
   $��8q�6    d� IsThreeState . $��Indeterminate��5K       5J      Z� 5K      5L   	   �� 5O      #��[ 6   $��RadioButton��5P      .@�5Q   
   	� $��8q�6   $��Normal��5R      	� 5S   
   $��8q�6   $
 trueY�5T      $��Checked��5U      	� 5V   
   $��8q�6   {�.  $��Normal��5X      5Y   	   �� 5\      #��[ 6   $��TextBox��5]      .@�5^   
   �� .+�5_      �� $��Center=�6   $��8q�5`      $��Edit Me��5a      �� 5b      $��Center=�6   $��8q�5c      $��	Read only��5d       �� 
IsReadOnly . 5e      �� 5f      $��Center=�6   $��8q�5g      $��Disabled��5h      {�.  5i      G� 5j      $��Center=�6   $��8q�5k      	 G� Password$	 Password��5l      
 G� PasswordChar$
 *��5m      5n      5o   	   �� 5r      #��[ 6   
���  ��$��
DatePicker��5t      .@�5v   
   (  "System.Windows.Controls.DatePicker  $	��200��6   $��0,5,0,10q�5w        5x   
   {�.  6   5y   	   �� 5|      #��[ 6   $��Calendar��5}      .@�5~   
   &   System.Windows.Controls.Calendar  5   	   �� 5�        HorizontalHIC#��[ 6   $��Slider��5�      .@�5�   
   �� .+�5�      �� $��8q�6    �� Orientation$ 
Horizontal=�5�      �� 5�      $��8q�6   $ 
Horizontal=�5�       �� TickPlacement$ TopLeft=�5�      �� 5�      $��8q�6   $ 
Horizontal=�5�      $ BottomRight=�5�      �� 5�      $��8q�6   $ 
Horizontal=�5�      $
 Both=�5�       �� TickFrequency$ 2Q�5�      5�      �� 5�   
    �� Orientation$ 
Horizontal=�6   $	��Top=�5�      .+�5�      �� $��8q�6   $ Vertical=�5�      �� 5�      $��8q�6   $ Vertical=�5�      $ TopLeft=�5�      �� 5�      $��8q�6   $ Vertical=�5�      $ BottomRight=�5�      �� 5�      $��8q�6   {�.  $ Vertical=�5�      $
 Both=�5�      $ 2Q�5�      5�      5�   	   �� 5�      #��[ 6   $��ToolBar��5�      .@�5�   
   �� .+�5�      `� .�5�      c� $��1��6   .@�5�      �� $��Button��6   �� 5�      $��CheckBox��6   �� 5�      $��TextBox��6   �� 5�      $��Disabled TextBox��6   {�.  5�      d� 5�      $��ToggleButton��6   	� 5�      $
 TrueY�6   $��RadioButton 1��5�      	� 5�      $
 TrueY�6   $��RadioButton 1��5�      �� 5�      $��Button��6    c� OverflowMode$ Always=�5�      �� 5�      $��CheckBox��6   $ Always=�5�      �� 5�      $��TextBox��6   $ Always=�5�      5�      5�      5�      5�   	   �� 5�      #��[ 6   $��Menu / ContextMenu��5�      .@�5�   
   o� $��8q�6   $	��400��5�      .@�5�      m� $��Top One��6   .@�5�      m� $��Sub One��6    m� InputGestureText$ Ctrl+L��5�      m� 5�      $��Sub Two (With an Icon)��6   $ Ctrl+A��5�       m� Icon 5�      A� 5�      $��16��6   $��16��5�      
����ح�5�      5�      m� 5�      $��	Sub Three��6   �� 5�      m� 5�      $��Sub Four��6   .@�5�      m� $��Sub One��6   m� 5�      $��Sub Two��6   m� 5�      $��	Sub Three��6   5�      m� 5�      $��Sub Five��6   5�      m� 5�      $��Top Two��6   .@�5�      m� $��Sub One��6   m� 5�      $��Sub Two (Checkable)��6    m� IsCheckable . 5�      m� 5�      $��	Sub Three��6   �� 5�      m� 5�      $��Sub Four��6   .@�5�      m� $��Sub One��6   m� 5�      $��Sub Two��6   m� 5�      $��	Sub Three��6   5�      m� 5�      $��Sub Five��6   5�      m� 5�      $��	Top Three��6   .@�5�      m� $��Sub One��6   m� 5�      $��Sub Two��6   m� 5�      $��	Sub Three��6   �� 5�      m� 5�      $��Sub Five��6   5�      m� 5�      -   5         m�Name$ topFour��5�      $��Menu with Many Items��5�      5�      5�      5�   	   �� 5�      #��[ 6   $��ComboBox��5�      .@�5�   
   �� $��8q�6   .@�5�      �� $��First Normal Item��6   �� 5�      $��Second Normal Item��6   �� 5�      $��Third Normal Item��6   �� 5�      $��Fourth Normal Item��6   �� 5�      $��Fifth Normal Item��6   5�      �� 5�   
   $��8q�6    �� 
IsEditable .  �� Text$ Edit Me��5�      .@�5�      �� $��First Normal Item��6   �� 5      $��Second Normal Item��6   �� 5     $��Third Normal Item��6   �� 5     $��Fourth Normal Item��6   �� 5     $��Fifth Normal Item��6   5     �� 5  
   $��8q�6   {�.  .@�5     ��  �� 
IsSelected . $��First Normal Item��5     �� 5	     $��Second Normal Item��6   �� 5
     $��Third Normal Item��6   �� 5     $��Fourth Normal Item��6   �� 5     $��Fifth Normal Item��6   5     5  	   �� 5     #��[ 6   $��ListBox��5     .@�5  
   �� $ 
Horizontal=�6   .+�5     �� $��8q�6    �� SelectionMode$ Extended=�5     $	��100��5     .@�5     �� $��First Normal Item��6   �� 5     $��Second Normal Item��6   {�.  5     �� 5     $��Third Normal Item��6   �� 5     $��Fourth Normal Item��6   �� 5     $��Fifth Normal Item��6   �� 5     $��Sixth Normal Item��6   �� 5     $��Seventh Normal Item��6   �� 5     $��Eighth Normal Item��6   5      �� 5!     $��8q�6   $ Extended=�5"     $��Visible=�5#     $	��100��5$     {�.  .@�5&     �� $��First Normal Item��6   �� 5'     $��Second Normal Item��6   �� 5(     $��Third Normal Item��6   �� 5)     $��Fourth Normal Item��6   �� 5*     $��Fifth Normal Item��6   �� 5+     $��Sixth Normal Item��6   �� 5,     $��Seventh Normal Item��6   �� 5-     $��Eighth Normal Item��6   5.     5/     50  	   �� 53     #��[ 6   $��ListView��54     .@�55  
   �� .+�56     �� $��8q�6   $	��100��57     $	��150��58      �� View 59     �� 5:      �� AllowsColumnReorder . .P�5;     �� $��Content��5<  !    �� Width$	 100��5=  !    �� DisplayMemberBinding 5;  !   �� 5=  !   �� 5>     $��Length��5?  !   $	 100��5@  !    5>  !   �� *Length+5@  !   5A     .@�5C     �� $��
First Item��6   �� 5D     $��Second Item��6   �� 5E     $��
Third Item��6   �� 5F     $��Fourth Item��6   �� 5G     $��
Fifth Item��6   �� 5H     $��
Sixth Item��6   �� 5I     $��Seventh Item��6   �� 5J     $��Eighth Item��6   5K     5L     5M  	   �� 5P     #��[ 6   $��TreeView��5Q     .@�5R  
   �� .+�5S     R� $	��150��6   $	��150��5T     $��8q�5U     .@�5V     Q� $��Top One��6   .@�5W     Q� $��Sub One��6   Q� 5X     $"��Sub Two (Longer than Normal)��6   Q� 5Y     $��	Sub Three��6   Q� 5Z     $��Sub Four��6   .@�5[     Q� $��Sub One��6   Q� 5\     $��Sub Two��6   Q� 5]     $��	Sub Three��6   5^     Q� 5_     $��Sub Five��6   5`     Q� 5a     $��Top Two��6   .@�5b     Q� $��Sub One��6   Q� 5c     $��Sub Two��6   Q� 5d     $��	Sub Three��6   Q� 5e     $��Sub Four��6   .@�5f     Q� $��Sub One��6   Q� 5g     $��Sub Two��6   Q� 5h     $��	Sub Three��6   5i     Q� 5j     $��Sub Five��6   5k     Q� 5l     $��	Top Three��6   .@�5m     Q� $��Sub One��6   Q� 5n     $��Sub Two��6   Q� 5o     $��	Sub Three��6   Q� 5p     $��Sub Five��6   5q     5r     5s     5t  	   �� 5w     #��[ 6   $��DataGrid��5x     .@�5y  
   �� .+�5z     &   System.Windows.Controls.DataGrid     AutoGenerateColumns .   �� AlternationCount$ 2��5{     ��5|     ��  �� Source# [   �� XPath$ *��    Columns  5~     0  *System.Windows.Controls.DataGridTextColumn  5     ,  &System.Windows.Controls.DataGridColumn!   Header$
! Name��5�  #   1  +System.Windows.Controls.DataGridBoundColumn"   Binding" 5  #   �� $ Title��5�  #     5�     $! Description��5�  #   " 5�  #   �� $ Summary��5�  #   4  .System.Windows.Controls.DataGridCheckBoxColumn  5�     $! InStock��5�  '   " 5�  '   �� $ @InStock��5�  '     5�     $! Count��5�  #   " 5�  #   �� $ @Number��5�  #   5�       5�      .  {�.  $��0,15,0,0q�5�     ��5�     �� # [  $ *��  5�       5�     $
! Name��5�  #   " 5�  #   �� $ Title��5�  #     5�     $! Description��5�  #   " 5�  #   �� $ Summary��5�  #     5�     $! InStock��5�  '   " 5�  '   �� $ @InStock��5�  '     5�     $! Count��5�  #   " 5�  #   �� $ @Number��5�  #   5�     5�     5�  	   �� 5�     #��[ 6   $��
TabControl��5�     .@�5�  
   �� $ 
Horizontal=�6   .+�5�     �� $��8q�6   $	��100��5�     $	��225��5�     .@�5�     �� $	��One��6   $	��One��5�     �� 5�     $	��Two��6   $	��Two��5�     {�.  5�     �� 5�     $��Three��6   $��Three��5�     �� 5�     $
��Four��6   $
��Four��5�     5�     5�     5�  	   �� 5�     #��[ 6   $��Expander��5�     .@�5�  
   �� $ 
Horizontal=�6   .+�5�     9� $	��200��6   $��8q�5�     $��Expand down��5�     .��5�     �� $	��100��6   5�     9� 5�     $	��200��6   # 9� ExpandDirection$
# Left=�6!   $��8q�5�     $��Expand left��5�     .��5�     �� $	��100��6   5�     9� 5�     $	��200��6   $# Right=�6!   $��8q�5�     $��Expand right��5�     .��5�     �� $	��100��6   5�     9� 5�     $	��200��6   $# Up=�6!   $��8q�5�     $��	Expand up��5�     .��5�     �� $	��100��6   5�     9� 5�     $	��200��6   $��8q�5�     $��Disabled��5�     {�.  .��5�     �� $	��100��6   5�     5�     5�  	   �� 5�     #��[ 6   $��	StatusBar��5�     .@�5�  
   �� $	��220��6   .+�5�     �� $��Bottom=�6   .@�5�     �� $��Right=�6   $��Ln 181, Col 20��5�     �� 5�     $��Right=�6   �� 5�     $	��80%��6   5�     5�     5�  	   �� 5�     #��[ 6   $��ProgressBar��5�     .@�5�  
   �� .+�5�     � $��Center=�6   $��8q�5�     $ � Value$$ 20Q�5�     � 5�     $��Center=�6   $��8q�5�     $$ 90Q�5�     � 5�     $��Center=�6   $��8q�5�     % � IsIndeterminate% . 5�     5�     5�  	   �� 5�     #��[ 6   $��GroupBox��5�     .@�5�  
   �� .+�5�     �� $��8q�6   $��This is the Header��5�     $	��200��5�     .��5�     �� $	��100��6   5�     5�     5�  	   �� 5�     #��[ 6   $��Frame��5�     .@�5�  
   �� .+�5       � &  � Source$& 
page1.xaml?�6   $	��200��5     $	��200��5     5     5  	   �� 5     #��[ 6   $��ScrollViewer��5     .@�5	  
   �� .+�5
     �� $	��200��6   $	��200��5       LeftScrollViewer#��[ 5     .��5     �� $	��400��6   $	��400��5     ��5     �� 5     ' �� 
StartPoint$	' 0,0&�6&   ( �� EndPoint$	( 0,1&�5  &   .��5     � ) � Color$
) #FFF��6!   * � Offset$* 0Q�5  !   � 5     $) 	#FF7381F9��6!   $* 1Q�5  !   5     5     5     5     5  	   �� 5     #��[ 6   $��ToolTip��5     .@�5   
   �� .+�5!     �� $��8q�6   $	��150��5"     $��50��5#     + �� CornerRadius$+ 2��5$     ���	#FFC5CBF9, � ToolTip$, This is a test tooltip.��5&  E   ._�5'     �� $��Center=�6   $��Center=�5(     ���	#FF888888�� 5)  0   $�(Hover Over Me)��64   6O   5*     5+     5,  	   �� 5/     #��[ 6   $��ContextMenu��50     .@�51  
   �� .+�52     �� $��8q�6   $	��150��53     $��50��54     $+ 2��55     ���	#FFC5CBF9- � ContextMenu- 57     �� 58     .@�59     m� $��Sub One��6   $ Ctrl+L��5:     m� 5;     $��Sub Two (With an Icon)��6   $ Ctrl+A��5<      5=     A� 5>     $��16��6   $��16��5?     
����ح�5@     5B     m� 5C     $��	Sub Three��6   �� 5D     m� 5E     $��Sub Four��6   .@�5F     m� $��Sub One��6   m� 5G     $��Sub Two��6   m� 5H     $��	Sub Three��6   5I     m� 5J     $��Sub Five��6   5K     ._�5M     �� $��Center=�6   $��Center=�5N     ���	#FF888888�� 5O  0   $�(Right-Click Me)��64   6P   5P     5Q     5R  	   �� 5U     #��[ 6   $��Window��5V     .@�5W  
   �� .+�5X     �� -   5        $��8q�5X     $	��150��5Y     $��50��5Z     $+ 2��5[     ��5]     �� 5^       ContentAreaColorLight# �  6    ._�5`     �� $��Center=�6   $��Center=�5a     ���	#FF888888�� 5b  0   $�
(Click Me)��64   6J   5c     5d     5e  	   �� 5h     #��[ 6   $��Navigation Window��5i     .@�5j  
   �� .+�5k     �� -   5        $��8q�5k     $	��150��5l     $��50��5m     $+ 2��5n     ���	#FFC5CBF9._�5q     �� $��Center=�6   $��Center=�5r     ���	#FF888888�� 5s  0   $�
(Click Me)��64   6J   5t     5u     5v  	   �� 5y     #��[ 6   $��DocumentViewer��5z     .@�5{  
   �� .+�5|     Z� $	��275��6   5}     5~  	   5�     5�     5�     