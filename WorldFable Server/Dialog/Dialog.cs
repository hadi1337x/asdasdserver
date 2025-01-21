using System.Collections.Generic;
using System.Text;

public class Dialog
{
    public string Title { get; set; }
    public string Icon { get; set; }
    public List<DialogElement> Elements { get; set; } = new List<DialogElement>();

    public string Serialize()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Title:{Title}");
        builder.AppendLine($"Icon:{Icon}");

        foreach (var element in Elements)
        {
            builder.AppendLine(element.Serialize());
        }

        return builder.ToString();
    }
}

public abstract class DialogElement
{
    public string Type { get; set; }

    public abstract string Serialize();
}

public class LabelElement : DialogElement
{
    public string Text { get; set; }
    public string IconPath { get; set; }

    public override string Serialize()
    {
        return $"Label:{Text}|{IconPath}";
    }
}


public class TextInputElement : DialogElement
{
    public string Name { get; set; }
    public string Placeholder { get; set; }
    public string Value { get; set; }
    public bool IsPassword { get; set; }

    public override string Serialize()
    {
        return $"TextInput:{Name}|{Placeholder}|{Value}|{(IsPassword ? "1" : "0")}";
    }
}

public class SpacerElement : DialogElement
{
    public override string Serialize()
    {
        return "Spacer";
    }
}

public class ButtonElement : DialogElement
{
    public string Text { get; set; }

    public override string Serialize()
    {
        return $"Button:{Text}";
    }
}
