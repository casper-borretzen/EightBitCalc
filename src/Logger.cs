class Logger
{
    public List<string> messages {get; private set;} = new List<string>();
    public List<string> assembly {get; private set;} = new List<string>();
    private ContainerScroll assemblyContainer;

    // Add a message to the message log
    public void AddMessage(string line)
    {
        messages.Add(DateTime.Now.ToString("HH:mm:ss") + " " + line.ToUpper());
    }

    // Add assembly code to the assembly log
    public void AddAssembly(string line)
    {
        assemblyContainer.AddContent(line);
    }

    // Render message log
    public void RenderMessages(int num = 8)
    {
        for (int i = num; i >= 0; i--)
        {
            Console.WriteLine(messages.Count > i ? " " + messages[messages.Count-(i+1)] : " -");
        }
    }

    // Constructor
    public Logger(ContainerScroll assemblyContainer)
    {
        this.assemblyContainer = assemblyContainer;
    }
}
