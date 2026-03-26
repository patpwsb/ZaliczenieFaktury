using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using FakturoNet.Models;
using FakturoNet.Services;

namespace FakturoNet;

public sealed class MainWindow : Window
{
    private const string WindowBackgroundColor = "#EEF3F7";
    private const string SurfaceColor = "#FFFFFF";
    private const string SurfaceMutedColor = "#F6FAFC";
    private const string SurfaceAccentColor = "#EFF7FA";
    private const string BorderColor = "#D3DFE7";
    private const string BorderStrongColor = "#B7C8D3";
    private const string TextPrimaryColor = "#152534";
    private const string TextSecondaryColor = "#5C6D79";
    private const string AccentColor = "#0B6A88";
    private const string AccentDarkColor = "#084E65";
    private const string AccentSoftColor = "#DCEFF6";
    private const string DangerColor = "#B54840";
    private const string DangerDarkColor = "#91332D";
    private const string InputBackgroundColor = "#FBFDFE";
    private const string InputReadOnlyBackgroundColor = "#EDF4F8";

    private readonly ContractorService _contractorService;
    private readonly InvoiceService _invoiceService;
    private readonly PdfInvoiceRenderer _pdfInvoiceRenderer;

    private readonly List<Contractor> _contractors = [];
    private readonly List<Invoice> _invoices = [];
    private readonly List<InvoiceItemRow> _invoiceItemRows = [];

    private readonly TabControl _tabs = new();
    private readonly TextBlock _statusText = new();
    private readonly Grid _dashboardMetricsGrid = new();
    private readonly StackPanel _recentInvoicesPanel = new();
    private readonly StackPanel _recentContractorsPanel = new();

    private readonly ListBox _contractorList = new();
    private readonly TextBlock _contractorFormTitle = new();
    private readonly TextBox _contractorCompanyNameBox = new();
    private readonly TextBox _contractorTaxIdBox = new();
    private readonly TextBox _contractorStreetBox = new();
    private readonly TextBox _contractorPostalCodeBox = new();
    private readonly TextBox _contractorCityBox = new();
    private readonly TextBox _contractorEmailBox = new();
    private readonly TextBox _contractorPhoneBox = new();

    private readonly TextBox _invoiceNumberBox = new();
    private readonly TextBox _invoiceIssueDateBox = new();
    private readonly TextBox _invoiceDueDateBox = new();
    private readonly TextBox _invoicePaymentMethodBox = new();
    private readonly ComboBox _invoiceContractorCombo = new();
    private readonly TextBox _invoiceCorrectionReasonBox = new();
    private readonly StackPanel _invoiceCorrectionPanel = new();
    private readonly Border _invoiceDocumentBadge = new();
    private readonly TextBlock _invoiceDocumentBadgeText = new();
    private readonly Border _invoiceStatusBadge = new();
    private readonly TextBlock _invoiceStatusBadgeText = new();
    private readonly TextBlock _invoiceReferenceText = new();
    private readonly TextBox _invoiceNotesBox = new();
    private readonly StackPanel _invoiceItemsPanel = new();
    private readonly TextBlock _invoiceNetTotalText = new();
    private readonly TextBlock _invoiceVatTotalText = new();
    private readonly TextBlock _invoiceGrossTotalText = new();
    private readonly TextBlock _invoiceFormTitle = new();
    private readonly TextBlock _invoiceFormSubtitle = new();
    private readonly Button _saveInvoiceButton = new();
    private readonly Button _cancelInvoiceEditButton = new();

    private readonly ListBox _invoiceList = new();
    private readonly StackPanel _invoicePreviewPanel = new();
    private readonly Button _exportInvoiceButton = new();
    private readonly Button _editInvoiceButton = new();
    private readonly Button _createCorrectionButton = new();
    private readonly Button _toggleInvoiceStatusButton = new();

    private Contractor? _selectedContractor;
    private Invoice? _selectedInvoice;
    private Guid? _editingInvoiceId;
    private InvoiceDocumentKind _invoiceDocumentKind = InvoiceDocumentKind.Standard;
    private InvoiceStatus _invoiceEditorStatus = InvoiceStatus.Open;
    private Guid? _correctedInvoiceId;
    private string? _correctedInvoiceNumber;
    private bool _initialized;

    public MainWindow(
        ContractorService contractorService,
        InvoiceService invoiceService,
        PdfInvoiceRenderer pdfInvoiceRenderer)
    {
        _contractorService = contractorService;
        _invoiceService = invoiceService;
        _pdfInvoiceRenderer = pdfInvoiceRenderer;

        Title = "FakturoNet Desktop";
        Width = 1280;
        Height = 780;
        MinWidth = 1024;
        MinHeight = 680;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Brush(WindowBackgroundColor);
        ShowInTaskbar = true;

        ConfigureListBox(_contractorList);
        ConfigureListBox(_invoiceList);

        Content = BuildLoadingView();
        Opened += OnOpened;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        try
        {
            StartupLog.Write("MainWindow.Opened start");
            WindowState = WindowState.Normal;
            Activate();
            Content = BuildRoot();
            StartupLog.Write("MainWindow.BuildRoot done");
            await RefreshDataAsync();
            StartupLog.Write("MainWindow.RefreshDataAsync done");
            ResetContractorForm();
            await ResetInvoiceFormAsync();
            StartupLog.Write("MainWindow.ResetInvoiceFormAsync done");
            SetStatus("Aplikacja desktopowa jest gotowa do pracy.");
        }
        catch (Exception exception)
        {
            StartupLog.Write("MainWindow.Opened exception");
            StartupLog.Write(exception.ToString());
            Content = BuildErrorView(exception);
            Width = 920;
            Height = 640;
            WindowState = WindowState.Normal;
            Activate();
        }
    }

