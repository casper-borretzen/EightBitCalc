namespace EightBitCalc;

class BinaryCalc
{
    // Build consts
    public const bool WIN_BUILD = false;  // Set to true if building for Windows
    private const byte VERSION_MAJOR = 8;
    private const byte VERSION_MINOR = 1;
    private const byte VERSION_PATCH = 0;

    // Set main classes
    Render render;
    Logger logger;
    Processor processor;
    Input input;


    // Main application state
    public enum STATE 
    {
        MAIN,
        HELP,
        SETTINGS,
        ASSEMBLY,
        ERROR,
    };

    // Substates for MAIN state
    public enum MAIN_SUB_STATE 
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

    // Byte reference type
    public enum BYTE_TYPE
    {
        BINARY,
        DECIMAL,
        HEX,
        ADDRESS
    };

    // Set color constants
    public const ConsoleColor COLOR_TITLEBAR_MAIN_FG      = ConsoleColor.Black;
    public const ConsoleColor COLOR_TITLEBAR_MAIN_BG      = ConsoleColor.Cyan;
    public const ConsoleColor COLOR_TITLEBAR_FG           = ConsoleColor.Black;
    public const ConsoleColor COLOR_TITLEBAR_BG           = ConsoleColor.Gray;
    public const ConsoleColor COLOR_INACTIVE_FG           = ConsoleColor.DarkGray;
    public const ConsoleColor COLOR_QUIT_PROMPT_FG        = ConsoleColor.Black;
    public const ConsoleColor COLOR_QUIT_PROMPT_BG        = ConsoleColor.DarkRed;
    public const ConsoleColor COLOR_SUB_STATE_FG          = ConsoleColor.Black;
    public const ConsoleColor COLOR_SUB_STATE_BG          = ConsoleColor.DarkGreen;
    public const ConsoleColor COLOR_REGISTER_ACTIVE_FG    = ConsoleColor.Black;
    public const ConsoleColor COLOR_REGISTER_ACTIVE_BG    = ConsoleColor.Yellow;
    public const ConsoleColor COLOR_REGISTER_INACTIVE_FG  = ConsoleColor.Black;
    public const ConsoleColor COLOR_REGISTER_INACTIVE_BG  = ConsoleColor.Gray;
    public const ConsoleColor COLOR_BIT_HL_FG             = ConsoleColor.Yellow;
    public const ConsoleColor COLOR_FLAG_SET_FG           = ConsoleColor.DarkGreen;
    public const ConsoleColor COLOR_FLAG_UNSET_FG         = ConsoleColor.DarkRed;
    public const ConsoleColor COLOR_CONTAINER_HEADER_FG   = ConsoleColor.Black;
    public const ConsoleColor COLOR_CONTAINER_HEADER_BG   = ConsoleColor.Magenta;
    public const ConsoleColor COLOR_CONTAINER_LINENUM_FG  = ConsoleColor.Gray;
    public const ConsoleColor COLOR_CONTAINER_LINENUM_BG  = ConsoleColor.DarkGray;
    
