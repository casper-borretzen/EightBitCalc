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

    private enum ByteType 
    {
        Binary,
        Decimal,
        Hex,
        Address
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
    private const int RENDER_HEIGHT = 53;
    private const int REGISTER_NUM = 3;
    private const int MEMORY_NUM = 8;
    private const int MESSAGES_NUM = 8;

    // Set global variables
    private bool running = true;
    private STATE appState = STATE.MAIN;
    private STATE appStatePrev = STATE.MAIN;
    private MAIN_SUB_STATE mainSubState = MAIN_SUB_STATE.DEFAULT;
    private string[] display = new string[CHAR_HEIGHT];
    private byte[] registers = new byte[REGISTER_NUM];
    private byte[] registersPrev = new byte[REGISTER_NUM];
    private byte[] memory = new byte[MEMORY_NUM];
    private byte registerIndex = 0;
    private bool carryFlag = false;
    private bool zeroFlag = false;
    private bool negativeFlag = false;
    private bool overflowFlag = false;
    private int windowWidth = 0;
    private int windowHeight = 0;
    private int messageIndex = 0;

    // Declare logger
    public List<string> messages = new List<string>();
    public List<string> assembly = new List<string>();

    // Settings
    private Dictionary<string, Setting> settings = new Dictionary<string, Setting>
    {
        { "uiHlChangedBit", new Setting(true,  "HIGHLIGHT BITS CHANGED IN PREVIOUS OPERATION") },
        { "flagsAutoCarry", new Setting(false, "AUTOMATICALLY SET/UNSET CARRY FLAG") },
        { "loggerAsmFile",  new Setting(false,  "(NOT IMPLEMENTED!) SAVE ASM FILE") }
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
        { STATE.SETTINGS, new ContainerToggle("PREFERENCES", RENDER_HEIGHT - 7) }
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
        mainSubState = MAIN_SUB_STATE.DEFAULT;

        // Reset sel position for container
        if (containerScroll.ContainsKey(newState)) { containerScroll[newState].Zero(); }
        else if (containerToggle.ContainsKey(newState)) { containerToggle[newState].Zero(); }
    }

    private void ChangeMainSubState(MAIN_SUB_STATE newSubState)
    {
        mainSubState = newSubState;
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
        messageIndex = 0;
    }

    // Add assembly code to the assembly log
    public void AddAssembly(string line)
    {
        containerScroll[STATE.ASSEMBLY].AddContent(line);
    }

    // Render message log
    public void RenderMessages(int num = MESSAGES_NUM)
    {
        for (int i = num - 1; i >= 0; i--)
        {
            Console.WriteLine(messages.Count > i + messageIndex ? " " + messages[messages.Count-(i+1+messageIndex)] : " -");
        }
    }

    public bool MessagesUp()
    {
        if ((messageIndex + MESSAGES_NUM) < messages.Count) { messageIndex++; return true; }
        return false;
    }
    
    public bool MessagesDown()
    {
        if (messageIndex > 0) { messageIndex--; return true; }
        return false;
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

    // Convert numerical value to hex value
    public string HexToString(int hex)
    {
        return hex.ToString("X2");
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

    // Change active register
    private void ChangeRegister(byte targetRegister)
    {
        registerIndex = targetRegister;
    }

    // Set the o(V)erflow flag to 0
    private void CLV(bool silent = false)
    {
        overflowFlag = false;
        if (!silent) { AddMessage("CLV: CLEAR OVERFLOW FLAG"); }
        AddAssembly("CLV");
    }

    // Format value reference mode
    private void FormatByteType(byte bits, ByteType type, out byte newBits, out string asm)
    {
        asm = "";
        newBits = bits;
        if (type == ByteType.Binary) { asm = "#%" + ByteToString(bits); }
        else if (type == ByteType.Decimal) { asm = "#" + bits.ToString(); }
        else if (type == ByteType.Hex) {asm = "#$" + HexToString(bits);}
        else if (type == ByteType.Address) {asm = "$" + HexToString(bits); newBits = memory[bits];}
    }

    // Logical AND operation
    private void AND(byte bits, ByteType type = ByteType.Binary, bool silent = false)
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
    private void ORA(byte bits, ByteType type = ByteType.Binary, bool silent = false)
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
    private void EOR(byte bits, ByteType type = ByteType.Binary, bool silent = false)
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
    private void LDA(byte bits, ByteType type = ByteType.Binary, bool silent = false)
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
    private void LDX(byte bits, ByteType type = ByteType.Binary, bool silent = false)
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
    private void LDY(byte bits, ByteType type = ByteType.Binary, bool silent = false)
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
        if ((registers[0] & 0b10000000) == 0b10000000) { carryFlag = true; }
        registers[0] = (byte)(registers[0] << 1);
        if (carryFlag) { registers[0] += 0b00000001; }
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
        if ((registers[0] & 0b00000001) == 0b00000001) {carryFlag = true;}
        registers[0] = (byte)(registers[0] >> 1);
        if (carryFlag) { registers[0] += 0b10000000; }
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
    private void ADC(byte bits, ByteType type = ByteType.Binary, bool silent = false, bool inc = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if (settings["flagsAutoCarry"].enabled && carryFlag) { CLC(); }
        if (carryFlag && inc) { bits = 0b00000000; }
        FormatByteType(bits, type, out bits, out string asm);
        byte bitsWithCarry = (byte)(bits + (byte)(carryFlag ? 0b00000001 : 0b00000000));
        if (((registers[0] & 0b10000000) == 0b10000000) && (((registers[0] + bits) & 0b10000000) == 0b00000000)) { carryFlag = true; }
        if (((registers[0] & 0b10000000) == 0b00000000) && (((registers[0] + bits) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[0] += bitsWithCarry;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "ADC: Add with carry            (" + ByteToString(bitsWithCarry) + ")" ); }
        AddAssembly("ADC " + asm);
    }

    // Subtract with carry operation
    private void SBC(byte bits, ByteType type = ByteType.Binary, bool silent = false, bool dec = false)
    {
        registersPrev[0] = registers[0];
        UnsetFlags();
        if (settings["flagsAutoCarry"].enabled && !carryFlag) { SEC(); }
        if (!carryFlag && dec) { bits = 0b00000000; }
        FormatByteType(bits, type, out bits, out string asm);
        byte bitsWithCarry = (byte)(bits + (byte)(carryFlag ? 0b00000000 : 0b00000001));
        if (registers[0] - bits < 0) { carryFlag = false; }
        if (((registers[0] & 0b10000000) == 0b00000000) && (((registers[0] - bits) & 0b10000000) == 0b10000000)) { overflowFlag = true; }
        registers[0] -= bitsWithCarry;
        SetZeroFlag();
        SetNegativeFlag();
        if (!silent) { AddMessage("[" + GetRegisterChar(0) + "] " + "SBC: Subtract with carry       (" + ByteToString(bitsWithCarry) + ")" ); }
        AddAssembly("SBC " + asm);
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
        if (registerIndex == 0) { LDA(memoryIndex, ByteType.Address); }
        else if (registerIndex == 1) { LDX(memoryIndex, ByteType.Address); }
        else if (registerIndex == 2) { LDY(memoryIndex, ByteType.Address); }
        return true;
    }

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
        return false;
    }
    
    private bool GetInputMainTransfer(ConsoleKeyInfo key, bool keyMod)
    {
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
        return false;
    }
    
    private bool GetInputMainStore(ConsoleKeyInfo key, bool keyMod)
    {
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
            case ConsoleKey.D5:
                return HandleInputStore(5, keyMod);
            case ConsoleKey.D6:
                return HandleInputStore(6, keyMod);
            case ConsoleKey.D7:
                return HandleInputStore(7, keyMod);
            case ConsoleKey.Escape:
                return true;

        }
        return false;
    }
    
    private bool GetInputMainLoad(ConsoleKeyInfo key, bool keyMod)
    {
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
            case ConsoleKey.D5:
                return HandleInputLoad(5, keyMod);
            case ConsoleKey.D6:
                return HandleInputLoad(6, keyMod);
            case ConsoleKey.D7:
                return HandleInputLoad(7, keyMod);
            case ConsoleKey.Escape:
                return true;

        }
        return false;
    }

    private bool GetInputMainQuit(ConsoleKeyInfo key, bool keyMod)
    {
        switch (key.Key)
        {
            case ConsoleKey.Y:
                running = false;
                return true;
            case ConsoleKey.N:
                ChangeMainSubState(MAIN_SUB_STATE.DEFAULT);
                return true;
        }
        return false;
    }

    private bool GetInputMainLogical(ConsoleKeyInfo key, bool keyMod)
    {
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
        return false;
    }

    private bool GetInputMainAnd(ConsoleKeyInfo key, bool keyMod)
    {
        switch (key.Key)
        {
            case ConsoleKey.D0:
                AND(0, ByteType.Address);
                return true;
            case ConsoleKey.D1:
                AND(1, ByteType.Address);
                return true;
            case ConsoleKey.D2:
                AND(2, ByteType.Address);
                return true;
            case ConsoleKey.D3:
                AND(3, ByteType.Address);
                return true;
            case ConsoleKey.D4:
                AND(4, ByteType.Address);
                return true;
            case ConsoleKey.D5:
                AND(5, ByteType.Address);
                return true;
            case ConsoleKey.D6:
                AND(6, ByteType.Address);
                return true;
            case ConsoleKey.D7:
                AND(7, ByteType.Address);
                return true;
            case ConsoleKey.Escape:
                ChangeMainSubState(MAIN_SUB_STATE.LOGICAL);
                return true;

        }
        return false;
    }

    private bool GetInputMainEor(ConsoleKeyInfo key, bool keyMod)
    {
        switch (key.Key)
        {
            case ConsoleKey.D0:
                EOR(0, ByteType.Address);
                return true;
            case ConsoleKey.D1:
                EOR(1, ByteType.Address);
                return true;
            case ConsoleKey.D2:
                EOR(2, ByteType.Address);
                return true;
            case ConsoleKey.D3:
                EOR(3, ByteType.Address);
                return true;
            case ConsoleKey.D4:
                EOR(4, ByteType.Address);
                return true;
            case ConsoleKey.D5:
                EOR(5, ByteType.Address);
                return true;
            case ConsoleKey.D6:
                EOR(6, ByteType.Address);
                return true;
            case ConsoleKey.D7:
                EOR(7, ByteType.Address);
                return true;
            case ConsoleKey.Escape:
                ChangeMainSubState(MAIN_SUB_STATE.LOGICAL);
                return true;

        }
        return false;
    }

    private bool GetInputMainOra(ConsoleKeyInfo key, bool keyMod)
    {
        switch (key.Key)
        {
            case ConsoleKey.D0:
                ORA(0, ByteType.Address);
                return true;
            case ConsoleKey.D1:
                ORA(1, ByteType.Address);
                return true;
            case ConsoleKey.D2:
                ORA(2, ByteType.Address);
                return true;
            case ConsoleKey.D3:
                ORA(3, ByteType.Address);
                return true;
            case ConsoleKey.D4:
                ORA(4, ByteType.Address);
                return true;
            case ConsoleKey.D5:
                ORA(5, ByteType.Address);
                return true;
            case ConsoleKey.D6:
                ORA(6, ByteType.Address);
                return true;
            case ConsoleKey.D7:
                ORA(7, ByteType.Address);
                return true;
            case ConsoleKey.Escape:
                ChangeMainSubState(MAIN_SUB_STATE.LOGICAL);
                return true;

        }
        return false;
    }
    
    private bool GetInputMainAdd(ConsoleKeyInfo key, bool keyMod)
    {
        switch (key.Key)
        {
            case ConsoleKey.D0:
                ADC(0, ByteType.Address);
                return true;
            case ConsoleKey.D1:
                ADC(1, ByteType.Address);
                return true;
            case ConsoleKey.D2:
                ADC(2, ByteType.Address);
                return true;
            case ConsoleKey.D3:
                ADC(3, ByteType.Address);
                return true;
            case ConsoleKey.D4:
                ADC(4, ByteType.Address);
                return true;
            case ConsoleKey.D5:
                ADC(5, ByteType.Address);
                return true;
            case ConsoleKey.D6:
                ADC(6, ByteType.Address);
                return true;
            case ConsoleKey.D7:
                ADC(7, ByteType.Address);
                return true;
            case ConsoleKey.Escape:
                return true;

        }
        return false;
    }
    
    private bool GetInputMainSubtract(ConsoleKeyInfo key, bool keyMod)
    {
        switch (key.Key)
        {
            case ConsoleKey.D0:
                SBC(0, ByteType.Address);
                return true;
            case ConsoleKey.D1:
                SBC(1, ByteType.Address);
                return true;
            case ConsoleKey.D2:
                SBC(2, ByteType.Address);
                return true;
            case ConsoleKey.D3:
                SBC(3, ByteType.Address);
                return true;
            case ConsoleKey.D4:
                SBC(4, ByteType.Address);
                return true;
            case ConsoleKey.D5:
                SBC(5, ByteType.Address);
                return true;
            case ConsoleKey.D6:
                SBC(6, ByteType.Address);
                return true;
            case ConsoleKey.D7:
                SBC(7, ByteType.Address);
                return true;
            case ConsoleKey.Escape:
                return true;
        }
        return false;
    }

    // Handle input in the MAIN screen
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
                ChangeMainSubState(MAIN_SUB_STATE.TRANSFER);
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
                if (registerIndex == 0) { ASL(); return true; }
                return false;
            case ConsoleKey.RightArrow:
                if (registerIndex == 0) { LSR(); return true; }
                return false;
            case ConsoleKey.UpArrow:
                if (!keyMod) { Increment(); return true; }
                return false;
            case ConsoleKey.DownArrow:
                if (!keyMod) { Decrement(); return true; }
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

    // Get input from user
    private void GetInput() 
    {
        // Loop until valid input is given
        bool validKey = false;
        while (!validKey)
        {
            // Wait for and get key input from user
            ConsoleKeyInfo key = Console.ReadKey(true);

            // Check if key modifiers was pressed
            bool keyMod = key.Modifiers.HasFlag(ConsoleModifiers.Shift);

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
                            if (validKey = GetInputMainAdd(key, keyMod)) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                            break;
                        case MAIN_SUB_STATE.SUBTRACT:
                            if (validKey = GetInputMainSubtract(key, keyMod)) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                            break;
                        case MAIN_SUB_STATE.TRANSFER:
                            if (validKey = GetInputMainTransfer(key, keyMod)) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                            break;
                        case MAIN_SUB_STATE.STORE:
                            if (validKey = GetInputMainStore(key, keyMod)) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                            break;
                        case MAIN_SUB_STATE.LOAD:
                            if (validKey = GetInputMainLoad(key, keyMod)) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                            break;
                        case MAIN_SUB_STATE.LOGICAL:
                            validKey = GetInputMainLogical(key, keyMod);
                            break;
                        case MAIN_SUB_STATE.AND:
                            validKey = GetInputMainAnd(key, keyMod);
                            if (validKey && key.Key != ConsoleKey.Escape) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                            break;
                        case MAIN_SUB_STATE.EOR:
                            validKey = GetInputMainEor(key, keyMod);
                            if (validKey && key.Key != ConsoleKey.Escape) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
                            break;
                        case MAIN_SUB_STATE.ORA:
                            validKey = GetInputMainOra(key, keyMod);
                            if (validKey && key.Key != ConsoleKey.Escape) { ChangeMainSubState(MAIN_SUB_STATE.DEFAULT); }
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
        }
    }
    
    // Main titlebar
    private void RenderTitlebar()
    {
        Console.WriteLine();
        Console.WriteLine(COLORS.FG_BLACK + COLORS.BG_BRIGHT_CYAN + (MAIN_TITLE.PadLeft((RENDER_WIDTH - MAIN_TITLE.Length) / 2 + MAIN_TITLE.Length, ' ')).PadRight(RENDER_WIDTH, ' ') + COLORS.DEFAULT);
    }

    // Render status and keybinds for DEFAULT substate
    private void RenderMainSubStateDefault()
    {
        Console.WriteLine(" <A> <X> <Y>       CHANGE ACTIVE REGISTER");
        Console.WriteLine(" <UP>              INCREMENT                  (N,Z)");
        Console.WriteLine(" <DOWN>            DECREMENT                  (N,Z)");
        Console.WriteLine((registerIndex == 0 ? "" : COLORS.FG_BRIGHT_BLACK ) + " <LEFT>            ARITHMETIC SHIFT LEFT      (N,Z,C)");
        Console.WriteLine(" <RIGHT>           LOGICAL SHIFT RIGHT        (N,Z,C)");
        //Console.WriteLine(" <A+LEFT>          ROTATE LEFT                (N,Z,C)");
        //Console.WriteLine(" <A+RIGHT>         ROTATE RIGHT               (N,Z,C)");
        Console.WriteLine(" <+>               ADD VALUE FROM MEMORY      (N,V,Z,C)");
        Console.WriteLine(" <->               SUBTRACT VALUE FROM MEMORY (N,V,Z,C)");
        Console.WriteLine(" <&>               LOGICAL OPERATIONS         (N,Z)");
        Console.WriteLine((registerIndex == 0 ? "" : COLORS.DEFAULT ) + " <S>               STORE VALUE IN MEMORY");
        Console.WriteLine(" <L>               LOAD VALUE FROM MEMORY     (N,Z)");
        Console.WriteLine(" <T>               TRANSFER VALUE ");
        Console.WriteLine(" <C>               TOGGLE CARRY FLAG");
        Console.WriteLine(" <V>               CLEAR OVERFLOW FLAG");
        Console.WriteLine(" <M>               ASSEMBLY LOG");
        Console.WriteLine(" <P>               PREFERENCES");
        //Console.WriteLine(" <:>               COMMAND MODE (NOT IMPLEMENTED)");
        Console.WriteLine(" <?>               HELP");
    }
    
    // Render status and keybinds for ADD substate
    private string RenderMainSubStateAdd()
    {
        Console.WriteLine(" <0>               ADC $00 (" + ByteToString(memory[0]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <1>               ADC $01 (" + ByteToString(memory[1]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <2>               ADC $02 (" + ByteToString(memory[2]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <3>               ADC $03 (" + ByteToString(memory[3]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <4>               ADC $04 (" + ByteToString(memory[4]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <5>               ADC $05 (" + ByteToString(memory[5]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <6>               ADC $06 (" + ByteToString(memory[6]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <7>               ADC $07 (" + ByteToString(memory[7]) + ")         (N,V,Z,C)");
        return "ADC: ADD WITH CARRY";
    }
    
    // Render status and keybinds for SUBTRACT substate
    private string RenderMainSubStateSubtract()
    {
        Console.WriteLine(" <0>               SBC $00 (" + ByteToString(memory[0]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <1>               SBC $01 (" + ByteToString(memory[1]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <2>               SBC $02 (" + ByteToString(memory[2]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <3>               SBC $03 (" + ByteToString(memory[3]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <4>               SBC $04 (" + ByteToString(memory[4]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <5>               SBC $05 (" + ByteToString(memory[5]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <6>               SBC $06 (" + ByteToString(memory[6]) + ")         (N,V,Z,C)");
        Console.WriteLine(" <7>               SBC $07 (" + ByteToString(memory[7]) + ")         (N,V,Z,C)");
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
        Console.WriteLine(" <5>               ST" + GetRegisterChar(registerIndex) + " $05 (" + ByteToString(memory[5]) + ")");
        Console.WriteLine(" <6>               ST" + GetRegisterChar(registerIndex) + " $06 (" + ByteToString(memory[6]) + ")");
        Console.WriteLine(" <7>               ST" + GetRegisterChar(registerIndex) + " $07 (" + ByteToString(memory[7]) + ")");
        return "STORE VALUE IN MEMORY";
    }

    // Render status and keybinds for LOAD substate
    private string RenderMainSubStateLoad()
    {
        Console.WriteLine(" <0>               LD" + GetRegisterChar(registerIndex) + " $00 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <1>               LD" + GetRegisterChar(registerIndex) + " $01 (" + ByteToString(memory[1]) + ")         (N,Z)");
        Console.WriteLine(" <2>               LD" + GetRegisterChar(registerIndex) + " $02 (" + ByteToString(memory[2]) + ")         (N,Z)");
        Console.WriteLine(" <3>               LD" + GetRegisterChar(registerIndex) + " $03 (" + ByteToString(memory[3]) + ")         (N,Z)");
        Console.WriteLine(" <4>               LD" + GetRegisterChar(registerIndex) + " $04 (" + ByteToString(memory[4]) + ")         (N,Z)");
        Console.WriteLine(" <5>               LD" + GetRegisterChar(registerIndex) + " $05 (" + ByteToString(memory[5]) + ")         (N,Z)");
        Console.WriteLine(" <6>               LD" + GetRegisterChar(registerIndex) + " $06 (" + ByteToString(memory[6]) + ")         (N,Z)");
        Console.WriteLine(" <7>               LD" + GetRegisterChar(registerIndex) + " $07 (" + ByteToString(memory[7]) + ")         (N,Z)");
        return "LOAD VALUE FROM MEMORY";
    }

    private string RenderMainSubStateLogical()
    {
        Console.WriteLine(" <A>               LOGICAL AND                (N,Z)");
        Console.WriteLine(" <E>               EXCLUSIVE OR               (N,Z)");
        Console.WriteLine(" <O>               LOGICAL INCLUSIVE OR       (N,Z)");
        return "LOGICAL OPERATIONS";
    }

    private string RenderMainSubStateAnd()
    {
        Console.WriteLine(" <0>               AND $00 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <1>               AND $01 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <2>               AND $02 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <3>               AND $03 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <4>               AND $04 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <5>               AND $05 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <6>               AND $06 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <7>               AND $07 (" + ByteToString(memory[0]) + ")         (N,Z)");
        return "LOGICAL AND";
    }

    private string RenderMainSubStateEor()
    {
        Console.WriteLine(" <0>               EOR $00 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <1>               EOR $01 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <2>               EOR $02 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <3>               EOR $03 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <4>               EOR $04 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <5>               EOR $05 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <6>               EOR $06 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <7>               EOR $07 (" + ByteToString(memory[0]) + ")         (N,Z)");
        return "EXCLUSIVE OR";
    }

    private string RenderMainSubStateOra()
    {
        Console.WriteLine(" <0>               ORA $00 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <1>               ORA $01 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <2>               ORA $02 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <3>               ORA $03 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <4>               ORA $04 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <5>               ORA $05 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <6>               ORA $06 (" + ByteToString(memory[0]) + ")         (N,Z)");
        Console.WriteLine(" <7>               ORA $07 (" + ByteToString(memory[0]) + ")         (N,Z)");
        return "LOGICAL INCLUSIVE OR";
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
        return "TRANSFER VALUE TO " + (registerIndex == 0 ? "REGISTER" : "ACCUMULATOR" );
    }

    // Render status and keybinds for all MAIN substates
    private void RenderMainSubState()
    {
        string footerTitle = "";

        switch (mainSubState)
        {
            case MAIN_SUB_STATE.DEFAULT or MAIN_SUB_STATE.QUIT:
                RenderMainSubStateDefault();
                break;
            case MAIN_SUB_STATE.ADD:
                footerTitle = RenderMainSubStateAdd();
                break;
            case MAIN_SUB_STATE.SUBTRACT:
                footerTitle = RenderMainSubStateSubtract();
                break;
            case MAIN_SUB_STATE.STORE:
                footerTitle = RenderMainSubStateStore();
                break;
            case MAIN_SUB_STATE.LOAD:
                footerTitle = RenderMainSubStateLoad();
                break;
            case MAIN_SUB_STATE.TRANSFER:
                footerTitle = RenderMainSubStateTransfer();
                break;
            case MAIN_SUB_STATE.LOGICAL:
                footerTitle = RenderMainSubStateLogical();
                break;
            case MAIN_SUB_STATE.AND:
                footerTitle = RenderMainSubStateAnd();
                break;
            case MAIN_SUB_STATE.EOR:
                footerTitle = RenderMainSubStateEor();
                break;
            case MAIN_SUB_STATE.ORA:
                footerTitle = RenderMainSubStateOra();
                break;
        }

        // Add empty lines to fill RENDER_HEIGHT
        for (int i = Console.CursorTop; i < RENDER_HEIGHT - 2; i++)
        {
            Console.WriteLine();
        }
        
        // Footer
        Console.WriteLine(Lines.seperator[0]);
        //if (mainSubState == MAIN_SUB_STATE.DEFAULT) { Console.WriteLine(" " + (COLORS.BG_DARK_WHITE + COLORS.FG_BLACK + " MAIN " + COLORS.DEFAULT).PadRight(40,' ') + " PRESS <ESC> TO QUIT PROGRAM"); }
        if (mainSubState == MAIN_SUB_STATE.DEFAULT) { Console.WriteLine(" PRESS <ESC> TO QUIT PROGRAM"); }
        else if (mainSubState == MAIN_SUB_STATE.QUIT) { Console.WriteLine(" " + COLORS.BG_DARK_RED + COLORS.FG_BLACK + " ARE YOU SURE YOU WANT TO QUIT? Y/N " + COLORS.DEFAULT); }
        else { Console.WriteLine(" " + (COLORS.BG_DARK_GREEN + COLORS.FG_BLACK + " " + footerTitle +" " + COLORS.DEFAULT).PadRight(47,' ') + " PRESS <ESC> TO CANCEL"); }
        //Console.WriteLine(" " + COLORS.BG_DARK_GREEN + COLORS.FG_BLACK + " NORMAL " + COLORS.DEFAULT);

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
            registerLineHex += " 0x" + HexToString(registers[i]) + "  ";
        }
        Console.WriteLine(registerLine);
        Console.WriteLine(registerLineHex);
        Console.WriteLine(Lines.seperator[0]);
        
        // Values stored in memory
        string memoryLine1 = " ";
        string memoryLine2 = " ";
        for (byte i = 0; i < MEMORY_NUM; i++)
        {
            memoryLine1 += " $" + (i.ToString()).PadLeft(2,'0') + ":  ";
            memoryLine2 += " 0x" + HexToString(memory[i]) + "  ";
        }
        Console.WriteLine(memoryLine1);
        Console.WriteLine(memoryLine2);
        //Console.WriteLine(Lines.seperator[0]);
        Console.WriteLine();
        Console.WriteLine(COLORS.BG_DARK_WHITE + COLORS.FG_BLACK + (" BINARY VALUE:").PadRight(RENDER_WIDTH,' ') + COLORS.DEFAULT);

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
                display[y] += DIGITS[currentDigit,y,x % CHAR_WIDTH] ? (bitChanged && settings["uiHlChangedBit"].enabled) ? COLORS.FG_BRIGHT_YELLOW + FILLED + COLORS.DEFAULT : FILLED : EMPTY;
            }

            // Render the results of the current row
            Console.WriteLine(display[y]);
        }
        Console.WriteLine();

        // Decimal values
        //Console.WriteLine(Lines.seperator[0]);
        Console.WriteLine(COLORS.BG_DARK_WHITE + COLORS.FG_BLACK + (" DECIMAL VALUE:").PadRight(RENDER_WIDTH,' ') + COLORS.DEFAULT);
        Console.WriteLine(" UNSIGNED VALUE:     " + (registers[registerIndex].ToString()).PadLeft(3,' ') + "        SIGNED VALUE:      " + (((int)((registers[registerIndex] & 0b01111111) - (registers[registerIndex] & 0b10000000))).ToString()).PadLeft(4,' '));
        //Console.WriteLine(" ASCII CHARACTER:  " + (registers[registerIndex] > 32 ? (char)registers[registerIndex] : "-"));

        //Console.WriteLine(Lines.seperator[0]);
        Console.WriteLine(COLORS.BG_DARK_WHITE + COLORS.FG_BLACK + (" FLAGS:").PadRight(RENDER_WIDTH,' ') + COLORS.DEFAULT);
        
        // Flags
        string carryFlagString = " (C) CARRY FLAG:       " + (carryFlag ? COLORS.FG_BRIGHT_GREEN + "1" : COLORS.FG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        string zeroFlagString = "        (Z) ZERO FLAG:        " + (zeroFlag ? COLORS.FG_BRIGHT_GREEN + "1"  : COLORS.FG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        string negativeFlagString = " (N) NEGATIVE FLAG:    " + (negativeFlag ? COLORS.FG_BRIGHT_GREEN + "1" : COLORS.FG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        string overFlowFlagString = "        (V) OVERFLOW FLAG:    " + (overflowFlag ? COLORS.FG_BRIGHT_GREEN + "1" : COLORS.FG_BRIGHT_RED + "0") + COLORS.DEFAULT;
        Console.WriteLine(carryFlagString + zeroFlagString);
        Console.WriteLine(negativeFlagString + overFlowFlagString);
        
        // Message log
        int messageIndexMax = Math.Max(messages.Count - MESSAGES_NUM, 0);
        int messageIndexNumDigits = Math.Max(messageIndexMax.ToString().Length, 2);
        Console.WriteLine(COLORS.BG_DARK_WHITE + COLORS.FG_BLACK + " MESSAGE LOG:" + ("<SHIFT> + <UP> / <DOWN> (" + messageIndex.ToString().PadLeft(messageIndexNumDigits,'0') + "/" + messageIndexMax.ToString().PadLeft(messageIndexNumDigits,'0') + ") ").PadLeft(43,' ') + COLORS.DEFAULT);
       
        // Render message log
        RenderMessages();
        
        //Console.WriteLine(Lines.seperator[0]);
        Console.WriteLine(COLORS.BG_DARK_WHITE + COLORS.FG_BLACK + (" KEY:              ACTION:                    FLAGS:").PadRight(RENDER_WIDTH,' ') + COLORS.DEFAULT);
        // Render status and available keybinds for the current substate
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
        Run();
    }
}

