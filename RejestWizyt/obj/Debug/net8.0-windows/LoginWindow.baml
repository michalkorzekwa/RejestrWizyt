   M S B A M L   `   `   `  ����  RejestWizyt    RejestWizyt.LoginWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Logowanie - Rejestr Wizyt��5   	   $	��500��6+   $	��400��68    � WindowStartupLocation$ CenterScreen=�5   	    � 
ResizeMode$ NoResize=�5   	    � Icon$ 
shield.ico��5   	   .��5      � ��5	   
   �� 5
       �� 
StartPoint$	 0,0&�6"    �� EndPoint$	 1,1&�63   .��5      �  � Color$ 	LightBlue��6    � Offset$ 0Q�61   � 5      $ White��6   $ 1Q�6-   5      .+�5   
   �� 
������� �� CornerRadius$ 10��6$   $��40q�66   	 �� Padding$	 30q�6B   
�������$��1q�5   )   
 J� Effect
 5      3  -System.Windows.Media.Effects.DropShadowEffect  5         Color$
 Gray��6#      	Direction$	 315Q�60      ShadowDepth$ 5Q�6@      Opacity$	 0.3Q�6P   ._�5      �� $��Center=�6   .+�5      �� $��Rejestr Wizyt��6   $��24(�61   $
��Bold"�6?   $��Center=�5      $��0,0,0,20q�69   
����  �6K   �� 5      $��Zaloguj sie do systemu��6   $��14(�6:   $��Center=�5      $��0,0,0,30q�69   
�������6K   �� 5      $��Nazwa uzytkownika:��6   $��0,0,0,5q�65   �� 5      -   5         ��Name$ UsernameTextBox��5      $��30��63   $��0,0,0,15q�6?   $��14(�5      $��5q�6(   �� 5!      $��Haslo:��6   $��0,0,0,5q�6)   G� 5"      -   5         G�Name$ PasswordBox��5"      $��30��63   $��0,0,0,20q�6?   $��14(�5#      $��5q�6,   �� 5%      -   5        $��Zaloguj��5%      $��35��6?   
����  �
�������$��14(�5&   B   $
��Bold"�5'      $��0,0,0,10q�6+   �� 5)      -   5         ��Name$ ErrorTextBlock��5)      $�� ��64   
���  ��$��Center=�5*      $��0,10,0,0q�69   $
��Wrap=�5+      5,      5-      5.      5/      