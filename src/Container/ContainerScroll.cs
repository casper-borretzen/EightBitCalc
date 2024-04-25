// A scrollable container of text
public class ContainerScroll : Container
{
    private List<string> content = new List<string>();
    private bool showLineNum = false;
   
    // Zero the scroll
    public override void Zero()
    {
        base.ZeroSrollPos();
    }
    
    // Scroll up
    public override bool Up()
    {
        return base.ScrollUp();
    }

    // Scroll down
    public override bool Down()
    {
        return base.ScrollDown();
    }

    // Link the content of the container with a list
    public void LinkContent(List<string> content)
    {
        this.content = content;
        base.contentSize = content.Count;
        base.SetScrollMax();
    }
    
    // Add line to content
    public void AddContent(string line)
    {
        content.Add(line);
        base.contentSize = content.Count;
        base.SetScrollMax();
    }

    // Render the content of a SCROLL type container
    protected override void RenderContent()
    {
        for (int i = base.scrollPos; i < base.scrollPos + base.size; i++)
        {
            if (i < base.contentSize)
            {
            
                // Show line number if enabled
                if (showLineNum)
                {
                    int lineNumDigits = (base.contentSize).ToString().Length;
                    Console.Write(" ");
                    Console.ForegroundColor = BinaryCalc.COLOR_CONTAINER_LINENUM_FG;
                    Console.BackgroundColor = BinaryCalc.COLOR_CONTAINER_LINENUM_BG;
                    Console.Write(" " + ((i+1).ToString()).PadLeft((lineNumDigits > 3 ? lineNumDigits : 3),' ') + " ");
                    Console.ResetColor();
                    Console.Write(" ");
                }

                // Render content
                Console.Write(content[i] + Environment.NewLine);
            }

            // Empty lines
            else { Console.Write(Environment.NewLine); }
        }
    }
    
    // Constructor
    public ContainerScroll(
            string title,
            int size,
            bool startAtBottom = false,
            bool showLineNum = false) : base(title, size, startAtBottom)
    {
        this.showLineNum = showLineNum;
    }
}
