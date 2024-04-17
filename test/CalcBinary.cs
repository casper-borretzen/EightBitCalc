// TODO:
// string[] display -> bool[,] display

class CalcBinary
{
    // Set global constants
    private const char EMPTY = ' ';
    private const char FILLED = '#';
    private const int CHAR_SPACING = 2;
    private const int CHAR_HEIGHT = 7;
    private const int CHAR_WIDTH = 5 + CHAR_SPACING;
    private const int CHAR_LIMIT = 8;
    private bool[,,] DIGITS = new bool[2,CHAR_HEIGHT,CHAR_WIDTH];

    // Set global variables
    private bool running = true;
    private bool error = false;
    private List<string> messageLog = new List<string>();
    private string[] display = new string[CHAR_HEIGHT];
    private string seperatorLine = "";
    private byte binaryValue = 0;
    private bool carryFlag = false;
    private bool zeroFlag = false;
    private bool negativeFlag = false;

    // Set console colors
    Dictionary<string, string> colors = new Dictionary<string, string> 
    {
        {"default",         "\x1B[0m"},
        {"fgBlack",         "\x1B[30m"},
        {"bgBlack",         "\x1B[40m"},
        {"fgDarkRed",       "\x1B[31m"},
        {"bgDarkRed",       "\x1B[41m"},
        {"fgDarkGreen",     "\x1B[32m"},
        {"bgDarkGreen",     "\x1B[42m"},
        {"fgDarkYellow",    "\x1B[33m"},
        {"bgDarkYellow",    "\x1B[43m"},
        {"fgDarkBlue",      "\x1B[34m"},
        {"bgDarkBlue",      "\x1B[44m"},
        {"fgDarkMagenta",   "\x1B[35m"},
        {"bgDarkMagenta",   "\x1B[45m"},
        {"fgDarkCyan",      "\x1B[36m"},
        {"bgDarkCyan",      "\x1B[46m"},
        {"fgDarkWhite",     "\x1B[37m"},
        {"bgDarkWhite",     "\x1B[47m"},
        {"fgBrightBlack",   "\x1B[90m"},
        {"bgBrightBlack",   "\x1B[100m"},
        {"fgBrightRed",     "\x1B[91m"},
        {"bgBrightRed",     "\x1B[101m"},
        {"fgBrightGreen",   "\x1B[92m"},
        {"bgBrightGreen",   "\x1B[102m"},
        {"fgBrightYellow",  "\x1B[93m"},
        {"bgBrightYellow",  "\x1B[103m"},
        {"fgBrightBlue",    "\x1B[94m"},
        {"bgBrightBlue",    "\x1B[104m"},
        {"fgBrightMagenta", "\x1B[95m"},
        {"bgBrightMagenta", "\x1B[105m"},
        {"fgBrightCyan",    "\x1B[96m"},
        {"bgBrightCyan",    "\x1B[106m"},
        {"fgWhite",         "\x1B[97m"},
        {"bgWhite",         "\x1B[107m"},
    };

    // Convert a byte to string
    private string ByteToString(byte bits)
    {
        return Convert.ToString(bits, 2).PadLeft(8, '0');
    }

    // Initialize values
    private void Init() 
    {
        // Format the message string
        MessageAdd("Welcome!");

        // Make seperator line
        for (int i = 0; i < CHAR_WIDTH * CHAR_LIMIT; i++)
        {
            seperatorLine += '-';
        }

        // Zero digit
        DIGITS[0,1,0] = true;
        DIGITS[0,2,0] = true;
        DIGITS[0,3,0] = true;
        DIGITS[0,4,0] = true;
        DIGITS[0,5,0] = true;
        DIGITS[0,0,1] = true;
        DIGITS[0,6,1] = true;
        DIGITS[0,0,2] = true;
        DIGITS[0,6,2] = true;
        DIGITS[0,0,3] = true;
        DIGITS[0,6,3] = true;
        DIGITS[0,1,4] = true;
        DIGITS[0,2,4] = true;
        DIGITS[0,3,4] = true;
        DIGITS[0,4,4] = true;
        DIGITS[0,5,4] = true;

        // One digit
        DIGITS[1,0,2] = true;
        DIGITS[1,1,1] = true;
        DIGITS[1,1,2] = true;
        DIGITS[1,2,2] = true;
        DIGITS[1,3,2] = true;
        DIGITS[1,4,2] = true;
        DIGITS[1,5,2] = true;
        DIGITS[1,6,1] = true;
        DIGITS[1,6,2] = true;
        DIGITS[1,6,3] = true;
    }

