class BinaryCalc
{
    // Set global constants
    private const char EMPTY = ' ';
    private const char FILLED = '#';
    private const int CHAR_SPACING = 2;
    private const int CHAR_HEIGHT = 7;
    private const int CHAR_WIDTH = 5 + CHAR_SPACING;
    private const int CHAR_LIMIT = 8;
    private bool[,,] DIGITS = new bool[2,CHAR_HEIGHT,CHAR_WIDTH];
    private const string MAIN_TITLE = "8-BIT BINARY CALCULATOR";
    private const int TOTAL_WIDTH = CHAR_LIMIT * CHAR_WIDTH;
    private const int REGISTER_NUM = 8;

    // Set global variables
    private bool running = true;
    private List<string> messageLog = new List<string>();
    private string[] display = new string[CHAR_HEIGHT];
    private string seperatorLine = "";
    private bool carryFlag = false;
    private bool zeroFlag = false;
    private bool negativeFlag = false;
    private bool overflowFlag = false;

    private byte[] registers = new byte[REGISTER_NUM];
    private byte registerIndex = 0;

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
    
    // Get the letter for a given register
    private char GetRegisterChar(byte index)
    {
        return (char)((int)'A' + index);
    }
    
    // Add message to the message log
    private void MessageAdd(string message)
    {
        messageLog.Add(DateTime.Now.ToString("HH:mm:ss") + ": " + message.ToUpper());
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

    // Unset all flags
    private void UnsetFlags()
    {
        carryFlag = false;
        zeroFlag = false;
        negativeFlag = false;
        overflowFlag = false;
    }

    // Check zero flag if value is zero
    private void SetZeroFlag() 
    {
        if (registers[registerIndex] == 0b00000000) { zeroFlag = true; }
    }

    // Set negative flag if bit 7 is set
    private void SetNegativeFlag()
    {
        if ((registers[registerIndex] & 0b10000000) == 0b10000000) { negativeFlag = true; }
    }

    private void ChangeRegister(byte targetRegister)
    {
        registerIndex = targetRegister;
    }

    // Exclusive OR operation
    private void EOR(byte bits, bool silent = false)
    {
        UnsetFlags();
        registers[registerIndex] = (byte)(registers[registerIndex] ^ bits);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(EOR) Exclusive OR -> " + ByteToString(bits)); }
    }

    // Arithmetic shift left operation
    private void ASL(bool silent = false)
    {
        UnsetFlags();
        if ((registers[registerIndex] & 0b10000000) == 0b10000000) { carryFlag = true; }
        registers[registerIndex] = (byte)(registers[registerIndex] << 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(ASL) Arithmetic shift left"); }
    }

    // Logical shift right operation
    private void LSR(bool silent = false)
    {
        UnsetFlags();
        if ((registers[registerIndex] & 0b00000001) == 0b00000001) {carryFlag = true;}
        registers[registerIndex] = (byte)(registers[registerIndex] >> 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(LSR) Logical shift right"); }
    }

    // Rotate left operation
    private void ROL(bool silent = false)
    {
        UnsetFlags();
        if ((registers[registerIndex] & 0b10000000) == 0b10000000) { carryFlag = true; }
        registers[registerIndex] = (byte)(registers[registerIndex] << 1);
        if (carryFlag) { registers[registerIndex] += 0b00000001; }
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(ROL) Rotate left"); }
    }
    
    // Rotate right operation
    private void ROR(bool silent = false)
    {
        UnsetFlags();
        if ((registers[registerIndex] & 0b00000001) == 0b00000001) {carryFlag = true;}
        registers[registerIndex] = (byte)(registers[registerIndex] >> 1);
        if (carryFlag) { registers[registerIndex] += 0b10000000; }
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(ROL) Rotate right"); }
    }

    // Decrement operation
    private void DEC(bool silent = false)
    {
        UnsetFlags();
        //if ((registers[registerIndex] & 0b00000001) == 0b00000001) {carryFlag = true;}
        registers[registerIndex] -= (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(DEC) Decrement"); }
    }
    
    // Increment operation
    private void INC(bool silent = false)
    {
        UnsetFlags();
        //if (registers[registerIndex] == 0b11111111) { carryFlag = true; }
        registers[registerIndex] += (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(INC) Increment"); }
    }

    // Add with carry operation
    private void ADC(byte bits, bool silent = false)
    {
        UnsetFlags();
        if (((registers[registerIndex] & 0b10000000) == 0b10000000) && (((registers[registerIndex] + bits) & 0b10000000) == 0b00000000)) { carryFlag = true; }
        if (((registers[registerIndex] & 0b10000000) == 0b00000000) && (((registers[registerIndex] + bits) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[registerIndex] += bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(ADC) Add with carry -> " + ByteToString(bits) ); }
    }
    
    // Subtract with carry operation
    private void SBC(byte bits, bool silent = false)
    {
        UnsetFlags();
        if (((registers[registerIndex] & 0b00000001) == 0b00000001) && (((registers[registerIndex] + bits) & 0b00000001) == 0b00000000)) { carryFlag = true; }
        if (((registers[registerIndex] & 0b10000000) == 0b00000000) && (((registers[registerIndex] + bits) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[registerIndex] -= bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(SBC) Subtract with carry -> " + ByteToString(bits) ); }
    }

    // Add value of the selected registry to the current registry
    private void AddRegistry(byte targetRegister, bool silent = false)
    {
        if (registerIndex != targetRegister)
        {
            byte bits = registers[targetRegister];
            ADC(bits, true);
            if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(ADC) Add with carry -> [" + GetRegisterChar(targetRegister) + "] " + ByteToString(bits) ); }
        }
    }

    // Subtract value of the current registry with the selected registry
    private void SubtractRegistry(byte targetRegister, bool silent = false)
    {
        if (registerIndex != targetRegister)
        {
            byte bits = registers[targetRegister];
            SBC(bits, true);
            if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "(SBC) Subtract with carry -> [" + GetRegisterChar(targetRegister) + "] " + ByteToString(bits) ); }
        }
    }

    // Get input from user
    private void GetInput() 
    {
        // Wait for and get key input from user
        ConsoleKeyInfo key = Console.ReadKey(true);
        
        // Check if shift was pressed
        bool keyShift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);
        bool keyCtrl = key.Modifiers.HasFlag(ConsoleModifiers.Control);

        // Check if the pressed key is a valid key
        switch (key.Key)
            {
            case ConsoleKey.A:
                if (keyShift) { AddRegistry(0b00000000); }
                else if (keyCtrl) { SubtractRegistry(0b00000000); }
                else { ChangeRegister(0b00000000); }
                break;
            case ConsoleKey.B:
                if (keyShift) { AddRegistry(0b00000001); }
                else if (keyCtrl) { SubtractRegistry(0b00000001); }
                else { ChangeRegister(0b00000001); }
                break;
            case ConsoleKey.C:
                if (keyShift) { AddRegistry(0b00000010); }
                else if (keyCtrl) { SubtractRegistry(0b00000010); }
                else { ChangeRegister(0b00000010); }
                break;
            case ConsoleKey.D:
                if (keyShift) { AddRegistry(0b00000011); }
                else if (keyCtrl) { SubtractRegistry(0b00000011); }
                else { ChangeRegister(0b00000011); }
                break;
            case ConsoleKey.E:
                if (keyShift) { AddRegistry(0b00000100); }
                else if (keyCtrl) { SubtractRegistry(0b00000100); }
                else { ChangeRegister(0b00000100); }
                break;
            case ConsoleKey.F:
                if (keyShift) { AddRegistry(0b00000101); }
                else if (keyCtrl) { SubtractRegistry(0b00000101); }
                else { ChangeRegister(0b00000101); }
                break;
            case ConsoleKey.G:
                if (keyShift) { AddRegistry(0b00000110); }
                else if (keyCtrl) { SubtractRegistry(0b00000110); }
                else { ChangeRegister(0b00000110); }
                break;
            case ConsoleKey.H:
                if (keyShift) { AddRegistry(0b00000111); }
                else if (keyCtrl) { SubtractRegistry(0b00000111); }
                else { ChangeRegister(0b00000111); }
                break;
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
            case ConsoleKey.Q:
                running = false;
                break;
            }
    }

    // Render the result on screen
    private void Render()
    {
        // Format the display string
        string displayLine = ByteToString(registers[registerIndex]);
        
        // Clear console
        Console.CursorVisible = false;
        Console.Clear();

        // Main title
        Console.WriteLine();
        Console.WriteLine(colors["fgBlack"] + colors["bgBrightCyan"] + (MAIN_TITLE.PadLeft((TOTAL_WIDTH - MAIN_TITLE.Length) / 2 + MAIN_TITLE.Length, ' ')).PadRight(TOTAL_WIDTH, ' ') + colors["default"]);
       
        // Register selector
        Console.WriteLine();
        string registerLine = " ";
        string registerLineHex = " ";
        for (byte i = 0; i < REGISTER_NUM; i++)
        {
            if (i == registerIndex) { registerLine += colors["fgBlack"] + colors["bgBrightYellow"]; }
            registerLine += "  " + GetRegisterChar(i) + "  ";
            if (i == registerIndex) { registerLine += colors["default"]; }
            registerLine += "  ";
            registerLineHex += " 0x" + registers[i].ToString("X2") + " ";
            registerLineHex += " ";
        }
        Console.WriteLine(registerLine);
        Console.WriteLine(registerLineHex);
        Console.WriteLine(seperatorLine);

        // Iterate through the x and y coords of the "pixels" and display the digits from the selected register
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

        // Decimal values
        Console.WriteLine(seperatorLine);
        Console.WriteLine(" UNSIGNED VALUE:     " + (registers[registerIndex].ToString()).PadLeft(3,' ') + "        SIGNED VALUE:      " + (((int)((registers[registerIndex] & 0b01111111) - (registers[registerIndex] & 0b10000000))).ToString()).PadLeft(4,' '));
        //Console.WriteLine(" ASCII CHARACTER:  " + (registers[registerIndex] > 32 ? (char)registers[registerIndex] : "-"));

        Console.WriteLine(seperatorLine);
        
        // Flags
        string carryFlagString = " (C) CARRY FLAG:       " + colors["fgBlack"] + (carryFlag ? colors["bgBrightGreen"] + "1" : colors["bgBrightRed"] + "0") + colors["default"];
        string zeroFlagString = "        (Z) ZERO FLAG:        " + colors["fgBlack"] + (zeroFlag ? colors["bgBrightGreen"] + "1"  : colors["bgBrightRed"] + "0") + colors["default"];
        string negativeFlagString = " (N) NEGATIVE FLAG:    " + colors["fgBlack"] + (negativeFlag ? colors["bgBrightGreen"] + "1" : colors["bgBrightRed"] + "0") + colors["default"];
        string overFlowFlagString = "        (V) OVERFLOW FLAG:    " + colors["fgBlack"] + (overflowFlag ? colors["bgBrightGreen"] + "1" : colors["bgBrightRed"] + "0") + colors["default"];
        Console.WriteLine(carryFlagString + zeroFlagString);
        Console.WriteLine(negativeFlagString + overFlowFlagString);
        
        Console.WriteLine(seperatorLine);
        
        // Message log
        Console.WriteLine(messageLog.Count > 7 ? " " + messageLog[messageLog.Count-8] : " -");
        Console.WriteLine(messageLog.Count > 6 ? " " + messageLog[messageLog.Count-7] : " -");
        Console.WriteLine(messageLog.Count > 5 ? " " + messageLog[messageLog.Count-6] : " -");
        Console.WriteLine(messageLog.Count > 4 ? " " + messageLog[messageLog.Count-5] : " -");
        Console.WriteLine(messageLog.Count > 3 ? " " + messageLog[messageLog.Count-4] : " -");
        Console.WriteLine(messageLog.Count > 2 ? " " + messageLog[messageLog.Count-3] : " -");
        Console.WriteLine(messageLog.Count > 1 ? " " + messageLog[messageLog.Count-2] : " -");
        Console.WriteLine(messageLog.Count > 0 ? " " + messageLog.Last() : " -");
        
        Console.WriteLine(seperatorLine);
        
        // Keyboard shortcuts
        Console.WriteLine(" [UP]              INCREMENT                  (N,Z)");
        Console.WriteLine(" [DOWN]            DECREMENT                  (N,Z)");
        Console.WriteLine(" [LEFT]            ARITHMETIC SHIFT LEFT      (N,Z,C)");
        Console.WriteLine(" [RIGHT]           LOGICAL SHIFT RIGHT        (N,Z,C)");
        Console.WriteLine(" [S+LEFT]          ROTATE LEFT                (N,Z,C)");
        Console.WriteLine(" [S+RIGHT]         ROTATE RIGHT               (N,Z,C)");
        Console.WriteLine(" [0] - [7]         EXCLUSIVE OR               (N,Z)");
        Console.WriteLine(" [S+A] - [S+H]     ADD WITH CARRY             (N,V,Z,C)");
        Console.WriteLine(" [C+A] - [C+H]     SUBTRACT WITH CARRY        (N,V,Z,C)");
        Console.WriteLine(" [A] - [H]         CHANGE ACTIVE REGISTER");
        Console.WriteLine(" [Q]               QUIT PROGRAM");
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
        Console.WriteLine(colors["default"]);
        Console.CursorVisible = true;
        Console.Clear();
    }
   
    // Class constructor
    public BinaryCalc()
    {
        Init();
        Run();
    }
}

