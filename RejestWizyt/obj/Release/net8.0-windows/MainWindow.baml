   M S B A M L   `   `   `  ���� O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem   RejestWizyt   RejestWizyt.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     6sys&clr-namespace:System;assembly=mscorlib       � Title$  Rejestr Wizyt��$	��600��$
��1000�� � Icon$ 
shield.ico��.���� -    ��Name$ 
TabControl��.@��� $��Rejestr Wizyt��.��� $��10q� � RowDefinitions �� $
��Auto ��� $
��Auto ��� $��* ��� $��* ��� $
��Auto �.+��� $��0�� �� Orientation$ 
Horizontal=�$��0,0,0,10q�
����ح�.+��� -    ��Name$ CurrentUserTextBlock��$ ��Użytkownik: Niezalogowany��$
��Bold"�$��Center=��� -    ��Name$ LogoutButton��$��Wyloguj��$��20,0,0,0q�${�False���� $��1��$ 
Horizontal=�$��0,0,0,10q�.+��� -    ��Name$ ImieTextBox��$	��120��$��5q�
 � Tag$ Imię��.������ 	 �� 
TargetType"	 ��
 �� 	Resources
    CueBannerBrush j� 
AlignmentX j� 
AlignmentY j� Stretch !� Visual��%R   &	        !� $
 Left=�$ Center=�$
 None=� �� $��Imię��
������� �� Triggers P�  P� Property � �� �� Empty P� Value# Z . ���  �� Property � �� �� Value# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $ NazwiskoTextBox��$	��120��$��5q�$ Nazwisko��.������ "	 ��
 ��%U   &	        !� $
 Left=�$ Center=�$
 None=� �� $��Nazwisko��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $ DoKogoTextBox��$	��120��$��5q�$ Do kogo��.������ "	 ��
 ��%T   &	        !� $
 Left=�$ Center=�$
 None=� �� $��Do kogo��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $ FirmaTextBox��$	��120��$��5q�$ Firma��.������ "	 ��
 ��%R   &	        !� $
 Left=�$ Center=�$
 None=� �� $��Firma��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $ UwagiTextBox��$	��120��$��5q�$ Uwagi��.������ "	 ��
 ��%R   &	        !� $
 Left=�$ Center=�$
 None=� �� $��Uwagi��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -	   $��Wejście��$��5q��� $��Osoby w budynku��$��2��$��0,0,0,10q�.��&   System.Windows.Controls.DataGrid  -
     Name$ WizytyOtwarteGrid��   AutoGenerateColumns$
 True��   
IsReadOnly$
 True��   SelectionMode$ Single=�   CanUserResizeColumns$
 True��   CanUserSortColumns$
 True��   GridLinesVisibility$ 
Horizontal=�   AlternatingRowBackground
 �����$
��True��$
��Auto=�$
��Auto=��� $��Historia wizyt (filtr)��$��3��$��0,0,0,10q�.���  �� $
��Auto ��� $��* �.+��� $��0��$ 
Horizontal=�$��0,0,0,5q�.+��� -   $ FiltrNazwiskoTextBox��$	��120��$��5q�$ Nazwisko��.������ "	 ��
 ��%U   &	        !� $
 Left=�$ Center=�$
 None=� �� $��Nazwisko��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $ FiltrDoKogoTextBox��$	��120��$��5q�$ Do kogo��.������ "	 ��
 ��%T   &	        !� $
 Left=�$ Center=�$
 None=� �� $��Do kogo��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.���� -   $ FiltrFirmaTextBox��$	��120��$��5q�$ Firma��.������ "	 ��
 ��%R   &	        !� $
 Left=�$ Center=�$
 None=� �� $��Firma��
������� P�  � ��# Z . ���  � ��# [  P�  � �� Z� . ���  � ��# [  P�  �@J�IsKeyboardFocused$
 True��. ���  � ��
 �����.��(  "System.Windows.Controls.DatePicker  -     Name$ DataOdPicker��$��5q�  -   $ DataDoPicker��$��5q��� -   $��Szukaj��$��5q�  -   $ HistoriaGrid��$��1��$
 True��$
 True��$
 True��$
 True��$ 
Horizontal=�
 �����$
��True��$
��Auto=�$
��Auto=��� $ 
Horizontal=�$��4��.+��� -   $��Zaznaczony: Wyjście��$��5q��� -   $ UsunWizyteButton��$��Usuń zaznaczoną��$��5q�
�������${�False���� -   $��Eksport CSV (filtr)��$��5q��� -   $��Eksport PDF (filtr)��$��5q��� $��
Ustawienia��.���� $
��Auto=�.��� $��20q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��0��$,��&Ustawienia połączenia z bazą danych��$��16(�$
��Bold"�$��0,0,0,20q��� $��1��$ 
Horizontal=�$��0,0,0,10q�.+��� $��Adres IP serwera:��$	��150��$��Center=��� -   $ ServerIpTextBox��$	��200��$��5q�$��Center=��� $��2��$ 
Horizontal=�$��0,0,0,10q�.+��� $��Nazwa bazy danych:��$	��150��$��Center=��� -   $ DatabaseNameTextBox��$	��200��$��5q�$��Center=��� $��3��$ 
Horizontal=�$��0,0,0,10q�.+��� $��Nazwa instancji:��$	��150��$��Center=��� -   $ InstanceNameTextBox��$	��200��$��5q�$��Center=��� $��4��$ 
Horizontal=�$��0,0,0,20q�.+��� -   $��Testuj połączenie��$��5q�
����ح��� -   $��Zapisz ustawienia��$��5q�
�������� -   $��Przywróć domyślne��$��5q�
��������� -   $ ConnectionStatusTextBlock��$��5��$��Status: Nie testowano��$��0,0,0,10q��� $��6��$
��0,10q��� $��7��$��Ustawienia interfejsu��$��16(�$
��Bold"�$��	0,10,0,10q��� $��8��$ 
Horizontal=�$��0,0,0,20q�.+��� $��Rozmiar czcionki:��$	��150��$��Center=��� -    ��Name$ FontSizeSlider��$	��200��$��5q� � Minimum$ 10Q�  � Maximum$  24Q�! � Value$! 12Q�$��Center=��� -   $ FontSizeValue��$��12��$��30��$��Center=�$��5,0,0,0q��� $��12��$��Informacje o połączeniu��$��0,10,0,0q�.���� -   $ ConnectionInfoTextBlock��$
��Wrap=�$T��NAktualny connection string zostanie wyświetlony tutaj po zapisaniu ustawień.��$��10q��� $
��Logi��.��� $��10q� �� $
��Auto ��� $��* ��� $
��Auto �.+��� $��0��$ 
Horizontal=�$��0,0,0,10q�.+��� -    $��Odśwież logi��$��5q�
����ح��� -!   $ ClearLogsButton��$��Wyczyść logi��$��5q�
�������${�False���� -"   " ��Name$" LogLevelFilter��$	��120��$��10,0,0,0q�.@��� $��	Wszystkie��# �� 
IsSelected$
# True���� $
��Info���� $��Warning���� $��Error��  -#   $ LogsGrid��$��1��$ False��$
 True��$
 True��$
 True��$ 
Horizontal=�
 �����$
��True��$
��Auto=�$
��Auto=�$   Columns$ 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn%   Header$% 	Data/Czas��&   Width5  /System.Windows.Controls.DataGridLengthConverter$	& 150 1  +System.Windows.Controls.DataGridBoundColumn'   Binding' �� *	Timestamp+( �� StringFormat$( dd.MM.yyyy HH:mm:ss��  $% Poziom��$& 80 ' �� *Level+  $% Użytkownik��$	& 120 ' �� *
Username+  $% Akcja��$	& 150 ' �� *Action+  $% Szczegóły��$& * ' �� *	Details+�� -$   $ LogsStatusTextBlock��$��2��$��Gotowe��$��0,10,0,0q��� -%   ) ��Name$) UsersTab��$��Użytkownicy��.��� $��10q�.+��� .+��� $ 
Horizontal=�$��0,0,0,10q�.+��� $��Nowy użytkownik:��$	��120��$��Center=��� -&   $ NewUsernameTextBox��$	��150��$��5q�$��Center=��� $��Hasło:��$��60��$��Center=�G� -'   * G�Name$* NewPasswordBox��$	��150��$��5q�$��Center=��� -(   + ��Name$+ IsAdminCheckBox��$��Administrator��$��Center=�$��10,0,0,0q��� -)   $��Dodaj��$��10,0,0,0q�
������  -*   $ 	UsersGrid��$	��300��$��0,10,0,0q�$ False��,   CanUserAddRows$, False��$
 True��$   $% Użytkownik��$& * ' �� *
Username+  $
% Rola��$	& 100 ' �� *Role+  $% 	Utworzony��$	& 150 ' �� *	CreatedAt+$( dd.MM.yyyy HH:mm��4  .System.Windows.Controls.DataGridTemplateColumn  $% Akcje��$	& 100 -   CellTemplate- �� .R��� -+   $��Usuń��
�������$��2q� �� *Id+