    // Set ANSI color constants
    public const string COLOR_ANSI_DEFAULT                = (WIN_BUILD ? "" : "\x1B[0m");
    public const string COLOR_ANSI_UL                     = (WIN_BUILD ? "" : "\x1B[4m");
    public const string COLOR_ANSI_FG_BLACK               = (WIN_BUILD ? "" : "\x1B[30m");
    public const string COLOR_ANSI_FG_WHITE               = (WIN_BUILD ? "" : "\x1B[97m");
    public const string COLOR_ANSI_FG_DARK_RED            = (WIN_BUILD ? "" : "\x1B[31m");
    public const string COLOR_ANSI_FG_DARK_GREEN          = (WIN_BUILD ? "" : "\x1B[32m");
    public const string COLOR_ANSI_FG_DARK_YELLOW         = (WIN_BUILD ? "" : "\x1B[33m");
    public const string COLOR_ANSI_FG_DARK_BLUE           = (WIN_BUILD ? "" : "\x1B[34m");
    public const string COLOR_ANSI_FG_DARK_MAGENTA        = (WIN_BUILD ? "" : "\x1B[35m");
    public const string COLOR_ANSI_FG_DARK_CYAN           = (WIN_BUILD ? "" : "\x1B[36m");
    public const string COLOR_ANSI_FG_DARK_WHITE          = (WIN_BUILD ? "" : "\x1B[37m");
    public const string COLOR_ANSI_FG_BRIGHT_BLACK        = (WIN_BUILD ? "" : "\x1B[90m");
    public const string COLOR_ANSI_FG_BRIGHT_RED          = (WIN_BUILD ? "" : "\x1B[91m");
    public const string COLOR_ANSI_FG_BRIGHT_GREEN        = (WIN_BUILD ? "" : "\x1B[92m");
    public const string COLOR_ANSI_FG_BRIGHT_YELLOW       = (WIN_BUILD ? "" : "\x1B[93m");
    public const string COLOR_ANSI_FG_BRIGHT_BLUE         = (WIN_BUILD ? "" : "\x1B[94m");
    public const string COLOR_ANSI_FG_BRIGHT_MAGENTA      = (WIN_BUILD ? "" : "\x1B[95m");
    public const string COLOR_ANSI_FG_BRIGHT_CYAN         = (WIN_BUILD ? "" : "\x1B[96m");
    public const string COLOR_ANSI_BG_BLACK               = (WIN_BUILD ? "" : "\x1B[40m");
    public const string COLOR_ANSI_BG_WHITE               = (WIN_BUILD ? "" : "\x1B[107m");
    public const string COLOR_ANSI_BG_DARK_RED            = (WIN_BUILD ? "" : "\x1B[41m");
    public const string COLOR_ANSI_BG_DARK_GREEN          = (WIN_BUILD ? "" : "\x1B[42m");
    public const string COLOR_ANSI_BG_DARK_YELLOW         = (WIN_BUILD ? "" : "\x1B[43m");
    public const string COLOR_ANSI_BG_DARK_BLUE           = (WIN_BUILD ? "" : "\x1B[44m");
    public const string COLOR_ANSI_BG_DARK_MAGENTA        = (WIN_BUILD ? "" : "\x1B[45m");
    public const string COLOR_ANSI_BG_DARK_CYAN           = (WIN_BUILD ? "" : "\x1B[46m");
    public const string COLOR_ANSI_BG_DARK_WHITE          = (WIN_BUILD ? "" : "\x1B[47m");
    public const string COLOR_ANSI_BG_BRIGHT_BLACK        = (WIN_BUILD ? "" : "\x1B[100m");
    public const string COLOR_ANSI_BG_BRIGHT_RED          = (WIN_BUILD ? "" : "\x1B[101m");
    public const string COLOR_ANSI_BG_BRIGHT_GREEN        = (WIN_BUILD ? "" : "\x1B[102m");
    public const string COLOR_ANSI_BG_BRIGHT_YELLOW       = (WIN_BUILD ? "" : "\x1B[103m");
    public const string COLOR_ANSI_BG_BRIGHT_BLUE         = (WIN_BUILD ? "" : "\x1B[104m");
    public const string COLOR_ANSI_BG_BRIGHT_MAGENTA      = (WIN_BUILD ? "" : "\x1B[105m");
    public const string COLOR_ANSI_BG_BRIGHT_CYAN         = (WIN_BUILD ? "" : "\x1B[106m");

    // Set private constants
    private const int CHAR_SPACING      = 2;
    private const int CHAR_LIMIT        = 8;

