// A scrollable container to hold lines to be rendered on screen
public abstract class Container {
    protected string title = "";
    protected int scrollPos = 0;
    protected int scrollMax = 0;
    protected int size = 0;
    protected int contentSize = 0;
    protected bool startAtBottom = false;

    // Zero the position of scroll/selection
    public abstract void Zero();
    
    // Move scroll/selection up
    public abstract bool Up();

    // Move scroll/selection down
    public abstract bool Down();

    // Render the container header
    protected void RenderHeader()
    {
        Console.Write(" ");
        Console.ForegroundColor = BinaryCalc.COLOR_CONTAINER_HEADER_FG;
        Console.BackgroundColor = BinaryCalc.COLOR_CONTAINER_HEADER_BG;
        Console.Write(" " + title + " ");
        Console.ResetColor();
        Console.Write("<UP> SCROLL UP / <DOWN> SCROLL DOWN".PadLeft(BinaryCalc.RENDER_WIDTH - title.Length - 4, ' '));
        Console.Write(Environment.NewLine);
        Console.Write(BinaryCalc.seperator[0] + Environment.NewLine);
    }

    // Render lines of content
    protected abstract void RenderContent();

    // Get the current scroll index as a string
    public string GetScrollPos()
    {
        return scrollMax == 0 ? "" : "(" + (scrollPos.ToString()).PadLeft(3,'0') + "/" + (scrollMax.ToString()).PadLeft(3,'0') + ")";
    }

    // Set the max scroll value
    protected void SetScrollMax()
    {
        scrollMax = (contentSize - size) > 0 ? (contentSize - size) : 0;
    }

    // Zero the scroll position
    protected void ZeroSrollPos()
    {
        if (startAtBottom) { scrollPos = scrollMax; }
        else { scrollPos = 0; }
    }

    // Try to scroll the content up
    protected bool ScrollUp()
    {
        if (scrollPos > 0) 
        { 
            // Scroll up is possible
            scrollPos--; 
            return true; 
        }

        // Scroll up is not possible
        return false;
    }

    // Try to scroll the content down
    protected bool ScrollDown()
    {
        if (scrollPos < scrollMax) 
        { 
            // Scroll down is possible
            scrollPos++; 
            return true;
        } 

        // Scoll down is not possible
        return false;
    }

    // If there is no content show empty text
    private void RenderEmpty()
    {
        for (int i = 0; i < size; i++)
        {
            if (i == 0) { Console.Write(" -" + Environment.NewLine); }
            else {Console.Write(Environment.NewLine); }
        }
    }

    // Render the Container
    public void Render()
    {
        // Check if there is content and show empty screen if content is empty
        RenderHeader();
        if (contentSize == 0) { RenderEmpty(); }
        else{ RenderContent(); }
        RenderFooter();
    }

    // Render the container footer
    private void RenderFooter()
    {
        Console.Write(BinaryCalc.seperator[0] + Environment.NewLine);
        Console.Write(" PRESS <ESC> TO RETURN TO MAIN SCREEN " + GetScrollPos().PadLeft(17) + Environment.NewLine);
    }

    // Constructor
    public Container(
            string title,
            int size,
            bool startAtBottom)
    {
        this.size = size;
        this.title = title;
        this.startAtBottom = startAtBottom;
    }
}
