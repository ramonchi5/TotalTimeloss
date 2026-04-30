using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

using LiveSplit.TimeFormatters;
using LiveSplit.UI;

namespace TotalTimeloss.UI.Components;

public partial class TotalTimelossSettings : UserControl
{
    public string InstanceName { get; set; }
    public string Label1Text { get; set; }
    public string Label2Text { get; set; }
    public string Label3Text { get; set; }

    public Color Label1Color { get; set; }
    public Color Label2Color { get; set; }
    public Color Label3Color { get; set; }
    public Color Time1Color { get; set; }
    public Color Time2Color { get; set; }
    public Color Time3Color { get; set; }

    public Color TextColor { get; set; }
    public bool OverrideTextColor { get; set; }
    public Color TimeColor { get; set; }
    public bool OverrideTimeColor { get; set; }
    public TimeAccuracy Accuracy { get; set; }

    public int MoveSobTimeLeft { get; set; }
    public int MoveBptTimeLeft { get; set; }
    public int MiddleValueXOffset { get; set; }
    public int Label1XOffset { get; set; }
    public int Label2XOffset { get; set; }
    public int Label3XOffset { get; set; }
    public int InnerRowGap { get; set; }
    public bool UnderlineLabels { get; set; }
    public bool UnderlineLabelSpaces { get; set; }
    public bool ShowTime1 { get; set; }
    public bool ShowTime2 { get; set; }
    public bool ShowTime3 { get; set; }

    public Color BackgroundColor { get; set; }
    public Color BackgroundColor2 { get; set; }
    public GradientType BackgroundGradient { get; set; }
    public string GradientString
    {
        get => BackgroundGradient.ToString();
        set
        {
            if (Enum.TryParse(value, out GradientType gradient))
                BackgroundGradient = gradient;
        }
    }

    public bool Display2Rows { get; set; }