    // Set public constants
    public const string MAIN_TITLE      = "8-BIT BINARY CALCULATOR";
    public const char EMPTY             = ' ';
    public const char FILLED            = '#';
    public const int CHAR_HEIGHT        = 7;
    public const int CHAR_WIDTH         = 5 + CHAR_SPACING;
    public const int RENDER_WIDTH       = CHAR_LIMIT * CHAR_WIDTH;
    public const int RENDER_HEIGHT      = 52;
    public const int MESSAGES_NUM       = 8;
    public const int REGISTER_NUM       = 3;
    public const int MEMORY_NUM         = 5;
    public const int INPUT_CHAR_LIMIT   = 14;

    // Set private variables
    private int windowWidth             = 0;
    private int windowHeight            = 0;
    private STATE appStatePrev          = STATE.MAIN;
    
    // Set public variables
    public bool running { set; get; }            = true;
    public STATE appState { get; private set; }               = STATE.MAIN;
    public MAIN_SUB_STATE mainSubState { get; private set; }  = MAIN_SUB_STATE.DEFAULT;

    // Memory for storing MEMORY_NUM bytes of data
    public byte[] memory { get; set; }  = new byte[MEMORY_NUM];

    // Make blank line and seperators
    public static string[] seperator =  
    {
        new String('-', RENDER_WIDTH),
        new String('/', RENDER_WIDTH)
    };

    // Set error types
    public Dictionary<string, bool> appError { get; } = new Dictionary<string, bool>
    {
        { "windowSize",     false }
    };

    // Settings
    public Dictionary<string, Setting> settings { get; } = new Dictionary<string, Setting>
    {
        { "uiHlChangedBit", new Setting(true,  "HIGHLIGHT BITS CHANGED IN PREVIOUS OPERATION") },
        { "flagsAutoCarry", new Setting(false, "AUTOMATICALLY SET/UNSET CARRY FLAG") },
        { "modKeyCtrl", new Setting((WIN_BUILD ? true : false), "USE CONTROL INSTEAD OF SHIFT AS MODIFIER KEY") }
    }; 
    
    // Create scroll containers
    public Dictionary<STATE, ContainerScroll> containerScroll { get; } = new Dictionary<STATE, ContainerScroll>
    {
        { STATE.HELP,       new ContainerScroll("HELP SCREEN",  RENDER_HEIGHT - 8) },
        { STATE.ASSEMBLY,   new ContainerScroll("ASSEMBLY LOG", RENDER_HEIGHT - 8, startAtBottom: true, showLineNum: true) }
    };

    // Create toggle containers
    public Dictionary<STATE, ContainerToggle> containerToggle { get; } = new Dictionary<STATE, ContainerToggle>
    {
        { STATE.SETTINGS,   new ContainerToggle("PREFERENCES", RENDER_HEIGHT - 8) }
    };