    private Control BuildLoadingView()
    {
        return new Grid
        {
            Children =
            {
                new Border
                {
                    Margin = new Thickness(40),
                    Padding = new Thickness(32),
                    CornerRadius = new CornerRadius(24),
                    Background = Brush(SurfaceColor),
                    BorderBrush = Brush(BorderColor),
                    BorderThickness = new Thickness(1),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = new StackPanel
                    {
                        Width = 520,
                        Spacing = 12,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "FakturoNet",
                                FontSize = 28,
                                FontWeight = FontWeight.Bold,
                                Foreground = Brush(TextPrimaryColor),
                                HorizontalAlignment = HorizontalAlignment.Center
                            },
                            new TextBlock
                            {
                                Text = "Trwa uruchamianie okna aplikacji...",
                                Foreground = Brush(TextSecondaryColor),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                TextWrapping = TextWrapping.Wrap
                            },
                            new SelectableTextBlock
                            {
                                Text = $"Log startu: {StartupLog.GetLogPath()}",
                                Foreground = Brush(AccentDarkColor),
                                HorizontalAlignment = HorizontalAlignment.Center
                            }
                        }
                    }
                }
            }
        };
    }

    private Control BuildErrorView(Exception exception)
    {
        return new ScrollViewer
        {
            Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "FakturoNet - blad inicjalizacji okna",
                        FontSize = 24,
                        FontWeight = FontWeight.Bold,
                        Foreground = Brush(TextPrimaryColor)
                    },
                    new TextBlock
                    {
                        Text = $"Log startu: {StartupLog.GetLogPath()}",
                        FontWeight = FontWeight.SemiBold,
                        Foreground = Brush(AccentDarkColor)
                    },
                    new SelectableTextBlock
                    {
                        Text = exception.ToString(),
                        Foreground = Brush(TextSecondaryColor),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private Control BuildRoot()
    {
        var root = new DockPanel();

        var header = BuildHeader();
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var statusBar = BuildStatusBar();
        DockPanel.SetDock(statusBar, Dock.Bottom);
        root.Children.Add(statusBar);

        _tabs.Margin = new Thickness(22, 18, 22, 18);
        _tabs.ItemsSource = new object[]
        {
            BuildDashboardTab(),
            BuildContractorsTab(),
            BuildInvoiceComposerTab(),
            BuildInvoicesTab()
        };

        root.Children.Add(_tabs);
        return root;
    }

    private Control BuildHeader()
    {
        var layout = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        layout.Children.Add(new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new TextBlock
                {
                    Text = "FakturoNet",
                    FontSize = 30,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brush(TextPrimaryColor)
                },
                new TextBlock
                {
                    Text = "Natywna aplikacja desktopowa w C# do faktur i kontrahentow.",
                    Foreground = Brush(TextSecondaryColor)
                }
            }
        });

        var tag = new Border
        {
            Padding = new Thickness(14, 8),
            CornerRadius = new CornerRadius(999),
            Background = Brush(AccentSoftColor),
            BorderBrush = Brush(BorderStrongColor),
            BorderThickness = new Thickness(1),
            Child = new TextBlock
            {
                Text = "Bez Electrona",
                Foreground = Brush(AccentDarkColor),
                FontWeight = FontWeight.SemiBold
            }
        };
        Grid.SetColumn(tag, 1);
        layout.Children.Add(tag);

        return new Border
        {
            Margin = new Thickness(22, 20, 22, 0),
            Padding = new Thickness(24, 18),
            CornerRadius = new CornerRadius(28),
            Background = Brush(SurfaceColor),
            BorderBrush = Brush(BorderColor),
            BorderThickness = new Thickness(1),
            Child = layout
        };
    }

    private Control BuildStatusBar()
    {
        _statusText.Foreground = Brush(TextPrimaryColor);
        _statusText.Text = "Uruchamianie...";

        return new Border
        {
            Margin = new Thickness(22, 0, 22, 20),
            Padding = new Thickness(18, 12),
            CornerRadius = new CornerRadius(18),
            Background = Brush(SurfaceAccentColor),
            BorderBrush = Brush(BorderColor),
            BorderThickness = new Thickness(1),
            Child = _statusText
        };
    }

    private TabItem BuildDashboardTab()
    {
        _dashboardMetricsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        _dashboardMetricsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        _dashboardMetricsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        _dashboardMetricsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
        _dashboardMetricsGrid.ColumnSpacing = 14;

        _recentInvoicesPanel.Spacing = 10;
        _recentContractorsPanel.Spacing = 10;

        var recentGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star))
            },
            ColumnSpacing = 18
        };

        var invoicesCard = CreateCard(
            "Ostatnie faktury",
            "Szybki podglad wystawionych dokumentow.",
            _recentInvoicesPanel);

        var contractorsCard = CreateCard(
            "Kontrahenci",
            "Najwazniejsze firmy z bazy.",
            _recentContractorsPanel);

        recentGrid.Children.Add(invoicesCard);
        Grid.SetColumn(contractorsCard, 1);
        recentGrid.Children.Add(contractorsCard);

        return new TabItem
        {
            Header = CreateTabHeader("Panel"),
            Content = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Spacing = 18,
                    Children =
                    {
                        CreateIntroCard(),
                        CreateCard("Statystyki", "Najwazniejsze liczby z Twojej bazy.", _dashboardMetricsGrid),
                        recentGrid
                    }
                }
            }
        };
    }

    private TabItem BuildContractorsTab()
    {
        _contractorList.SelectionChanged += (_, _) => SelectContractorFromList();

        var contractorListBody = new DockPanel
        {
            LastChildFill = true
        };

        var contractorToolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 14),
            Children =
            {
                CreateActionButton("Nowy", async () =>
                {
                    ResetContractorForm();
                    await Task.CompletedTask;
                }, isSecondary: true),
                CreateActionButton("Odswiez", async () => await RefreshDataAsync(), isSecondary: true)
            }
        };
        DockPanel.SetDock(contractorToolbar, Dock.Top);
        contractorListBody.Children.Add(contractorToolbar);
        contractorListBody.Children.Add(_contractorList);

        var left = CreateCard(
            "Lista kontrahentow",
            "Wybierz firme z lewej strony, aby ja edytowac.",
            contractorListBody);

        _contractorFormTitle.FontSize = 22;
        _contractorFormTitle.FontWeight = FontWeight.Bold;
        _contractorFormTitle.Foreground = Brush(TextPrimaryColor);

        var formPanel = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                _contractorFormTitle,
                CreateField("Nazwa firmy", _contractorCompanyNameBox, "Np. Studio Kreska"),
                CreateField("NIP", _contractorTaxIdBox, "Np. 7292837465"),
                CreateField("Ulica i numer", _contractorStreetBox, "Np. ul. Prosta 18"),
                CreateField("Kod pocztowy", _contractorPostalCodeBox, "Np. 00-850"),
                CreateField("Miasto", _contractorCityBox, "Np. Warszawa"),
                CreateField("E-mail", _contractorEmailBox, "Np. kontakt@firma.pl"),
                CreateField("Telefon", _contractorPhoneBox, "Np. +48 500 600 700"),
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Margin = new Thickness(0, 10, 0, 0),
                    Children =
                    {
                        CreateActionButton("Zapisz kontrahenta", SaveContractorAsync),
                        CreateActionButton("Usun", DeleteSelectedContractorAsync, isDanger: true),
                        CreateActionButton("Wyczysc formularz", async () =>
                        {
                            ResetContractorForm();
                            await Task.CompletedTask;
                        }, isSecondary: true)
                    }
                }
            }
        };

        var right = CreateCard("Edycja kontrahenta", "Formularz zapisu danych firmy.", formPanel);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(0.42, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(0.58, GridUnitType.Star))
            },
            ColumnSpacing = 18
        };

        grid.Children.Add(left);
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);

        return new TabItem
        {
            Header = CreateTabHeader("Kontrahenci"),
            Content = grid
        };
    }

    private TabItem BuildInvoiceComposerTab()
    {
        _invoiceIssueDateBox.LostFocus += async (_, _) => await RefreshInvoiceNumberPreviewAsync();
        _invoiceItemsPanel.Spacing = 10;
        _invoiceFormTitle.FontSize = 22;
        _invoiceFormTitle.FontWeight = FontWeight.Bold;
        _invoiceFormTitle.Foreground = Brush(TextPrimaryColor);
        _invoiceFormSubtitle.Foreground = Brush(TextSecondaryColor);
        _invoiceFormSubtitle.TextWrapping = TextWrapping.Wrap;
        ConfigureBadge(_invoiceDocumentBadge, _invoiceDocumentBadgeText, AccentSoftColor, AccentDarkColor);
        ConfigureBadge(_invoiceStatusBadge, _invoiceStatusBadgeText, SurfaceMutedColor, TextPrimaryColor);
        _invoiceReferenceText.Foreground = Brush(AccentDarkColor);
        _invoiceReferenceText.FontWeight = FontWeight.SemiBold;
        _invoiceReferenceText.IsVisible = false;
        _invoiceCorrectionPanel.Spacing = 8;
        _invoiceCorrectionPanel.IsVisible = false;
        _invoiceCorrectionPanel.Children.Clear();
        _invoiceCorrectionPanel.Children.Add(CreateField("Powod korekty", _invoiceCorrectionReasonBox, "Np. zmiana pozycji lub ceny"));
        _invoiceCorrectionPanel.Children.Add(new TextBlock
        {
            Text = "Korekta zapisze osobny dokument i zachowa numer faktury zrodlowej.",
            Foreground = Brush(TextSecondaryColor),
            TextWrapping = TextWrapping.Wrap
        });

        var metaPanel = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                CreateField("Numer faktury", _invoiceNumberBox, isReadOnly: true),
                CreateField("Data wystawienia (rrrr-mm-dd)", _invoiceIssueDateBox, "2026-03-19"),
                CreateField("Termin platnosci (rrrr-mm-dd)", _invoiceDueDateBox, "2026-04-02"),
                CreateField("Forma platnosci", _invoicePaymentMethodBox, "Przelew"),
                CreateField("Kontrahent", _invoiceContractorCombo),
                _invoiceCorrectionPanel,
                CreateField("Uwagi", _invoiceNotesBox, "Dodatkowe informacje dla klienta")
            }
        };

        var itemsSection = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        CreateActionButton("Dodaj pozycje", async () =>
                        {
                            AddInvoiceItemRow();
                            await Task.CompletedTask;
                        }, isSecondary: true),
                        CreateActionButton("Wyczysc formularz", ResetInvoiceFormAsync, isSecondary: true)
                    }
                },
                _invoiceItemsPanel
            }
        };

        var editorStatePanel = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                _invoiceFormTitle,
                _invoiceFormSubtitle,
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new Border
                        {
                            Margin = new Thickness(0, 0, 10, 0),
                            Child = _invoiceDocumentBadge
                        },
                        _invoiceStatusBadge
                    }
                },
                _invoiceReferenceText
            }
        };

        var summaryGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star))
            },
            ColumnSpacing = 14
        };

        var netCard = CreateMetricCard("Razem netto", _invoiceNetTotalText);
        var vatCard = CreateMetricCard("VAT", _invoiceVatTotalText);
        var grossCard = CreateMetricCard("Do zaplaty", _invoiceGrossTotalText, highlight: true);

        summaryGrid.Children.Add(netCard);
        Grid.SetColumn(vatCard, 1);
        summaryGrid.Children.Add(vatCard);
        Grid.SetColumn(grossCard, 2);
        summaryGrid.Children.Add(grossCard);

        _saveInvoiceButton.Content = "Zapisz fakture";
        ApplyButtonStyle(_saveInvoiceButton);
        _saveInvoiceButton.Click += async (_, _) => await SaveInvoiceAsync();

        _cancelInvoiceEditButton.Content = "Anuluj edycje";
        ApplyButtonStyle(_cancelInvoiceEditButton, isSecondary: true);
        _cancelInvoiceEditButton.IsVisible = false;
        _cancelInvoiceEditButton.Click += async (_, _) => await ResetInvoiceFormAsync();

        return new TabItem
        {
            Header = CreateTabHeader("Nowa faktura"),
            Content = new ScrollViewer
            {
                Content = new StackPanel
                {
                    Spacing = 18,
                    Children =
                    {
                        editorStatePanel,
                        CreateCard("Naglowek faktury", "Ustaw podstawowe dane dokumentu.", metaPanel),
                        CreateCard("Pozycje faktury", "Dodawaj uslugi lub produkty i licz netto, VAT oraz brutto.", itemsSection),
                        summaryGrid,
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 10,
                            Children =
                            {
                                _saveInvoiceButton,
                                _cancelInvoiceEditButton,
                                CreateActionButton("Przejdz do listy faktur", async () =>
                                {
                                    _tabs.SelectedIndex = 3;
                                    await Task.CompletedTask;
                                }, isSecondary: true)
                            }
                        }
                    }
                }
            }
        };
    }

    private TabItem BuildInvoicesTab()
    {
        _invoiceList.SelectionChanged += (_, _) => SelectInvoiceFromList();

        _editInvoiceButton.Content = "Edytuj fakture";
        _editInvoiceButton.IsEnabled = false;
        ApplyButtonStyle(_editInvoiceButton, isSecondary: true);
        _editInvoiceButton.Click += async (_, _) => await StartEditingSelectedInvoiceAsync();

        _createCorrectionButton.Content = "Wystaw korekte";
        _createCorrectionButton.IsEnabled = false;
        ApplyButtonStyle(_createCorrectionButton, isSecondary: true);
        _createCorrectionButton.Click += async (_, _) => await StartCorrectionForSelectedInvoiceAsync();

        _toggleInvoiceStatusButton.Content = "Zamknij fakture";
        _toggleInvoiceStatusButton.IsEnabled = false;
        ApplyButtonStyle(_toggleInvoiceStatusButton, isSecondary: true);
        _toggleInvoiceStatusButton.Click += async (_, _) => await ToggleSelectedInvoiceStatusAsync();

        _exportInvoiceButton.Content = "Eksportuj PDF do Downloads";
        _exportInvoiceButton.IsEnabled = false;
        ApplyButtonStyle(_exportInvoiceButton);
        _exportInvoiceButton.Click += async (_, _) => await ExportSelectedInvoicePdfAsync();

        var left = CreateCard(
            "Wystawione faktury",
            "Wybierz dokument, aby zobaczyc szczegoly i pobrac PDF.",
            _invoiceList);

        var rightContent = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    Children =
                    {
                        _editInvoiceButton,
                        _createCorrectionButton,
                        _toggleInvoiceStatusButton,
                        _exportInvoiceButton
                    }
                },
                _invoicePreviewPanel
            }
        };

        var right = CreateCard(
            "Podglad dokumentu",
            "Szczegoly wybranej faktury i pozycje do rozliczenia.",
            new ScrollViewer
            {
                Content = rightContent
            });

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(0.42, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(0.58, GridUnitType.Star))
            },
            ColumnSpacing = 18
        };

        grid.Children.Add(left);
        Grid.SetColumn(right, 1);
        grid.Children.Add(right);

        return new TabItem
        {
            Header = CreateTabHeader("Faktury"),
            Content = grid
        };
    }

    private Control CreateIntroCard()
    {
        return new Border
        {
            Padding = new Thickness(28),
            CornerRadius = new CornerRadius(28),
            Background = Brush(SurfaceColor),
            BorderBrush = Brush(BorderColor),
            BorderThickness = new Thickness(1),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Border
                    {
                        Padding = new Thickness(12, 6),
                        CornerRadius = new CornerRadius(999),
                        Background = Brush(AccentSoftColor),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Child = new TextBlock
                        {
                            Text = "Przejrzysty panel pracy",
                            FontSize = 12,
                            FontWeight = FontWeight.SemiBold,
                            Foreground = Brush(AccentDarkColor)
                        }
                    },
                    new TextBlock
                    {
                        Text = "Wystawiaj faktury, zarzadzaj kontrahentami i zapisuj PDF bez przegladarkowej otoczki.",
                        FontSize = 30,
                        FontWeight = FontWeight.Bold,
                        Foreground = Brush(TextPrimaryColor),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = "Ta wersja aplikacji jest natywna dla .NET, a dane przechowuje lokalnie w JSON.",
                        Foreground = Brush(TextSecondaryColor),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private Border CreateCard(string title, string subtitle, Control body)
    {
        return new Border
        {
            Padding = new Thickness(22),
            CornerRadius = new CornerRadius(24),
            Background = Brush(SurfaceColor),
            BorderBrush = Brush(BorderColor),
            BorderThickness = new Thickness(1),
            Child = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 4,
                        Children =
                        {
                            new Border
                            {
                                Width = 44,
                                Height = 4,
                                CornerRadius = new CornerRadius(999),
                                Background = Brush(AccentColor),
                                HorizontalAlignment = HorizontalAlignment.Left
                            },
                            new TextBlock
                            {
                                Text = title,
                                FontSize = 21,
                                FontWeight = FontWeight.Bold,
                                Foreground = Brush(TextPrimaryColor)
                            },
                            new TextBlock
                            {
                                Text = subtitle,
                                Foreground = Brush(TextSecondaryColor),
                                TextWrapping = TextWrapping.Wrap
                            }
                        }
                    },
                    body
                }
            }
        };
    }

    private Border CreateMetricCard(string label, TextBlock valueTarget, bool highlight = false)
    {
        valueTarget.FontSize = 28;
        valueTarget.FontWeight = FontWeight.Bold;
        valueTarget.Text = "0.00";
        valueTarget.Foreground = highlight ? Brushes.White : Brush(TextPrimaryColor);

        var accentForeground = highlight ? Brush(SurfaceColor) : Brush(TextSecondaryColor);

        return new Border
        {
            Padding = new Thickness(18),
            CornerRadius = new CornerRadius(22),
            Background = highlight ? Brush(AccentColor) : Brush(SurfaceMutedColor),
            BorderBrush = highlight ? Brush(AccentDarkColor) : Brush(BorderColor),
            BorderThickness = new Thickness(1),
            Child = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock
                    {
                        Text = label,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = accentForeground
                    },
                    valueTarget
                }
            }
        };
    }

    private Control CreateField(string label, Control control, string? watermark = null, bool isReadOnly = false)
    {
        if (control is TextBox textBox)
        {
            textBox.Watermark = watermark;
            textBox.IsReadOnly = isReadOnly;
            textBox.Background = Brush(isReadOnly ? InputReadOnlyBackgroundColor : InputBackgroundColor);
            textBox.Foreground = Brush(TextPrimaryColor);
            textBox.BorderBrush = Brush(isReadOnly ? BorderStrongColor : BorderColor);
            textBox.BorderThickness = new Thickness(1);
            textBox.Padding = new Thickness(12, 10);
            textBox.CornerRadius = new CornerRadius(16);
            textBox.MinHeight = 46;
        }

        if (control is ComboBox comboBox)
        {
            comboBox.Background = Brush(InputBackgroundColor);
            comboBox.Foreground = Brush(TextPrimaryColor);
            comboBox.BorderBrush = Brush(BorderColor);
            comboBox.BorderThickness = new Thickness(1);
            comboBox.Padding = new Thickness(10, 8);
            comboBox.MinHeight = 46;
        }

        return new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    FontSize = 13,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = Brush(TextPrimaryColor)
                },
                control
            }
        };
    }

    private Button CreateActionButton(string text, Func<Task> action, bool isSecondary = false, bool isDanger = false)
    {
        var button = new Button
        {
            Content = text
        };

        ApplyButtonStyle(button, isSecondary, isDanger);
        button.Click += async (_, _) => await action();
        return button;
    }

    private static TextBlock CreateTabHeader(string text) =>
        new()
        {
            Text = text,
            Foreground = Brush(TextPrimaryColor),
            FontWeight = FontWeight.SemiBold
        };

    private void ApplyButtonStyle(Button button, bool isSecondary = false, bool isDanger = false)
    {
        var background = isDanger
            ? Brush(DangerColor)
            : isSecondary
                ? Brush(SurfaceColor)
                : Brush(AccentColor);

        var foreground = isDanger || !isSecondary
            ? Brush(SurfaceColor)
            : Brush(TextPrimaryColor);

        var border = isDanger
            ? Brush(DangerDarkColor)
            : isSecondary
                ? Brush(BorderStrongColor)
                : Brush(AccentDarkColor);

        button.Background = background;
        button.Foreground = foreground;
        button.BorderBrush = border;
        button.BorderThickness = new Thickness(1);
        button.Padding = new Thickness(18, 12);
        button.CornerRadius = new CornerRadius(999);
        button.FontWeight = FontWeight.SemiBold;
        button.MinHeight = 46;
    }

    private void ConfigureBadge(Border badge, TextBlock text, string backgroundColor, string foregroundColor)
    {
        text.Foreground = Brush(foregroundColor);
        text.FontSize = 12;
        text.FontWeight = FontWeight.SemiBold;

        badge.Background = Brush(backgroundColor);
        badge.BorderBrush = Brush(BorderStrongColor);
        badge.BorderThickness = new Thickness(1);
        badge.CornerRadius = new CornerRadius(999);
        badge.Padding = new Thickness(12, 6);
        badge.Child = text;
    }

    private void ConfigureListBox(ListBox listBox)
    {
        listBox.Background = Brush(SurfaceMutedColor);
        listBox.BorderBrush = Brush(BorderColor);
        listBox.BorderThickness = new Thickness(1);
        listBox.FontSize = 14;
        listBox.ItemTemplate = CreateListEntryTemplate();
    }

    private static IDataTemplate CreateListEntryTemplate() =>
        new FuncDataTemplate<ListEntry>((entry, _) =>
            new StackPanel
            {
                Margin = new Thickness(8, 10),
                Spacing = 3,
                Children =
                {
                    new TextBlock
                    {
                        Text = entry?.Title ?? string.Empty,
                        FontSize = 15,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = Brush(TextPrimaryColor),
                        TextWrapping = TextWrapping.Wrap
                    },
                    new TextBlock
                    {
                        Text = entry?.Subtitle ?? string.Empty,
                        FontSize = 12,
                        Foreground = Brush(TextSecondaryColor),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            });

    private async Task RefreshDataAsync(Guid? contractorToSelect = null, Guid? invoiceToSelect = null)
    {
        _contractors.Clear();
        _contractors.AddRange(await _contractorService.GetAllAsync());

        _invoices.Clear();
        _invoices.AddRange(await _invoiceService.GetAllAsync());

        RefreshDashboard();
        RefreshContractorList();
        RefreshContractorCombo();
        RefreshInvoiceList();

        if (contractorToSelect.HasValue)
        {
            SelectContractorById(contractorToSelect.Value);
        }
        else
        {
            _contractorList.SelectedIndex = -1;
        }

        if (invoiceToSelect.HasValue)
        {
            SelectInvoiceById(invoiceToSelect.Value);
        }
        else if (_selectedInvoice is not null)
        {
            SelectInvoiceById(_selectedInvoice.Id);
        }
        else
        {
            _invoiceList.SelectedIndex = -1;
            RenderInvoicePreview(null);
        }
    }

    private void RefreshDashboard()
    {
        _dashboardMetricsGrid.Children.Clear();
        _recentInvoicesPanel.Children.Clear();
        _recentContractorsPanel.Children.Clear();

        var dashboard = _invoiceService.BuildDashboard(_contractors, _invoices);
        var values = new[]
        {
            $"{dashboard.ContractorCount}",
            $"{dashboard.InvoiceCount}",
            $"{dashboard.TotalGross:0.00} PLN",
            $"{dashboard.CurrentMonthGross:0.00} PLN"
        };

        var labels = new[]
        {
            "Kontrahenci",
            "Faktury",
            "Przychod lacznie",
            "Biezacy miesiac"
        };

        for (var index = 0; index < values.Length; index++)
        {
            var valueText = new TextBlock();
            var card = CreateMetricCard(labels[index], valueText, highlight: index == values.Length - 1);
            valueText.Text = values[index];
            Grid.SetColumn(card, index);
            _dashboardMetricsGrid.Children.Add(card);
        }

        if (dashboard.RecentInvoices.Count == 0)
        {
            _recentInvoicesPanel.Children.Add(CreateListHint("Brak faktur. Wystaw pierwsza z zakladki 'Nowa faktura'."));
        }
        else
        {
            foreach (var invoice in dashboard.RecentInvoices)
            {
                _recentInvoicesPanel.Children.Add(CreateInfoTile(
                    $"{invoice.GetDocumentShortLabel()} {invoice.Number}",
                    $"{invoice.Buyer.CompanyName} · {invoice.GetStatusLabel()} · {invoice.GrossTotal:0.00} PLN"));
            }
        }

        if (dashboard.Contractors.Count == 0)
        {
            _recentContractorsPanel.Children.Add(CreateListHint("Brak kontrahentow. Dodaj pierwsza firme w zakladce 'Kontrahenci'."));
        }
        else
        {
            foreach (var contractor in dashboard.Contractors)
            {
                _recentContractorsPanel.Children.Add(CreateInfoTile(
                    contractor.CompanyName,
                    $"{contractor.TaxId} · {contractor.City}"));
            }
        }
    }

    private Border CreateInfoTile(string title, string subtitle)
    {
        return new Border
        {
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(18),
            Background = Brush(SurfaceMutedColor),
            BorderBrush = Brush(BorderColor),
            BorderThickness = new Thickness(1),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        FontSize = 15,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = Brush(TextPrimaryColor)
                    },
                    new TextBlock
                    {
                        Text = subtitle,
                        Foreground = Brush(TextSecondaryColor),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private TextBlock CreateListHint(string text) =>
        new()
        {
            Text = text,
            Foreground = Brush(TextSecondaryColor),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(2)
        };

    private void RefreshContractorList()
    {
        _contractorList.ItemsSource = _contractors
            .Select(contractor => new ListEntry(
                contractor.CompanyName,
                $"{contractor.TaxId} · {contractor.City}"))
            .ToList();
    }

    private void RefreshContractorCombo()
    {
        _invoiceContractorCombo.ItemsSource = _contractors
            .Select(contractor => $"{contractor.CompanyName} ({contractor.TaxId})")
            .ToList();
    }

    private void RefreshInvoiceList()
    {
        _invoiceList.ItemsSource = _invoices
            .Select(invoice => new ListEntry(
                $"{invoice.GetDocumentShortLabel()} {invoice.Number}",
                $"{invoice.Buyer.CompanyName} · {invoice.GetStatusLabel()} · {invoice.GrossTotal:0.00} PLN"))
            .ToList();
    }

    private void ResetContractorForm()
    {
        _selectedContractor = null;
        _contractorList.SelectedIndex = -1;
        _contractorFormTitle.Text = "Nowy kontrahent";
        _contractorCompanyNameBox.Text = string.Empty;
        _contractorTaxIdBox.Text = string.Empty;
        _contractorStreetBox.Text = string.Empty;
        _contractorPostalCodeBox.Text = string.Empty;
        _contractorCityBox.Text = string.Empty;
        _contractorEmailBox.Text = string.Empty;
        _contractorPhoneBox.Text = string.Empty;
    }

    private void PopulateContractorForm(Contractor contractor)
    {
        _contractorFormTitle.Text = $"Edycja: {contractor.CompanyName}";
        _contractorCompanyNameBox.Text = contractor.CompanyName;
        _contractorTaxIdBox.Text = contractor.TaxId;
        _contractorStreetBox.Text = contractor.Street;
        _contractorPostalCodeBox.Text = contractor.PostalCode;
        _contractorCityBox.Text = contractor.City;
        _contractorEmailBox.Text = contractor.Email;
        _contractorPhoneBox.Text = contractor.Phone;
    }

    private void SelectContractorFromList()
    {
        var index = _contractorList.SelectedIndex;
        if (index < 0 || index >= _contractors.Count)
        {
            _selectedContractor = null;
            return;
        }

        _selectedContractor = _contractors[index];
        PopulateContractorForm(_selectedContractor);
    }

    private void SelectContractorById(Guid id)
    {
        var index = _contractors.FindIndex(contractor => contractor.Id == id);
        if (index >= 0)
        {
            _contractorList.SelectedIndex = index;
            _selectedContractor = _contractors[index];
            PopulateContractorForm(_selectedContractor);
        }
    }

    private async Task SaveContractorAsync()
    {
        var contractor = new Contractor
        {
            Id = _selectedContractor?.Id ?? Guid.NewGuid(),
            CompanyName = _contractorCompanyNameBox.Text?.Trim() ?? string.Empty,
            TaxId = _contractorTaxIdBox.Text?.Trim() ?? string.Empty,
            Street = _contractorStreetBox.Text?.Trim() ?? string.Empty,
            PostalCode = _contractorPostalCodeBox.Text?.Trim() ?? string.Empty,
            City = _contractorCityBox.Text?.Trim() ?? string.Empty,
            Email = _contractorEmailBox.Text?.Trim() ?? string.Empty,
            Phone = _contractorPhoneBox.Text?.Trim() ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(contractor.CompanyName) ||
            string.IsNullOrWhiteSpace(contractor.TaxId) ||
            string.IsNullOrWhiteSpace(contractor.Street) ||
            string.IsNullOrWhiteSpace(contractor.PostalCode) ||
            string.IsNullOrWhiteSpace(contractor.City))
        {
            SetStatus("Uzupelnij nazwe firmy, NIP i pelny adres kontrahenta.");
            return;
        }

        await _contractorService.SaveAsync(contractor);
        await RefreshDataAsync(contractor.Id, _selectedInvoice?.Id);
        SetStatus($"Kontrahent '{contractor.CompanyName}' zostal zapisany.");
    }

    private async Task DeleteSelectedContractorAsync()
    {
        if (_selectedContractor is null)
        {
            SetStatus("Najpierw wybierz kontrahenta do usuniecia.");
            return;
        }

        if (await _invoiceService.HasOpenInvoicesForContractorAsync(_selectedContractor.Id))
        {
            SetStatus("Nie mozna usunac kontrahenta, dopoki ma przypisana co najmniej jedna otwarta fakture.");
            return;
        }

        var companyName = _selectedContractor.CompanyName;
        await _contractorService.DeleteAsync(_selectedContractor.Id);
        ResetContractorForm();
        await RefreshDataAsync();
        SetStatus($"Kontrahent '{companyName}' zostal usuniety.");
    }

    private async Task ResetInvoiceFormAsync()
    {
        _editingInvoiceId = null;
        _invoiceDocumentKind = InvoiceDocumentKind.Standard;
        _invoiceEditorStatus = InvoiceStatus.Open;
        _correctedInvoiceId = null;
        _correctedInvoiceNumber = null;
        _invoiceIssueDateBox.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _invoiceDueDateBox.Text = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _invoicePaymentMethodBox.Text = "Przelew";
        _invoiceContractorCombo.SelectedIndex = _contractors.Count > 0 ? 0 : -1;
        _invoiceCorrectionReasonBox.Text = string.Empty;
        _invoiceNotesBox.Text = string.Empty;
        _invoiceItemsPanel.Children.Clear();
        _invoiceItemRows.Clear();
        AddInvoiceItemRow();
        await RefreshInvoiceNumberPreviewAsync();
        UpdateInvoiceEditorContext(
            "Nowa faktura",
            "Tworz nowy dokument i zapisuj go do lokalnej bazy.",
            "Zapisz fakture",
            showCancel: false);
        RecalculateInvoiceSummary();
    }

    private async Task RefreshInvoiceNumberPreviewAsync()
    {
        if (_editingInvoiceId.HasValue)
        {
            return;
        }

        if (!TryParseDate(_invoiceIssueDateBox.Text, out var issueDate))
        {
            _invoiceNumberBox.Text = "Niepoprawna data";
            return;
        }

        _invoiceNumberBox.Text = _invoiceDocumentKind == InvoiceDocumentKind.Correction
            ? await _invoiceService.GenerateNextCorrectionNumberAsync(issueDate)
            : await _invoiceService.GenerateNextNumberAsync(issueDate);
    }

    private void AddInvoiceItemRow(InvoiceItem? preset = null)
    {
        var row = new InvoiceItemRow(preset, RecalculateInvoiceSummary, RemoveInvoiceItemRow);
        _invoiceItemRows.Add(row);
        _invoiceItemsPanel.Children.Add(row.View);
        RecalculateInvoiceSummary();
    }

    private void RemoveInvoiceItemRow(InvoiceItemRow row)
    {
        _invoiceItemRows.Remove(row);
        _invoiceItemsPanel.Children.Remove(row.View);

        if (_invoiceItemRows.Count == 0)
        {
            AddInvoiceItemRow();
        }

        RecalculateInvoiceSummary();
    }

    private void RecalculateInvoiceSummary()
    {
        decimal net = 0;
        decimal vat = 0;
        decimal gross = 0;

        foreach (var row in _invoiceItemRows)
        {
            var totals = row.CalculateTotals();
            net += totals.Net;
            vat += totals.Vat;
            gross += totals.Gross;
        }

        _invoiceNetTotalText.Text = $"{net:0.00} PLN";
        _invoiceVatTotalText.Text = $"{vat:0.00} PLN";
        _invoiceGrossTotalText.Text = $"{gross:0.00} PLN";
    }

    private async Task SaveInvoiceAsync()
    {
        if (!TryParseDate(_invoiceIssueDateBox.Text, out var issueDate))
        {
            SetStatus("Podaj poprawna date wystawienia w formacie rrrr-mm-dd.");
            return;
        }

        if (!TryParseDate(_invoiceDueDateBox.Text, out var dueDate))
        {
            SetStatus("Podaj poprawny termin platnosci w formacie rrrr-mm-dd.");
            return;
        }

        if (dueDate < issueDate)
        {
            SetStatus("Termin platnosci nie moze byc wczesniejszy niz data wystawienia.");
            return;
        }

        if (_invoiceContractorCombo.SelectedIndex < 0 || _invoiceContractorCombo.SelectedIndex >= _contractors.Count)
        {
            SetStatus("Wybierz kontrahenta do faktury.");
            return;
        }

        var items = _invoiceItemRows
            .Select(row => row.BuildItem())
            .Where(item => !string.IsNullOrWhiteSpace(item.Description))
            .ToList();

        if (items.Count == 0)
        {
            SetStatus("Dodaj przynajmniej jedna pozycje faktury.");
            return;
        }

        var form = new InvoiceFormViewModel
        {
            Id = _editingInvoiceId,
            DocumentKind = _invoiceDocumentKind,
            Status = _invoiceEditorStatus,
            Number = _invoiceNumberBox.Text ?? string.Empty,
            CorrectedInvoiceId = _correctedInvoiceId,
            CorrectedInvoiceNumber = _correctedInvoiceNumber,
            CorrectionReason = string.IsNullOrWhiteSpace(_invoiceCorrectionReasonBox.Text) ? null : _invoiceCorrectionReasonBox.Text.Trim(),
            IssueDate = issueDate,
            DueDate = dueDate,
            PaymentMethod = _invoicePaymentMethodBox.Text?.Trim() ?? "Przelew",
            ContractorId = _contractors[_invoiceContractorCombo.SelectedIndex].Id,
            Notes = string.IsNullOrWhiteSpace(_invoiceNotesBox.Text) ? null : _invoiceNotesBox.Text.Trim(),
            Items = items
        };

        var wasEditing = _editingInvoiceId.HasValue;
        var invoice = wasEditing
            ? await _invoiceService.UpdateAsync(_editingInvoiceId!.Value, form)
            : await _invoiceService.CreateAsync(form);
        await RefreshDataAsync(invoiceToSelect: invoice.Id);
        await ResetInvoiceFormAsync();
        _tabs.SelectedIndex = 3;
        SetStatus(wasEditing
            ? $"{invoice.GetDocumentLabel()} '{invoice.Number}' zostala zaktualizowana."
            : $"{invoice.GetDocumentLabel()} '{invoice.Number}' zostala zapisana.");
    }

    private void SelectInvoiceFromList()
    {
        var index = _invoiceList.SelectedIndex;
        if (index < 0 || index >= _invoices.Count)
        {
            _selectedInvoice = null;
            RenderInvoicePreview(null);
            return;
        }

        _selectedInvoice = _invoices[index];
        RenderInvoicePreview(_selectedInvoice);
    }

    private void SelectInvoiceById(Guid id)
    {
        var index = _invoices.FindIndex(invoice => invoice.Id == id);
        if (index >= 0)
        {
            _invoiceList.SelectedIndex = index;
            _selectedInvoice = _invoices[index];
            RenderInvoicePreview(_selectedInvoice);
        }
    }

    private void RenderInvoicePreview(Invoice? invoice)
    {
        _invoicePreviewPanel.Children.Clear();
        _invoicePreviewPanel.Spacing = 10;
        UpdateInvoiceActionButtons(invoice);

        if (invoice is null)
        {
            _invoicePreviewPanel.Children.Add(CreateListHint("Wybierz fakture z listy po lewej, aby zobaczyc szczegoly."));
            return;
        }

        _invoicePreviewPanel.Children.Add(CreateInfoTile(
            $"{invoice.GetDocumentLabel()} {invoice.Number}",
            $"Status: {invoice.GetStatusLabel()} · Data: {invoice.IssueDate:yyyy-MM-dd} · Termin: {invoice.DueDate:yyyy-MM-dd} · {invoice.PaymentMethod}"));

        if (invoice.DocumentKind == InvoiceDocumentKind.Correction && !string.IsNullOrWhiteSpace(invoice.CorrectedInvoiceNumber))
        {
            _invoicePreviewPanel.Children.Add(CreateInfoTile(
                "Dokument zrodlowy",
                $"Korekta do faktury: {invoice.CorrectedInvoiceNumber}"));
        }

        if (!string.IsNullOrWhiteSpace(invoice.CorrectionReason))
        {
            _invoicePreviewPanel.Children.Add(CreateInfoTile("Powod korekty", invoice.CorrectionReason));
        }

        _invoicePreviewPanel.Children.Add(CreateInfoTile(
            "Sprzedawca",
            $"{invoice.Seller.CompanyName}\nNIP: {invoice.Seller.TaxId}\n{invoice.Seller.Street}\n{invoice.Seller.PostalCode} {invoice.Seller.City}"));

        _invoicePreviewPanel.Children.Add(CreateInfoTile(
            "Nabywca",
            $"{invoice.Buyer.CompanyName}\nNIP: {invoice.Buyer.TaxId}\n{invoice.Buyer.Street}\n{invoice.Buyer.PostalCode} {invoice.Buyer.City}"));

        foreach (var item in invoice.Items)
        {
            _invoicePreviewPanel.Children.Add(CreateInfoTile(
                item.Description,
                $"{item.Quantity:0.##} {item.Unit} · netto {item.LineNetTotal:0.00} PLN · VAT {item.LineVatTotal:0.00} PLN · brutto {item.LineGrossTotal:0.00} PLN"));
        }

        _invoicePreviewPanel.Children.Add(CreateInfoTile(
            "Podsumowanie",
            $"Netto: {invoice.NetTotal:0.00} PLN\nVAT: {invoice.VatTotal:0.00} PLN\nDo zaplaty: {invoice.GrossTotal:0.00} PLN"));

        if (!string.IsNullOrWhiteSpace(invoice.Notes))
        {
            _invoicePreviewPanel.Children.Add(CreateInfoTile("Uwagi", invoice.Notes));
        }
    }

    private async Task StartEditingSelectedInvoiceAsync()
    {
        if (_selectedInvoice is null)
        {
            SetStatus("Wybierz fakture, aby przejsc do edycji.");
            return;
        }

        await LoadInvoiceIntoEditorAsync(_selectedInvoice);
        _tabs.SelectedIndex = 2;
        SetStatus($"Edytujesz dokument '{_selectedInvoice.Number}'.");
    }

    private async Task StartCorrectionForSelectedInvoiceAsync()
    {
        if (_selectedInvoice is null)
        {
            SetStatus("Wybierz fakture, aby wystawic korekte.");
            return;
        }

        if (_selectedInvoice.DocumentKind == InvoiceDocumentKind.Correction)
        {
            SetStatus("Korekte mozna wystawic tylko do zwyklej faktury, nie do innej korekty.");
            return;
        }

        var form = await _invoiceService.CreateCorrectionFormModelAsync(_selectedInvoice, _contractors);
        ApplyInvoiceFormToEditor(
            form,
            title: $"Korekta do {form.CorrectedInvoiceNumber}",
            subtitle: "Tworzysz nowy dokument korygujacy na podstawie wybranej faktury.",
            saveButtonText: "Zapisz korekte",
            showCancel: true);
        _tabs.SelectedIndex = 2;
        SetStatus($"Przygotowano korekte do faktury '{_selectedInvoice.Number}'.");
    }

    private async Task ToggleSelectedInvoiceStatusAsync()
    {
        if (_selectedInvoice is null)
        {
            SetStatus("Wybierz fakture, aby zmienic jej status.");
            return;
        }

        var newStatus = _selectedInvoice.Status == InvoiceStatus.Open
            ? InvoiceStatus.Closed
            : InvoiceStatus.Open;

        var invoice = await _invoiceService.SetStatusAsync(_selectedInvoice.Id, newStatus);
        await RefreshDataAsync(invoiceToSelect: invoice.Id);
        SetStatus(newStatus == InvoiceStatus.Closed
            ? $"Faktura '{invoice.Number}' zostala zamknieta."
            : $"Faktura '{invoice.Number}' zostala ponownie otwarta.");
    }

    private async Task ExportSelectedInvoicePdfAsync()
    {
        if (_selectedInvoice is null)
        {
            SetStatus("Wybierz fakture, aby zapisac PDF.");
            return;
        }

        var downloadsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");

        Directory.CreateDirectory(downloadsDirectory);

        var fileName = $"{_selectedInvoice.Number.Replace('/', '-')}.pdf";
        var path = Path.Combine(downloadsDirectory, fileName);
        var pdf = _pdfInvoiceRenderer.Render(_selectedInvoice);

        await File.WriteAllBytesAsync(path, pdf);
        SetStatus($"PDF zapisany w: {path}");
    }

    private void UpdateInvoiceActionButtons(Invoice? invoice)
    {
        _exportInvoiceButton.IsEnabled = invoice is not null;
        _editInvoiceButton.IsEnabled = invoice is not null;
        _toggleInvoiceStatusButton.IsEnabled = invoice is not null;
        _createCorrectionButton.IsEnabled = invoice is not null && invoice.DocumentKind == InvoiceDocumentKind.Standard;

        _toggleInvoiceStatusButton.Content = invoice?.Status == InvoiceStatus.Closed
            ? "Otworz ponownie"
            : "Zamknij fakture";
    }

    private void UpdateInvoiceEditorContext(string title, string subtitle, string saveButtonText, bool showCancel)
    {
        _invoiceFormTitle.Text = title;
        _invoiceFormSubtitle.Text = subtitle;
        _saveInvoiceButton.Content = saveButtonText;
        _cancelInvoiceEditButton.IsVisible = showCancel;
        _invoiceDocumentBadgeText.Text = _invoiceDocumentKind == InvoiceDocumentKind.Correction
            ? "Korekta faktury"
            : "Faktura VAT";
        _invoiceStatusBadgeText.Text = _invoiceEditorStatus == InvoiceStatus.Closed
            ? "Zamknieta"
            : "Otwarta";
        _invoiceDocumentBadge.Background = _invoiceDocumentKind == InvoiceDocumentKind.Correction
            ? Brush("#FFF0D8")
            : Brush(AccentSoftColor);
        _invoiceDocumentBadgeText.Foreground = _invoiceDocumentKind == InvoiceDocumentKind.Correction
            ? Brush("#8A5A12")
            : Brush(AccentDarkColor);
        _invoiceStatusBadge.Background = _invoiceEditorStatus == InvoiceStatus.Closed
            ? Brush("#EAEFF3")
            : Brush("#E4F5EB");
        _invoiceStatusBadgeText.Foreground = _invoiceEditorStatus == InvoiceStatus.Closed
            ? Brush(TextSecondaryColor)
            : Brush("#1D6B43");
        _invoiceCorrectionPanel.IsVisible = _invoiceDocumentKind == InvoiceDocumentKind.Correction;
        _invoiceReferenceText.IsVisible = _invoiceDocumentKind == InvoiceDocumentKind.Correction && !string.IsNullOrWhiteSpace(_correctedInvoiceNumber);
        _invoiceReferenceText.Text = _invoiceReferenceText.IsVisible
            ? $"Dokument zrodlowy: {_correctedInvoiceNumber}"
            : string.Empty;
    }

    private void ApplyInvoiceFormToEditor(
        InvoiceFormViewModel form,
        string title,
        string subtitle,
        string saveButtonText,
        bool showCancel)
    {
        _editingInvoiceId = form.Id;
        _invoiceDocumentKind = form.DocumentKind;
        _invoiceEditorStatus = form.Status;
        _correctedInvoiceId = form.CorrectedInvoiceId;
        _correctedInvoiceNumber = form.CorrectedInvoiceNumber;

        _invoiceNumberBox.Text = form.Number;
        _invoiceIssueDateBox.Text = form.IssueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _invoiceDueDateBox.Text = form.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _invoicePaymentMethodBox.Text = form.PaymentMethod;
        _invoiceCorrectionReasonBox.Text = form.CorrectionReason ?? string.Empty;
        _invoiceNotesBox.Text = form.Notes ?? string.Empty;

        var contractorIndex = _contractors.FindIndex(contractor => contractor.Id == form.ContractorId);
        _invoiceContractorCombo.SelectedIndex = contractorIndex;

        _invoiceItemsPanel.Children.Clear();
        _invoiceItemRows.Clear();

        foreach (var item in form.Items)
        {
            AddInvoiceItemRow(item);
        }

        if (_invoiceItemRows.Count == 0)
        {
            AddInvoiceItemRow();
        }

        UpdateInvoiceEditorContext(title, subtitle, saveButtonText, showCancel);
        RecalculateInvoiceSummary();
    }

    private void SetStatus(string message)
    {
        _statusText.Text = $"{DateTime.Now:HH:mm:ss} · {message}";
    }

    private async Task LoadInvoiceIntoEditorAsync(Invoice invoice)
    {
        var form = _invoiceService.CreateFormModel(invoice, _contractors);
        var title = invoice.DocumentKind == InvoiceDocumentKind.Correction
            ? $"Edycja korekty {invoice.Number}"
            : $"Edycja faktury {invoice.Number}";

        ApplyInvoiceFormToEditor(
            form,
            title,
            "Zmien dane dokumentu, pozycje lub kontrahenta, a potem zapisz poprawki.",
            "Zapisz zmiany",
            showCancel: true);
        await Task.CompletedTask;
    }

    private static bool TryParseDate(string? value, out DateTime date) =>
        DateTime.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);

    private static SolidColorBrush Brush(string hex) => new(Color.Parse(hex));

    private sealed record ListEntry(string Title, string Subtitle);

    private sealed class InvoiceItemRow
    {
        private readonly Action _onChanged;
        private readonly Action<InvoiceItemRow> _onRemove;
        private readonly TextBox _descriptionBox = new();
        private readonly TextBox _quantityBox = new();
        private readonly TextBox _unitBox = new();
        private readonly TextBox _netPriceBox = new();
        private readonly TextBox _vatRateBox = new();
        private readonly TextBlock _netValueText = new();
        private readonly TextBlock _vatValueText = new();
        private readonly TextBlock _grossValueText = new();

        public InvoiceItemRow(InvoiceItem? preset, Action onChanged, Action<InvoiceItemRow> onRemove)
        {
            _onChanged = onChanged;
            _onRemove = onRemove;

            _descriptionBox.Text = preset?.Description ?? string.Empty;
            _descriptionBox.Watermark = "Np. abonament miesieczny";
            _descriptionBox.MinWidth = 260;

            _quantityBox.Text = (preset?.Quantity ?? 1m).ToString("0.##", CultureInfo.InvariantCulture);
            _quantityBox.Width = 84;

            _unitBox.Text = preset?.Unit ?? "szt.";
            _unitBox.Width = 84;

            _netPriceBox.Text = (preset?.UnitNetPrice ?? 0m).ToString("0.00", CultureInfo.InvariantCulture);
            _netPriceBox.Width = 110;

            _vatRateBox.Text = (preset?.VatRate ?? 23m).ToString("0.##", CultureInfo.InvariantCulture);
            _vatRateBox.Width = 84;

            foreach (var box in new[] { _descriptionBox, _quantityBox, _unitBox, _netPriceBox, _vatRateBox })
            {
                box.Background = Brush(InputBackgroundColor);
                box.Foreground = Brush(TextPrimaryColor);
                box.BorderBrush = Brush(BorderColor);
                box.BorderThickness = new Thickness(1);
                box.Padding = new Thickness(10, 8);
                box.CornerRadius = new CornerRadius(14);
                box.TextChanged += (_, _) =>
                {
                    UpdateTotals();
                    _onChanged();
                };
            }

            _netValueText.Foreground = Brush(TextPrimaryColor);
            _vatValueText.Foreground = Brush(TextPrimaryColor);
            _grossValueText.Foreground = Brush(TextPrimaryColor);

            UpdateTotals();

            var removeButton = new Button
            {
                Content = "Usun",
                Background = Brush(SurfaceColor),
                Foreground = Brush(TextPrimaryColor),
                BorderBrush = Brush(BorderStrongColor),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14, 10),
                CornerRadius = new CornerRadius(999)
            };
            removeButton.Click += (_, _) => _onRemove(this);

            View = new Border
            {
                Padding = new Thickness(14),
                CornerRadius = new CornerRadius(18),
                Background = Brush(SurfaceMutedColor),
                BorderBrush = Brush(BorderColor),
                BorderThickness = new Thickness(1),
                Child = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new WrapPanel
                        {
                            Orientation = Orientation.Horizontal,
                            ItemHeight = 42,
                            ItemWidth = double.NaN,
                            Children =
                            {
                                CreateFieldBlock("Opis", _descriptionBox),
                                CreateFieldBlock("Ilosc", _quantityBox),
                                CreateFieldBlock("Jednostka", _unitBox),
                                CreateFieldBlock("Cena netto", _netPriceBox),
                                CreateFieldBlock("VAT %", _vatRateBox)
                            }
                        },
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 18,
                            VerticalAlignment = VerticalAlignment.Center,
                            Children =
                            {
                                CreateMetric("Netto", _netValueText),
                                CreateMetric("VAT", _vatValueText),
                                CreateMetric("Brutto", _grossValueText),
                                removeButton
                            }
                        }
                    }
                }
            };
        }

        public Border View { get; }

        public (decimal Net, decimal Vat, decimal Gross) CalculateTotals()
        {
            var quantity = ParseDecimal(_quantityBox.Text, 1m);
            var unitNetPrice = ParseDecimal(_netPriceBox.Text, 0m);
            var vatRate = ParseDecimal(_vatRateBox.Text, 23m);

            var net = Math.Round(quantity * unitNetPrice, 2);
            var vat = Math.Round(net * vatRate / 100m, 2);
            var gross = Math.Round(net + vat, 2);
            return (net, vat, gross);
        }

        public InvoiceItem BuildItem()
        {
            var item = new InvoiceItem
            {
                Description = _descriptionBox.Text?.Trim() ?? string.Empty,
                Quantity = ParseDecimal(_quantityBox.Text, 1m),
                Unit = string.IsNullOrWhiteSpace(_unitBox.Text) ? "szt." : _unitBox.Text.Trim(),
                UnitNetPrice = ParseDecimal(_netPriceBox.Text, 0m),
                VatRate = ParseDecimal(_vatRateBox.Text, 23m)
            };

            item.Recalculate();
            return item;
        }

        private void UpdateTotals()
        {
            var totals = CalculateTotals();
            _netValueText.Text = $"{totals.Net:0.00} PLN";
            _vatValueText.Text = $"{totals.Vat:0.00} PLN";
            _grossValueText.Text = $"{totals.Gross:0.00} PLN";
        }

        private static decimal ParseDecimal(string? value, decimal fallback)
        {
            var normalized = (value ?? string.Empty).Replace(",", ".", StringComparison.Ordinal).Trim();
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
                ? result
                : fallback;
        }

        private static Control CreateFieldBlock(string label, Control control) =>
            new StackPanel
            {
                Spacing = 4,
                Margin = new Thickness(0, 0, 12, 0),
                Children =
                {
                    new TextBlock
                    {
                        Text = label,
                        FontSize = 12,
                        Foreground = Brush(TextSecondaryColor)
                    },
                    control
                }
            };

        private static Control CreateMetric(string label, TextBlock value) =>
            new StackPanel
            {
                Spacing = 2,
                Children =
                {
                    new TextBlock
                    {
                        Text = label,
                        FontSize = 12,
                        Foreground = Brush(TextSecondaryColor)
                    },
                    value
                }
            };
    }
}
