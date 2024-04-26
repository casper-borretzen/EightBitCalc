namespace EightBitCalc;

class Logger
{
    // Class references
    private BinaryCalc binaryCalc;

    // Set vars
    public int messageIndex { set; get; } = 0;

    // Logger lists and .asm log file
    public List<string> messages { get; set; } = new List<string>();
    public List<string> assembly { get; set; } = new List<string>();
    private StreamWriter asmFile;

    // Add message to the message log
    public void AddMessage(string line, bool error = false)
    {
        messages.Add(DateTime.Now.ToString("HH:mm:ss") + " " + (error ? BinaryCalc.COLOR_ANSI_FG_DARK_RED + "ERROR:" + BinaryCalc.COLOR_ANSI_DEFAULT + " " + line.ToUpper() : line.ToUpper()));
        messageIndex = 0;
    }

    // Add assembly code to the assembly log
    public void AddAssembly(string line)
    {
        binaryCalc.containerScroll[BinaryCalc.STATE.ASSEMBLY].AddContent(line);
        asmFile.WriteLine(line);
    }

    // Scroll message log up
    public bool MessagesUp()
    {
        if ((messageIndex + BinaryCalc.MESSAGES_NUM) < messages.Count) { messageIndex++; return true; }
        return false;
    }

    // Scroll message log down
    public bool MessagesDown()
    {
        if (messageIndex > 0) { messageIndex--; return true; }
        return false;
    }

    public void SaveToFile() 
    {
        asmFile.Close();
    }

    // Constructor
    public Logger(BinaryCalc binaryCalc) 
    {
        this.binaryCalc = binaryCalc;
        asmFile = new StreamWriter("log.asm");
    }
}