    // List for HELP screen text
    private List <string> help = new List <string>() 
    {
            "",
            " APPLICATION VERSION " + VERSION_MAJOR.ToString() + "." + VERSION_MINOR.ToString() + "." + VERSION_PATCH.ToString(),
            "",
            " " + COLOR_ANSI_UL + "ASSEMBLY LOG:" + COLOR_ANSI_DEFAULT,
            " THE ASSEMBLY LOG IS AUTOMATICALLY SAVED",
            " TO LOG.ASM WHEN QUITTING.",
            "",
            " " + COLOR_ANSI_UL + "MEMORY:" + COLOR_ANSI_DEFAULT,
            " THERE ARE 5 BYTES OF MEMORY AVAILABLE",
            " IN THE ADDRESS RANGE $00 - $04.",
            "",
            " " + COLOR_ANSI_UL + "FLAGS:" + COLOR_ANSI_DEFAULT,
            " FOUR FLAGS ARE IMPLEMENTED.",
            " FLAGS ARE SET AFTER PERFORMING OPERATIONS.",
            " FLAGS ARE INDICATED BY PARANTHESIS.",
            "",
            " " + COLOR_ANSI_UL + "(C) CARRY FLAG:" + COLOR_ANSI_DEFAULT,
            " FUNCTIONALITY DIFFERS DEPENDING ON OPERATION.",
            "",
            " " + COLOR_ANSI_UL + "(Z) ZERO FLAG:" + COLOR_ANSI_DEFAULT,
            " IS SET TO 1 IF THE RESULT IS ZERO.",
            "",
            " " + COLOR_ANSI_UL + "(O) OVERFLOW FLAG:" + COLOR_ANSI_DEFAULT,
            " FUNCTIONALITY DIFFERS DEPENDING ON OPERATION.",
            "",
            " " + COLOR_ANSI_UL + "(N) NEGATIVE FLAG:" + COLOR_ANSI_DEFAULT,
            " IS SET TO 1 IF BIT 7 IS 1.",
            "",
            " " + COLOR_ANSI_UL + "ACCUMULATOR AND REGISTERS:" + COLOR_ANSI_DEFAULT,
            " THE ACCUMULATOR [A] AND THE REGISTERS [X] AND [Y]",
            " EACH HOLDS ONE BYTE OF DATA.",
            " ARITHMETIC OPERATIONS ARE AVAILABLE ON THE",
            " ACCUMULATOR, BUT NOT ON THE REGISTERS.",
            " INCREMENT AND DECREMENT OPERATIONS ARE AVAILABLE",
            " ON THE REGISTERS, BUT NOT ON THE ACCUMULATOR.",
            " ACCUMULATOR AND REGISTERS ARE INDICATED",
            " BY SQUARE BRACKETS.",
            "",
            " " + COLOR_ANSI_UL + "LOADING / STORING MODES:" + COLOR_ANSI_DEFAULT,
            " IMMEDIATE MODE:            LDA #20",
            " LOADS [A] WITH THE LITERAL DECIMAL VALUE 20.",
            " ",
            " ABSOLUTE (ZERO-PAGE) MODE: LDA $05",
            " LOADS [A] WITH THE VALUE FROM MEMORY ADDRESS $05",
            " ",
            " IMMEDIATE MODE:            LDA #$20",
            " LOADS [A] WITH THE LITERAL HEXADECIMAL NUMBER 20.",
            "",
            " " + COLOR_ANSI_UL + "BASE-2 (BINARY) TO BASE-10 (DECIMAL):" + COLOR_ANSI_DEFAULT,
            "  128's  64's  32's  16's   8's   4's   2's   1's",
            "    |     |     |     |     |     |     |     |",
            "    0     0     0     0     0     1     1     1  =  7",
            "    0     1     1     1     1     1     1     1  =  127",
            "    1     0     0     0     0     0     0     1  =  128",
            "    1     1     1     1     1     1     1     1  =  255",
            "",
            " " + COLOR_ANSI_UL + "NEGATIVE NUMBERS USING TWO\'S COMPLIMENT:" + COLOR_ANSI_DEFAULT,
            " -128's  64's  32's  16's   8's   4's   2's   1's",
            "    |     |     |     |     |     |     |     |",
            "    0     0     0     0     0     1     1     1  =  7",
            "    0     1     1     1     1     1     1     1  =  127",
            "    1     0     0     0     0     0     0     1  = -128",
            "    1     1     1     1     1     1     1     1  = -1",
            "",
            seperator[1],
            "",
            " " + COLOR_ANSI_UL + "LDA: LOAD ACCUMULATOR" + COLOR_ANSI_DEFAULT + "                        (N,Z)",
            " LOAD A BYTE OF DATA INTO THE ACCUMULATOR.",
            "",
            " " + COLOR_ANSI_UL + "LDX: LOAD X REGISTER" + COLOR_ANSI_DEFAULT + "                         (N,Z)",
            " LOAD A BYTE OF DATA INTO THE X REGISTER.",
            "",
            " " + COLOR_ANSI_UL + "LDX: LOAD Y REGISTER" + COLOR_ANSI_DEFAULT + "                         (N,Z)",
            " LOAD A BYTE OF DATA INTO THE Y REGISTER.",
            "",
            " " + COLOR_ANSI_UL + "STA: STORE ACCUMULATOR" + COLOR_ANSI_DEFAULT + "                       (N,Z)",
            " STORE THE CONTENT OF THE ACCUMULATOR INTO MEMORY.",
            "",
            " " + COLOR_ANSI_UL + "STX: STORE X REGISTER" + COLOR_ANSI_DEFAULT + "                        (N,Z)",
            " STORE THE CONTENT OF THE X REGISTER INTO MEMORY.",
            "",
            " " + COLOR_ANSI_UL + "STY: STORE Y REGISTER" + COLOR_ANSI_DEFAULT + "                        (N,Z)",
            " STORE THE CONTENT OF THE Y REGISTER INTO MEMORY.",
            "",
            " " + COLOR_ANSI_UL + "TAX: TRANSFER ACCUMULATOR TO X" + COLOR_ANSI_DEFAULT + "               (N,Z)",
            " COPIES THE DATA FROM [A] TO [X].",
            "",
            " " + COLOR_ANSI_UL + "TAY: TRANSFER ACCUMULATOR TO Y" + COLOR_ANSI_DEFAULT + "               (N,Z)",
            " COPIES THE DATA FROM [A] TO [Y].",
            "",
            " " + COLOR_ANSI_UL + "TXA: TRANSFER X TO ACCUMULATOR" + COLOR_ANSI_DEFAULT + "               (N,Z)",
            " COPIES THE DATA FROM [X] TO [A].",
            "",
            " " + COLOR_ANSI_UL + "TYA: TRANSFER Y TO ACCUMULATOR" + COLOR_ANSI_DEFAULT + "               (N,Z)",
            " COPIES THE DATA FROM [Y] TO [A].",
            "",
            " " + COLOR_ANSI_UL + "AND: LOGICAL AND" + COLOR_ANSI_DEFAULT + "                             (N,Z)",
            " THE RESULT OF A LOGICAL AND IS ONLY TRUE",
            " IF BOTH INPUTS ARE TRUE.",
            " CAN BE USED TO MASK BITS.",
            " CAN BE USED TO CHECK FOR EVEN/ODD NUMBERS.",
            " CAN BE USED TO CHECK IF A NUMBER IS",
            " DIVISIBLE BY 2/4/6/8 ETC.",
            "",
            " " + COLOR_ANSI_UL + "EOR: EXCLUSIVE OR" + COLOR_ANSI_DEFAULT + "                            (N,Z)",
            " AN EXCLUSIVE OR IS SIMILAR TO LOGICAL OR, WITH THE",
            " EXCEPTION THAT IS IS FALSE WHEN BOTH INPUTS ARE TRUE.",
            " EOR CAN BE USED TO FLIP BITS.",
            "",
            " " + COLOR_ANSI_UL + "ORA: LOGICAL INCLUSIVE OR" + COLOR_ANSI_DEFAULT + "                    (N,Z)",
            " THE RESULT OF A LOGICAL INCLUSIVE OR IS TRUE IF",
            " AT LEAST ONE OF THE INPUTS ARE TRUE.",
            " ORA CAN BE USED TO SET A PARTICULAR BIT TO TRUE.",
            " ORA + EOR CAN BE USED TO SET A BIT TO FALSE.",
            "",
            " " + COLOR_ANSI_UL + "ADC: ADD WITH CARRY" + COLOR_ANSI_DEFAULT + "                          (N,V,Z,C)",
            " ADDS A VALUE TOGETHER WITH THE CARRY BIT TO [A].",
            " THE (C) FLAG IS SET IF THE RESULTING VALUE",
            " IS ABOVE 255 (UNSIGNED).", 
            " THE (V) FLAG IS SET IF ADDING TO A POSITIVE",
            " NUMBER AND ENDING UP WITH A NEGATIVE NUMBER.",
            "",
            " " + COLOR_ANSI_UL + "SBC: SUBTRACT WITH CARRY" + COLOR_ANSI_DEFAULT + "                     (N,V,Z,C)",
            " SUBTRACTS A VALUE TOGETHER WITH THE",
            " NOT OF THE CARRY BIT TO [A].",
            " THE (C) FLAG IS CLEAR IF THE RESULTING VALUE",
            " IS LESS THAN 0 (UNSIGNED).", 
            " THE (V) FLAG IS SET IF SUBTRACTING FROM A NEGATIVE",
            " NUMBER AND ENDING UP WITH A POSITIVE NUMBER.",
            "",
            " " + COLOR_ANSI_UL + "INX: INCREMENT X REGISTER" + COLOR_ANSI_DEFAULT + "                    (N,Z)",
            " ADDS ONE TO THE VALUE OF THE X REGISTER.",
            "",
            " " + COLOR_ANSI_UL + "INY: INCREMENT Y REGISTER" + COLOR_ANSI_DEFAULT + "                    (N,Z)",
            " ADDS ONE TO THE VALUE OF THE Y REGISTER.",
            "",
            " " + COLOR_ANSI_UL + "DEX: DECREMENT X REGISTER" + COLOR_ANSI_DEFAULT + "                    (N,Z)",
            " SUBTRACTS ONE FROM THE VALUE OF THE X REGISTER.",
            "",
            " " + COLOR_ANSI_UL + "DEY: DECREMENT Y REGISTER" + COLOR_ANSI_DEFAULT + "                    (N,Z)",
            " SUBTRACTS ONE FROM THE VALUE OF THE Y REGISTER.",
            "",
            " " + COLOR_ANSI_UL + "ASL: ARITHMETIC SHIFT LEFT" + COLOR_ANSI_DEFAULT + "                   (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE LEFT",
            " INSERTING A 0 IN THE RIGHTMOST BIT",
            " AND MOVING THE LEFTMOST BIT TO THE (C) FLAG.",
            " THIS OPERATION IS EQUIVALENT TO MULTIPLYING BY 2.",
            "",
            " " + COLOR_ANSI_UL + "LSR: LOGICAL SHIFT RIGHT" + COLOR_ANSI_DEFAULT + "                     (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE RIGHT",
            " INSERTING A 0 IN THE LEFTMOST BIT",
            " AND MOVING THE RIGHTMOST BIT TO THE (C) FLAG.",
            " THIS OPERATION IS EQUIVALENT TO DIVIDING BY 2.",
            "",
            " " + COLOR_ANSI_UL + "ROL: ROTATE LEFT" + COLOR_ANSI_DEFAULT + "                             (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE LEFT",
            " THE LEFTMOST BIT MOVES OVER TO THE RIGHTMOST SIDE.",
            "",
            " " + COLOR_ANSI_UL + "ROR: ROTATE RIGHT" + COLOR_ANSI_DEFAULT + "                            (N,Z,C)",
            " MOVES ALL BITS ONE STEP TO THE RIGHT",
            " THE RIGHTMOST BIT MOVES OVER TO THE LEFTMOST SIDE.",
            "",
            " " + COLOR_ANSI_UL + "CLC: CLEAR CARRY FLAG" + COLOR_ANSI_DEFAULT + "                        (N,Z,C)",
            " SETS THE (C) FLAG TO 0.",
            " USUALLY PERFORMED BEFORE ADDITION.",
            "",
            " " + COLOR_ANSI_UL + "SEC: SET CARRY FLAG" + COLOR_ANSI_DEFAULT + "                          (N,Z,C)",
            " SETS THE (C) FLAG TO 1.",
            " USUALLY PERFORMED BEFORE SUBTRACTION.",
            "",
            " " + COLOR_ANSI_UL + "CLV: CLEAR OVERFLOW FLAG" + COLOR_ANSI_DEFAULT + "                     (N,Z,C)",
            " SETS THE (V) FLAG TO 0.",
            "",
    };



///////////////////////////////////////////////////////////////////////////////
// CORE METHODS:
///////////////////////////////////////////////////////////////////////////////
    
