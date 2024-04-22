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
    private const int RENDER_HEIGHT = 44;
    private const int REGISTER_NUM = 3;

    // Set global variables
    private bool running = true;
    private STATE appState = STATE.MAIN;
    private STATE appStatePrev = STATE.MAIN;
    private string[] display = new string[CHAR_HEIGHT];
    private byte[] registers = new byte[REGISTER_NUM];
    private byte[] registersPrev = new byte[REGISTER_NUM];
    private byte registerIndex = 0;
    private bool carryFlag = false;
    private bool zeroFlag = false;
    private bool negativeFlag = false;
    private bool overflowFlag = false;
    private int windowWidth = 0;
    private int windowHeight = 0;

    // Declare logger
    public List<string> messages = new List<string>();
    public List<string> assembly = new List<string>();

    // Settings
    private Dictionary<string, Setting> settings = new Dictionary<string, Setting>
    {
        { "uiHlChangedBit", new Setting(true,  "HIGHLIGHT BITS CHANGED IN PREVIOUS OPERATION") },
        { "flagsAutoCarry", new Setting(false, "AUTOMATICALLY SET/UNSET CARRY FLAG") },
        { "loggerAsmFile",  new Setting(true,  "(NOT IMPLEMENTED!) SAVE ASM FILE") }
    }; 

    // Dictionary for HELP content
    List <string> help = new List<string>();
    
    // Create scroll containers
    Dictionary<STATE, ContainerScroll> containerScroll = new Dictionary<STATE, ContainerScroll>
    {
        { STATE.HELP,     new ContainerScroll("HELP SCREEN",  RENDER_HEIGHT - 7) },
        { STATE.ASSEMBLY, new ContainerScroll("ASSEMBLY LOG", RENDER_HEIGHT - 7, startAtBottom: true, showLineNum: true) }
    };

    // Create toggle containers
    Dictionary<STATE, ContainerToggle> containerToggle = new Dictionary<STATE, ContainerToggle>
    {
        { STATE.SETTINGS, new ContainerToggle("SETTINGS", RENDER_HEIGHT - 7) }
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

    // Add message to the message log
    public void AddMessage(string line)
    {
        messages.Add(DateTime.Now.ToString("HH:mm:ss") + " " + line.ToUpper());
    }

    // Add assembly code to the assembly log
    public void AddAssembly(string line)
    {
        containerScroll[STATE.ASSEMBLY].AddContent(line);
    }

    // Render message log
    public void RenderMessages(int num = 8)
    {
        for (int i = num; i >= 0; i--)
        {
            Console.WriteLine(messages.Count > i ? " " + messages[messages.Count-(i+1)] : " -");
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
        //return (char)((int)'A' + index);
        return (char)((int)'A' + (index == 0 ? 0 : index + 22));
    }

    // Initialize values
    private void Init() 
    {
        // Generate blank line and seperator lines
        Lines.Init(RENDER_WIDTH);

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
            Lines.seperator[1],
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
            Lines.seperator[1],
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
            Lines.seperator[1],
            "",
            " A REGISTER HOLDS ONE BYTE OF DATA.",
            " REGISTERS ARE INDICATED BY SQUARE BRACKETS.",
            " EIGHT REGISTERS [A] TO [H] ARE IMPLEMENTED.",
            "",
            Lines.seperator[1],
            "",
            " KEYBINDINGS AND MODIFIERS:",
            " <KEY>",
            " ...",
            "",
        };

        // Link lists with containers
        containerScroll[STATE.HELP].LinkContent(help);
        containerScroll[STATE.ASSEMBLY].LinkContent(assembly);
        containerToggle[STATE.SETTINGS].LinkContent(settings);

        AddMessage("Welcome!");
    }

    // Unset all flags
    private void UnsetFlags()
    {
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
        if (!silent) { AddMessage("[" + GetRegisterChar(registerIndex) + "] " + "EOR: Exclusive OR         <- " + ByteToString(bits)); }
        AddAssembly("EOR #%" + ByteToString(bits));
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
        if (!silent) { AddMessage("[" + GetRegisterChar(registerIndex) + "] " + "ASL: Arithmetic shift left"); }
        AddAssembly("ASL");
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
        if (!silent) { AddMessage("[" + GetRegisterChar(registerIndex) + "] " + "LSR: Logical shift right"); }
        AddAssembly("LSR");
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
        if (!silent) { AddMessage("[" + GetRegisterChar(registerIndex) + "] " + "ROL: Rotate left"); }
        AddAssembly("ROL");
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
        if (!silent) { AddMessage("[" + GetRegisterChar(registerIndex) + "] " + "ROR: Rotate right"); }
        AddAssembly("ROR");
    }

    // Decrement X operation
    private void DEX(bool silent = false)
    {
        registersPrev[1] = registers[1];
        UnsetFlags();
        registers[1] -= (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(1) + "] " + "DEX: Decrement X"); }
        AddAssembly("DEX");
    }
    
    // Decrement Y operation
    private void DEY(bool silent = false)
    {
        registersPrev[2] = registers[2];
        UnsetFlags();
        registers[2] -= (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(2) + "] " + "DEY: Decrement Y"); }
        AddAssembly("DEY");
    }
    
    // Increment X operation
    private void INX(bool silent = false)
    {
        registersPrev[1] = registers[1];
        UnsetFlags();
        registers[1] += (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(1) + "] " + "INX: Increment X"); }
        AddAssembly("INX");
    }
    
    // Increment operation
    private void INY(bool silent = false)
    {
        registersPrev[2] = registers[2];
        UnsetFlags();
        registers[2] += (byte)0b00000001;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(2) + "] " + "INY: Increment Y"); }
        AddAssembly("INY");
    }

    // Add with carry operation
    private void ADC(byte bits, bool silent = false, bool inc = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if (settings["flagsAutoCarry"].enabled && carryFlag) { CLC(); }
        if (carryFlag && inc) { bits = 0b00000000; }
        byte bitsWithCarry = (byte)(bits + (byte)(carryFlag ? 0b00000001 : 0b00000000));
        registers[0] += bitsWithCarry;
        if (((registers[0] & 0b10000000) == 0b10000000) && (((registers[0] + bits) & 0b10000000) == 0b00000000)) { carryFlag = true; }
        if (((registers[0] & 0b10000000) == 0b00000000) && (((registers[0] + bits) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "ADC: Add with carry       <- " + ByteToString(bitsWithCarry) ); }
        AddAssembly("ADC #%" + ByteToString(bits));
    }
    
    private void CLC(bool silent = false)
    {
        carryFlag = false;
        if (!silent) { AddMessage("CLC: CLEAR CARRY FLAG"); }
        AddAssembly("CLC");
    }
    
    private void SEC(bool silent = false)
    {
        carryFlag = true;
        if (!silent) { AddMessage("SEC: SET CARRY FLAG"); }
        AddAssembly("SEC");
    }

    // Subtract with carry operation
    private void SBC(byte bits, bool silent = false, bool dec = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if (settings["flagsAutoCarry"].enabled && !carryFlag) { SEC(); }
        if (!carryFlag && dec) { bits = 0b00000000; }
        byte bitsWithCarry = (byte)(bits + (byte)(carryFlag ? 0b00000000 : 0b00000001));
        registers[0] -= bitsWithCarry;
        //if (registers[0] < (byte)(registers[0] - bits)) { carryFlag = false; }
        if (((registers[0] & 0b10000000) == 0b00000000) && (((registers[0] - bits) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "SBC: Subtract with carry  <- " + ByteToString(bitsWithCarry) ); }
        AddAssembly("SBC #%" + ByteToString(bits));
    }

    private void Increment()
    {
        switch (registerIndex)
        {
            case 0:
                ADC((byte)(0b00000001), inc: true);
                break;
            case 1:
                INX();
                break;
            case 2:
                INY();
                break;
        }
    }
    
    private void Decrement()
    {
        switch (registerIndex)
        {
            case 0:
                SBC((byte)(0b00000001), dec: true);
                break;
            case 1:
                DEX();
                break;
            case 2:
                DEY();
                break;
        }
    }

    // Add value of the selected registry to the current registry
    private void AddRegister(byte targetRegister, bool silent = false)
    {
        if (registerIndex != targetRegister)
        {
            byte bits = registers[targetRegister];
            ADC(bits, true);
            if (!silent) { AddMessage("[" + GetRegisterChar(registerIndex) + "] " + "ADC: Add with carry       <- [" + GetRegisterChar(targetRegister) + "] " + ByteToString(bits) ); }
        }
    }

    // Subtract value of the current registry with the selected registry
    private void SubtractRegister(byte targetRegister, bool silent = false)
    {
        if (registerIndex != targetRegister)
        {
            byte bits = registers[targetRegister];
            SBC(bits, true);
            if (!silent) { AddMessage("[" + GetRegisterChar(registerIndex) + "] " + "SBC: Subtract with carry  <- [" + GetRegisterChar(targetRegister) + "] " + ByteToString(bits) ); }
        }
    }

    // Handle input in the MAIN screen
    private bool GetInputMain(ConsoleKeyInfo key, bool modOne, bool modTwo)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.A:
                if (!modOne && !modTwo) { ChangeRegister(0); return true; }
                return false;
            case ConsoleKey.X:
                if (registerIndex == 0 && modOne) { AddRegister(0b00000001); return true;}
                else if (registerIndex == 0 && modTwo) { SubtractRegister(1); return true; }
                else if (!modOne && !modTwo) { ChangeRegister(1); return true; }
                return false;
            case ConsoleKey.Y:
                if (registerIndex == 0 && modOne) { AddRegister(2); return true;}
                else if (registerIndex == 0 && modTwo) { SubtractRegister(2); return true; }
                else if (!modOne && !modTwo) { ChangeRegister(2); return true; }
                return false;
            case ConsoleKey.D0:
                if (registerIndex == 0) { EOR(0b00000001); return true; }
                return false;
            case ConsoleKey.D1:
                if (registerIndex == 0) { EOR(0b00000010); return true; }
                return false;
            case ConsoleKey.D2:
                if (registerIndex == 0) { EOR(0b00000100); return true; }
                return false;
            case ConsoleKey.D3:
                if (registerIndex == 0) { EOR(0b00001000); return true; }
                return false;
            case ConsoleKey.D4:
                if (registerIndex == 0) { EOR(0b00010000); return true; }
                return false;
            case ConsoleKey.D5:
                if (registerIndex == 0) { EOR(0b00100000); return true; }
                return false;
            case ConsoleKey.D6:
                if (registerIndex == 0) { EOR(0b01000000); return true; }
                return false;
            case ConsoleKey.D7:
                if (registerIndex == 0) { EOR(0b10000000); return true; }
                return false;
            case ConsoleKey.LeftArrow:
                if (registerIndex == 0) { if (modOne) { ROL(); } else { ASL(); } return true; }
                return false;
            case ConsoleKey.RightArrow:
                if (registerIndex == 0) {if (modOne) { ROR(); } else { LSR(); } return true; }
                return false;
            case ConsoleKey.UpArrow:
                Increment();
                return true;
            case ConsoleKey.DownArrow:
                Decrement();
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
        Console.WriteLine(Lines.seperator[0]);

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
        Console.WriteLine(Lines.seperator[0]);
        Console.WriteLine(" UNSIGNED VALUE:     " + (registers[registerIndex].ToString()).PadLeft(3,' ') + "        SIGNED VALUE:      " + (((int)((registers[registerIndex] & 0b01111111) - (registers[registerIndex] & 0b10000000))).ToString()).PadLeft(4,' '));
        //Console.WriteLine(" ASCII CHARACTER:  " + (registers[registerIndex] > 32 ? (char)registers[registerIndex] : "-"));

        Console.WriteLine(Lines.seperator[0]);
        
        // Flags
        string carryFlagString = " (C) CARRY FLAG:       " + COLORS.FG_BLACK + (carryFlag ? COLORS.BG_BRIGHT_GREEN + "1" : COLORS.BG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        string zeroFlagString = "        (Z) ZERO FLAG:        " + COLORS.FG_BLACK + (zeroFlag ? COLORS.BG_BRIGHT_GREEN + "1"  : COLORS.BG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        string negativeFlagString = " (N) NEGATIVE FLAG:    " + COLORS.FG_BLACK + (negativeFlag ? COLORS.BG_BRIGHT_GREEN + "1" : COLORS.BG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        string overFlowFlagString = "        (V) OVERFLOW FLAG:    " + COLORS.FG_BLACK + (overflowFlag ? COLORS.BG_BRIGHT_GREEN + "1" : COLORS.BG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        Console.WriteLine(carryFlagString + zeroFlagString);
        Console.WriteLine(negativeFlagString + overFlowFlagString);
        
        Console.WriteLine(Lines.seperator[0]);
       
        // Render message log
        RenderMessages();
        
        Console.WriteLine(Lines.seperator[0]);
        
        // Keyboard shortcuts
        Console.WriteLine(" <UP>              INCREMENT                  (N,Z)");
        Console.WriteLine(" <DOWN>            DECREMENT                  (N,Z)");
        Console.WriteLine((registerIndex == 0 ? "" : COLORS.FG_BRIGHT_BLACK ) + " <LEFT>            ARITHMETIC SHIFT LEFT      (N,Z,C)");
        Console.WriteLine(" <RIGHT>           LOGICAL SHIFT RIGHT        (N,Z,C)");
        Console.WriteLine(" <$1+LEFT>         ROTATE LEFT                (N,Z,C)");
        Console.WriteLine(" <$1+RIGHT>        ROTATE RIGHT               (N,Z,C)");
        Console.WriteLine(" <0> - <7>         EXCLUSIVE OR               (N,Z)");
        Console.WriteLine(" <$1+A> - <$1+H>   ADD WITH CARRY             (N,V,Z,C)");
        Console.WriteLine(" <$2+A> - <$2+H>   SUBTRACT WITH CARRY        (N,V,Z,C)");
        Console.WriteLine((registerIndex == 0 ? "" : COLORS.DEFAULT ) + " <A> <X> <Y>       CHANGE ACTIVE REGISTER");
        Console.WriteLine(" <M>               ASSEMBLY LOG");
        Console.WriteLine(" <S>               SETTINGS");
        Console.WriteLine(" <?>               HELP");
        Console.WriteLine(Lines.seperator[0]);
        Console.WriteLine(" PRESS <ESC> TO QUIT PROGRAM");
    }

    // Render the HELP screen
    private void RenderHelp(){
        
        // Main title
        RenderTitlebar();
        Console.WriteLine();

        // Render the HELP container
        containerScroll[STATE.HELP].Render();
    }
    
    // Render the SETTINGS screen
    private void RenderSettings(){
        
        // Main title
        RenderTitlebar();
        Console.WriteLine();
        
        // Render the SETTINGS container
        containerToggle[STATE.SETTINGS].Render();
    }

    // Render the ASSEMBLY screen
    private void RenderAssembly(){
        
        // Main title
        RenderTitlebar();
        Console.WriteLine();

        // Render the ASSEMBLY container
        containerScroll[STATE.ASSEMBLY].Render();
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
            Console.WriteLine(Lines.blank);
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
    public void Run()
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
    }
}

