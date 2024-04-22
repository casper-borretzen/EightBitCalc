class BinaryCalc
{
    // Set global enums
    private enum STATE 
    {
        MAIN,
        HELP,
        SETTINGS,
        ASSEMBLY,
        ERROR,
    };

    // Set global constants
    private const char EMPTY = ' ';
    private const char FILLED = '#';
    private const int CHAR_SPACING = 2;
    private const int CHAR_HEIGHT = 7;
    private const int CHAR_WIDTH = 5 + CHAR_SPACING;
    private const int CHAR_LIMIT = 8;
    private bool[,,] DIGITS = new bool[2,CHAR_HEIGHT,CHAR_WIDTH];
    private const string MAIN_TITLE = "8-BIT BINARY CALCULATOR";
    private const int RENDER_WIDTH = CHAR_LIMIT * CHAR_WIDTH;
    private const int RENDER_HEIGHT = 43;
    private const int REGISTER_NUM = 8;

    // Set global variables
    private bool running = true;
    private STATE appState = STATE.MAIN;
    private STATE appStatePrev = STATE.MAIN;
    private List<string> messageLog = new List<string>();
    private string[] display = new string[CHAR_HEIGHT];
    private string[] seperatorLine = new string[2];
    private string blankLine = "";
    private byte[] registers = new byte[REGISTER_NUM];
    private byte[] registersPrev = new byte[REGISTER_NUM];
    private byte registerIndex = 0;
    private bool carryFlag = false;
    private bool zeroFlag = false;
    private bool negativeFlag = false;
    private bool overflowFlag = false;
    private int windowWidth = 0;
    private int windowHeight = 0;
    
    // Settings
    private Dictionary<string, Setting> settings = new Dictionary<string, Setting>
    {
        { "uiHlChangedBit", new Setting(true, "HIGHLIGHT BITS CHANGED IN PREVIOUS OPERATION") },
        { "flagAutoCarry",  new Setting(true, "(NOT IMPLEMENTED!) AUTO CARRY") },
        { "loggerAsmFile",  new Setting(true, "(NOT IMPLEMENTED!) SAVE ASM FILE") }
    }; 

    // Dictionaries for container content
    List <string> help = new List<string>();
    List <string> assembly = new List<string>();
    
    // Create scroll containers
    Dictionary<STATE, ContainerScroll> containerScroll = new Dictionary<STATE, ContainerScroll>
    {
        { STATE.HELP,     new ContainerScroll(RENDER_HEIGHT - 7) },
        { STATE.ASSEMBLY, new ContainerScroll(RENDER_HEIGHT - 7, startAtBottom: true, showLineNum: true) }
    };

    // Create toggle containers
    Dictionary<STATE, ContainerToggle> containerToggle = new Dictionary<STATE, ContainerToggle>
    {
        { STATE.SETTINGS, new ContainerToggle(RENDER_HEIGHT - 7) }
    };

    // Set error types
    Dictionary<string, bool> appError = new Dictionary<string, bool>
    {
        { "windowSize",     false }
    };

    // Change the app STATE
    private void ChangeState(STATE newState)
    {
        appStatePrev = appState;
        appState = newState;

        // Reset sel position for container
        if (containerScroll.ContainsKey(newState)) { containerScroll[newState].Zero(); }
        else if (containerToggle.ContainsKey(newState)) { containerToggle[newState].Zero(); }
    }

    // Check console window size in rows and columns
    private void CheckWindowSize()
    {
        if (Console.WindowWidth != windowWidth || Console.WindowHeight != windowHeight)
        {
            // Update the window size vars
            windowWidth = Console.WindowWidth;
            windowHeight = Console.WindowHeight;

            // Check if the window is too small
            if (windowWidth < RENDER_WIDTH || windowHeight < RENDER_HEIGHT)
            {
                ChangeState(STATE.ERROR);
                appError["windowSize"] = true;
            }
            else 
            {
                ChangeState(appStatePrev);
                appError["windowSize"] = false;
            }
        }
    }

    // Convert a byte to an eight digit string string
    private string ByteToString(byte bits)
    {
        return Convert.ToString(bits, 2).PadLeft(8, '0');
    }
    
    // Get the letter for a given register
    private char GetRegisterChar(byte index)
    {
        return (char)((int)'A' + index);
    }
    
    // Add a message to the message log
    private void MessageAdd(string message)
    {
        messageLog.Add(DateTime.Now.ToString("HH:mm:ss") + " " + message.ToUpper());
    }
    
    // Add assembly code to the assembly log
    private void AssemblyAdd(string line)
    {
        containerScroll[STATE.ASSEMBLY].AddContent(line);
    }

    // Initialize values
    private void Init() 
    {
        MessageAdd("Welcome!");

        // Make seperator and blank line
        for (int i = 0; i < RENDER_WIDTH; i++)
        {
            blankLine += ' ';
            seperatorLine[0] += '-';
            seperatorLine[1] += '*';
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
        //DIGITS[0,3,2] = true;

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

        // Set HELP text
        help = new List<string> 
        {
            "",
            " TODO: ... ABOUT BINARY, BITS AND BYTES...",
            " (ADD TEXT HERE ..)",
            "",
            " BASE-2 (BINARY) TO BASE-10 (DECIMAL):",
            "  128's  64's  32's  16's   8's   4's   2's   1's",
            "    |     |     |     |     |     |     |     |",
            "    0     0     0     0     0     1     1     1  =  7",
            "    0     1     1     1     1     1     1     1  =  127",
            "    1     0     0     0     0     0     0     1  =  128",
            "    1     1     1     1     1     1     1     1  =  255",
            "",
            " NEGATIVE NUMBERS USING TWO\'S COMPLIMENT:",
            " -128's  64's  32's  16's   8's   4's   2's   1's",
            "    |     |     |     |     |     |     |     |",
            "    0     0     0     0     0     1     1     1  =  7",
            "    0     1     1     1     1     1     1     1  =  127",
            "    1     0     0     0     0     0     0     1  = -128",
            "    1     1     1     1     1     1     1     1  = -1",
            "",
            seperatorLine[1],
            "",
            " " + COLORS.UL + "ADC: ADD WITH CARRY" + COLORS.DEFAULT + "                          (N,V,Z,C)",
            " ADDS THE VALUE OF A CHOSEN REGISTER [A] - [H]",
            " TO THE CURRENT REGISTER.",
            " THE (C)ARRY FLAG IS SET IF THE RESULTING VALUE",
            " IS ABOVE 255 (UNSIGNED).", 
            " THE O(V)ERFLOW FLAG IS SET IF ADDING TO A POSITIVE",
            " NUMBER AND ENDING UP WITH A NEGATIVE NUMBER.",
            "",
            " " + COLORS.UL + "SBC: SUBTRACT WITH CARRY" + COLORS.DEFAULT + "                     (N,V,Z,C)",
            " SUBTRACTS THE VALUE OF A CHOSEN REGISTER [A] - [H]",
            " FROM THE CURRENT REGISTER.",
            " THE (C)ARRY FLAG IS CLEAR IF THE RESULTING VALUE",
            " IS LESS THAN 0 (UNSIGNED).", 
            " THE O(V)ERFLOW FLAG IS SET IF SUBTRACTING FROM A",
            " NEGATIVE NUMBER AND ENDING UP WITH A POSITIVE NUMBER.",
            "",
            " " + COLORS.UL + "INC: INCREMENT" + COLORS.DEFAULT + "                               (N,Z)",
            " ADDS ONE TO THE VALUE OF THE CURRENT REGISTER.",
            "",
            " " + COLORS.UL + "DEC: DECREMENT" + COLORS.DEFAULT + "                               (N,Z)",
            " SUBTRACTS ONE FROM THE VALUE OF THE CURRENT REGISTER.",
            "",
            " " + COLORS.UL + "ASL: ARITHMETIC SHIFT LEFT" + COLORS.DEFAULT + "                   (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE LEFT",
            " INSERTING A 0 IN THE RIGHTMOST BIT",
            " AND MOVING THE LEFTMOST BIT TO THE (C)ARRY FLAG.",
            " THIS OPERATION IS EQUIVALENT TO MULTIPLYING BY 2.",
            "",
            " " + COLORS.UL + "LSR: LOGICAL SHIFT RIGHT" + COLORS.DEFAULT + "                     (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE RIGHT",
            " INSERTING A 0 IN THE LEFTMOST BIT",
            " AND MOVING THE RIGHTMOST BIT TO THE (C)ARRY FLAG.",
            " THIS OPERATION IS EQUIVALENT TO DIVIDING BY 2.",
            "",
            " " + COLORS.UL + "ROL: ROTATE LEFT" + COLORS.DEFAULT + "                             (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE LEFT",
            " THE LEFTMOST BIT MOVES OVER TO THE RIGHTMOST SIDE.",
            "",
            " " + COLORS.UL + "ROR: ROTATE RIGHT" + COLORS.DEFAULT + "                            (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE RIGHT",
            " THE RIGHTMOST BIT MOVES OVER TO THE LEFTMOST SIDE.",
            "",
            " " + COLORS.UL + "AND: LOGICAL AND" + COLORS.DEFAULT + "                             (N,Z)",
            " THE RESULT OF A LOGICAL AND IS ONLY TRUE",
            " IF BOTH INPUTS ARE TRUE.",
            " CAN BE USED TO MASK BITS.",
            " CAN BE USED TO CHECK FOR EVEN/ODD NUMBERS.",
            " CAN BE USED TO CHECK IF A NUMBER IS",
            " DIVISIBLE BY 2/4/6/8 ETC.",
            "",
            " " + COLORS.UL + "EOR: EXCLUSIVE OR" + COLORS.DEFAULT + "                            (N,Z)",
            " AN EXCLUSIVE OR IS SIMILAR TO LOGICAL OR, WITH THE",
            " EXCEPTION THAT IS IS FALSE WHEN BOTH INPUTS ARE TRUE.",
            " EOR CAN BE USED TO FLIP BITS.",
            "",
            " " + COLORS.UL + "ORA: LOGICAL INCLUSIVE OR" + COLORS.DEFAULT + "                    (N,Z)",
            " THE RESULT OF A LOGICAL INCLUSIVE OR IS TRUE IF",
            " AT LEAST ONE OF THE INPUTS ARE TRUE.",
            " ORA CAN BE USED TO SET A PARTICULAR BIT TO TRUE.",
            " ORA + EOR CAN BE USED TO SET A PARTICULAR BIT TO FALSE.",
            "",
            seperatorLine[1],
            "",
            " FLAGS ARE SET AFTER PERFORMING OPERATIONS.",
            " FLAGS ARE INDICATED BY PARANTHESIS.",
            "",
            " FOUR FLAGS ARE IMPLEMENTED:",
            " (C) CARRY FLAG",
            " (Z) ZERO FLAG:     IS SET TO 1 IF THE RESULT IS ZERO",
            " (O) OVERFLOW FLAG",
            " (N) NEGATIVE FLAG: IS SET TO 1 IF BIT 7 IS 1",
            "",
            seperatorLine[1],
            "",
            " A REGISTER HOLDS ONE BYTE OF DATA.",
            " REGISTERS ARE INDICATED BY SQUARE BRACKETS.",
            " EIGHT REGISTERS [A] TO [H] ARE IMPLEMENTED.",
            "",
            seperatorLine[1],
            "",
            " KEYBINDINGS AND MODIFIERS:",
            " <KEY>",
            " ...",
            "",
        };

        //containers[STATE.SETTINGS].SetContent(new List <string>
        //{
        //    "Highlight changed bit",
        //    "(not implemented) Automatically set/clear carry",
        //    "(not implemented) Save assembly log to file"
        //});
        
        containerScroll[STATE.HELP].LinkContent(help);
        containerScroll[STATE.ASSEMBLY].LinkContent(assembly);
        containerToggle[STATE.SETTINGS].LinkContent(settings);
    }

    // Unset all flags
    private void UnsetFlags()
    {
        carryFlag = false;
        zeroFlag = false;
        negativeFlag = false;
        overflowFlag = false;
    }

    // Set zero flag if value is zero
    private void SetZeroFlag() 
    {
        if (registers[registerIndex] == 0b00000000) { zeroFlag = true; }
    }

    // Set negative flag if bit 7 is set
    private void SetNegativeFlag()
    {
        if ((registers[registerIndex] & 0b10000000) == 0b10000000) { negativeFlag = true; }
    }

    // Change active register
    private void ChangeRegister(byte targetRegister)
    {
        registerIndex = targetRegister;
    }

    // Exclusive OR operation
    private void EOR(byte bits, bool silent = false)
    {
        registersPrev[registerIndex] = registers[registerIndex];
        UnsetFlags();
        registers[registerIndex] = (byte)(registers[registerIndex] ^ bits);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "EOR: Exclusive OR         <- " + ByteToString(bits)); }
        AssemblyAdd("EOR #%" + ByteToString(bits));
    }

    // Arithmetic shift left operation
    private void ASL(bool silent = false)
    {
        registersPrev[registerIndex] = registers[registerIndex];
        UnsetFlags();
        if ((registers[registerIndex] & 0b10000000) == 0b10000000) { carryFlag = true; }
        registers[registerIndex] = (byte)(registers[registerIndex] << 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "ASL: Arithmetic shift left"); }
        AssemblyAdd("ASL");
    }

    // Logical shift right operation
    private void LSR(bool silent = false)
    {
        registersPrev[registerIndex] = registers[registerIndex];
        UnsetFlags();
        if ((registers[registerIndex] & 0b00000001) == 0b00000001) {carryFlag = true;}
        registers[registerIndex] = (byte)(registers[registerIndex] >> 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "LSR: Logical shift right"); }
        AssemblyAdd("LSR");
    }

    // Rotate left operation
    private void ROL(bool silent = false)
    {
        registersPrev[registerIndex] = registers[registerIndex];
        UnsetFlags();
        if ((registers[registerIndex] & 0b10000000) == 0b10000000) { carryFlag = true; }
        registers[registerIndex] = (byte)(registers[registerIndex] << 1);
        if (carryFlag) { registers[registerIndex] += 0b00000001; }
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "ROL: Rotate left"); }
        AssemblyAdd("ROL");
    }
    
    // Rotate right operation
    private void ROR(bool silent = false)
    {
        registersPrev[registerIndex] = registers[registerIndex];
        UnsetFlags();
        if ((registers[registerIndex] & 0b00000001) == 0b00000001) {carryFlag = true;}
        registers[registerIndex] = (byte)(registers[registerIndex] >> 1);
        if (carryFlag) { registers[registerIndex] += 0b10000000; }
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "ROR: Rotate right"); }
        AssemblyAdd("ROR");
    }

    // Decrement operation
    private void DEC(bool silent = false)
    {
        registersPrev[registerIndex] = registers[registerIndex];
        UnsetFlags();
        //if ((registers[registerIndex] & 0b00000001) == 0b00000001) {carryFlag = true;}
        registers[registerIndex] -= (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "DEC: Decrement"); }
        AssemblyAdd("DEC");
    }
    
    // Increment operation
    private void INC(bool silent = false)
    {
        registersPrev[registerIndex] = registers[registerIndex];
        UnsetFlags();
        //if (registers[registerIndex] == 0b11111111) { carryFlag = true; }
        registers[registerIndex] += (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "INC: Increment"); }
        AssemblyAdd("INC");
    }

    // Add with carry operation
    private void ADC(byte bits, bool silent = false)
    {
        registersPrev[registerIndex] = registers[registerIndex];
        UnsetFlags();
        if (((registers[registerIndex] & 0b10000000) == 0b10000000) && (((registers[registerIndex] + bits) & 0b10000000) == 0b00000000)) { carryFlag = true; }
        if (((registers[registerIndex] & 0b10000000) == 0b00000000) && (((registers[registerIndex] + bits) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[registerIndex] += bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "ADC: Add with carry       <- " + ByteToString(bits) ); }
        AssemblyAdd("ADC #%" + ByteToString(bits));
    }
    
    // Subtract with carry operation
    private void SBC(byte bits, bool silent = false)
    {
        registersPrev[registerIndex] = registers[registerIndex];
        UnsetFlags();
        if (((registers[registerIndex] & 0b00000001) == 0b00000001) && (((registers[registerIndex] + bits) & 0b00000001) == 0b00000000)) { carryFlag = true; }
        if (((registers[registerIndex] & 0b10000000) == 0b00000000) && (((registers[registerIndex] + bits) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[registerIndex] -= bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "SBC: Subtract with carry  <- " + ByteToString(bits) ); }
        AssemblyAdd("SBC #%" + ByteToString(bits));
    }

    // Add value of the selected registry to the current registry
    private void AddRegistry(byte targetRegister, bool silent = false)
    {
        if (registerIndex != targetRegister)
        {
            byte bits = registers[targetRegister];
            ADC(bits, true);
            if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "ADC: Add with carry       <- [" + GetRegisterChar(targetRegister) + "] " + ByteToString(bits) ); }
        }
    }

    // Subtract value of the current registry with the selected registry
    private void SubtractRegistry(byte targetRegister, bool silent = false)
    {
        if (registerIndex != targetRegister)
        {
            byte bits = registers[targetRegister];
            SBC(bits, true);
            if (!silent) { MessageAdd("[" + GetRegisterChar(registerIndex) + "] " + "SBC: Subtract with carry  <- [" + GetRegisterChar(targetRegister) + "] " + ByteToString(bits) ); }
        }
    }

    // Handle input in the MAIN screen
    private bool GetInputMain(ConsoleKeyInfo key, bool modOne, bool modTwo)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.A:
                if (modOne) { AddRegistry(0b00000000); }
                else if (modTwo) { SubtractRegistry(0b00000000); }
                else { ChangeRegister(0b00000000); }
                return true;
            case ConsoleKey.B:
                if (modOne) { AddRegistry(0b00000001); }
                else if (modTwo) { SubtractRegistry(0b00000001); }
                else { ChangeRegister(0b00000001); }
                return true;
            case ConsoleKey.C:
                if (modOne) { AddRegistry(0b00000010); }
                else if (modTwo) { SubtractRegistry(0b00000010); }
                else { ChangeRegister(0b00000010); }
                return true;
            case ConsoleKey.D:
                if (modOne) { AddRegistry(0b00000011); }
                else if (modTwo) { SubtractRegistry(0b00000011); }
                else { ChangeRegister(0b00000011); }
                return true;
            case ConsoleKey.E:
                if (modOne) { AddRegistry(0b00000100); }
                else if (modTwo) { SubtractRegistry(0b00000100); }
                else { ChangeRegister(0b00000100); }
                return true;
            case ConsoleKey.F:
                if (modOne) { AddRegistry(0b00000101); }
                else if (modTwo) { SubtractRegistry(0b00000101); }
                else { ChangeRegister(0b00000101); }
                return true;
            case ConsoleKey.G:
                if (modOne) { AddRegistry(0b00000110); }
                else if (modTwo) { SubtractRegistry(0b00000110); }
                else { ChangeRegister(0b00000110); }
                return true;
            case ConsoleKey.H:
                if (modOne) { AddRegistry(0b00000111); }
                else if (modTwo) { SubtractRegistry(0b00000111); }
                else { ChangeRegister(0b00000111); }
                return true;
            case ConsoleKey.D0:
                EOR(0b00000001);
                return true;
            case ConsoleKey.D1:
                EOR(0b00000010);
                return true;
            case ConsoleKey.D2:
                EOR(0b00000100);
                return true;
            case ConsoleKey.D3:
                EOR(0b00001000);
                return true;
            case ConsoleKey.D4:
                EOR(0b00010000);
                return true;
            case ConsoleKey.D5:
                EOR(0b00100000);
                return true;
            case ConsoleKey.D6:
                EOR(0b01000000);
                return true;
            case ConsoleKey.D7:
                EOR(0b10000000);
                return true;
            case ConsoleKey.LeftArrow:
                if (modOne) { ROL(); } else { ASL(); }
                return true;
            case ConsoleKey.RightArrow:
                if (modOne) { ROR(); } else { LSR(); }
                return true;
            case ConsoleKey.UpArrow:
                INC();
                return true;
            case ConsoleKey.DownArrow:
                DEC();
                return true;
            case ConsoleKey.M:
                ChangeState(STATE.ASSEMBLY);
                return true;
            case ConsoleKey.S:
                ChangeState(STATE.SETTINGS);
                return true;
            case ConsoleKey.Escape:
                running = false;
                return true;
        }
        switch (key.KeyChar)
        {
            case '?':
                ChangeState(STATE.HELP);
                return true;
        }

        // No valid input
        return false;
    }

    // Handle input in the HELP screen
    private bool GetInputHelp(ConsoleKeyInfo key, bool modOne, bool modTwo)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                return containerScroll[STATE.HELP].Up();
            case ConsoleKey.DownArrow:
                return containerScroll[STATE.HELP].Down();
            case ConsoleKey.Escape:
                ChangeState(STATE.MAIN);
                return true;
        }

        // No valid input
        return false;
    }

    // Handle input in the SETTINGS screen
    private bool GetInputSettings(ConsoleKeyInfo key, bool modOne, bool modTwo)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                return containerToggle[STATE.SETTINGS].Up();
            case ConsoleKey.DownArrow:
                return containerToggle[STATE.SETTINGS].Down();
            case ConsoleKey.LeftArrow:
                return containerToggle[STATE.SETTINGS].Toggle();
            case ConsoleKey.RightArrow:
                return containerToggle[STATE.SETTINGS].Toggle();
            case ConsoleKey.Escape:
                ChangeState(STATE.MAIN);
                return true;
        }

        // No valid input
        return false;
    }

    // Handle input in the MESSAGES screen
    private bool GetInputAssembly(ConsoleKeyInfo key, bool modOne, bool modTwo)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                return containerScroll[STATE.ASSEMBLY].Up();
            case ConsoleKey.DownArrow:
                return containerScroll[STATE.ASSEMBLY].Down();
            case ConsoleKey.Escape:
                ChangeState(STATE.MAIN);
                return true;
        }

        // No valid input
        return false;
    }

    // Handle input in the ERROR screen
    private bool GetInputError(ConsoleKeyInfo key, bool modOne, bool modTwo)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.Enter:
                Console.Clear();
                return true;
            case ConsoleKey.Escape:
                running = false;
                return true;
        }

        // No valid input
        return false;
    }

    // Get input from user
    private void GetInput() 
    {
        // Loop until valid input is given
        bool validKey = false;
        while (!validKey)
        {
            // Wait for and get key input from user
            ConsoleKeyInfo key = Console.ReadKey(true);

            // Check if shift was pressed
            bool keyShift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);
            bool keyAlt = key.Modifiers.HasFlag(ConsoleModifiers.Alt);
            
            // Call the correct GetInput function for the current application STATE
            switch (appState)
            {
                case STATE.MAIN:
                    validKey = GetInputMain(key, keyShift, keyAlt);
                    break;
                case STATE.HELP:
                    validKey = GetInputHelp(key, keyShift, keyAlt);
                    break;
                case STATE.SETTINGS:
                    validKey = GetInputSettings(key, keyShift, keyAlt);
                    break;
                case STATE.ASSEMBLY:
                    validKey = GetInputAssembly(key, keyShift, keyAlt);
                    break;
                case STATE.ERROR:
                    validKey = GetInputError(key, keyShift, keyAlt);
                    break;
            }
        }
    }
    
    // Main titlebar
    private void RenderTitlebar()
    {
        Console.WriteLine();
        Console.WriteLine(COLORS.FG_BLACK + COLORS.BG_BRIGHT_CYAN + (MAIN_TITLE.PadLeft((RENDER_WIDTH - MAIN_TITLE.Length) / 2 + MAIN_TITLE.Length, ' ')).PadRight(RENDER_WIDTH, ' ') + COLORS.DEFAULT);
    }

    // Render the MAIN screen
    private void RenderMain()
    {
        // Main title
        RenderTitlebar();

        // Format the display string
        string displayLine = ByteToString(registers[registerIndex]);
       
        // Register selector
        Console.WriteLine();
        string registerLine = " ";
        string registerLineHex = " ";
        for (byte i = 0; i < REGISTER_NUM; i++)
        {
            if (i == registerIndex) { registerLine += COLORS.FG_BLACK + COLORS.BG_BRIGHT_YELLOW; }
            registerLine += "  " + GetRegisterChar(i) + "  ";
            if (i == registerIndex) { registerLine += COLORS.DEFAULT; }
            registerLine += "  ";
            registerLineHex += " 0x" + registers[i].ToString("X2") + " ";
            registerLineHex += " ";
        }
        Console.WriteLine(registerLine);
        Console.WriteLine(registerLineHex);
        Console.WriteLine(seperatorLine[0]);

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

                // Check if the current bit was changed by last operation
                byte bitToCheck = (byte)(0b10000000 >> (byte)(x / CHAR_WIDTH));
                bool bitChanged = (registers[registerIndex] & bitToCheck) != (registersPrev[registerIndex] & bitToCheck);
                
                // Set the value of the current "pixel"
                display[y] += DIGITS[currentDigit,y,x % CHAR_WIDTH] ? (bitChanged && settings["uiHlChangedBit"].enabled) ? COLORS.FG_BRIGHT_YELLOW + FILLED + COLORS.DEFAULT : FILLED : EMPTY;
            }

            // Render the results of the current row
            Console.WriteLine(display[y]);
        }

        // Decimal values
        Console.WriteLine(seperatorLine[0]);
        Console.WriteLine(" UNSIGNED VALUE:     " + (registers[registerIndex].ToString()).PadLeft(3,' ') + "        SIGNED VALUE:      " + (((int)((registers[registerIndex] & 0b01111111) - (registers[registerIndex] & 0b10000000))).ToString()).PadLeft(4,' '));
        //Console.WriteLine(" ASCII CHARACTER:  " + (registers[registerIndex] > 32 ? (char)registers[registerIndex] : "-"));

        Console.WriteLine(seperatorLine[0]);
        
        // Flags
        string carryFlagString = " (C) CARRY FLAG:       " + COLORS.FG_BLACK + (carryFlag ? COLORS.BG_BRIGHT_GREEN + "1" : COLORS.BG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        string zeroFlagString = "        (Z) ZERO FLAG:        " + COLORS.FG_BLACK + (zeroFlag ? COLORS.BG_BRIGHT_GREEN + "1"  : COLORS.BG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        string negativeFlagString = " (N) NEGATIVE FLAG:    " + COLORS.FG_BLACK + (negativeFlag ? COLORS.BG_BRIGHT_GREEN + "1" : COLORS.BG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        string overFlowFlagString = "        (V) OVERFLOW FLAG:    " + COLORS.FG_BLACK + (overflowFlag ? COLORS.BG_BRIGHT_GREEN + "1" : COLORS.BG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        Console.WriteLine(carryFlagString + zeroFlagString);
        Console.WriteLine(negativeFlagString + overFlowFlagString);
        
        Console.WriteLine(seperatorLine[0]);
        
        // Message log
        Console.WriteLine(messageLog.Count > 7 ? " " + messageLog[messageLog.Count-8] : " -");
        Console.WriteLine(messageLog.Count > 6 ? " " + messageLog[messageLog.Count-7] : " -");
        Console.WriteLine(messageLog.Count > 5 ? " " + messageLog[messageLog.Count-6] : " -");
        Console.WriteLine(messageLog.Count > 4 ? " " + messageLog[messageLog.Count-5] : " -");
        Console.WriteLine(messageLog.Count > 3 ? " " + messageLog[messageLog.Count-4] : " -");
        Console.WriteLine(messageLog.Count > 2 ? " " + messageLog[messageLog.Count-3] : " -");
        Console.WriteLine(messageLog.Count > 1 ? " " + messageLog[messageLog.Count-2] : " -");
        Console.WriteLine(messageLog.Count > 0 ? " " + messageLog.Last() : " -");
        
        Console.WriteLine(seperatorLine[0]);
        
        // Keyboard shortcuts
        Console.WriteLine(" <UP>              INCREMENT                  (N,Z)");
        Console.WriteLine(" <DOWN>            DECREMENT                  (N,Z)");
        Console.WriteLine(" <LEFT>            ARITHMETIC SHIFT LEFT      (N,Z,C)");
        Console.WriteLine(" <RIGHT>           LOGICAL SHIFT RIGHT        (N,Z,C)");
        Console.WriteLine(" <S+LEFT>          ROTATE LEFT                (N,Z,C)");
        Console.WriteLine(" <S+RIGHT>         ROTATE RIGHT               (N,Z,C)");
        Console.WriteLine(" <0> - <7>         EXCLUSIVE OR               (N,Z)");
        Console.WriteLine(" <S+A> - <S+H>     ADD WITH CARRY             (N,V,Z,C)");
        Console.WriteLine(" <A+A] - <A+H>     SUBTRACT WITH CARRY        (N,V,Z,C)");
        Console.WriteLine(" <A> - <H>         CHANGE ACTIVE REGISTER");
        Console.WriteLine(" <M>               ASSEMBLY LOG");
        Console.WriteLine(" <?>               HELP");
        Console.WriteLine(" <S>               SETTINGS");
        Console.WriteLine(seperatorLine[0]);
        Console.WriteLine(" PRESS <ESC> TO QUIT PROGRAM");
    }

    // Render the HELP screen
    private void RenderHelp(){
        
        // Main title
        RenderTitlebar();
        Console.WriteLine();
        Console.WriteLine(" " + COLORS.BG_BRIGHT_MAGENTA + COLORS.FG_BLACK + " HELP SCREEN " + COLORS.DEFAULT + "      <UP> SCROLL UP / <DOWN> SCROLL DOWN");
        Console.WriteLine(seperatorLine[0]);

        // Render the HELP container
        containerScroll[STATE.HELP].Render();

        // Show keybinds
        Console.WriteLine(seperatorLine[0]);
        Console.WriteLine(" PRESS <ESC> TO RETURN TO MAIN SCREEN " + containerScroll[STATE.HELP].GetScrollPos().PadLeft(17));
    }
    
    // Render the SETTINGS screen
    private void RenderSettings(){
        
        // Main title
        RenderTitlebar();
        Console.WriteLine();
        Console.WriteLine(" " + COLORS.BG_BRIGHT_MAGENTA + COLORS.FG_BLACK + " SETTINGS " + COLORS.DEFAULT + "   <UP> SELECTION UP / <DOWN> SELECTION DOWN");
        Console.WriteLine(seperatorLine[0]);

        // Render the SETTINGS container
        containerToggle[STATE.SETTINGS].Render();

        // Show keybinds
        Console.WriteLine(seperatorLine[0]);
        Console.WriteLine(" PRESS <ESC> TO RETURN TO MAIN SCREEN");
    }

    // Render the ASSEMBLY screen
    private void RenderAssembly(){
        
        // Main title
        RenderTitlebar();
        Console.WriteLine();
        Console.WriteLine(" " + COLORS.BG_BRIGHT_MAGENTA + COLORS.FG_BLACK + " ASSEMBLY LOG " + COLORS.DEFAULT + "     <UP> SCROLL UP / <DOWN> SCROLL DOWN");
        Console.WriteLine(seperatorLine[0]);

        // Render the ASSEMBLY container
        containerScroll[STATE.ASSEMBLY].Render();

        // Show keybinds
        Console.WriteLine(seperatorLine[0]);
        Console.WriteLine(" PRESS <ESC> TO RETURN TO MAIN SCREEN " + containerScroll[STATE.ASSEMBLY].GetScrollPos().PadLeft(17));
    }

    // Render the ERROR screen
    private void RenderError()
    {
        // Clear the console
        Console.Clear();

        // Show error message
        Console.WriteLine("ERROR!");

        // Show error message if window size is too small
        if (appError["windowSize"])
        {
            Console.WriteLine("WINDOW SIZE IS TOO SMALL!");
            Console.WriteLine("PLEASE RESIZE THE WINDOW");
            Console.WriteLine("AND PRESS <ENTER>");
            Console.WriteLine("OR PRESS <ESC> TO QUIT");
        }
    }

    // Render the result on screen
    private void Render()
    {
        // Clear the console
        Console.SetCursorPosition(0,0);
        for (int i = 0; i < RENDER_HEIGHT; i++){
            Console.WriteLine(blankLine);
        }
        Console.SetCursorPosition(0,0);
      
        // Call the correct Render function for the current application STATE
        switch (appState)
        {
            case STATE.MAIN:
                RenderMain();
                break;
            case STATE.HELP:
                RenderHelp();
                break;
            case STATE.SETTINGS:
                RenderSettings();
                break;
            case STATE.ASSEMBLY:
                RenderAssembly();
                break;
            case STATE.ERROR:
                RenderError();
                break;
        }
    }
    
    // Main loop
    private void Run()
    {
        // Clear console and hide cursor
        Console.CursorVisible = false;
        Console.Clear();
        
        while (running == true)
        {
            CheckWindowSize();
            Render();
            GetInput();
        }

        // Exit the program and reset the console
        Console.WriteLine(COLORS.DEFAULT);
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