    // Reset console
    private void ConsoleReset() 
    {
        Console.CursorVisible = false;
        Console.ResetColor();
        Console.Clear();
    }

    // Check console window size in rows and columns
    private void CheckWindowSize()
    {
        if ((windowWidth = Console.WindowWidth) < RENDER_WIDTH || (windowHeight = Console.WindowHeight) < RENDER_HEIGHT)
        {
            if (appState != STATE.ERROR) { ChangeState(STATE.ERROR); }
            appError["windowSize"] = true;
        }
        else if (appState == STATE.ERROR)
        {
            ChangeState(appStatePrev);
            appError["windowSize"] = false;
        }
    }

    // Change the app STATE
    public void ChangeState(STATE newState)
    {
        appStatePrev = appState;
        appState = newState;
        ChangeMainSubState(MAIN_SUB_STATE.DEFAULT);
        input.ChangeInputMode(Input.INPUT_MODE.NORMAL);
        logger.messageIndex = 0;

        // Reset scroll/selection for container
        if (containerScroll.ContainsKey(newState)) { containerScroll[newState].Zero(); }
        else if (containerToggle.ContainsKey(newState)) { containerToggle[newState].Zero(); }
    }

    // Change the MAIN_SUB_STATE
    public void ChangeMainSubState(MAIN_SUB_STATE newSubState)
    {
        mainSubState = newSubState;
    }