    // Add message to the message log
    private void MessageAdd(string message)
    {
        messageLog.Add(DateTime.Now.ToString("HH:mm:ss") + ": " + message.ToUpper());
    }

    // Unset all flags
    private void UnsetFlags()
    {
        carryFlag = false;
        zeroFlag = false;
        negativeFlag = false;
    }

    // Check zero flag if value is zero
    private void SetZeroFlag() 
    {
        if (binaryValue == 0b00000000) { zeroFlag = true; }
    }

    // Set negative flag if bit 7 is set
    private void SetNegativeFlag()
    {
        if ((binaryValue & 0b10000000) == 0b10000000) { negativeFlag = true; }
    }

    // Rxclusive OR operation
    private void EOR(byte bits, bool silent = false)
    {
        UnsetFlags();
        binaryValue = (byte)(binaryValue ^ bits);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("(EOR) Exclusive OR -> " + ByteToString(bits)); }
    }

    // Arithmetic shift left operation
    private void ASL(bool silent = false)
    {
        UnsetFlags();
        if ((binaryValue & 0b10000000) == 0b10000000) { carryFlag = true; }
        binaryValue = (byte)(binaryValue << 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("(ASL) Arithmetic shift left"); }
    }

    // Logical shift right operation
    private void LSR(bool silent = false)
    {
        UnsetFlags();
        if ((binaryValue & 0b00000001) == 0b00000001) {carryFlag = true;}
        binaryValue = (byte)(binaryValue >> 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("(LSR) Logical shift right"); }
    }

    // Rotate left operation
    private void ROL(bool silent = false)
    {
        UnsetFlags();
        if ((binaryValue & 0b10000000) == 0b10000000) { carryFlag = true; }
        binaryValue = (byte)(binaryValue << 1);
        if (carryFlag) { binaryValue += 0b00000001; }
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("(ROL) Rotate left"); }
    }
    
    // Rotate right operation
    private void ROR(bool silent = false)
    {
        UnsetFlags();
        if ((binaryValue & 0b00000001) == 0b00000001) {carryFlag = true;}
        binaryValue = (byte)(binaryValue >> 1);
        if (carryFlag) { binaryValue += 0b10000000; }
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("(ROL) Rotate right"); }
    }

    // Decrement operation
    private void DEC(bool silent = false)
    {
        UnsetFlags();
        if ((binaryValue & 0b00000001) == 0b00000001) {carryFlag = true;}
        binaryValue -= (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("(DEC) Decrement"); }
    }
    
    // Increment operation
    private void INC(bool silent = false)
    {
        UnsetFlags();
        if (binaryValue == 255) { carryFlag = true; }
        binaryValue += (byte)0b00000001;
        SetZeroFlag();
        if (!silent) { MessageAdd("(INC) Increment"); }
    }

    // Get input from user
    private void GetInput() 
    {
        // Wait for and get key input from user
        ConsoleKeyInfo key = Console.ReadKey(true);
        
        // Check if shift was pressed
        bool keyShift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);

        // Check if the pressed key is a valid key
        switch (key.Key)
            {
            case ConsoleKey.D0:
                EOR(0b00000001);
                break;
            case ConsoleKey.D1:
                EOR(0b00000010);
                break;
            case ConsoleKey.D2:
                EOR(0b00000100);
                break;
            case ConsoleKey.D3:
                EOR(0b00001000);
                break;
            case ConsoleKey.D4:
                EOR(0b00010000);
                break;
            case ConsoleKey.D5:
                EOR(0b00100000);
                break;
            case ConsoleKey.D6:
                EOR(0b01000000);
                break;
            case ConsoleKey.D7:
                EOR(0b10000000);
                break;
            case ConsoleKey.LeftArrow:
                if (keyShift) { ROL(); } else { ASL(); }
                break;
            case ConsoleKey.RightArrow:
                if (keyShift) { ROR(); } else { LSR(); }
                break;
            case ConsoleKey.UpArrow:
                INC();
                break;
            case ConsoleKey.DownArrow:
                DEC();
                break;
            case ConsoleKey.Escape:
                running = false;
                break;
            }
    }

    // Render the result on screen
    private void Render()
    {
        // Format the display string
        string displayLine = ByteToString(binaryValue);
        
        // Clear console
        Console.CursorVisible = false;
        Console.Clear();

        // Text above the display
        Console.WriteLine();
        string mainTitle = "SIMPLE 8-BIT BINARY CALCULATOR";
        Console.WriteLine(colors["fgBlack"] + colors["bgBrightCyan"] + (mainTitle.PadLeft((int)((CHAR_LIMIT * CHAR_WIDTH) - (mainTitle.Length / 2)), ' ')).PadRight(CHAR_LIMIT * CHAR_WIDTH, ' ') + colors["default"]);
        Console.WriteLine(messageLog.Count > 4 ? " " + messageLog[messageLog.Count-5] : " -");
        Console.WriteLine(messageLog.Count > 3 ? " " + messageLog[messageLog.Count-4] : " -");
        Console.WriteLine(messageLog.Count > 2 ? " " + messageLog[messageLog.Count-3] : " -");
        Console.WriteLine(messageLog.Count > 1 ? " " + messageLog[messageLog.Count-2] : " -");
        Console.WriteLine(messageLog.Count > 0 ? " " + messageLog.Last() : " -");
        Console.WriteLine(seperatorLine);

        // Iterate through the x and y coords of the "pixels" 
        // and display the character from the binary value string
        for (int y = 0; y < CHAR_HEIGHT; y++)
        {
            display[y] = " ";
            for (int x = 0; x < CHAR_WIDTH * displayLine.Length; x++)
            {
                // Find the current char from the input string
                char currentChar = displayLine[x / CHAR_WIDTH];

                // Convert the char to a index number for the DIGITS array
                int currentDigit = 0;
                if (Char.IsNumber(currentChar))
                {
                    currentDigit = (int)Char.GetNumericValue(currentChar); 
                }

                // Set the value of the current "pixel"
                display[y] += DIGITS[currentDigit,y,x % CHAR_WIDTH] ? FILLED : EMPTY;
            }

            // Render the results of the current row
            Console.WriteLine(display[y]);
        }

        // Text under the display
        Console.WriteLine(seperatorLine);
        Console.WriteLine(" UNSIGNED VALUE:   " + binaryValue.ToString());
        Console.WriteLine(" SIGNED VALUE:     " + ((int)((binaryValue & 0b01111111) - (binaryValue & 0b10000000))).ToString());
        Console.WriteLine(" ASCII CHARACTER:  " + (binaryValue > 32 ? (char)binaryValue : "-"));
        Console.WriteLine(seperatorLine);
        Console.WriteLine(" CARRY FLAG:       " + colors["fgBlack"] + (carryFlag ? colors["bgBrightGreen"] + "1" : colors["bgBrightRed"] + "0") + colors["default"]);
        Console.WriteLine(" ZERO FLAG:        " + colors["fgBlack"] + (zeroFlag ? colors["bgBrightGreen"] + "1"  : colors["bgBrightRed"] + "0") + colors["default"]);
        Console.WriteLine(" NEGATIVE FLAG:    " + colors["fgBlack"] + (negativeFlag ? colors["bgBrightGreen"] + "1" : colors["bgBrightRed"] + "0") + colors["default"]);
        Console.WriteLine(seperatorLine);
        Console.WriteLine(" [UP]      (INC) INCREMENT");
        Console.WriteLine(" [DOWN]    (DEC) DECREMENT");
        Console.WriteLine(" [LEFT]    (ASL) ARITHMETIC SHIFT LEFT");
        Console.WriteLine(" [RIGHT]   (LSR) LOGICAL SHIFT RIGHT");
        Console.WriteLine(" [S-LEFT]  (ROL) ROTATE LEFT");
        Console.WriteLine(" [S-RIGHT] (ROR) ROTATE RIGHT");
        Console.WriteLine(" [0] - [7] (EOR) EXCLUSIVE OR");
        Console.WriteLine(" [ESC]     QUIT PROGRAM");
    }
    
    // Main loop
    private void Run()
    {
        while (running == true)
        {

            Render();
            GetInput();
        }

        // Exit the program and clear the console
        Console.Clear();
    }
   
    // Class constructor
    public CalcBinary()
    {
        Init();
        Run();
    }
}

