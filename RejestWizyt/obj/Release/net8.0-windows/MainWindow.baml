   M S B A M L   `   `   `  ���� O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem   RejestWizyt   RejestWizyt.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     6sys&clr-namespace:System;assembly=mscorlib       � Title$  Rejestr Wizyt��$	��600��$
��1000�� � Icon$ 
shield.ico��.���� .@��� $��Rejestr Wizyt��.��� $��10q� � RowDefinitions �� $
��Auto ��� $��* ��� $��* ��� $
��Auto �.+���  �� Orientation$ 
Horizontal=�$��0,0,0,10q�.+��� -    ��Name$ ImieTextBox��$	��120��$��5q�
 � Tag$ Imię��.������  �� 
TargetType" �� �� 	Resources    CueBannerBrush j� 
AlignmentX	 j� 
AlignmentY
 j� Stretch !� Visual��%R   &	        !� $
 Left=�$	 Center=�$

 None=� �� $��Imię��
������� �� Triggers P�  P� Property � �� �� Empty P� Value# Z . ���  �� Property � �� �� Value# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $ NazwiskoTextBox��$	��120��$��5q�$ Nazwisko��.������ " �� ��%U   &	        !� $
 Left=�$	 Center=�$

 None=� �� $��Nazwisko��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $ DoKogoTextBox��$	��120��$��5q�$ Do kogo��.������ " �� ��%T   &	        !� $
 Left=�$	 Center=�$

 None=� �� $��Do kogo��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $��Wejście��$��5q��� $��Osoby w budynku��$��1��$��0,0,0,10q�.��&   System.Windows.Controls.DataGrid  -     Name$ WizytyOtwarteGrid��   AutoGenerateColumns$
 True��   
IsReadOnly$
 True��   SelectionMode$ Single=�   CanUserResizeColumns$
 True��   CanUserSortColumns$
 True��   GridLinesVisibility$ 
Horizontal=�   AlternatingRowBackground
 �����$
��True��$
��Auto=�$
��Auto=��� $��Historia wizyt (filtr)��$��2��$��0,0,0,10q�.���  �� $
��Auto ��� $��* �.+��� $��0��$ 
Horizontal=�$��0,0,0,5q�.+��� -   $ FiltrNazwiskoTextBox��$	��120��$��5q�$ Nazwisko��.������ " �� ��%U   &	        !� $
 Left=�$	 Center=�$

 None=� �� $��Nazwisko��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $ FiltrDoKogoTextBox��$	��120��$��5q�$ Do kogo��.������ " �� ��%T   &	        !� $
 Left=�$	 Center=�$

 None=� �� $��Do kogo��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.��(  "System.Windows.Controls.DatePicker  -     Name$ DataOdPicker��$��5q�  -	   $ DataDoPicker��$��5q��� -
   $��Szukaj��$��5q�  -   $ HistoriaGrid��$��1��$
 True��$
 True��$
 True��$
 True��$ 
Horizontal=�
 �����$
��True��$
��Auto=�$
��Auto=��� $ 
Horizontal=�$��3��.+��� -   $��Zaznaczony: Wyjście��$��5q��� -   $��Usuń zaznaczoną��$��5q�
��������� -   $��Eksport CSV (filtr)��$��5q��� -   $��Eksport PDF (filtr)��$��5q��� $��
Ustawienia��.��� $��20q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��0��$,��&Ustawienia połączenia z bazą danych��$��16(�$
��Bold"�$��0,0,0,20q��� $��1��$ 
Horizontal=�$��0,0,0,10q�.+��� $��Adres IP serwera:��$	��150��$��Center=��� -   $ ServerIpTextBox��$	��200��$��5q�$��Center=��� $��2��$ 
Horizontal=�$��0,0,0,10q�.+��� $��Nazwa bazy danych:��$	��150��$��Center=��� -   $ DatabaseNameTextBox��$	��200��$��5q�$��Center=��� $��3��$ 
Horizontal=�$��0,0,0,10q�.+��� $��Nazwa instancji:��$	��150��$��Center=��� -   $ InstanceNameTextBox��$	��200��$��5q�$��Center=��� $��4��$ 
Horizontal=�$��0,0,0,20q�.+��� -   $��Testuj połączenie��$��5q�
����ح��� -   $��Zapisz ustawienia��$��5q�
�������� -   $��Przywróć domyślne��$��5q�
��������� -    ��Name$ ConnectionStatusTextBlock��$��5��$��Status: Nie testowano��$��0,0,0,10q��� $��6��$��Informacje o połączeniu��.���� -   $ ConnectionInfoTextBlock��$
��Wrap=�$T��NAktualny connection string zostanie wyświetlony tutaj po zapisaniu ustawień.��$��10q