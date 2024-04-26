namespace EightBitCalc;

class Input
{
    // Class references
    private BinaryCalc binaryCalc;
    private Processor processor;
    private Logger logger;

    // Keyboard input modes in MAIN state
    public enum INPUT_MODE
    {
        NORMAL,
        COMMAND,
        NUMERICAL_DEC,
        NUMERICAL_BIN,
        NUMERICAL_HEX
    };

    // Set vars
    public INPUT_MODE inputMode { get; private set; } = INPUT_MODE.NORMAL;

    private string inputString = "";

    // Change the INPUT_MODE
    public void ChangeInputMode(INPUT_MODE newInputMode)
    {
        inputMode = newInputMode;
        if (inputMode == INPUT_MODE.NORMAL) { Console.CursorVisible = false; }
        else { Console.CursorVisible = true; }
        InputClear();
    }



    ///////////////////////////////////////////////////////////////////////////////
    // INPUT HANDLING METHODS:
    ///////////////////////////////////////////////////////////////////////////////

    // Dynmaic increment
    private void HandleInputIncrement()
    {
        switch (processor.registerIndex)
        {
            case 0:
                processor.ADC((byte)(0b00000001), inc: true);
                break;
            case 1:
                processor.INX();
                break;
            case 2:
                processor.INY();
                break;
        }
    }

    // Dynamic decrement
    private void HandleInputDecrement()
    {
        switch (processor.registerIndex)
        {
            case 0:
                processor.SBC((byte)(0b00000001), dec: true);
                break;
            case 1:
                processor.DEX();
                break;
            case 2:
                processor.DEY();
                break;
        }
    }

    // Store value to memory from register
    private bool HandleInputStore(byte memoryIndex, bool keyMod)
    {
        if (processor.registerIndex == 0) { processor.STA(memoryIndex); }
        else if (processor.registerIndex == 1) { processor.STX(memoryIndex); }
        else if (processor.registerIndex == 2) { processor.STY(memoryIndex); }
        return true;
    }

    // Load value to register from memory
    private bool HandleInputLoad(byte memoryIndex, bool keyMod)
    {
        if (processor.registerIndex == 0) { processor.LDA(memoryIndex, BinaryCalc.BYTE_TYPE.ADDRESS); }
        else if (processor.registerIndex == 1) { processor.LDX(memoryIndex, BinaryCalc.BYTE_TYPE.ADDRESS); }
        else if (processor.registerIndex == 2) { processor.LDY(memoryIndex, BinaryCalc.BYTE_TYPE.ADDRESS); }
        return true;
    }