    // Format value reference mode
    public void FormatByteType(byte bits, BYTE_TYPE type, out byte newBits, out string asm)
    {
        asm = "";
        newBits = bits;
        if (type == BYTE_TYPE.BINARY) { asm = "#%" + ByteToString(bits); }
        else if (type == BYTE_TYPE.DECIMAL) { asm = "#" + bits.ToString(); }
        else if (type == BYTE_TYPE.HEX) {asm = "#$" + HexToString(bits);}
        else if (type == BYTE_TYPE.ADDRESS) {asm = "$" + HexToString(bits); newBits = memory[bits];}
    }
    
    // Get the letter for a given register
    public static char GetRegisterChar(byte index)
    {
        return (char)((int)'A' + (index == 0 ? 0 : index + 22));
    }

    // Convert a byte to an eight digit binary string
    public static string ByteToString(byte bits)
    {
        return Convert.ToString(bits, 2).PadLeft(8, '0');
    }

    // Convert numerical value to hex string
    public static string HexToString(int hex)
    {
        return hex.ToString("X2");
    }



///////////////////////////////////////////////////////////////////////////////
// CONSTRUCTOR AND MAIN METHODS:
///////////////////////////////////////////////////////////////////////////////

    // Initialize values
    private void Init() 
    {
        // Link lists with containers
        containerScroll[STATE.HELP].LinkContent(help);
        containerScroll[STATE.ASSEMBLY].LinkContent(logger.assembly);
        containerToggle[STATE.SETTINGS].LinkContent(settings);

        logger.AddMessage("Welcome!");
    }

    // Main loop
    public void Run()
    {
        // Clear console and hide cursor
        ConsoleReset();
        
        // Set window size to fit the app (if running in windows)
        if (WIN_BUILD) { Console.SetWindowSize(RENDER_WIDTH, RENDER_HEIGHT); Console.SetBufferSize(RENDER_WIDTH + 2, RENDER_HEIGHT); }

        while (running == true)
        {
            CheckWindowSize();
            render.RenderConsole();
            input.GetInput();
        }

        // Exit the program and reset the console
        ConsoleReset();

        // Save the assembly log to .asm file
        logger.SaveToFile();
    }
   
    // Class constructor
    public BinaryCalc()
    {
        logger = new Logger(binaryCalc: this);
        processor = new Processor(binaryCalc: this, logger: logger);
        input = new Input(binaryCalc: this, processor: processor, logger: logger);
        render = new Render(binaryCalc: this, logger: logger, processor: processor, input: input);

        Init();
        Run();
    }
}

