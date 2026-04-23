Patryk 84454, Michał 84457

# FakturoNet

Natywna aplikacja desktopowa w `C#` zbudowana w `Avalonia UI`, bez Electrona i bez warstwy ASP.NET. `Avalonia` pelni tutaj role odpowiednika `WPF` na macOS, a eksport dokumentow jest realizowany przez `QuestPDF`.

## Co potrafi

- dodawanie, edycja i usuwanie kontrahentow,
- wystawianie faktur z wieloma pozycjami,
- edycja wczesniej zapisanych faktur,
- wystawianie korekt do istniejacych faktur,
- oznaczanie faktur jako otwarte lub zamkniete,
- automatyczne liczenie netto, VAT i brutto,
- podglad zapisanych faktur w oknie aplikacji,
- eksport `PDF` do katalogu `Downloads`,
- blokada usuwania kontrahenta, gdy ma otwarta fakture,
- zapis danych do lokalnych plikow `JSON` w katalogu projektu `AppData`.

## Jak uruchomic

1. Przejdz do katalogu projektu.
2. Uruchom:

```bash
dotnet restore ./FakturoNet.csproj
dotnet run --project ./FakturoNet.csproj
```

Po starcie otworzy sie okno aplikacji desktopowej.

## Jak edytowac lub korygowac fakture

1. Przejdz do zakladki `Faktury`.
2. Wybierz dokument z listy.
3. Kliknij `Edytuj fakture`.
4. Po zapisaniu zmiany zostana naniesione na istniejacy dokument, bez tworzenia duplikatu.

Aby wystawic korekte:

1. Wejdz do zakladki `Faktury`.
2. Wybierz zwykla fakture.
3. Kliknij `Wystaw korekte`.
4. Zmien pozycje, kwoty lub powod korekty i zapisz nowy dokument.

## Gdzie sa dane

- konfiguracja wystawcy: `appsettings.json`,
- dane aplikacji: lokalny katalog `AppData`,
- eksport PDF: katalog `~/Downloads`.

## Struktura

- [MainWindow.cs](/Users/patrykparfienczyk/Desktop/zaliczeniec%23/MainWindow.cs) zawiera interfejs desktopowy i obsluge akcji,
- [Services/JsonDataStore.cs](/Users/patrykparfienczyk/Desktop/zaliczeniec%23/Services/JsonDataStore.cs) odpowiada za lokalny zapis danych,
- [Services/InvoiceService.cs](/Users/patrykparfienczyk/Desktop/zaliczeniec%23/Services/InvoiceService.cs) spina logike faktur,
- [Services/PdfInvoiceRenderer.cs](/Users/patrykparfienczyk/Desktop/zaliczeniec%23/Services/PdfInvoiceRenderer.cs) generuje pliki PDF przez `QuestPDF`.