    private LayoutMode _mode;
    public LayoutMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            ApplyMode();
        }
    }

    private Panel _scrollPanel = null!;
    private Panel _contentPanel = null!;
    private bool _resetScrollPosition;
    private TextBox _instanceNameBox = null!;
    private CheckBox _showLabelsCheck = null!;
    private CheckBox _underlineLabelsCheck = null!;
    private CheckBox _underlineSpacesCheck = null!;
    private CheckBox _overrideLabelColorsCheck = null!;
    private CheckBox _overrideTimeColorsCheck = null!;
    private TextBox _label1Box = null!;
    private TextBox _label2Box = null!;
    private TextBox _label3Box = null!;
    private Button _label1ColorButton = null!;
    private Button _label2ColorButton = null!;
    private Button _label3ColorButton = null!;
    private NumericUpDown _label1OffsetBox = null!;
    private NumericUpDown _label2OffsetBox = null!;
    private NumericUpDown _label3OffsetBox = null!;
    private CheckBox _showTime1Check = null!;
    private CheckBox _showTime2Check = null!;
    private CheckBox _showTime3Check = null!;
    private Button _time1ColorButton = null!;
    private Button _time2ColorButton = null!;
    private Button _time3ColorButton = null!;
    private NumericUpDown _sobOffsetBox = null!;
    private NumericUpDown _middleOffsetBox = null!;
    private NumericUpDown _bptOffsetBox = null!;
    private NumericUpDown _innerGapBox = null!;
    private RadioButton _secondsRadio = null!;
    private RadioButton _tenthsRadio = null!;
    private RadioButton _hundredthsRadio = null!;
    private RadioButton _millisecondsRadio = null!;
    private Button _backgroundColorButton = null!;
    private Button _backgroundColor2Button = null!;
    private ComboBox _gradientCombo = null!;

    public TotalTimelossSettings()
    {
        InstanceName = string.Empty;
        Label1Text = "SoB";
        Label2Text = string.Empty;
        Label3Text = "BPT";

        TextColor = Color.White;
        TimeColor = Color.White;
        Label1Color = TextColor;
        Label2Color = TextColor;
        Label3Color = TextColor;
        Time1Color = TimeColor;
        Time2Color = TimeColor;
        Time3Color = TimeColor;

        OverrideTextColor = false;
        OverrideTimeColor = false;
        Accuracy = TimeAccuracy.Hundredths;

        MoveSobTimeLeft = 0;
        MoveBptTimeLeft = 0;
        MiddleValueXOffset = 0;
        Label1XOffset = 0;
        Label2XOffset = 0;
        Label3XOffset = 0;
        InnerRowGap = 0;
        UnderlineLabels = false;
        UnderlineLabelSpaces = false;
        ShowTime1 = true;
        ShowTime2 = true;
        ShowTime3 = true;

        BackgroundColor = Color.Transparent;
        BackgroundColor2 = Color.Transparent;
        BackgroundGradient = GradientType.Plain;
        Display2Rows = true;

        BuildUI();
        UpdateControlsFromSettings();
    }

    private void BuildUI()
    {
        SuspendLayout();
        Controls.Clear();

        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = false;
        Dock = DockStyle.Fill;
        Padding = Padding.Empty;
        Size = new Size(476, 640);

        _scrollPanel = new Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = new Padding(7)
        };

        _contentPanel = new Panel
        {
            Location = new Point(0, 0),
            Margin = Padding.Empty,
            AutoSize = false,
            Padding = Padding.Empty
        };

        _scrollPanel.Controls.Add(_contentPanel);
        Controls.Add(_scrollPanel);

        AddSettingsSection(MakeSection("Instance", BuildInstanceSection()));
        AddSettingsSection(MakeSection("Background", BuildBackgroundSection()));
        AddSettingsSection(MakeSection("Accuracy", BuildAccuracySection()));
        AddSettingsSection(MakeSection("Options", BuildOptionsSection()));
        AddSettingsSection(MakeSection("Labels", BuildLabelsSection()));
        AddSettingsSection(MakeSection("Times", BuildTimesSection()));

        Resize += (sender, args) => ResizeSections();
        _scrollPanel.Resize += (sender, args) => ResizeSections();
        _resetScrollPosition = true;
        ResizeSections();
        ResumeLayout(false);
        PerformLayout();
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);

        if (Parent != null)
            QueueScrollReset();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (Visible)
            QueueScrollReset();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (Visible)
            QueueScrollReset();
    }

    public void PrepareForDisplay()
    {
        QueueScrollReset();
    }

    private void QueueScrollReset()
    {
        _resetScrollPosition = true;

        if (!IsHandleCreated || IsDisposed)
            return;

        BeginInvoke(new Action(() =>
        {
            if (IsDisposed)
                return;

            _resetScrollPosition = true;
            ResizeSections();
        }));
    }

    private void AddSettingsSection(Control section)
    {
        section.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        section.TabIndex = _contentPanel.Controls.Count;
        _contentPanel.Controls.Add(section);
    }

    private Control BuildInstanceSection()
    {
        var table = new TableLayoutPanel
        {
            AutoSize = false,
            ColumnCount = 2,
            RowCount = 1,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Width = 420,
            Height = 29
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));

        table.Controls.Add(MakeLabel("Name:"), 0, 0);
        _instanceNameBox = MakeTextBox(InstanceName, 80);
        _instanceNameBox.TextChanged += (sender, args) => InstanceName = _instanceNameBox.Text;
        table.Controls.Add(_instanceNameBox, 1, 0);

        return table;
    }

    private Control BuildBackgroundSection()
    {
        var table = new TableLayoutPanel
        {
            AutoSize = false,
            ColumnCount = 6,
            RowCount = 1,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Width = 420,
            Height = 29
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30f));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));

        table.Controls.Add(MakeLabel("Gradient:"), 0, 0);
        _gradientCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill
        };
        _gradientCombo.Items.AddRange(new object[] { "Plain", "Vertical", "Horizontal" });
        _gradientCombo.SelectedIndexChanged += (sender, args) =>
        {
            if (_gradientCombo.SelectedItem != null)
                GradientString = _gradientCombo.SelectedItem.ToString();
            UpdateBackgroundControlStates();
        };
        table.Controls.Add(_gradientCombo, 1, 0);

        table.Controls.Add(MakeLabel("Color 1:"), 2, 0);
        _backgroundColorButton = MakeColorButton(BackgroundColor, color => BackgroundColor = color);
        table.Controls.Add(_backgroundColorButton, 3, 0);

        table.Controls.Add(MakeLabel("Color 2:"), 4, 0);
        _backgroundColor2Button = MakeColorButton(BackgroundColor2, color => BackgroundColor2 = color);
        table.Controls.Add(_backgroundColor2Button, 5, 0);

        return table;
    }

    private Control BuildAccuracySection()
    {
        var flow = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        _secondsRadio = MakeRadio("Seconds");
        _tenthsRadio = MakeRadio("Tenths");
        _hundredthsRadio = MakeRadio("Hundredths");
        _millisecondsRadio = MakeRadio("Milliseconds");

        _secondsRadio.CheckedChanged += (sender, args) => { if (_secondsRadio.Checked) Accuracy = TimeAccuracy.Seconds; };
        _tenthsRadio.CheckedChanged += (sender, args) => { if (_tenthsRadio.Checked) Accuracy = TimeAccuracy.Tenths; };
        _hundredthsRadio.CheckedChanged += (sender, args) => { if (_hundredthsRadio.Checked) Accuracy = TimeAccuracy.Hundredths; };
        _millisecondsRadio.CheckedChanged += (sender, args) => { if (_millisecondsRadio.Checked) Accuracy = TimeAccuracy.Milliseconds; };

        flow.Controls.Add(_secondsRadio);
        flow.Controls.Add(_tenthsRadio);
        flow.Controls.Add(_hundredthsRadio);
        flow.Controls.Add(_millisecondsRadio);
        return flow;
    }

    private Control BuildOptionsSection()
    {
        var table = new TableLayoutPanel
        {
            AutoSize = false,
            ColumnCount = 4,
            RowCount = 3,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Width = 420,
            Height = 87
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76f));
        for (int i = 0; i < 3; i++)
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));

        _showLabelsCheck = new CheckBox { Text = "Show Labels", AutoSize = true };
        _showLabelsCheck.CheckedChanged += (sender, args) => Display2Rows = _showLabelsCheck.Checked;
        table.Controls.Add(_showLabelsCheck, 0, 0);

        _underlineLabelsCheck = new CheckBox { Text = "Underline Labels", AutoSize = true };
        _underlineLabelsCheck.CheckedChanged += (sender, args) => UnderlineLabels = _underlineLabelsCheck.Checked;
        table.Controls.Add(_underlineLabelsCheck, 1, 0);

        _underlineSpacesCheck = new CheckBox { Text = "Underline Spaces", AutoSize = true };
        _underlineSpacesCheck.CheckedChanged += (sender, args) => UnderlineLabelSpaces = _underlineSpacesCheck.Checked;
        table.SetColumnSpan(_underlineSpacesCheck, 2);
        table.Controls.Add(_underlineSpacesCheck, 2, 0);

        _overrideLabelColorsCheck = new CheckBox { Text = "Override Label Colors", AutoSize = true };
        _overrideLabelColorsCheck.CheckedChanged += (sender, args) =>
        {
            OverrideTextColor = _overrideLabelColorsCheck.Checked;
            UpdateOverrideControlStates();
        };
        table.Controls.Add(_overrideLabelColorsCheck, 0, 1);

        _overrideTimeColorsCheck = new CheckBox { Text = "Override Time Colors", AutoSize = true };
        _overrideTimeColorsCheck.CheckedChanged += (sender, args) =>
        {
            OverrideTimeColor = _overrideTimeColorsCheck.Checked;
            UpdateOverrideControlStates();
        };
        table.Controls.Add(_overrideTimeColorsCheck, 1, 1);

        var gapLabel = MakeLabel("Distance between labels and times:");
        table.SetColumnSpan(gapLabel, 3);
        table.Controls.Add(gapLabel, 0, 2);
        _innerGapBox = MakeNumber(-1000, 1000, InnerRowGap);
        _innerGapBox.ValueChanged += (sender, args) => InnerRowGap = (int)_innerGapBox.Value;
        table.Controls.Add(_innerGapBox, 3, 2);

        return table;
    }

    private Control BuildLabelsSection()
    {
        var table = MakeCompactRows(3, includeShowColumn: false);

        AddLabelRow(
            table,
            0,
            "Label 1 - SoB",
            () => Label1Text,
            value => Label1Text = value,
            () => Label1Color,
            value =>
            {
                Label1Color = value;
                TextColor = Label1Color;
            },
            () => Label1XOffset,
            value => Label1XOffset = value,
            out _label1Box,
            out _label1ColorButton,
            out _label1OffsetBox);
        AddLabelRow(
            table,
            1,
            "Label 2 - Delta",
            () => Label2Text,
            value => Label2Text = value,
            () => Label2Color,
            value => Label2Color = value,
            () => Label2XOffset,
            value => Label2XOffset = value,
            out _label2Box,
            out _label2ColorButton,
            out _label2OffsetBox);
        AddLabelRow(
            table,
            2,
            "Label 3 - BPT",
            () => Label3Text,
            value => Label3Text = value,
            () => Label3Color,
            value => Label3Color = value,
            () => Label3XOffset,
            value => Label3XOffset = value,
            out _label3Box,
            out _label3ColorButton,
            out _label3OffsetBox);

        return table;
    }

    private Control BuildTimesSection()
    {
        var table = MakeCompactRows(3, includeShowColumn: true);

        AddTimeRow(
            table,
            0,
            "Time 1 - SoB",
            () => ShowTime1,
            value => ShowTime1 = value,
            () => Time1Color,
            value =>
            {
                Time1Color = value;
                TimeColor = Time1Color;
            },
            () => MoveSobTimeLeft,
            value => MoveSobTimeLeft = value,
            out _showTime1Check,
            out _time1ColorButton,
            out _sobOffsetBox);
        AddTimeRow(
            table,
            1,
            "Time 2 - Delta",
            () => ShowTime2,
            value => ShowTime2 = value,
            () => Time2Color,
            value => Time2Color = value,
            () => MiddleValueXOffset,
            value => MiddleValueXOffset = value,
            out _showTime2Check,
            out _time2ColorButton,
            out _middleOffsetBox);
        AddTimeRow(
            table,
            2,
            "Time 3 - BPT",
            () => ShowTime3,
            value => ShowTime3 = value,
            () => Time3Color,
            value => Time3Color = value,
            () => MoveBptTimeLeft,
            value => MoveBptTimeLeft = value,
            out _showTime3Check,
            out _time3ColorButton,
            out _bptOffsetBox);

        return table;
    }

    private static TableLayoutPanel MakeCompactRows(int rows, bool includeShowColumn)
    {
        var table = new TableLayoutPanel
        {
            AutoSize = false,
            ColumnCount = includeShowColumn ? 6 : 5,
            RowCount = rows,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Width = 420,
            Height = rows * 29
        };

        if (includeShowColumn)
        {
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        }
        else
        {
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        }

        for (int i = 0; i < rows; i++)
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));

        return table;
    }

    private void AddLabelRow(
        TableLayoutPanel table,
        int row,
        string title,
        Func<string> getText,
        Action<string> setText,
        Func<Color> getColor,
        Action<Color> setColor,
        Func<int> getMove,
        Action<int> setMove,
        out TextBox textBox,
        out Button colorButton,
        out NumericUpDown moveBox)
    {
        table.Controls.Add(MakeCellLabel(title), 0, row);
        table.Controls.Add(MakeCellLabel("Move (px):"), 1, row);

        moveBox = MakeNumber(-500, 500, getMove());
        moveBox.ValueChanged += (sender, args) => setMove((int)((NumericUpDown)sender!).Value);
        table.Controls.Add(moveBox, 2, row);

        colorButton = MakeColorButton(getColor(), setColor);
        table.Controls.Add(colorButton, 3, row);

        textBox = MakeTextBox(getText(), 80);
        textBox.TextChanged += (sender, args) => setText(((TextBox)sender!).Text);
        table.Controls.Add(textBox, 4, row);
    }

    private void AddTimeRow(
        TableLayoutPanel table,
        int row,
        string title,
        Func<bool> getShow,
        Action<bool> setShow,
        Func<Color> getColor,
        Action<Color> setColor,
        Func<int> getMove,
        Action<int> setMove,
        out CheckBox showCheck,
        out Button colorButton,
        out NumericUpDown moveBox)
    {
        table.Controls.Add(MakeCellLabel(title), 0, row);

        showCheck = new CheckBox
        {
            AutoSize = false,
            Width = 18,
            Height = 18,
            Anchor = AnchorStyles.Left,
            CheckAlign = ContentAlignment.MiddleCenter,
            Text = string.Empty
        };
        showCheck.Checked = getShow();
        showCheck.CheckedChanged += (sender, args) => setShow(((CheckBox)sender!).Checked);
        table.Controls.Add(showCheck, 1, row);

        table.Controls.Add(MakeCellLabel("Move (px):"), 2, row);

        moveBox = MakeNumber(-500, 500, getMove());
        moveBox.ValueChanged += (sender, args) => setMove((int)((NumericUpDown)sender!).Value);
        table.Controls.Add(moveBox, 3, row);

        colorButton = MakeColorButton(getColor(), setColor);
        table.Controls.Add(colorButton, 4, row);
    }

    private static GroupBox MakeSection(string title, Control content)
    {
        int sectionWidth = 440;
        int contentWidth = sectionWidth - 18;
        Size preferred = content.GetPreferredSize(new Size(contentWidth, 0));
        content.Location = new Point(8, 19);
        content.Size = new Size(contentWidth, preferred.Height);

        var group = new GroupBox
        {
            Text = title,
            Padding = new Padding(6),
            Margin = new Padding(0, 0, 0, 6),
            Size = new Size(sectionWidth, Math.Max(48, preferred.Height + 30))
        };
        group.Controls.Add(content);
        return group;
    }

    private void ResizeSections()
    {
        if (_scrollPanel == null || _contentPanel == null)
            return;

        int previousScrollY = _resetScrollPosition ? 0 : -_scrollPanel.AutoScrollPosition.Y;

        _scrollPanel.SuspendLayout();
        _contentPanel.SuspendLayout();

        _scrollPanel.AutoScrollPosition = Point.Empty;

        int width = Math.Max(320, _scrollPanel.ClientSize.Width - _scrollPanel.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth - 4);
        int y = 0;
        _contentPanel.Location = new Point(_scrollPanel.Padding.Left, _scrollPanel.Padding.Top);
        _contentPanel.Width = width;

        foreach (Control control in _contentPanel.Controls)
        {
            control.Location = new Point(0, y);
            control.Width = width;
            if (control is GroupBox group && group.Controls.Count > 0)
            {
                Control content = group.Controls[0];
                int contentWidth = Math.Max(240, width - 18);
                Size preferred = content.GetPreferredSize(new Size(contentWidth, 0));
                content.Width = contentWidth;
                content.Height = preferred.Height;
                group.Height = Math.Max(48, content.Height + 30);
            }

            y = control.Bottom + control.Margin.Bottom;
        }

        _contentPanel.Height = y;
        _scrollPanel.AutoScrollMinSize = new Size(0, y + _scrollPanel.Padding.Vertical);

        int maxScrollY = Math.Max(0, y + _scrollPanel.Padding.Vertical - _scrollPanel.ClientSize.Height);
        if (previousScrollY > maxScrollY)
            previousScrollY = maxScrollY;
        if (previousScrollY > 0)
            _scrollPanel.AutoScrollPosition = new Point(0, previousScrollY);
        else
            _scrollPanel.AutoScrollPosition = Point.Empty;

        _resetScrollPosition = false;

        _contentPanel.ResumeLayout(true);
        _scrollPanel.ResumeLayout(true);
    }

    private static Label MakeLabel(string text) => new Label
    {
        Text = text,
        AutoSize = true,
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static Label MakeCellLabel(string text) => new Label
    {
        Text = text,
        AutoSize = false,
        Dock = DockStyle.Fill,
        Margin = Padding.Empty,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static TextBox MakeTextBox(string text, int maxLength) => new TextBox
    {
        Text = text,
        MaxLength = maxLength,
        Dock = DockStyle.Fill
    };

    private static RadioButton MakeRadio(string text) => new RadioButton
    {
        Text = text,
        AutoSize = true,
        Margin = new Padding(0, 0, 8, 0)
    };

    private static NumericUpDown MakeNumber(int minimum, int maximum, int value)
    {
        return new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            DecimalPlaces = 0,
            Width = 70,
            Value = Clamp(value, minimum, maximum),
            Anchor = AnchorStyles.Left
        };
    }

    private Button MakeColorButton(Color initial, Action<Color> setter)
    {
        var button = new Button
        {
            BackColor = initial,
            FlatStyle = FlatStyle.Popup,
            UseVisualStyleBackColor = false,
            Width = 23,
            Height = 23,
            Anchor = AnchorStyles.Left
        };
        button.Click += (sender, args) =>
        {
            SettingsHelper.ColorButtonClick(button, this);
            setter(button.BackColor);
        };
        return button;
    }

    private void UpdateControlsFromSettings()
    {
        _instanceNameBox.Text = InstanceName;
        _showLabelsCheck.Checked = Display2Rows;
        _underlineLabelsCheck.Checked = UnderlineLabels;
        _underlineSpacesCheck.Checked = UnderlineLabelSpaces;
        _overrideLabelColorsCheck.Checked = OverrideTextColor;
        _overrideTimeColorsCheck.Checked = OverrideTimeColor;

        _label1Box.Text = Label1Text;
        _label2Box.Text = Label2Text;
        _label3Box.Text = Label3Text;
        _label1ColorButton.BackColor = Label1Color;
        _label2ColorButton.BackColor = Label2Color;
        _label3ColorButton.BackColor = Label3Color;
        _label1OffsetBox.Value = Clamp(Label1XOffset, (int)_label1OffsetBox.Minimum, (int)_label1OffsetBox.Maximum);
        _label2OffsetBox.Value = Clamp(Label2XOffset, (int)_label2OffsetBox.Minimum, (int)_label2OffsetBox.Maximum);
        _label3OffsetBox.Value = Clamp(Label3XOffset, (int)_label3OffsetBox.Minimum, (int)_label3OffsetBox.Maximum);

        _showTime1Check.Checked = ShowTime1;
        _showTime2Check.Checked = ShowTime2;
        _showTime3Check.Checked = ShowTime3;
        _time1ColorButton.BackColor = Time1Color;
        _time2ColorButton.BackColor = Time2Color;
        _time3ColorButton.BackColor = Time3Color;
        _sobOffsetBox.Value = Clamp(MoveSobTimeLeft, (int)_sobOffsetBox.Minimum, (int)_sobOffsetBox.Maximum);
        _middleOffsetBox.Value = Clamp(MiddleValueXOffset, (int)_middleOffsetBox.Minimum, (int)_middleOffsetBox.Maximum);
        _bptOffsetBox.Value = Clamp(MoveBptTimeLeft, (int)_bptOffsetBox.Minimum, (int)_bptOffsetBox.Maximum);
        _innerGapBox.Value = Clamp(InnerRowGap, (int)_innerGapBox.Minimum, (int)_innerGapBox.Maximum);

        _secondsRadio.Checked = Accuracy == TimeAccuracy.Seconds;
        _tenthsRadio.Checked = Accuracy == TimeAccuracy.Tenths;
        _hundredthsRadio.Checked = Accuracy == TimeAccuracy.Hundredths;
        _millisecondsRadio.Checked = Accuracy == TimeAccuracy.Milliseconds;

        _backgroundColorButton.BackColor = BackgroundColor;
        _backgroundColor2Button.BackColor = BackgroundColor2;
        _gradientCombo.SelectedItem = GradientString;

        UpdateOverrideControlStates();
        UpdateBackgroundControlStates();
        ApplyMode();
    }

    private void UpdateOverrideControlStates()
    {
        if (_label1ColorButton != null)
        {
            _label1ColorButton.Enabled = OverrideTextColor;
            _label2ColorButton.Enabled = OverrideTextColor;
            _label3ColorButton.Enabled = OverrideTextColor;
        }

        if (_time1ColorButton != null)
        {
            _time1ColorButton.Enabled = OverrideTimeColor;
            _time2ColorButton.Enabled = OverrideTimeColor;
            _time3ColorButton.Enabled = OverrideTimeColor;
        }
    }

    private void UpdateBackgroundControlStates()
    {
        if (_backgroundColor2Button != null)
            _backgroundColor2Button.Enabled = BackgroundGradient != GradientType.Plain;
    }

    private void ApplyMode()
    {
        if (_showLabelsCheck == null)
            return;

        if (Mode == LayoutMode.Horizontal)
        {
            _showLabelsCheck.Enabled = false;
            _showLabelsCheck.Checked = true;
        }
        else
        {
            _showLabelsCheck.Enabled = true;
        }
    }

    public void SetSettings(XmlNode node)
    {
        if (node is not XmlElement element)
            return;

        Color oldTextColor = ReadColor(element, "TextColor", TextColor);
        Color oldTimeColor = ReadColor(element, "TimeColor", TimeColor);

        OverrideTextColor = ReadBool(element, "OverrideTextColor", OverrideTextColor);
        OverrideTimeColor = ReadBool(element, "OverrideTimeColor", OverrideTimeColor);

        TextColor = oldTextColor;
        TimeColor = oldTimeColor;

        InstanceName = ReadString(element, "InstanceName", ReadString(element, "DisplayText", InstanceName));
        Label1Text = ReadString(element, "Label1Text", Label1Text);
        Label2Text = ReadString(element, "Label2Text", Label2Text);
        Label3Text = ReadString(element, "Label3Text", Label3Text);

        Label1Color = ReadColor(element, "Label1Color", oldTextColor);
        Label2Color = ReadColor(element, "Label2Color", oldTextColor);
        Label3Color = ReadColor(element, "Label3Color", oldTextColor);
        Time1Color = ReadColor(element, "Time1Color", oldTimeColor);
        Time2Color = ReadColor(element, "Time2Color", oldTimeColor);
        Time3Color = ReadColor(element, "Time3Color", oldTimeColor);

        TextColor = Label1Color;
        TimeColor = Time1Color;

        Accuracy = ReadEnum(element, "Accuracy", Accuracy);
        MoveSobTimeLeft = ReadInt(element, "MoveSobTimeLeft", MoveSobTimeLeft);
        MoveBptTimeLeft = ReadInt(element, "MoveBptTimeLeft", MoveBptTimeLeft);
        MiddleValueXOffset = ReadInt(element, "MiddleValueXOffset", MiddleValueXOffset);
        Label1XOffset = ReadInt(element, "Label1XOffset", Label1XOffset);
        Label2XOffset = ReadInt(element, "Label2XOffset", Label2XOffset);
        Label3XOffset = ReadInt(element, "Label3XOffset", Label3XOffset);
        InnerRowGap = ReadInt(element, "InnerRowGap", InnerRowGap);
        UnderlineLabels = ReadBool(element, "UnderlineLabels", UnderlineLabels);
        UnderlineLabelSpaces = ReadBool(element, "UnderlineLabelSpaces", UnderlineLabelSpaces);
        ShowTime1 = ReadBool(element, "ShowTime1", ShowTime1);
        ShowTime2 = ReadBool(element, "ShowTime2", ShowTime2);
        ShowTime3 = ReadBool(element, "ShowTime3", ShowTime3);

        BackgroundColor = ReadColor(element, "BackgroundColor", BackgroundColor);
        BackgroundColor2 = ReadColor(element, "BackgroundColor2", BackgroundColor2);
        GradientString = ReadString(element, "BackgroundGradient", GradientString);
        Display2Rows = ReadBool(element, "Display2Rows", Display2Rows);

        UpdateControlsFromSettings();
    }

    public XmlNode GetSettings(XmlDocument document)
    {
        XmlElement parent = document.CreateElement("Settings");
        CreateSettingsNode(document, parent);
        return parent;
    }

    public int GetSettingsHashCode()
    {
        return CreateSettingsNode(null!, null!);
    }

    private int CreateSettingsNode(XmlDocument document, XmlElement parent)
    {
        return SettingsHelper.CreateSetting(document, parent, "Version", "3.2") ^
               SettingsHelper.CreateSetting(document, parent, "InstanceName", InstanceName) ^
               SettingsHelper.CreateSetting(document, parent, "DisplayText", InstanceName) ^
               SettingsHelper.CreateSetting(document, parent, "TextColor", TextColor) ^
               SettingsHelper.CreateSetting(document, parent, "OverrideTextColor", OverrideTextColor) ^
               SettingsHelper.CreateSetting(document, parent, "TimeColor", TimeColor) ^
               SettingsHelper.CreateSetting(document, parent, "OverrideTimeColor", OverrideTimeColor) ^
               SettingsHelper.CreateSetting(document, parent, "Label1Text", Label1Text) ^
               SettingsHelper.CreateSetting(document, parent, "Label2Text", Label2Text) ^
               SettingsHelper.CreateSetting(document, parent, "Label3Text", Label3Text) ^
               SettingsHelper.CreateSetting(document, parent, "Label1Color", Label1Color) ^
               SettingsHelper.CreateSetting(document, parent, "Label2Color", Label2Color) ^
               SettingsHelper.CreateSetting(document, parent, "Label3Color", Label3Color) ^
               SettingsHelper.CreateSetting(document, parent, "Time1Color", Time1Color) ^
               SettingsHelper.CreateSetting(document, parent, "Time2Color", Time2Color) ^
               SettingsHelper.CreateSetting(document, parent, "Time3Color", Time3Color) ^
               SettingsHelper.CreateSetting(document, parent, "Accuracy", Accuracy) ^
               SettingsHelper.CreateSetting(document, parent, "MoveSobTimeLeft", MoveSobTimeLeft) ^
               SettingsHelper.CreateSetting(document, parent, "MoveBptTimeLeft", MoveBptTimeLeft) ^
               SettingsHelper.CreateSetting(document, parent, "MiddleValueXOffset", MiddleValueXOffset) ^
               SettingsHelper.CreateSetting(document, parent, "Label1XOffset", Label1XOffset) ^
               SettingsHelper.CreateSetting(document, parent, "Label2XOffset", Label2XOffset) ^
               SettingsHelper.CreateSetting(document, parent, "Label3XOffset", Label3XOffset) ^
               SettingsHelper.CreateSetting(document, parent, "InnerRowGap", InnerRowGap) ^
               SettingsHelper.CreateSetting(document, parent, "UnderlineLabels", UnderlineLabels) ^
               SettingsHelper.CreateSetting(document, parent, "UnderlineLabelSpaces", UnderlineLabelSpaces) ^
               SettingsHelper.CreateSetting(document, parent, "ShowTime1", ShowTime1) ^
               SettingsHelper.CreateSetting(document, parent, "ShowTime2", ShowTime2) ^
               SettingsHelper.CreateSetting(document, parent, "ShowTime3", ShowTime3) ^
               SettingsHelper.CreateSetting(document, parent, "BackgroundColor", BackgroundColor) ^
               SettingsHelper.CreateSetting(document, parent, "BackgroundColor2", BackgroundColor2) ^
               SettingsHelper.CreateSetting(document, parent, "BackgroundGradient", BackgroundGradient) ^
               SettingsHelper.CreateSetting(document, parent, "Display2Rows", Display2Rows);
    }

    private static string ReadString(XmlElement element, string name, string fallback)
    {
        XmlElement? node = element[name];
        return node == null ? fallback : node.InnerText;
    }

    private static bool ReadBool(XmlElement element, string name, bool fallback)
    {
        XmlElement? node = element[name];
        return node == null ? fallback : SettingsHelper.ParseBool(node, fallback);
    }

    private static int ReadInt(XmlElement element, string name, int fallback)
    {
        string? text = element[name]?.InnerText;
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            ? value
            : fallback;
    }

    private static Color ReadColor(XmlElement element, string name, Color fallback)
    {
        XmlElement? node = element[name];
        return node == null ? fallback : SettingsHelper.ParseColor(node);
    }

    private static T ReadEnum<T>(XmlElement element, string name, T fallback) where T : struct
    {
        string? text = element[name]?.InnerText;
        return Enum.TryParse(text, out T value) ? value : fallback;
    }

    private static int Clamp(int value, int minimum, int maximum)
    {
        if (value < minimum)
            return minimum;
        if (value > maximum)
            return maximum;
        return value;
    }

    private void chkOverrideTimeColor_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is CheckBox checkBox)
            OverrideTimeColor = checkBox.Checked;
        UpdateOverrideControlStates();
    }

    private void chkOverrideTextColor_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is CheckBox checkBox)
            OverrideTextColor = checkBox.Checked;
        UpdateOverrideControlStates();
    }

    private void TotalTimelossSettings_Load(object sender, EventArgs e)
    {
        UpdateControlsFromSettings();
    }

    private void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is ComboBox combo && combo.SelectedItem != null)
            GradientString = combo.SelectedItem.ToString();
        UpdateBackgroundControlStates();
    }

    private void rdoHundredths_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is RadioButton radio && radio.Checked)
            Accuracy = TimeAccuracy.Hundredths;
    }

    private void rdoTenths_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is RadioButton radio && radio.Checked)
            Accuracy = TimeAccuracy.Tenths;
    }

    private void rdoSeconds_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is RadioButton radio && radio.Checked)
            Accuracy = TimeAccuracy.Seconds;
    }

    private void rdoMilliseconds_CheckedChanged(object sender, EventArgs e)
    {
        if (sender is RadioButton radio && radio.Checked)
            Accuracy = TimeAccuracy.Milliseconds;
    }

    private void ColorButtonClick(object sender, EventArgs e)
    {
        if (sender is Button button)
            SettingsHelper.ColorButtonClick(button, this);
    }
}
