namespace EightBitCalc;

// Scrollable container with entries that can be toggled on/off
public class ContainerToggle : Container
{
    private int selPos = 0;
    private Dictionary<string, Setting> content = new Dictionary<string, Setting>();

    // Zero the scroll num and selected index
    public override void Zero()
    {
        if (base.startAtBottom) { selPos = base.contentSize; }
        else { selPos = 0; }
        base.ZeroSrollPos();
    }
    
    // Move up
    public override bool Up()
    {
        if (selPos > 0)
        {
            selPos--;
            if (selPos < base.scrollPos) { base.ScrollUp(); }
            return true;
        }
        return false;
    }

    // Move down
    public override bool Down()
    {
        if (selPos < base.contentSize - 1)
        {
            selPos++;
            if (selPos > scrollPos + size) { base.ScrollDown(); }
            return true;
        }
        return false;
    }

    // Link the content of the container with a list
    public void LinkContent(Dictionary<string, Setting> content)
    {
        this.content = content;
        base.contentSize = content.Count;
        base.SetScrollMax();
    }
    
    // Toggle the bool value for the currently selected item
    public bool Toggle()
    {
        content.ElementAt(selPos).Value.Toggle();
        return true;
    }
    
    // Render the content the container
    protected override void RenderContent()
    {
        for (int i = base.scrollPos; i < base.scrollPos + base.size; i++)
        {
            if (i < base.contentSize)
            {
                Console.Write(" " + 
                    (i == selPos ? "<" : " ") + 
                    " [" + (content.ElementAt(i).Value.enabled == true ? "X" : "-") + "] " + 
                    (i == selPos ? ">" : " ") + 
                    " " + content.ElementAt(i).Value.description 
                    + Environment.NewLine);
            }

            // Empty lines
            else { Console.Write(Environment.NewLine); }
        }
    }
    
    // Constructor
    public ContainerToggle(
            string title,
            int size, 
            bool startAtBottom = false) : base(title, size, startAtBottom)
    {
    }
}
