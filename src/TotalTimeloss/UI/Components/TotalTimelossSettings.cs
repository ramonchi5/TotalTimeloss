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

    private TextBox _label1Box = null!;
    private TextBox _label2Box = null!;
    private TextBox _label3Box = null!;
    private CheckBox _showLabelsCheck = null!;
    private CheckBox _underlineLabelsCheck = null!;
    private CheckBox _overrideLabelColorsCheck = null!;
    private CheckBox _overrideTimeColorsCheck = null!;
    private Button _label1ColorButton = null!;
    private Button _label2ColorButton = null!;
    private Button _label3ColorButton = null!;
    private Button _time1ColorButton = null!;
    private Button _time2ColorButton = null!;
    private Button _time3ColorButton = null!;
    private NumericUpDown _sobOffsetBox = null!;
    private NumericUpDown _bptOffsetBox = null!;
    private NumericUpDown _middleOffsetBox = null!;
    private NumericUpDown _label1OffsetBox = null!;
    private NumericUpDown _label2OffsetBox = null!;
    private NumericUpDown _label3OffsetBox = null!;
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
        AutoScroll = true;
        Padding = new Padding(7);
        Size = new Size(476, 520);

        var flow = new FlowLayoutPanel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        flow.Controls.Add(MakeSection("Labels", BuildLabelsSection()));
        flow.Controls.Add(MakeSection("Label Colors", BuildLabelColorsSection()));
        flow.Controls.Add(MakeSection("Time Colors", BuildTimeColorsSection()));
        flow.Controls.Add(MakeSection("Layout / Spacing", BuildLayoutSection()));
        flow.Controls.Add(MakeSection("Accuracy", BuildAccuracySection()));
        flow.Controls.Add(MakeSection("Background", BuildBackgroundSection()));

        Controls.Add(flow);
        ResumeLayout(false);
    }

    private Control BuildLabelsSection()
    {
        var table = MakeGrid(5);

        _showLabelsCheck = new CheckBox { Text = "Show Labels", AutoSize = true };
        _showLabelsCheck.CheckedChanged += (sender, args) => Display2Rows = _showLabelsCheck.Checked;
        AddSpanningControl(table, _showLabelsCheck, 0);

        table.Controls.Add(MakeLabel("Label 1 Text:"), 0, 1);
        _label1Box = MakeTextBox(Label1Text, 80);
        _label1Box.TextChanged += (sender, args) => Label1Text = _label1Box.Text;
        table.Controls.Add(_label1Box, 1, 1);

        table.Controls.Add(MakeLabel("Label 2 Text:"), 0, 2);
        _label2Box = MakeTextBox(Label2Text, 80);
        _label2Box.TextChanged += (sender, args) => Label2Text = _label2Box.Text;
        table.Controls.Add(_label2Box, 1, 2);

        table.Controls.Add(MakeLabel("Label 3 Text:"), 0, 3);
        _label3Box = MakeTextBox(Label3Text, 80);
        _label3Box.TextChanged += (sender, args) => Label3Text = _label3Box.Text;
        table.Controls.Add(_label3Box, 1, 3);

        _underlineLabelsCheck = new CheckBox { Text = "Underline Labels", AutoSize = true };
        _underlineLabelsCheck.CheckedChanged += (sender, args) => UnderlineLabels = _underlineLabelsCheck.Checked;
        AddSpanningControl(table, _underlineLabelsCheck, 4);

        return table;
    }

    private Control BuildLabelColorsSection()
    {
        var table = MakeGrid(7);

        _overrideLabelColorsCheck = new CheckBox { Text = "Override Layout Settings", AutoSize = true };
        _overrideLabelColorsCheck.CheckedChanged += (sender, args) =>
        {
            OverrideTextColor = _overrideLabelColorsCheck.Checked;
            UpdateOverrideControlStates();
        };
        AddSpanningControl(table, _overrideLabelColorsCheck, 0);

        table.Controls.Add(MakeLabel("Label 1 Color:"), 0, 1);
        _label1ColorButton = MakeColorButton(Label1Color, color =>
        {
            Label1Color = color;
            TextColor = Label1Color;
        });
        table.Controls.Add(_label1ColorButton, 1, 1);

        table.Controls.Add(MakeLabel("Label 2 Color:"), 0, 2);
        _label2ColorButton = MakeColorButton(Label2Color, color => Label2Color = color);
        table.Controls.Add(_label2ColorButton, 1, 2);

        table.Controls.Add(MakeLabel("Label 3 Color:"), 0, 3);
        _label3ColorButton = MakeColorButton(Label3Color, color => Label3Color = color);
        table.Controls.Add(_label3ColorButton, 1, 3);

        return table;
    }

    private Control BuildTimeColorsSection()
    {
        var table = MakeGrid(4);

        _overrideTimeColorsCheck = new CheckBox { Text = "Override Layout Settings", AutoSize = true };
        _overrideTimeColorsCheck.CheckedChanged += (sender, args) =>
        {
            OverrideTimeColor = _overrideTimeColorsCheck.Checked;
            UpdateOverrideControlStates();
        };
        AddSpanningControl(table, _overrideTimeColorsCheck, 0);

        table.Controls.Add(MakeLabel("Time 1 Color:"), 0, 1);
        _time1ColorButton = MakeColorButton(Time1Color, color =>
        {
            Time1Color = color;
            TimeColor = Time1Color;
        });
        table.Controls.Add(_time1ColorButton, 1, 1);

        table.Controls.Add(MakeLabel("Time 2 Color:"), 0, 2);
        _time2ColorButton = MakeColorButton(Time2Color, color => Time2Color = color);
        table.Controls.Add(_time2ColorButton, 1, 2);

        table.Controls.Add(MakeLabel("Time 3 Color:"), 0, 3);
        _time3ColorButton = MakeColorButton(Time3Color, color => Time3Color = color);
        table.Controls.Add(_time3ColorButton, 1, 3);

        return table;
    }

    private Control BuildLayoutSection()
    {
        var table = MakeGrid(4);

        table.Controls.Add(MakeLabel("Move SoB Time Left (px):"), 0, 0);
        _sobOffsetBox = MakeNumber(-500, 500, MoveSobTimeLeft);
        _sobOffsetBox.ValueChanged += (sender, args) => MoveSobTimeLeft = (int)_sobOffsetBox.Value;
        table.Controls.Add(_sobOffsetBox, 1, 0);

        table.Controls.Add(MakeLabel("Move BPT Time Left (px):"), 0, 1);
        _bptOffsetBox = MakeNumber(-500, 500, MoveBptTimeLeft);
        _bptOffsetBox.ValueChanged += (sender, args) => MoveBptTimeLeft = (int)_bptOffsetBox.Value;
        table.Controls.Add(_bptOffsetBox, 1, 1);

        table.Controls.Add(MakeLabel("Move Delta Right (px):"), 0, 2);
        _middleOffsetBox = MakeNumber(-500, 500, MiddleValueXOffset);
        _middleOffsetBox.ValueChanged += (sender, args) => MiddleValueXOffset = (int)_middleOffsetBox.Value;
        table.Controls.Add(_middleOffsetBox, 1, 2);

        table.Controls.Add(MakeLabel("Label 1 X Offset (px):"), 0, 3);
        _label1OffsetBox = MakeNumber(-500, 500, Label1XOffset);
        _label1OffsetBox.ValueChanged += (sender, args) => Label1XOffset = (int)_label1OffsetBox.Value;
        table.Controls.Add(_label1OffsetBox, 1, 3);

        table.Controls.Add(MakeLabel("Label 2 X Offset (px):"), 0, 4);
        _label2OffsetBox = MakeNumber(-500, 500, Label2XOffset);
        _label2OffsetBox.ValueChanged += (sender, args) => Label2XOffset = (int)_label2OffsetBox.Value;
        table.Controls.Add(_label2OffsetBox, 1, 4);

        table.Controls.Add(MakeLabel("Label 3 X Offset (px):"), 0, 5);
        _label3OffsetBox = MakeNumber(-500, 500, Label3XOffset);
        _label3OffsetBox.ValueChanged += (sender, args) => Label3XOffset = (int)_label3OffsetBox.Value;
        table.Controls.Add(_label3OffsetBox, 1, 5);

        table.Controls.Add(MakeLabel("Label/Time Gap (px):"), 0, 6);
        _innerGapBox = MakeNumber(-60, 60, InnerRowGap);
        _innerGapBox.ValueChanged += (sender, args) => InnerRowGap = (int)_innerGapBox.Value;
        table.Controls.Add(_innerGapBox, 1, 6);

        return table;
    }

    private Control BuildAccuracySection()
    {
        var flow = new FlowLayoutPanel
        {
            AutoSize = true,
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

    private Control BuildBackgroundSection()
    {
        var table = MakeGrid(3);

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

        table.Controls.Add(MakeLabel("Color 1:"), 0, 1);
        _backgroundColorButton = MakeColorButton(BackgroundColor, color => BackgroundColor = color);
        table.Controls.Add(_backgroundColorButton, 1, 1);

        table.Controls.Add(MakeLabel("Color 2:"), 0, 2);
        _backgroundColor2Button = MakeColorButton(BackgroundColor2, color => BackgroundColor2 = color);
        table.Controls.Add(_backgroundColor2Button, 1, 2);

        return table;
    }

    private static GroupBox MakeSection(string title, Control content)
    {
        Size preferred = content.GetPreferredSize(new Size(420, 0));
        content.Location = new Point(6, 19);
        content.Size = new Size(420, preferred.Height);

        var group = new GroupBox
        {
            Text = title,
            Padding = new Padding(6),
            Margin = new Padding(0, 0, 0, 6),
            Size = new Size(440, Math.Max(48, preferred.Height + 28))
        };
        group.Controls.Add(content);
        return group;
    }

    private static TableLayoutPanel MakeGrid(int rows)
    {
        var table = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            RowCount = rows,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Width = 420
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170f));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        for (int i = 0; i < rows; i++)
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));
        table.Height = rows * 29;
        return table;
    }

    private static void AddSpanningControl(TableLayoutPanel table, Control control, int row)
    {
        table.SetColumnSpan(control, 2);
        table.Controls.Add(control, 0, row);
    }

    private static Label MakeLabel(string text) => new Label
    {
        Text = text,
        AutoSize = true,
        Anchor = AnchorStyles.Left | AnchorStyles.Right,
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
        var numeric = new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            DecimalPlaces = 0,
            Width = 70,
            Value = Clamp(value, minimum, maximum)
        };
        return numeric;
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
        _label1Box.Text = Label1Text;
        _label2Box.Text = Label2Text;
        _label3Box.Text = Label3Text;
        _showLabelsCheck.Checked = Display2Rows;
        _underlineLabelsCheck.Checked = UnderlineLabels;
        _overrideLabelColorsCheck.Checked = OverrideTextColor;
        _overrideTimeColorsCheck.Checked = OverrideTimeColor;

        _label1ColorButton.BackColor = Label1Color;
        _label2ColorButton.BackColor = Label2Color;
        _label3ColorButton.BackColor = Label3Color;
        _time1ColorButton.BackColor = Time1Color;
        _time2ColorButton.BackColor = Time2Color;
        _time3ColorButton.BackColor = Time3Color;

        _sobOffsetBox.Value = Clamp(MoveSobTimeLeft, (int)_sobOffsetBox.Minimum, (int)_sobOffsetBox.Maximum);
        _bptOffsetBox.Value = Clamp(MoveBptTimeLeft, (int)_bptOffsetBox.Minimum, (int)_bptOffsetBox.Maximum);
        _middleOffsetBox.Value = Clamp(MiddleValueXOffset, (int)_middleOffsetBox.Minimum, (int)_middleOffsetBox.Maximum);
        _label1OffsetBox.Value = Clamp(Label1XOffset, (int)_label1OffsetBox.Minimum, (int)_label1OffsetBox.Maximum);
        _label2OffsetBox.Value = Clamp(Label2XOffset, (int)_label2OffsetBox.Minimum, (int)_label2OffsetBox.Maximum);
        _label3OffsetBox.Value = Clamp(Label3XOffset, (int)_label3OffsetBox.Minimum, (int)_label3OffsetBox.Maximum);
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
        return SettingsHelper.CreateSetting(document, parent, "Version", "2.0") ^
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