    // Handle input in MAIN_SUB_STATE.TRANSFER mode
    private bool HandleInputTransfer(byte targetRegisterIndex, bool keyMod)
    {
        if (targetRegisterIndex != processor.registerIndex)
        {
            if (processor.registerIndex == 0)
            {
                if (targetRegisterIndex == 1) { processor.TAX(); return true; }
                else if (targetRegisterIndex == 2) { processor.TAY(); return true; }
            }
            else if (processor.registerIndex == 1)
            {
                if (targetRegisterIndex == 0) { processor.TXA(); return true; }
            }
            else if (processor.registerIndex == 2)
            {
                if (targetRegisterIndex == 0) { processor.TYA(); return true; }
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
                binaryCalc.running = false;
                return true;
            case ConsoleKey.N:
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT);
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
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.AND);
                return true;
            case ConsoleKey.E:
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.EOR);
                return true;
            case ConsoleKey.O:
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.ORA);
                return true;
            case ConsoleKey.Escape:
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT);
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
                processor.AND(0, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                processor.AND(1, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                processor.AND(2, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                processor.AND(3, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                processor.AND(4, BinaryCalc.BYTE_TYPE.ADDRESS);
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
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.LOGICAL);
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
                processor.EOR(0, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                processor.EOR(1, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                processor.EOR(2, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                processor.EOR(3, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                processor.EOR(4, BinaryCalc.BYTE_TYPE.ADDRESS);
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
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.LOGICAL);
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
                processor.ORA(0, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                processor.ORA(1, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                processor.ORA(2, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                processor.ORA(3, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                processor.ORA(4, BinaryCalc.BYTE_TYPE.ADDRESS);
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
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.LOGICAL);
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
                processor.ADC(0, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                processor.ADC(1, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                processor.ADC(2, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                processor.ADC(3, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                processor.ADC(4, BinaryCalc.BYTE_TYPE.ADDRESS);
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
                processor.SBC(0, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D1:
                processor.SBC(1, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D2:
                processor.SBC(2, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D3:
                processor.SBC(3, BinaryCalc.BYTE_TYPE.ADDRESS);
                return true;
            case ConsoleKey.D4:
                processor.SBC(4, BinaryCalc.BYTE_TYPE.ADDRESS);
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
                if (keyMod) { return logger.MessagesUp(); }
                return false;
            case ConsoleKey.DownArrow:
                if (keyMod) { return logger.MessagesDown(); }
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
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.STORE);
                return true;
            case ConsoleKey.L:
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.LOAD);
                return true;
            case ConsoleKey.T:
                if (processor.registerIndex == 1) { processor.TXA(); }
                else if (processor.registerIndex == 2) { processor.TYA(); }
                else { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.TRANSFER); }
                return true;
            case ConsoleKey.C:
                if (processor.carryFlag) { processor.CLC(); }
                else { processor.SEC(); }
                return true;
            case ConsoleKey.V:
                if (processor.overflowFlag) { processor.CLV(); return true; }
                return false;
            case ConsoleKey.A:
                if (processor.registerIndex != 0) { processor.ChangeRegister(0); return true; }
                return false;
            case ConsoleKey.X:
                if (processor.registerIndex != 1) { processor.ChangeRegister(1); return true; }
                return false;
            case ConsoleKey.Y:
                if (processor.registerIndex != 2) { processor.ChangeRegister(2); return true; }
                return false;
            case ConsoleKey.LeftArrow:
                if (processor.registerIndex == 0)
                {
                    if (keyMod) { processor.ROL(); }
                    else { processor.ASL(); }
                    return true;
                }
                return false;
            case ConsoleKey.RightArrow:
                if (processor.registerIndex == 0)
                {
                    if (keyMod) { processor.ROR(); }
                    else { processor.LSR(); }
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
                binaryCalc.ChangeState(BinaryCalc.STATE.ASSEMBLY);
                return true;
            case ConsoleKey.P:
                binaryCalc.ChangeState(BinaryCalc.STATE.SETTINGS);
                return true;
            case ConsoleKey.Escape:
                binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.QUIT);
                return true;
        }
        switch (key.KeyChar)
        {
            case '+':
                if (processor.registerIndex == 0) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.ADD); return true; }
                return false;
            case '-':
                if (processor.registerIndex == 0) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.SUBTRACT); return true; }
                return false;
            case '?':
                binaryCalc.ChangeState(BinaryCalc.STATE.HELP);
                return true;
            case '&':
                if (processor.registerIndex == 0) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.LOGICAL); return true; }
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
                return binaryCalc.containerScroll[BinaryCalc.STATE.HELP].Up();
            case ConsoleKey.DownArrow:
                return binaryCalc.containerScroll[BinaryCalc.STATE.HELP].Down();
            case ConsoleKey.Escape:
                binaryCalc.ChangeState(BinaryCalc.STATE.MAIN);
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
                return binaryCalc.containerToggle[BinaryCalc.STATE.SETTINGS].Up();
            case ConsoleKey.DownArrow:
                return binaryCalc.containerToggle[BinaryCalc.STATE.SETTINGS].Down();
            case ConsoleKey.LeftArrow:
                return binaryCalc.containerToggle[BinaryCalc.STATE.SETTINGS].Toggle();
            case ConsoleKey.RightArrow:
                return binaryCalc.containerToggle[BinaryCalc.STATE.SETTINGS].Toggle();
            case ConsoleKey.Escape:
                binaryCalc.ChangeState(BinaryCalc.STATE.MAIN);
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
                return binaryCalc.containerScroll[BinaryCalc.STATE.ASSEMBLY].Up();
            case ConsoleKey.DownArrow:
                return binaryCalc.containerScroll[BinaryCalc.STATE.ASSEMBLY].Down();
            case ConsoleKey.Escape:
                binaryCalc.ChangeState(BinaryCalc.STATE.MAIN);
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
                binaryCalc.running = false;
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
        if (FormatInputLine().Length < BinaryCalc.INPUT_CHAR_LIMIT) { inputString += newChar; return true; }
        return false;
    }

    // Erase the last character in the input line, or exit input mode if there are no characters
    private void InputEraseLastChar()
    {
        if (inputString.Length > 0) { inputString = inputString.Remove(inputString.Length - 1); }
        else { ChangeInputMode(INPUT_MODE.NORMAL); }
    }

    // Check the number from the user input
    private bool InputCheckValueNumerical(BinaryCalc.BYTE_TYPE byteType, out byte bits)
    {
        bits = 0;
        int inputValue = 0;

        switch (byteType)
        {
            case BinaryCalc.BYTE_TYPE.DECIMAL:
                if (inputString.Length > 8) { return false; }
                inputValue = Convert.ToInt32(inputString);
                break;
            case BinaryCalc.BYTE_TYPE.BINARY:
                if (inputString.Length != 8) { return false; }
                inputValue = Convert.ToInt32(inputString, 2);
                break;
            case BinaryCalc.BYTE_TYPE.HEX:
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
            BinaryCalc.BYTE_TYPE byteType = BinaryCalc.BYTE_TYPE.BINARY;

            // Format bits and find byte type according to current INPUT_MODE
            switch (inputMode)
            {
                case INPUT_MODE.NUMERICAL_DEC:
                    byteType = BinaryCalc.BYTE_TYPE.DECIMAL;
                    break;
                case INPUT_MODE.NUMERICAL_BIN:
                    byteType = BinaryCalc.BYTE_TYPE.BINARY;
                    break;
                case INPUT_MODE.NUMERICAL_HEX:
                    byteType = BinaryCalc.BYTE_TYPE.HEX;
                    break;
            }

            // Check that the number from the user is a valid number (0-255)
            if (InputCheckValueNumerical(byteType, out byte bits))
            {
                // Perform correct operation according to current STATE
                switch (binaryCalc.appState)
                {
                    case BinaryCalc.STATE.MAIN:
                        switch (binaryCalc.mainSubState)
                        {
                            case BinaryCalc.MAIN_SUB_STATE.AND:
                                processor.AND(bits, byteType);
                                return true;
                            case BinaryCalc.MAIN_SUB_STATE.EOR:
                                processor.EOR(bits, byteType);
                                return true;
                            case BinaryCalc.MAIN_SUB_STATE.LOAD:
                                if (processor.registerIndex == 0) { processor.LDA(bits, byteType); }
                                else if (processor.registerIndex == 1) { processor.LDX(bits, byteType); }
                                else if (processor.registerIndex == 2) { processor.LDY(bits, byteType); }
                                return true;
                            case BinaryCalc.MAIN_SUB_STATE.ORA:
                                processor.ORA(bits, byteType);
                                return true;
                            case BinaryCalc.MAIN_SUB_STATE.ADD:
                                processor.ADC(bits, byteType);
                                return true;
                            case BinaryCalc.MAIN_SUB_STATE.SUBTRACT:
                                processor.SBC(bits, byteType);
                                return true;
                        }
                        break;
                }
            }

            // Invalid number
            else
            {
                logger.AddMessage("Invalid number! " + "\"" + FormatInputLine() + "\"", error: true);
            }
        }

        // Input was not valid
        return false;
    }

    // Render the bottom input line if in COMMAND or NUMERICAL input mode
    private string FormatInputLine()
    {
        string inputLineFormatted = "";
        switch (binaryCalc.appState)
        {
            case BinaryCalc.STATE.MAIN:
                switch (binaryCalc.mainSubState)
                {
                    case BinaryCalc.MAIN_SUB_STATE.ADD:
                        inputLineFormatted += "ADC ";
                        break;
                    case BinaryCalc.MAIN_SUB_STATE.AND:
                        inputLineFormatted += "AND ";
                        break;
                    case BinaryCalc.MAIN_SUB_STATE.EOR:
                        inputLineFormatted += "EOR ";
                        break;
                    case BinaryCalc.MAIN_SUB_STATE.LOAD:
                        inputLineFormatted += "LD" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " ";
                        break;
                    case BinaryCalc.MAIN_SUB_STATE.ORA:
                        inputLineFormatted += "ORA ";
                        break;
                    case BinaryCalc.MAIN_SUB_STATE.STORE:
                        inputLineFormatted += "ST" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " ";
                        break;
                    case BinaryCalc.MAIN_SUB_STATE.SUBTRACT:
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
    public void RenderInputLine()
    {
        string inputLineFormatted = " :" + FormatInputLine();
        Console.Write(inputLineFormatted);
        Console.SetCursorPosition(inputLineFormatted.Length, BinaryCalc.RENDER_HEIGHT - 1);
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
                if (InputRunNumerical()) { binaryCalc.ChangeState(BinaryCalc.STATE.MAIN); }
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
    public void GetInput()
    {
        // Loop until valid input is given
        bool validKey = false;
        while (!validKey)
        {
            // Wait for and get key input from user
            ConsoleKeyInfo key = Console.ReadKey(true);

            // Check if key modifier was pressed
            bool keyMod = (binaryCalc.settings["modKeyCtrl"].enabled ? key.Modifiers.HasFlag(ConsoleModifiers.Control) : key.Modifiers.HasFlag(ConsoleModifiers.Shift));

            // Call the correct GetInput for the current INPUT_MODE
            switch (inputMode)
            {
                case INPUT_MODE.NUMERICAL_DEC or INPUT_MODE.NUMERICAL_BIN or INPUT_MODE.NUMERICAL_HEX:
                    validKey = GetInputModeNumerical(key, keyMod);
                    break;
                case INPUT_MODE.NORMAL:
                    // Call the correct GetInput function for the current application STATE
                    switch (binaryCalc.appState)
                    {
                        case BinaryCalc.STATE.MAIN:
                            if (validKey = GetInputMainGlobal(key, keyMod)) { break; }
                            switch (binaryCalc.mainSubState)
                            {
                                case BinaryCalc.MAIN_SUB_STATE.QUIT:
                                    validKey = GetInputMainQuit(key, keyMod);
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.DEFAULT:
                                    validKey = GetInputMain(key, keyMod);
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.ADD:
                                    validKey = GetInputMainAdd(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.SUBTRACT:
                                    validKey = GetInputMainSubtract(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.TRANSFER:
                                    if (validKey = GetInputMainTransfer(key, keyMod)) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.STORE:
                                    if (validKey = GetInputMainStore(key, keyMod)) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.LOAD:
                                    validKey = GetInputMainLoad(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.LOGICAL:
                                    validKey = GetInputMainLogical(key, keyMod);
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.AND:
                                    validKey = GetInputMainAnd(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.EOR:
                                    validKey = GetInputMainEor(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT); }
                                    break;
                                case BinaryCalc.MAIN_SUB_STATE.ORA:
                                    validKey = GetInputMainOra(key, keyMod);
                                    if (validKey && key.Key != ConsoleKey.Escape && key.Key != ConsoleKey.D && key.Key != ConsoleKey.B && key.Key != ConsoleKey.H) { binaryCalc.ChangeMainSubState(BinaryCalc.MAIN_SUB_STATE.DEFAULT); }
                                    break;
                            }
                            break;
                        case BinaryCalc.STATE.HELP:
                            validKey = GetInputHelp(key, keyMod);
                            break;
                        case BinaryCalc.STATE.SETTINGS:
                            validKey = GetInputSettings(key, keyMod);
                            break;
                        case BinaryCalc.STATE.ASSEMBLY:
                            validKey = GetInputAssembly(key, keyMod);
                            break;
                        case BinaryCalc.STATE.ERROR:
                            validKey = GetInputError(key, keyMod);
                            break;
                    }
                    break;
            }
        }
    }

    // Constructor
    public Input(BinaryCalc binaryCalc, Processor processor, Logger logger) 
    {
        this.binaryCalc = binaryCalc;
        this.processor = processor;
        this.logger = logger;
    }
}