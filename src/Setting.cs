namespace EightBitCalc;

public class Setting
{
    public bool enabled { get; private set; }
    public string description { get; private set; }

    public void Toggle()
    {
        enabled = !enabled;
    }

    public Setting(bool enabled, string description)
    {
        this.enabled = enabled;
        this.description = description;
    }
}
