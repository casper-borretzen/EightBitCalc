class BinaryCalc
{
    // Main application state
    private enum STATE 
    {
        MAIN,
        HELP,
        SETTINGS,
        ASSEMBLY,
        ERROR,
    };

    // Substates for MAIN state
    private enum MAIN_SUB_STATE 
    {
        DEFAULT,
        ADD,
        SUBTRACT,
        TRANSFER,
        STORE,
        LOAD,
        LOGICAL,
        EOR,
        AND,
        ORA,
        QUIT
    };

    // Different input modes
    private enum INPUT_MODE
    {
        NORMAL,
        COMMAND,
        NUMERICAL_DEC,
        NUMERICAL_BIN,
        NUMERICAL_HEX
    };

    // Different byte types
    private enum BYTE_TYPE 
    {
        BINARY,
        DECIMAL,
        HEX,
        ADDRESS
    };
    
    // Set color constants
    public const string COLOR_DEFAULT           = "\x1B[0m";
    public const string COLOR_UL                = "\x1B[4m";
    public const string COLOR_FG_BLACK          = "\x1B[30m";
    public const string COLOR_FG_WHITE          = "\x1B[97m";
    public const string COLOR_FG_DARK_RED       = "\x1B[31m";
    public const string COLOR_FG_DARK_GREEN     = "\x1B[32m";
    public const string COLOR_FG_DARK_YELLOW    = "\x1B[33m";
    public const string COLOR_FG_DARK_BLUE      = "\x1B[34m";
    public const string COLOR_FG_DARK_MAGENTA   = "\x1B[35m";
    public const string COLOR_FG_DARK_CYAN      = "\x1B[36m";
    public const string COLOR_FG_DARK_WHITE     = "\x1B[37m";
    public const string COLOR_FG_BRIGHT_BLACK   = "\x1B[90m";
    public const string COLOR_FG_BRIGHT_RED     = "\x1B[91m";
    public const string COLOR_FG_BRIGHT_GREEN   = "\x1B[92m";
    public const string COLOR_FG_BRIGHT_YELLOW  = "\x1B[93m";
    public const string COLOR_FG_BRIGHT_BLUE    = "\x1B[94m";
    public const string COLOR_FG_BRIGHT_MAGENTA = "\x1B[95m";
    public const string COLOR_FG_BRIGHT_CYAN    = "\x1B[96m";
    public const string COLOR_BG_BLACK          = "\x1B[40m";
    public const string COLOR_BG_WHITE          = "\x1B[107m";
    public const string COLOR_BG_DARK_RED       = "\x1B[41m";
    public const string COLOR_BG_DARK_GREEN     = "\x1B[42m";
    public const string COLOR_BG_DARK_YELLOW    = "\x1B[43m";
    public const string COLOR_BG_DARK_BLUE      = "\x1B[44m";
    public const string COLOR_BG_DARK_MAGENTA   = "\x1B[45m";
    public const string COLOR_BG_DARK_CYAN      = "\x1B[46m";
    public const string COLOR_BG_DARK_WHITE     = "\x1B[47m";
    public const string COLOR_BG_BRIGHT_BLACK   = "\x1B[100m";
    public const string COLOR_BG_BRIGHT_RED     = "\x1B[101m";
    public const string COLOR_BG_BRIGHT_GREEN   = "\x1B[102m";
    public const string COLOR_BG_BRIGHT_YELLOW  = "\x1B[103m";
    public const string COLOR_BG_BRIGHT_BLUE    = "\x1B[104m";
    public const string COLOR_BG_BRIGHT_MAGENTA = "\x1B[105m";
    public const string COLOR_BG_BRIGHT_CYAN    = "\x1B[106m";

    // Set class constants
    private const string MAIN_TITLE     = "8-BIT BINARY CALCULATOR";
    private const byte VERSION_MAJOR    = 8;
    private const byte VERSION_MINOR    = 0;
    private const byte VERSION_PATCH    = 0;
    private const char EMPTY            = ' ';
    private const char FILLED           = '#';
    private const int CHAR_SPACING      = 2;
    private const int CHAR_HEIGHT       = 7;
    private const int CHAR_WIDTH        = 5 + CHAR_SPACING;
    private const int CHAR_LIMIT        = 8;
    private const int RENDER_WIDTH      = CHAR_LIMIT * CHAR_WIDTH;
    private const int RENDER_HEIGHT     = 52;
    private const int REGISTER_NUM      = 3;
    private const int MEMORY_NUM        = 5;
    private const int MESSAGES_NUM      = 8;
    private const int INPUT_CHAR_LIMIT  = 14;

    // Set class variables
    private bool running                = true;
    private int windowWidth             = 0;
    private int windowHeight            = 0;
    private STATE appState              = STATE.MAIN;
    private STATE appStatePrev          = STATE.MAIN;
    private MAIN_SUB_STATE mainSubState = MAIN_SUB_STATE.DEFAULT;
    private INPUT_MODE inputMode        = INPUT_MODE.NORMAL;
    private bool[,,] digits             = new bool[2,CHAR_HEIGHT,CHAR_WIDTH];
    private string[] display            = new string[CHAR_HEIGHT];
    private byte[] registers            = new byte[REGISTER_NUM];
    private byte[] registersPrev        = new byte[REGISTER_NUM];
    private byte[] memory               = new byte[MEMORY_NUM];
    private byte registerIndex          = 0;
    private int messageIndex            = 0;
    private bool carryFlag              = false;
    private bool zeroFlag               = false;
    private bool negativeFlag           = false;
    private bool overflowFlag           = false;
    private string inputString          = "";

    // Make blank line and seperators
    private string blank = new String(' ', RENDER_WIDTH);
    public static string[] seperator =  
    {
        new String('-', RENDER_WIDTH),
        new String('/', RENDER_WIDTH)
    };

    // Logger lists and .asm log file
    public List<string> messages = new List<string>();
    public List<string> assembly = new List<string>();
    private StreamWriter asmFile = new StreamWriter("log.asm");

    // Set error types
    private Dictionary<string, bool> appError = new Dictionary<string, bool>
    {
        { "windowSize",     false }
    };

    // Settings
    private Dictionary<string, Setting> settings = new Dictionary<string, Setting>
    {
        { "uiHlChangedBit", new Setting(true,  "HIGHLIGHT BITS CHANGED IN PREVIOUS OPERATION") },
        { "flagsAutoCarry", new Setting(false, "AUTOMATICALLY SET/UNSET CARRY FLAG") }
    }; 
    
    // Create scroll containers
    private Dictionary<STATE, ContainerScroll> containerScroll = new Dictionary<STATE, ContainerScroll>
    {
        { STATE.HELP,       new ContainerScroll("HELP SCREEN",  RENDER_HEIGHT - 8) },
        { STATE.ASSEMBLY,   new ContainerScroll("ASSEMBLY LOG", RENDER_HEIGHT - 8, startAtBottom: true, showLineNum: true) }
    };

    // Create toggle containers
    private Dictionary<STATE, ContainerToggle> containerToggle = new Dictionary<STATE, ContainerToggle>
    {
        { STATE.SETTINGS,   new ContainerToggle("PREFERENCES", RENDER_HEIGHT - 8) }
    };

    // List for HELP screen text
    private List <string> help = new List <string>() 
    {
            "",
            " APPLICATION VERSION " + VERSION_MAJOR.ToString() + "." + VERSION_MINOR.ToString() + "." + VERSION_PATCH.ToString(),
            "",
            " " + COLOR_UL + "ASSEMBLY LOG:" + COLOR_DEFAULT,
            " THE ASSEMBLY LOG IS AUTOMATICALLY SAVED",
            " TO LOG.ASM WHEN QUITTING.",
            "",
            " " + COLOR_UL + "MEMORY:" + COLOR_DEFAULT,
            " THERE ARE 5 BYTES OF MEMORY AVAILABLE",
            " IN THE ADDRESS RANGE $00 - $04.",
            "",
            " " + COLOR_UL + "FLAGS:" + COLOR_DEFAULT,
            " FOUR FLAGS ARE IMPLEMENTED.",
            " FLAGS ARE SET AFTER PERFORMING OPERATIONS.",
            " FLAGS ARE INDICATED BY PARANTHESIS.",
            "",
            " " + COLOR_UL + "(C) CARRY FLAG:" + COLOR_DEFAULT,
            " FUNCTIONALITY DIFFERS DEPENDING ON OPERATION.",
            "",
            " " + COLOR_UL + "(Z) ZERO FLAG:" + COLOR_DEFAULT,
            " IS SET TO 1 IF THE RESULT IS ZERO.",
            "",
            " " + COLOR_UL + "(O) OVERFLOW FLAG:" + COLOR_DEFAULT,
            " FUNCTIONALITY DIFFERS DEPENDING ON OPERATION.",
            "",
            " " + COLOR_UL + "(N) NEGATIVE FLAG:" + COLOR_DEFAULT,
            " IS SET TO 1 IF BIT 7 IS 1.",
            "",
            " " + COLOR_UL + "ACCUMULATOR AND REGISTERS:" + COLOR_DEFAULT,
            " THE ACCUMULATOR [A] AND THE REGISTERS [X] AND [Y]",
            " EACH HOLDS ONE BYTE OF DATA.",
            " ARITHMETIC OPERATIONS ARE AVAILABLE ON THE",
            " ACCUMULATOR, BUT NOT ON THE REGISTERS.",
            " INCREMENT AND DECREMENT OPERATIONS ARE AVAILABLE",
            " ON THE REGISTERS, BUT NOT ON THE ACCUMULATOR.",
            " ACCUMULATOR AND REGISTERS ARE INDICATED",
            " BY SQUARE BRACKETS.",
            "",
            " " + COLOR_UL + "LOADING / STORING MODES:" + COLOR_DEFAULT,
            " IMMEDIATE MODE:            LDA #20",
            " LOADS [A] WITH THE LITERAL DECIMAL VALUE 20.",
            " ",
            " ABSOLUTE (ZERO-PAGE) MODE: LDA $05",
            " LOADS [A] WITH THE VALUE FROM MEMORY ADDRESS $05",
            " ",
            " IMMEDIATE MODE:            LDA #$20",
            " LOADS [A] WITH THE LITERAL HEXADECIMAL NUMBER 20.",
            "",
            " " + COLOR_UL + "BASE-2 (BINARY) TO BASE-10 (DECIMAL):" + COLOR_DEFAULT,
            "  128's  64's  32's  16's   8's   4's   2's   1's",
            "    |     |     |     |     |     |     |     |",
            "    0     0     0     0     0     1     1     1  =  7",
            "    0     1     1     1     1     1     1     1  =  127",
            "    1     0     0     0     0     0     0     1  =  128",
            "    1     1     1     1     1     1     1     1  =  255",
            "",
            " " + COLOR_UL + "NEGATIVE NUMBERS USING TWO\'S COMPLIMENT:" + COLOR_DEFAULT,
            " -128's  64's  32's  16's   8's   4's   2's   1's",
            "    |     |     |     |     |     |     |     |",
            "    0     0     0     0     0     1     1     1  =  7",
            "    0     1     1     1     1     1     1     1  =  127",
            "    1     0     0     0     0     0     0     1  = -128",
            "    1     1     1     1     1     1     1     1  = -1",
            "",
            seperator[1],
            "",
            " " + COLOR_UL + "LDA: LOAD ACCUMULATOR" + COLOR_DEFAULT + "                        (N,Z)",
            " LOAD A BYTE OF DATA INTO THE ACCUMULATOR.",
            "",
            " " + COLOR_UL + "LDX: LOAD X REGISTER" + COLOR_DEFAULT + "                         (N,Z)",
            " LOAD A BYTE OF DATA INTO THE X REGISTER.",
            "",
            " " + COLOR_UL + "LDX: LOAD Y REGISTER" + COLOR_DEFAULT + "                         (N,Z)",
            " LOAD A BYTE OF DATA INTO THE Y REGISTER.",
            "",
            " " + COLOR_UL + "STA: STORE ACCUMULATOR" + COLOR_DEFAULT + "                       (N,Z)",
            " STORE THE CONTENT OF THE ACCUMULATOR INTO MEMORY.",
            "",
            " " + COLOR_UL + "STX: STORE X REGISTER" + COLOR_DEFAULT + "                        (N,Z)",
            " STORE THE CONTENT OF THE X REGISTER INTO MEMORY.",
            "",
            " " + COLOR_UL + "STY: STORE Y REGISTER" + COLOR_DEFAULT + "                        (N,Z)",
            " STORE THE CONTENT OF THE Y REGISTER INTO MEMORY.",
            "",
            " " + COLOR_UL + "TAX: TRANSFER ACCUMULATOR TO X" + COLOR_DEFAULT + "               (N,Z)",
            " COPIES THE DATA FROM [A] TO [X].",
            "",
            " " + COLOR_UL + "TAY: TRANSFER ACCUMULATOR TO Y" + COLOR_DEFAULT + "               (N,Z)",
            " COPIES THE DATA FROM [A] TO [Y].",
            "",
            " " + COLOR_UL + "TXA: TRANSFER X TO ACCUMULATOR" + COLOR_DEFAULT + "               (N,Z)",
            " COPIES THE DATA FROM [X] TO [A].",
            "",
            " " + COLOR_UL + "TYA: TRANSFER Y TO ACCUMULATOR" + COLOR_DEFAULT + "               (N,Z)",
            " COPIES THE DATA FROM [Y] TO [A].",
            "",
            " " + COLOR_UL + "AND: LOGICAL AND" + COLOR_DEFAULT + "                             (N,Z)",
            " THE RESULT OF A LOGICAL AND IS ONLY TRUE",
            " IF BOTH INPUTS ARE TRUE.",
            " CAN BE USED TO MASK BITS.",
            " CAN BE USED TO CHECK FOR EVEN/ODD NUMBERS.",
            " CAN BE USED TO CHECK IF A NUMBER IS",
            " DIVISIBLE BY 2/4/6/8 ETC.",
            "",
            " " + COLOR_UL + "EOR: EXCLUSIVE OR" + COLOR_DEFAULT + "                            (N,Z)",
            " AN EXCLUSIVE OR IS SIMILAR TO LOGICAL OR, WITH THE",
            " EXCEPTION THAT IS IS FALSE WHEN BOTH INPUTS ARE TRUE.",
            " EOR CAN BE USED TO FLIP BITS.",
            "",
            " " + COLOR_UL + "ORA: LOGICAL INCLUSIVE OR" + COLOR_DEFAULT + "                    (N,Z)",
            " THE RESULT OF A LOGICAL INCLUSIVE OR IS TRUE IF",
            " AT LEAST ONE OF THE INPUTS ARE TRUE.",
            " ORA CAN BE USED TO SET A PARTICULAR BIT TO TRUE.",
            " ORA + EOR CAN BE USED TO SET A BIT TO FALSE.",
            "",
            " " + COLOR_UL + "ADC: ADD WITH CARRY" + COLOR_DEFAULT + "                          (N,V,Z,C)",
            " ADDS A VALUE TOGETHER WITH THE CARRY BIT TO [A].",
            " THE (C) FLAG IS SET IF THE RESULTING VALUE",
            " IS ABOVE 255 (UNSIGNED).", 
            " THE (V) FLAG IS SET IF ADDING TO A POSITIVE",
            " NUMBER AND ENDING UP WITH A NEGATIVE NUMBER.",
            "",
            " " + COLOR_UL + "SBC: SUBTRACT WITH CARRY" + COLOR_DEFAULT + "                     (N,V,Z,C)",
            " SUBTRACTS A VALUE TOGETHER WITH THE",
            " NOT OF THE CARRY BIT TO [A].",
            " THE (C) FLAG IS CLEAR IF THE RESULTING VALUE",
            " IS LESS THAN 0 (UNSIGNED).", 
            " THE (V) FLAG IS SET IF SUBTRACTING FROM A NEGATIVE",
            " NUMBER AND ENDING UP WITH A POSITIVE NUMBER.",
            "",
            " " + COLOR_UL + "INX: INCREMENT X REGISTER" + COLOR_DEFAULT + "                    (N,Z)",
            " ADDS ONE TO THE VALUE OF THE X REGISTER.",
            "",
            " " + COLOR_UL + "INY: INCREMENT Y REGISTER" + COLOR_DEFAULT + "                    (N,Z)",
            " ADDS ONE TO THE VALUE OF THE Y REGISTER.",
            "",
            " " + COLOR_UL + "DEX: DECREMENT X REGISTER" + COLOR_DEFAULT + "                    (N,Z)",
            " SUBTRACTS ONE FROM THE VALUE OF THE X REGISTER.",
            "",
            " " + COLOR_UL + "DEY: DECREMENT Y REGISTER" + COLOR_DEFAULT + "                    (N,Z)",
            " SUBTRACTS ONE FROM THE VALUE OF THE Y REGISTER.",
            "",
            " " + COLOR_UL + "ASL: ARITHMETIC SHIFT LEFT" + COLOR_DEFAULT + "                   (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE LEFT",
            " INSERTING A 0 IN THE RIGHTMOST BIT",
            " AND MOVING THE LEFTMOST BIT TO THE (C) FLAG.",
            " THIS OPERATION IS EQUIVALENT TO MULTIPLYING BY 2.",
            "",
            " " + COLOR_UL + "LSR: LOGICAL SHIFT RIGHT" + COLOR_DEFAULT + "                     (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE RIGHT",
            " INSERTING A 0 IN THE LEFTMOST BIT",
            " AND MOVING THE RIGHTMOST BIT TO THE (C) FLAG.",
            " THIS OPERATION IS EQUIVALENT TO DIVIDING BY 2.",
            "",
            " " + COLOR_UL + "ROL: ROTATE LEFT" + COLOR_DEFAULT + "                             (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE LEFT",
            " THE LEFTMOST BIT MOVES OVER TO THE RIGHTMOST SIDE.",
            "",
            " " + COLOR_UL + "ROR: ROTATE RIGHT" + COLOR_DEFAULT + "                            (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE RIGHT",
            " THE RIGHTMOST BIT MOVES OVER TO THE LEFTMOST SIDE.",
            "",
            " " + COLOR_UL + "CLC: CLEAR CARRY FLAG" + COLOR_DEFAULT + "                        (N,Z,C)",
            " SETS THE (C) FLAG TO 0.",
            " USUALLY PERFORMED BEFORE ADDITION.",
            "",
            " " + COLOR_UL + "SEC: SET CARRY FLAG" + COLOR_DEFAULT + "                          (N,Z,C)",
            " SETS THE (C) FLAG TO 1.",
            " USUALLY PERFORMED BEFORE SUBTRACTION.",
            "",
            " " + COLOR_UL + "CLV: CLEAR OVERFLOW FLAG" + COLOR_DEFAULT + "                     (N,Z,C)",
            " SETS THE (V) FLAG TO 0.",
            "",
    };



///////////////////////////////////////////////////////////////////////////////
// CORE METHODS:
///////////////////////////////////////////////////////////////////////////////

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

    // Change the app STATE
    private void ChangeState(STATE newState)
    {
        appStatePrev = appState;
        appState = newState;
        ChangeMainSubState(MAIN_SUB_STATE.DEFAULT);
        ChangeInputMode(INPUT_MODE.NORMAL);
        messageIndex = 0;

        // Reset scroll/selection for container
        if (containerScroll.ContainsKey(newState)) { containerScroll[newState].Zero(); }
        else if (containerToggle.ContainsKey(newState)) { containerToggle[newState].Zero(); }
    }

    // Change the MAIN_SUB_STATE
    private void ChangeMainSubState(MAIN_SUB_STATE newSubState)
    {
        mainSubState = newSubState;
    }
    
    // Change the INPUT_MODE
    private void ChangeInputMode(INPUT_MODE newInputMode)
    {
        inputMode = newInputMode;
        if (inputMode == INPUT_MODE.NORMAL) { Console.CursorVisible = false; }
        else { Console.CursorVisible = true; }
        InputClear();
    }

    // Unset flags
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

    // Add message to the message log
    public void AddMessage(string line, bool error = false)
    {
        messages.Add(DateTime.Now.ToString("HH:mm:ss") + " " + (error ? COLOR_FG_DARK_RED + "ERROR:" + COLOR_DEFAULT + " " + line.ToUpper() : line.ToUpper()) );
        messageIndex = 0;
    }

    // Add assembly code to the assembly log
    public void AddAssembly(string line)
    {
        containerScroll[STATE.ASSEMBLY].AddContent(line);
        asmFile.WriteLine(line);
    }

    // Scroll message log up
    public bool MessagesUp()
    {
        if ((messageIndex + MESSAGES_NUM) < messages.Count) { messageIndex++; return true; }
        return false;
    }
    
    // Scroll message log down
    public bool MessagesDown()
    {
        if (messageIndex > 0) { messageIndex--; return true; }
        return false;
    }

    // Format value reference mode
    private void FormatByteType(byte bits, BYTE_TYPE type, out byte newBits, out string asm)
    {
        asm = "";
        newBits = bits;
        if (type == BYTE_TYPE.BINARY) { asm = "#%" + ByteToString(bits); }
        else if (type == BYTE_TYPE.DECIMAL) { asm = "#" + bits.ToString(); }
        else if (type == BYTE_TYPE.HEX) {asm = "#$" + HexToString(bits);}
        else if (type == BYTE_TYPE.ADDRESS) {asm = "$" + HexToString(bits); newBits = memory[bits];}
    }

    // Change active register
    private void ChangeRegister(byte targetRegister)
    {
        registerIndex = targetRegister;
    }
    
    // Get the letter for a given register
    private char GetRegisterChar(byte index)
    {
        return (char)((int)'A' + (index == 0 ? 0 : index + 22));
    }

    // Convert a byte to an eight digit binary string
    private string ByteToString(byte bits)
    {
        return Convert.ToString(bits, 2).PadLeft(8, '0');
    }

    // Convert numerical value to hex string
    public string HexToString(int hex)
    {
        return hex.ToString("X2");
    }



///////////////////////////////////////////////////////////////////////////////
// OPERATIONS:
///////////////////////////////////////////////////////////////////////////////
   
    // Set the (C)arry flag to 0
    private void CLC(bool silent = false)
    {
        carryFlag = false;
        if (!silent) { AddMessage("CLC: CLEAR CARRY FLAG"); }
        AddAssembly("CLC");
    }
    
    // Set the (C)arry flag to 1
    private void SEC(bool silent = false)
    {
        carryFlag = true;
        if (!silent) { AddMessage("SEC: SET CARRY FLAG"); }
        AddAssembly("SEC");
    }

    // Set the o(V)erflow flag to 0
    private void CLV(bool silent = false)
    {
        overflowFlag = false;
        if (!silent) { AddMessage("CLV: CLEAR OVERFLOW FLAG"); }
        AddAssembly("CLV");
    }

    // Logical AND operation
    private void AND(byte bits, BYTE_TYPE type = BYTE_TYPE.BINARY, bool silent = false)
    {
        FormatByteType(bits, type, out bits, out string asm);
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = (byte)(registers[0] & bits);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "AND: Logical AND               (" + ByteToString(bits) + ")"); }
        AddAssembly("AND " + asm); 
    } 

    // Logical inclusive OR operation
    private void ORA(byte bits, BYTE_TYPE type = BYTE_TYPE.BINARY, bool silent = false)
    {
        FormatByteType(bits, type, out bits, out string asm);
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = (byte)(registers[0] | bits);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "ORA: Logical Inclusive OR      (" + ByteToString(bits) + ")"); }
        AddAssembly("ORA " + asm); 
    }

    // Exclusive OR operation
    private void EOR(byte bits, BYTE_TYPE type = BYTE_TYPE.BINARY, bool silent = false)
    {
        FormatByteType(bits, type, out bits, out string asm);
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = (byte)(registers[0] ^ bits);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "EOR: Exclusive OR              (" + ByteToString(bits) + ")"); }
        AddAssembly("EOR " + asm);
    }

    // Load A operation
    private void LDA(byte bits, BYTE_TYPE type = BYTE_TYPE.BINARY, bool silent = false)
    {
        FormatByteType(bits, type, out bits, out string asm);
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "LDA: Load Accumulator          (" + ByteToString(bits) + ")"); }
        AddAssembly("LDA " + asm); 
    }

    // Load X operation
    private void LDX(byte bits, BYTE_TYPE type = BYTE_TYPE.BINARY, bool silent = false)
    {
        FormatByteType(bits, type, out bits, out string asm);
        registersPrev[1] = registers[1];
        UnsetFlags();
        registers[1] = bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(1) + "] " + "LDX: Load X Register           (" + ByteToString(bits) + ")"); }
        AddAssembly("LDX " + asm); 
    }

    // Load Y operation
    private void LDY(byte bits, BYTE_TYPE type = BYTE_TYPE.BINARY, bool silent = false)
    {
        FormatByteType(bits, type, out bits, out string asm);
        registersPrev[2] = registers[2];
        UnsetFlags();
        registers[2] = bits;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(2) + "] " + "LDY: Load Y Register           (" + ByteToString(bits) + ")"); }
        AddAssembly("LDY " + asm); 
    }

    // Store A operation
    private void STA(byte position, bool silent = false)
    {
        memory[position] = registers[0];
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "STA: Store Accumulator         (" + ByteToString(memory[position]) + ")"); }
        AddAssembly("STA $" + HexToString(position)); 
    }

    // Store X operation
    private void STX(byte position, bool silent = false)
    {
        memory[position] = registers[1];
        if (!silent) { AddMessage("[" + GetRegisterChar(1) + "] " + "STX: Store X Register          (" + ByteToString(memory[position]) + ")"); }
        AddAssembly("STX $" + HexToString(position)); 
    }

    // Store Y operation
    private void STY(byte position, bool silent = false)
    {
        memory[position] = registers[2];
        if (!silent) { AddMessage("[" + GetRegisterChar(2) + "] " + "STY: Store Y Register          (" + ByteToString(memory[position]) + ")"); }
        AddAssembly("STY $" + HexToString(position)); 
    }

    // Transfer A to X operation
    private void TAX(bool silent = false)
    {
        registersPrev[1] = registers[1];
        UnsetFlags();
        registers[1] = registers[0];
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "TAX: Transfer A to X           (" + ByteToString(registers[0]) + ")"); }
        AddAssembly("TAX"); 
    }

    // Transfer A to Y operation
    private void TAY(bool silent = false)
    {
        registersPrev[2] = registers[2];
        UnsetFlags();
        registers[2] = registers[0];
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "TAY: Transfer A to Y           (" + ByteToString(registers[0]) + ")"); }
        AddAssembly("TAY"); 
    }

    // Transfer X to A operation
    private void TXA(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = registers[1];
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(1) + "] " + "TXA: Transfer X to A           (" + ByteToString(registers[1]) + ")"); }
        AddAssembly("TXA"); 
    }

    // Transfer Y to A operation
    private void TYA(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        registers[0] = registers[2];
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(2) + "] " + "TYA: Transfer Y to A           (" + ByteToString(registers[2]) + ")"); }
        AddAssembly("TYA"); 
    }

    // Arithmetic shift left operation
    private void ASL(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if ((registers[0] & 0b10000000) == 0b10000000) { carryFlag = true; }
        registers[0] = (byte)(registers[0] << 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "ASL: Arithmetic shift left"); }
        AddAssembly("ASL");
    }

    // Logical shift right operation
    private void LSR(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if ((registers[0] & 0b00000001) == 0b00000001) {carryFlag = true;}
        registers[0] = (byte)(registers[0] >> 1);
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "LSR: Logical shift right"); }
        AddAssembly("LSR");
    }

    // Rotate left operation
    private void ROL(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        byte extraBit = 0b00000000;
        if ((registers[0] & 0b10000000) == 0b10000000) { carryFlag = true; extraBit = 0b00000001; }
        registers[0] = (byte)(registers[0] << 1);
        registers[0] += extraBit;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "ROL: Rotate left"); }
        AddAssembly("ROL");
    }
    
    // Rotate right operation
    private void ROR(bool silent = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        byte extraBit = 0b00000000;
        if ((registers[0] & 0b00000001) == 0b00000001) {carryFlag = true; extraBit = 0b10000000; }
        registers[0] = (byte)(registers[0] >> 1);
        registers[0] += extraBit;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "ROR: Rotate right"); }
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
    
    // Increment Y operation
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
    private void ADC(byte bits, BYTE_TYPE type = BYTE_TYPE.BINARY, bool silent = false, bool inc = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if (settings["flagsAutoCarry"].enabled && carryFlag) { CLC(); }
        if (carryFlag && inc) { bits = 0b00000000; }
        FormatByteType(bits, type, out bits, out string asm);
        byte bitsWithCarry = (byte)(bits + (byte)(carryFlag ? 0b00000001 : 0b00000000));
        if (((registers[0] & 0b10000000) == 0b10000000) && (((registers[0] + bitsWithCarry) & 0b10000000) == 0b00000000)) { carryFlag = true; }
        if (((registers[0] & 0b10000000) == 0b00000000) && (((registers[0] + bitsWithCarry) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[0] += bitsWithCarry;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "ADC: Add with carry            (" + ByteToString(bitsWithCarry) + ")" ); }
        AddAssembly("ADC " + asm);
    }

    // Subtract with carry operation
    private void SBC(byte bits, BYTE_TYPE type = BYTE_TYPE.BINARY, bool silent = false, bool dec = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if (settings["flagsAutoCarry"].enabled && !carryFlag) { SEC(); }
        if (!carryFlag && dec) { bits = 0b00000000; }
        FormatByteType(bits, type, out bits, out string asm);
        byte bitsWithCarry = (byte)(bits + (byte)(carryFlag ? 0b00000000 : 0b00000001));
        if (registers[0] - bitsWithCarry < 0) { carryFlag = false; }
        if (((registers[0] & 0b10000000) == 0b00000000) && (((registers[0] - bitsWithCarry) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[0] -= bitsWithCarry;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "SBC: Subtract with carry       (" + ByteToString(bitsWithCarry) + ")" ); }
        AddAssembly("SBC " + asm);
    }



///////////////////////////////////////////////////////////////////////////////
// RENDER METHODS:
///////////////////////////////////////////////////////////////////////////////

    // Render message log
    public void RenderMessages(int num = MESSAGES_NUM)
    {
        for (int i = num - 1; i >= 0; i--)
        {
            Console.WriteLine(messages.Count > i + messageIndex ? " " + messages[messages.Count-(i+1+messageIndex)] : " -");
        }
    }

    // Main titlebar
    private void RenderTitlebar()
    {
        Console.WriteLine();
        Console.WriteLine(COLOR_FG_BLACK + COLOR_BG_BRIGHT_CYAN + (MAIN_TITLE.PadLeft((RENDER_WIDTH - MAIN_TITLE.Length) / 2 + MAIN_TITLE.Length, ' ')).PadRight(RENDER_WIDTH, ' ') + COLOR_DEFAULT);
    }

    // Render status and keybinds for DEFAULT substate
    private void RenderMainSubStateDefault(string color, string colorEnd)
    {
        Console.WriteLine(" <A> <X> <Y>       CHANGE ACTIVE REGISTER");
        Console.WriteLine(" <UP>              " + ((registerIndex == 0) ? "ADC #%0000000" + ((!settings["flagsAutoCarry"].enabled && carryFlag) ? "0" : "1") + "             (N,V,Z,C)"  : "IN" + GetRegisterChar(registerIndex) + ": INCREMENT " + GetRegisterChar(registerIndex) + " REGISTER  (N,Z)"));
        Console.WriteLine(" <DOWN>            " + ((registerIndex == 0) ? "SBC #%0000000" + ((!settings["flagsAutoCarry"].enabled && !carryFlag) ? "0" : "1") + "             (N,V,Z,C)"  : "IN" + GetRegisterChar(registerIndex) + ": DECREMENT " + GetRegisterChar(registerIndex) + " REGISTER  (N,Z)"));
        Console.WriteLine(((registerIndex == 0 && inputMode == INPUT_MODE.NORMAL) ? "" : COLOR_FG_BRIGHT_BLACK ) + " <LEFT>            ASL: ARITHMETIC SHIFT LEFT (N,Z,C)");
        Console.WriteLine(" <RIGHT>           LSR: LOGICAL SHIFT RIGHT   (N,Z,C)");
        Console.WriteLine(" <SHIFT> + <LEFT>  ROL: ROTATE LEFT           (N,Z,C)");
        Console.WriteLine(" <SHIFT> + <RIGHT> ROR: ROTATE RIGHT          (N,Z,C)");
        Console.WriteLine(" <+>               ADC: ADD WITH CARRY        (N,V,Z,C)");
        Console.WriteLine(" <->               SBC: SUBTRACT WITH CARRY   (N,V,Z,C)");
        Console.WriteLine(" <&>               LOGICAL OPERATIONS         (N,Z)");
        Console.WriteLine(((registerIndex == 0 && inputMode == INPUT_MODE.NORMAL) ? "" : COLOR_DEFAULT) + " <S>               " + (registerIndex == 0 ? "STA: STORE ACCUMULATOR" : "ST" + GetRegisterChar(registerIndex) + ": STORE " + GetRegisterChar(registerIndex) + " REGISTER"));
        Console.WriteLine(" <L>               " + (registerIndex == 0 ? "LDA: LOAD ACCUMULATOR      (N,Z)" : "LD" + GetRegisterChar(registerIndex) + ": LOAD " + GetRegisterChar(registerIndex) + " REGISTER       (N,Z)" ));
        Console.WriteLine(" <T>               " + (registerIndex == 0 ? "TRANSFER ACCUMULATOR       (N,Z)" : "T" + GetRegisterChar(registerIndex) + "A: TRANSFER " + GetRegisterChar(registerIndex) + " TO A       (N,Z)" ));
        Console.WriteLine(" <C>               TOGGLE CARRY FLAG");
        Console.WriteLine(" <V>               CLEAR OVERFLOW FLAG");
        Console.WriteLine(" <M>               ASSEMBLY LOG");
        Console.WriteLine(" <P>               PREFERENCES");
        Console.WriteLine(" <?>               HELP" + colorEnd);
    }
    
    // Render status and keybinds for ADD substate
    private string RenderMainSubStateAdd(string color, string colorEnd)
    {
        Console.WriteLine(color + " <D>               ENTER DECIMAL VALUE        (N,V,Z,C)");
        Console.WriteLine(" <B>               ENTER BINARY VALUE         (N,V,Z,C)");
        Console.WriteLine(" <H>               ENTER HEXADECIMAL VALUE    (N,V,Z,C)");
        Console.WriteLine(" <0>               ADC $00 (" + ByteToString(memory[0]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <1>               ADC $01 (" + ByteToString(memory[1]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <2>               ADC $02 (" + ByteToString(memory[2]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <3>               ADC $03 (" + ByteToString(memory[3]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <4>               ADC $04 (" + ByteToString(memory[4]) + ")         (N,V,Z,C)" + colorEnd);
        return "ADC: ADD WITH CARRY";
    }
    
    // Render status and keybinds for SUBTRACT substate
    private string RenderMainSubStateSubtract(string color, string colorEnd)
    {
        Console.WriteLine(color + " <D>               ENTER DECIMAL VALUE        (N,V,Z,C)");
        Console.WriteLine(" <B>               ENTER BINARY VALUE         (N,V,Z,C)");
        Console.WriteLine(" <H>               ENTER HEXADECIMAL VALUE    (N,V,Z,C)");
        Console.WriteLine(" <0>               SBC $00 (" + ByteToString(memory[0]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <1>               SBC $01 (" + ByteToString(memory[1]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <2>               SBC $02 (" + ByteToString(memory[2]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <3>               SBC $03 (" + ByteToString(memory[3]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <4>               SBC $04 (" + ByteToString(memory[4]) + ")         (N,V,Z,C)" + colorEnd);
        return "SBC: SUBTRACT WITH CARRY";
    }

    // Render status and keybinds for STORE substate
    private string RenderMainSubStateStore()
    {
        Console.WriteLine(" <0>               ST" + GetRegisterChar(registerIndex) + " $00 (" + ByteToString(memory[0]) + ")");
        Console.WriteLine(" <1>               ST" + GetRegisterChar(registerIndex) + " $01 (" + ByteToString(memory[1]) + ")");
        Console.WriteLine(" <2>               ST" + GetRegisterChar(registerIndex) + " $02 (" + ByteToString(memory[2]) + ")");
        Console.WriteLine(" <3>               ST" + GetRegisterChar(registerIndex) + " $03 (" + ByteToString(memory[3]) + ")");
        Console.WriteLine(" <4>               ST" + GetRegisterChar(registerIndex) + " $04 (" + ByteToString(memory[4]) + ")");
        return (registerIndex == 0 ? "STA: STORE ACCUMULATOR" : "ST" + GetRegisterChar(registerIndex) + ": LOAD " + GetRegisterChar(registerIndex) + " REGISTER" );
    }

    // Render status and keybinds for LOAD substate
    private string RenderMainSubStateLoad(string color, string colorEnd)
    {
        Console.WriteLine(color + " <D>               ENTER DECIMAL VALUE        (N,Z)");
        Console.WriteLine(" <B>               ENTER BINARY VALUE         (N,Z)");
        Console.WriteLine(" <H>               ENTER HEXADECIMAL VALUE    (N,Z)");
        Console.WriteLine(" <0>               LD" + GetRegisterChar(registerIndex) + " $00 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <1>               LD" + GetRegisterChar(registerIndex) + " $01 (" + ByteToString(memory[1]) + ")         (N,Z)");
        Console.WriteLine(" <2>               LD" + GetRegisterChar(registerIndex) + " $02 (" + ByteToString(memory[2]) + ")         (N,Z)");
        Console.WriteLine(" <3>               LD" + GetRegisterChar(registerIndex) + " $03 (" + ByteToString(memory[3]) + ")         (N,Z)");
        Console.WriteLine(" <4>               LD" + GetRegisterChar(registerIndex) + " $04 (" + ByteToString(memory[4]) + ")         (N,Z)" + colorEnd);
        return (registerIndex == 0 ? "LDA: LOAD ACCUMULATOR" : "LD" + GetRegisterChar(registerIndex) + ": LOAD " + GetRegisterChar(registerIndex) + " REGISTER" );
    }

    // Render status and keybinds for LOGICAL substate
    private string RenderMainSubStateLogical()
    {
        Console.WriteLine(" <A>               AND: LOGICAL AND                (N,Z)");
        Console.WriteLine(" <E>               EOR: EXCLUSIVE OR               (N,Z)");
        Console.WriteLine(" <O>               ORA: LOGICAL INCLUSIVE OR       (N,Z)");
        return "LOGICAL OPERATIONS";
    }

    // Render status and keybinds for AND substate
    private string RenderMainSubStateAnd(string color, string colorEnd)
    {
        Console.WriteLine(color + " <D>               ENTER DECIMAL VALUE        (N,Z)");
        Console.WriteLine(" <B>               ENTER BINARY VALUE         (N,Z)");
        Console.WriteLine(" <H>               ENTER HEXADECIMAL VALUE    (N,Z)");
        Console.WriteLine(" <0>               AND $00 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <1>               AND $01 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <2>               AND $02 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <3>               AND $03 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <4>               AND $04 (" + ByteToString(memory[0]) + ")         (N,Z)" + colorEnd);
        return "AND: LOGICAL AND";
    }

    // Render status and keybinds for EOR substate
    private string RenderMainSubStateEor(string color, string colorEnd)
    {
        Console.WriteLine(color + " <D>               ENTER DECIMAL VALUE");
        Console.WriteLine(" <B>               ENTER BINARY VALUE");
        Console.WriteLine(" <H>               ENTER HEXADECIMAL VALUE");
        Console.WriteLine(" <0>               EOR $00 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <1>               EOR $01 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <2>               EOR $02 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <3>               EOR $03 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <4>               EOR $04 (" + ByteToString(memory[0]) + ")         (N,Z)" + colorEnd);
        return "EOR: EXCLUSIVE OR";
    }

    // Render status and keybinds for ORA substate
    private string RenderMainSubStateOra(string color, string colorEnd)
    {
        Console.WriteLine(color + " <D>               ENTER DECIMAL VALUE");
        Console.WriteLine(" <B>               ENTER BINARY VALUE");
        Console.WriteLine(" <H>               ENTER HEXADECIMAL VALUE");
        Console.WriteLine(" <0>               ORA $00 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <1>               ORA $01 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <2>               ORA $02 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <3>               ORA $03 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <4>               ORA $04 (" + ByteToString(memory[0]) + ")         (N,Z)" + colorEnd);
        return "ORA: LOGICAL INCLUSIVE OR";
    }

    // Render status and keybinds for TRANSFER substate
    private string RenderMainSubStateTransfer()
    {
        if (registerIndex == 0) 
        {
            Console.WriteLine(" <X>               TAX                    (N,Z)");
            Console.WriteLine(" <Y>               TAY                    (N,Z)");
        }
        else if (registerIndex == 1) 
        {
            Console.WriteLine(" <A>               TXA                    (N,Z)");
            Console.WriteLine();
        }
        else if (registerIndex == 2) 
        {
            Console.WriteLine(" <A>               TYA                    (N,Z)");
            Console.WriteLine();
        }
        return "TRANSFER " + (registerIndex == 0 ? "ACCUMULATOR" : GetRegisterChar(registerIndex) + " REGISTER" );
    }

    // Render status and keybinds for all MAIN substates
    private void RenderMainSubState()
    {
        string footerTitle = "";
        string color = (inputMode != INPUT_MODE.NORMAL ? COLOR_FG_BRIGHT_BLACK : "" );
        string colorEnd = (inputMode != INPUT_MODE.NORMAL ? COLOR_DEFAULT : "" );

        // Check substate and call appropriate render method
        switch (mainSubState)
        {
            case MAIN_SUB_STATE.DEFAULT or MAIN_SUB_STATE.QUIT:
                RenderMainSubStateDefault(color, colorEnd);
                break;
            case MAIN_SUB_STATE.ADD:
                footerTitle = RenderMainSubStateAdd(color, colorEnd);
                break;
            case MAIN_SUB_STATE.SUBTRACT:
                footerTitle = RenderMainSubStateSubtract(color, colorEnd);
                break;
            case MAIN_SUB_STATE.STORE:
                footerTitle = RenderMainSubStateStore();
                break;
            case MAIN_SUB_STATE.LOAD:
                footerTitle = RenderMainSubStateLoad(color, colorEnd);
                break;
            case MAIN_SUB_STATE.TRANSFER:
                footerTitle = RenderMainSubStateTransfer();
                break;
            case MAIN_SUB_STATE.LOGICAL:
                footerTitle = RenderMainSubStateLogical();
                break;
            case MAIN_SUB_STATE.AND:
                footerTitle = RenderMainSubStateAnd(color, colorEnd);
                break;
            case MAIN_SUB_STATE.EOR:
                footerTitle = RenderMainSubStateEor(color, colorEnd);
                break;
            case MAIN_SUB_STATE.ORA:
                footerTitle = RenderMainSubStateOra(color, colorEnd);
                break;
        }

        // Add empty lines to fill RENDER_HEIGHT
        for (int i = Console.CursorTop; i < RENDER_HEIGHT - 3; i++)
        {
            Console.WriteLine();
        }
        
        // Footer
        Console.WriteLine(seperator[0]);
        if (mainSubState == MAIN_SUB_STATE.DEFAULT) { Console.WriteLine(" PRESS <ESC> TO QUIT"); }
        else if (mainSubState == MAIN_SUB_STATE.QUIT) { Console.WriteLine(" " + COLOR_BG_DARK_RED + COLOR_FG_BLACK + " ARE YOU SURE YOU WANT TO QUIT? Y/N " + COLOR_DEFAULT); }
        else { Console.WriteLine(" " + (COLOR_BG_DARK_GREEN + COLOR_FG_BLACK + " " + footerTitle +" " + COLOR_DEFAULT).PadRight(46,' ') + " PRESS <ESC> TO CANCEL"); }

        if (inputMode != INPUT_MODE.NORMAL)
        {
            RenderInputLine();
        }

    }

    // Render the MAIN screen
    private void RenderMain()
    {
        // Main title
        RenderTitlebar();

        // Format the display string
        string displayLine = ByteToString(registers[registerIndex]);
        
        // Vertical spacer
        Console.WriteLine();
       
        // Register selector
        string storageLine = " ";
        string storageLineHex = " ";
        for (byte i = 0; i < REGISTER_NUM; i++)
        {
            if (i == registerIndex) { storageLine += COLOR_FG_BLACK + COLOR_BG_BRIGHT_YELLOW; }
            else { storageLine += COLOR_FG_BLACK + COLOR_BG_BRIGHT_BLACK; }
            storageLine += "  " + GetRegisterChar(i) + "  " + COLOR_DEFAULT + "  ";
            if (i != registerIndex) { storageLineHex += COLOR_FG_DARK_WHITE; }
            storageLineHex += " 0x" + HexToString(registers[i]) + "  ";
            if (i != registerIndex) { storageLineHex += COLOR_DEFAULT; }
        }
        
        // Memory status
        for (byte i = 0; i < MEMORY_NUM; i++)
        {
            storageLine += " $" + (i.ToString()).PadLeft(2,'0') + ":  ";
            storageLineHex += " 0x" + HexToString(memory[i]) + "  ";
        }
        Console.WriteLine(storageLine);
        Console.WriteLine(storageLineHex);

        // Vertical spacer
        Console.WriteLine();
        
        // Binary value titlebar
        Console.WriteLine(COLOR_BG_DARK_WHITE + COLOR_FG_BLACK + (" BINARY VALUE:").PadRight(RENDER_WIDTH,' ') + COLOR_DEFAULT);

        // Vertical spacer
        Console.WriteLine();

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
                display[y] += digits[currentDigit,y,x % CHAR_WIDTH] ? (bitChanged && settings["uiHlChangedBit"].enabled) ? COLOR_FG_BRIGHT_YELLOW + FILLED + COLOR_DEFAULT : FILLED : EMPTY;
            }

            // Render the results of the current row
            Console.WriteLine(display[y]);
        }

        // Vertical spacer
        Console.WriteLine();

        // Decimal values titlebar
        Console.WriteLine(COLOR_BG_DARK_WHITE + COLOR_FG_BLACK + (" DECIMAL VALUE:").PadRight(RENDER_WIDTH,' ') + COLOR_DEFAULT);
        
        // Decimal values
        Console.WriteLine(" UNSIGNED VALUE:     " + (registers[registerIndex].ToString()).PadLeft(3,' ') + "        SIGNED VALUE:      " + (((int)((registers[registerIndex] & 0b01111111) - (registers[registerIndex] & 0b10000000))).ToString()).PadLeft(4,' '));

        // Flags titlebar
        Console.WriteLine(COLOR_BG_DARK_WHITE + COLOR_FG_BLACK + (" FLAGS:").PadRight(RENDER_WIDTH,' ') + COLOR_DEFAULT);
        
        // Flags
        string carryFlagString = " (C) CARRY FLAG:       " + (carryFlag ? COLOR_FG_DARK_GREEN + "1" : COLOR_FG_DARK_RED + "0") + COLOR_DEFAULT;
        string zeroFlagString = "        (Z) ZERO FLAG:        " + (zeroFlag ? COLOR_FG_DARK_GREEN + "1"  : COLOR_FG_DARK_RED + "0") + COLOR_DEFAULT;
        string negativeFlagString = " (N) NEGATIVE FLAG:    " + (negativeFlag ? COLOR_FG_DARK_GREEN + "1" : COLOR_FG_DARK_RED + "0") + COLOR_DEFAULT;
        string overFlowFlagString = "        (V) OVERFLOW FLAG:    " + (overflowFlag ? COLOR_FG_DARK_GREEN + "1" : COLOR_FG_DARK_RED + "0") + COLOR_DEFAULT;
        Console.WriteLine(carryFlagString + zeroFlagString);
        Console.WriteLine(negativeFlagString + overFlowFlagString);
        
        // Message log titlebar
        int messageIndexMax = Math.Max(messages.Count - MESSAGES_NUM, 0);
        int messageIndexNumDigits = Math.Max(messageIndexMax.ToString().Length, 2);
        Console.WriteLine(COLOR_BG_DARK_WHITE + COLOR_FG_BLACK + " MESSAGE LOG:" + ("<SHIFT> + <UP> / <DOWN> (" + messageIndex.ToString().PadLeft(messageIndexNumDigits,'0') + "/" + messageIndexMax.ToString().PadLeft(messageIndexNumDigits,'0') + ") ").PadLeft(43,' ') + COLOR_DEFAULT);
       
        // Render message log
        RenderMessages();
        
        // Keybinds titlebar
        Console.WriteLine(COLOR_BG_DARK_WHITE + COLOR_FG_BLACK + (" KEY:              ACTION:                    FLAGS:").PadRight(RENDER_WIDTH,' ') + COLOR_DEFAULT);
        
        // Render status, available keybinds and footer for the current substate
        RenderMainSubState();
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
            Console.WriteLine(blank);
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



///////////////////////////////////////////////////////////////////////////////
// INPUT HANDLING METHODS:
///////////////////////////////////////////////////////////////////////////////

    // Dynmaic increment
    private void HandleInputIncrement()
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
    
    // Dynamic decrement
    private void HandleInputDecrement()
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

    // Store value to memory from register
    private bool HandleInputStore(byte memoryIndex, bool keyMod)
    {
        if (registerIndex == 0) { STA(memoryIndex); }
        else if (registerIndex == 1) { STX(memoryIndex); }
        else if (registerIndex == 2) { STY(memoryIndex); }
        return true;
    }
    
    // Load value to register from memory
    private bool HandleInputLoad(byte memoryIndex, bool keyMod)
    {
        if (registerIndex == 0) { LDA(memoryIndex, BYTE_TYPE.ADDRESS); }
        else if (registerIndex == 1) { LDX(memoryIndex, BYTE_TYPE.ADDRESS); }
        else if (registerIndex == 2) { LDY(memoryIndex, BYTE_TYPE.ADDRESS); }
        return true;
    }

    // Handle input in MAIN_SUB_STATE.TRANSFER mode
    private bool HandleInputTransfer(byte targetRegisterIndex, bool keyMod)
    {
        if (targetRegisterIndex != registerIndex)
        {
            if (registerIndex == 0) 
            {
                if (targetRegisterIndex == 1) { TAX(); return true; }
                else if (targetRegisterIndex == 2) { TAY(); return true; }
            }
            else if (registerIndex == 1)
            {
                if (targetRegisterIndex == 0) { TXA(); return true; }
            }
            else if (registerIndex == 2)
            {
                if (targetRegisterIndex == 0) { TYA(); return true; }
            }
        }

        // No valid input
        return false;
    }
    
    // Check input in MAIN_SUB_STATE.TRANSFER mode
    private bool GetInputMainTransfer(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.A:
                return HandleInputTransfer(0, keyMod);
            case ConsoleKey.X:
                return HandleInputTransfer(1, keyMod);
            case ConsoleKey.Y:
                return HandleInputTransfer(2, keyMod);
            case ConsoleKey.Escape:
                return true;

        }

        // No valid input
        return false;
    }
    
    // Check input in MAIN_SUB_STATE.STORE mode
    private bool GetInputMainStore(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.D0:
                return HandleInputStore(0, keyMod);
            case ConsoleKey.D1:
                return HandleInputStore(1, keyMod);
            case ConsoleKey.D2:
                return HandleInputStore(2, keyMod);
            case ConsoleKey.D3:
                return HandleInputStore(3, keyMod);
            case ConsoleKey.D4:
                return HandleInputStore(4, keyMod);
            case ConsoleKey.Escape:
                return true;

        }

        // No valid input
        return false;
    }
    
    // Check input in MAIN_SUB_STATE.LOAD mode
    private bool GetInputMainLoad(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.D0:
                return HandleInputLoad(0, keyMod);
            case ConsoleKey.D1:
                return HandleInputLoad(1, keyMod);
            case ConsoleKey.D2:
                return HandleInputLoad(2, keyMod);
            case ConsoleKey.D3:
                return HandleInputLoad(3, keyMod);
            case ConsoleKey.D4:
                return HandleInputLoad(4, keyMod);
            case ConsoleKey.D:
                ChangeInputMode(INPUT_MODE.NUMERICAL_DEC);
                return true;
            case ConsoleKey.B:
                ChangeInputMode(INPUT_MODE.NUMERICAL_BIN);
                return true;
            case ConsoleKey.H:
                ChangeInputMode(INPUT_MODE.NUMERICAL_HEX);
                return true;
            case ConsoleKey.Escape:
                return true;

        }

        // No valid input
        return false;
    }

    // Check input in MAIN_SUB_STATE.QUIT mode
    private bool GetInputMainQuit(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.Y:
                running = false;
                return true;
            case ConsoleKey.N:
                ChangeMainSubState(MAIN_SUB_STATE.DEFAULT);
                return true;
        }

        // No valid input
        return false;
    }

    // Check input in MAIN_SUB_STATE.LOGICAL mode
    private bool GetInputMainLogical(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.A:
                ChangeMainSubState(MAIN_SUB_STATE.AND);
                return true;
            case ConsoleKey.E:
                ChangeMainSubState(MAIN_SUB_STATE.EOR);
                return true;
            case ConsoleKey.O:
                ChangeMainSubState(MAIN_SUB_STATE.ORA);
                return true;
            case ConsoleKey.Escape:
                ChangeMainSubState(MAIN_SUB_STATE.DEFAULT);
                return true;

        }

        // No valid input
        return false;
    }

    // Check input in MAIN_SUB_STATE.AND mode
    private bool GetInputMainAnd(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.D0:
                AND(0, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                AND(1, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                AND(2, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                AND(3, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                AND(4, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D:
                ChangeInputMode(INPUT_MODE.NUMERICAL_DEC);
                return true;
            case ConsoleKey.B:
                ChangeInputMode(INPUT_MODE.NUMERICAL_BIN);
                return true;
            case ConsoleKey.H:
                ChangeInputMode(INPUT_MODE.NUMERICAL_HEX);
                return true;
            case ConsoleKey.Escape:
                ChangeMainSubState(MAIN_SUB_STATE.LOGICAL);
                return true;

        }

        // No valid input
        return false;
    }

    // Check input in MAIN_SUB_STATE.EOR mode
    private bool GetInputMainEor(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.D0:
                EOR(0, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                EOR(1, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                EOR(2, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                EOR(3, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                EOR(4, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D:
                ChangeInputMode(INPUT_MODE.NUMERICAL_DEC);
                return true;
            case ConsoleKey.B:
                ChangeInputMode(INPUT_MODE.NUMERICAL_BIN);
                return true;
            case ConsoleKey.H:
                ChangeInputMode(INPUT_MODE.NUMERICAL_HEX);
                return true;
            case ConsoleKey.Escape:
                ChangeMainSubState(MAIN_SUB_STATE.LOGICAL);
                return true;

        }

        // No valid input
        return false;
    }

    // Check input in MAIN_SUB_STATE.ORA mode
    private bool GetInputMainOra(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.D0:
                ORA(0, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                ORA(1, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                ORA(2, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                ORA(3, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                ORA(4, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D:
                ChangeInputMode(INPUT_MODE.NUMERICAL_DEC);
                return true;
            case ConsoleKey.B:
                ChangeInputMode(INPUT_MODE.NUMERICAL_BIN);
                return true;
            case ConsoleKey.H:
                ChangeInputMode(INPUT_MODE.NUMERICAL_HEX);
                return true;
            case ConsoleKey.Escape:
                ChangeMainSubState(MAIN_SUB_STATE.LOGICAL);
                return true;

        }

        // No valid input
        return false;
    }
    
    // Check input in MAIN_SUB_STATE.ADD mode
    private bool GetInputMainAdd(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.D0:
                ADC(0, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                ADC(1, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                ADC(2, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                ADC(3, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                ADC(4, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D:
                ChangeInputMode(INPUT_MODE.NUMERICAL_DEC);
                return true;
            case ConsoleKey.B:
                ChangeInputMode(INPUT_MODE.NUMERICAL_BIN);
                return true;
            case ConsoleKey.H:
                ChangeInputMode(INPUT_MODE.NUMERICAL_HEX);
                return true;
            case ConsoleKey.Escape:
                return true;

        }

        // No valid input
        return false;
    }
    
    // Check input in MAIN_SUB_STATE.SUBTRACT mode
    private bool GetInputMainSubtract(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.D0:
                SBC(0, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                SBC(1, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                SBC(2, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                SBC(3, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                SBC(4, BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D:
                ChangeInputMode(INPUT_MODE.NUMERICAL_DEC);
                return true;
            case ConsoleKey.B:
                ChangeInputMode(INPUT_MODE.NUMERICAL_BIN);
                return true;
            case ConsoleKey.H:
                ChangeInputMode(INPUT_MODE.NUMERICAL_HEX);
                return true;
            case ConsoleKey.Escape:
                return true;
        }

        // No valid input
        return false;
    }

    // Check input in the MAIN screen, valid in all sub states
    private bool GetInputMainGlobal(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                if(keyMod) { return MessagesUp(); }
                return false;
            case ConsoleKey.DownArrow:
                if(keyMod) { return MessagesDown(); }
                return false;
        }

        // No valid input
        return false;
    }

    // Check input in the MAIN screen
    private bool GetInputMain(ConsoleKeyInfo key, bool keyMod)
    {
        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.S:
                ChangeMainSubState(MAIN_SUB_STATE.STORE);
                return true;
            case ConsoleKey.L:
                ChangeMainSubState(MAIN_SUB_STATE.LOAD);
                return true;
            case ConsoleKey.T:
                if (registerIndex == 1) { TXA(); }
                else if (registerIndex == 2) { TYA(); }
                else { ChangeMainSubState(MAIN_SUB_STATE.TRANSFER); }
                return true;
            case ConsoleKey.Add:
                if (registerIndex == 0) { ChangeMainSubState(MAIN_SUB_STATE.ADD); return true; }
                return false;
            case ConsoleKey.Subtract:
                if (registerIndex == 0) { ChangeMainSubState(MAIN_SUB_STATE.SUBTRACT); return true; }
                return false;
            case ConsoleKey.C:
                if (carryFlag) { CLC(); }
                else { SEC(); }
                return true;
            case ConsoleKey.V:
                if (overflowFlag) { CLV(); return true; }
                return false;
            case ConsoleKey.A:
                if (registerIndex != 0) { ChangeRegister(0); return true; }
                return false;
            case ConsoleKey.X:
                if (registerIndex != 1) { ChangeRegister(1); return true; }
                return false;
            case ConsoleKey.Y:
                if (registerIndex != 2) { ChangeRegister(2); return true; }
                return false;
            case ConsoleKey.LeftArrow:
                if (registerIndex == 0) 
                { 
                    if (keyMod) { ROL(); }
                    else { ASL(); }
                    return true; 
                }
                return false;
            case ConsoleKey.RightArrow:
                if (registerIndex == 0) 
                { 
                    if (keyMod) { ROR(); }
                    else { LSR(); }
                    return true; 
                }
                return false;
            case ConsoleKey.UpArrow:
                if (!keyMod) { HandleInputIncrement(); return true; }
                return false;
            case ConsoleKey.DownArrow:
                if (!keyMod) { HandleInputDecrement(); return true; }
                return false;
            case ConsoleKey.M:
                ChangeState(STATE.ASSEMBLY);
                return true;
            case ConsoleKey.P:
                ChangeState(STATE.SETTINGS);
                return true;
            case ConsoleKey.Escape:
                ChangeMainSubState(MAIN_SUB_STATE.QUIT);
                return true;
        }
        switch (key.KeyChar)
        {
            case '?':
                ChangeState(STATE.HELP);
                return true;
            case '&':
                if (registerIndex == 0) { ChangeMainSubState(MAIN_SUB_STATE.LOGICAL); return true; }
                return false;
        }

        // No valid input
        return false;
    }

    // Handle input in the HELP screen
    private bool GetInputHelp(ConsoleKeyInfo key, bool keyMod)
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
    private bool GetInputSettings(ConsoleKeyInfo key, bool keyMod)
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
    private bool GetInputAssembly(ConsoleKeyInfo key, bool keyMod)
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
    private bool GetInputError(ConsoleKeyInfo key, bool keyMod)
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
   
    // Clear the input line
    private void InputClear()
    {
        inputString = "";
    }

    // Add a character to the input line
    private bool InputAddChar(char newChar)
    {
        if (FormatInputLine().Length < INPUT_CHAR_LIMIT) { inputString += newChar; return true;}
        return false;
    }

    // Erase the last character in the input line, or exit input mode if there are no characters
    private void InputEraseLastChar()
    {
        if (inputString.Length > 0) { inputString = inputString.Remove(inputString.Length - 1); }
        else { ChangeInputMode(INPUT_MODE.NORMAL); }
    }

    // Check the number from the user input
    private bool InputCheckValueNumerical(BYTE_TYPE byteType, out byte bits)
    {
        bits = 0;
        int inputValue = 0;

        switch (byteType)
        {
            case BYTE_TYPE.DECIMAL:
                if (inputString.Length > 8) { return false; }
                inputValue = Convert.ToInt32(inputString);
                break;
            case BYTE_TYPE.BINARY:
                if (inputString.Length != 8) { return false; }
                inputValue = Convert.ToInt32(inputString, 2);
                break;
            case BYTE_TYPE.HEX:
                if (inputString.Length > 2) { return false; }
                inputValue = Convert.ToInt32(inputString.ToLower(), 16);
                break;
        }

        // If the number is under 256 it is valid
        if (inputValue < 256) { bits = (byte)inputValue; return true; }

        // Number is not valid
        return false;
    }

    // Run the command from the NUMERICAL input
    private bool InputRunNumerical()
    {
        // Check that there is at least one character in the input
        if (inputString.Length > 0)
        {
            BYTE_TYPE byteType = BYTE_TYPE.BINARY;

            // Format bits and find byte type according to current INPUT_MODE
            switch (inputMode)
            {
                case INPUT_MODE.NUMERICAL_DEC:
                    byteType = BYTE_TYPE.DECIMAL;
                    break;
                case INPUT_MODE.NUMERICAL_BIN:
                    byteType = BYTE_TYPE.BINARY;
                    break;
                case INPUT_MODE.NUMERICAL_HEX:
                    byteType = BYTE_TYPE.HEX;
                    break;
            }

            // Check that the number from the user is a valid number (0-255)
            if (InputCheckValueNumerical(byteType, out byte bits))
            {
                // Perform correct operation according to current STATE
                switch (appState)
                {
                    case STATE.MAIN:
                        switch (mainSubState)
                        {
                            case MAIN_SUB_STATE.AND:
                                AND(bits, byteType);
                                return true;
                            case MAIN_SUB_STATE.EOR:
                                EOR(bits, byteType);
                                return true;
                            case MAIN_SUB_STATE.LOAD:
                                if (registerIndex == 0) { LDA(bits, byteType); }
                                else if (registerIndex == 1) { LDX(bits, byteType); }
                                else if (registerIndex == 2) { LDY(bits, byteType); }
                                return true;
                            case MAIN_SUB_STATE.ORA:
                                ORA(bits, byteType);
                                return true;
                            case MAIN_SUB_STATE.ADD:
                                ADC(bits, byteType);
                                return true;
                            case MAIN_SUB_STATE.SUBTRACT:
                                SBC(bits, byteType);
                                return true;
                        }
                        break;
                }
            }

            // Invalid number
            else
            {
                AddMessage("Invalid number! " + "\"" + FormatInputLine() + "\"", error: true);
            }
        }

        // Input was not valid
        return false;
    }

    // Render the bottom input line if in COMMAND or NUMERICAL input mode
    private string FormatInputLine()
    {
        string inputLineFormatted = "";
        switch (appState)
        {
            case STATE.MAIN:
                switch (mainSubState)
                {
                    case MAIN_SUB_STATE.ADD:
                        inputLineFormatted += "ADC ";
                        break;
                    case MAIN_SUB_STATE.AND:
                        inputLineFormatted += "AND ";
                        break;
                    case MAIN_SUB_STATE.EOR:
                        inputLineFormatted += "EOR ";
                        break;
                    case MAIN_SUB_STATE.LOAD:
                        inputLineFormatted += "LD" + GetRegisterChar(registerIndex) + " ";
                        break;
                    case MAIN_SUB_STATE.ORA:
                        inputLineFormatted += "ORA ";
                        break;
                    case MAIN_SUB_STATE.STORE:
                        inputLineFormatted += "ST" + GetRegisterChar(registerIndex) + " ";
                        break;
                    case MAIN_SUB_STATE.SUBTRACT:
                        inputLineFormatted += "SBC ";
                        break;
                }
                break;
        }

        switch (inputMode)
        {
            case INPUT_MODE.NUMERICAL_DEC:
                inputLineFormatted += "#";
                break;
            case INPUT_MODE.NUMERICAL_BIN:
                inputLineFormatted += "#%";
                break;
            case INPUT_MODE.NUMERICAL_HEX:
                inputLineFormatted += "#$";
                break;
        }

        return inputLineFormatted + inputString;
    }

    // Render the bottom input line if in COMMAND or NUMERICAL input mode
    private void RenderInputLine()
    {
        string inputLineFormatted = " :" + FormatInputLine();
        Console.WriteLine(inputLineFormatted);
        Console.SetCursorPosition(inputLineFormatted.Length,RENDER_HEIGHT-1);
    }

    // Handle input in the NUMERICAL input modes
    private bool GetInputModeNumerical(ConsoleKeyInfo key, bool keyMod)
    {
        // Allow A - F if in HEX input mode
        if (inputMode == INPUT_MODE.NUMERICAL_HEX)
        {
            switch (key.Key)
            {
                case ConsoleKey.A:
                    return InputAddChar('A');
                case ConsoleKey.B:
                    return InputAddChar('B');
                case ConsoleKey.C:
                    return InputAddChar('C');
                case ConsoleKey.D:
                    return InputAddChar('D');
                case ConsoleKey.E:
                    return InputAddChar('E');
                case ConsoleKey.F:
                    return InputAddChar('F');
            }
        }
        
        // Allow 2-9 if in DEC or HEX input mode
        if (inputMode == INPUT_MODE.NUMERICAL_DEC || inputMode == INPUT_MODE.NUMERICAL_HEX)
        {
            switch (key.Key)
            {
                case ConsoleKey.D2:
                    return InputAddChar('2');
                case ConsoleKey.D3:
                    return InputAddChar('3');
                case ConsoleKey.D4:
                    return InputAddChar('4');
                case ConsoleKey.D5:
                    return InputAddChar('5');
                case ConsoleKey.D6:
                    return InputAddChar('6');
                case ConsoleKey.D7:
                    return InputAddChar('7');
            }
        }

        // Check if the pressed key is a valid key
        switch (key.Key)
        {
            case ConsoleKey.D0:
                return InputAddChar('0');
            case ConsoleKey.D1:
                return InputAddChar('1');
            case ConsoleKey.Backspace:
                InputEraseLastChar();
                return true;
            case ConsoleKey.Enter:
                if (InputRunNumerical()) { ChangeState(STATE.MAIN); }
                ChangeInputMode(INPUT_MODE.NORMAL);
                return true;
            case ConsoleKey.Escape:
                ChangeInputMode(INPUT_MODE.NORMAL);
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

            // Check if key modifier was pressed
            bool keyMod = key.Modifiers.HasFlag(ConsoleModifiers.Shift);

            // Call the correct GetInput for the current INPUT_MODE
            switch (inputMode)
            {
                case INPUT_MODE.NUMERICAL_DEC or INPUT_MODE.NUMERICAL_BIN or INPUT_MODE.NUMERICAL_HEX:
                    validKey = GetInputModeNumerical(key, keyMod);
                    break;
                case INPUT_MODE.NORMAL:
                    // Call the correct GetInput function for the current application STATE
                    switch (appState)
                    {
                        case STATE.MAIN:
                            if (validKey = GetInputMainGlobal(key, keyMod)) { break; }
                            switch (mainSubState)
                            {
                                case MAIN_SUB_STATE.QUIT:
                                    validKey = GetInputMainQuit(key, keyMod);
                                    break;
                                case MAIN_SUB_STATE.DEFAULT:
                                    validKey = GetInputMain(key, keyMod);
                                    break;
                                case MAIN_SUB_STATE.ADD:
                                    validKey = GetInputMainAdd(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case MAIN_SUB_STATE.SUBTRACT:
                                    validKey = GetInputMainSubtract(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case MAIN_SUB_STATE.TRANSFER:
                                    if (validKey = GetInputMainTransfer(key, keyMod)) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case MAIN_SUB_STATE.STORE:
                                    if (validKey = GetInputMainStore(key, keyMod)) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case MAIN_SUB_STATE.LOAD:
                                    validKey = GetInputMainLoad(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case MAIN_SUB_STATE.LOGICAL:
                                    validKey = GetInputMainLogical(key, keyMod);
                                    break;
                                case MAIN_SUB_STATE.AND:
                                    validKey = GetInputMainAnd(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case MAIN_SUB_STATE.EOR:
                                    validKey = GetInputMainEor(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case MAIN_SUB_STATE.ORA:
                                    validKey = GetInputMainOra(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                                    break;
                            }
                            break;
                        case STATE.HELP:
                            validKey = GetInputHelp(key, keyMod);
                            break;
                        case STATE.SETTINGS:
                            validKey = GetInputSettings(key, keyMod);
                            break;
                        case STATE.ASSEMBLY:
                            validKey = GetInputAssembly(key, keyMod);
                            break;
                        case STATE.ERROR:
                            validKey = GetInputError(key, keyMod);
                            break;
                    }
                    break;
            }
        }
    }
    


///////////////////////////////////////////////////////////////////////////////
// CONSTRUCTOR AND MAIN METHODS:
///////////////////////////////////////////////////////////////////////////////

    // Initialize values
    private void Init() 
    {
        // Zero digit
        digits[0,1,0] = true;
        digits[0,2,0] = true;
        digits[0,3,0] = true;
        digits[0,4,0] = true;
        digits[0,5,0] = true;
        digits[0,0,1] = true;
        digits[0,6,1] = true;
        digits[0,0,2] = true;
        digits[0,6,2] = true;
        digits[0,0,3] = true;
        digits[0,6,3] = true;
        digits[0,1,4] = true;
        digits[0,2,4] = true;
        digits[0,3,4] = true;
        digits[0,4,4] = true;
        digits[0,5,4] = true;

        // One digit
        digits[1,0,2] = true;
        digits[1,1,1] = true;
        digits[1,1,2] = true;
        digits[1,2,2] = true;
        digits[1,3,2] = true;
        digits[1,4,2] = true;
        digits[1,5,2] = true;
        digits[1,6,1] = true;
        digits[1,6,2] = true;
        digits[1,6,3] = true;

        // Link lists with containers
        containerScroll[STATE.HELP].LinkContent(help);
        containerScroll[STATE.ASSEMBLY].LinkContent(assembly);
        containerToggle[STATE.SETTINGS].LinkContent(settings);

        AddMessage("Welcome!");
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
        Console.WriteLine(COLOR_DEFAULT);
        Console.CursorVisible = true;
        Console.Clear();
        asmFile.Close();
    }
   
    // Class constructor
    public BinaryCalc()
    {
        Init();
        Run();
    }
}

