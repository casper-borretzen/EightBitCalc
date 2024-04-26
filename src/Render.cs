using System.Net.NetworkInformation;
using System.Numerics;

namespace EightBitCalc;

internal class Render
{
    // Class references
    private BinaryCalc binaryCalc;
    private Logger logger;
    private Processor processor;
    private Input input;

    // Set vars
    private bool[,,] digits = new bool[2, BinaryCalc.CHAR_HEIGHT, BinaryCalc.CHAR_WIDTH];
    private string blank = new String(' ', BinaryCalc.RENDER_WIDTH);



    ///////////////////////////////////////////////////////////////////////////////
    // RENDER METHODS:
    ///////////////////////////////////////////////////////////////////////////////

    // Make 0 and 1 digit
    private void SetupDigits()
    {
        // Zero digit
        digits[0, 1, 0] = true;
        digits[0, 2, 0] = true;
        digits[0, 3, 0] = true;
        digits[0, 4, 0] = true;
        digits[0, 5, 0] = true;
        digits[0, 0, 1] = true;
        digits[0, 6, 1] = true;
        digits[0, 0, 2] = true;
        digits[0, 6, 2] = true;
        digits[0, 0, 3] = true;
        digits[0, 6, 3] = true;
        digits[0, 1, 4] = true;
        digits[0, 2, 4] = true;
        digits[0, 3, 4] = true;
        digits[0, 4, 4] = true;
        digits[0, 5, 4] = true;

        // One digit
        digits[1, 0, 2] = true;
        digits[1, 1, 1] = true;
        digits[1, 1, 2] = true;
        digits[1, 2, 2] = true;
        digits[1, 3, 2] = true;
        digits[1, 4, 2] = true;
        digits[1, 5, 2] = true;
        digits[1, 6, 1] = true;
        digits[1, 6, 2] = true;
        digits[1, 6, 3] = true;
    }

    // Render message log
    public void RenderMessages(int num = BinaryCalc.MESSAGES_NUM)
    {
        for (int i = num - 1; i >= 0; i--)
        {
            Console.Write((logger.messages.Count > i + logger.messageIndex ? " " + logger.messages[logger.messages.Count - (i + 1 + logger.messageIndex)] : " -") + Environment.NewLine);
        }
    }

    // Main titlebar
    private void RenderMainTitlebar()
    {
        Console.Write(Environment.NewLine);
        Console.ForegroundColor = BinaryCalc.COLOR_TITLEBAR_MAIN_FG;
        Console.BackgroundColor = BinaryCalc.COLOR_TITLEBAR_MAIN_BG;
        Console.Write((BinaryCalc.MAIN_TITLE.PadLeft((BinaryCalc.RENDER_WIDTH - BinaryCalc.MAIN_TITLE.Length) / 2 + BinaryCalc.MAIN_TITLE.Length, ' ')).PadRight(BinaryCalc.RENDER_WIDTH, ' ') + Environment.NewLine);
        Console.ResetColor();
        Console.Write(Environment.NewLine);
    }

    // Titlebar
    private void RenderTitlebar(string title)
    {
        Console.ForegroundColor = BinaryCalc.COLOR_TITLEBAR_FG;
        Console.BackgroundColor = BinaryCalc.COLOR_TITLEBAR_BG;
        Console.Write((" " + title).PadRight(BinaryCalc.RENDER_WIDTH, ' ') + Environment.NewLine);
        Console.ResetColor();
    }

    // Render status and keybinds for DEFAULT substate
    private void RenderMainSubStateDefault()
    {
        Console.Write(" <A> <X> <Y>       CHANGE ACTIVE REGISTER" + Environment.NewLine);
        Console.Write(" <UP>              " + ((processor.registerIndex == 0) ?
            "ADC #%0000000" + ((!binaryCalc.settings["flagsAutoCarry"].enabled && processor.carryFlag) ? "0" : "1") + "             (N,V,Z,C)" :
            "IN" + BinaryCalc.GetRegisterChar(processor.registerIndex) + ": INCREMENT " + BinaryCalc.GetRegisterChar(processor.registerIndex) + " REGISTER  (N,Z)") +
            Environment.NewLine);
        Console.Write(" <DOWN>            " + ((processor.registerIndex == 0) ?
            "SBC #%0000000" + ((!binaryCalc.settings["flagsAutoCarry"].enabled && !processor.carryFlag) ? "0" : "1") + "             (N,V,Z,C)" :
            "IN" + BinaryCalc.GetRegisterChar(processor.registerIndex) + ": DECREMENT " + BinaryCalc.GetRegisterChar(processor.registerIndex) + " REGISTER  (N,Z)")
            + Environment.NewLine);
        if (processor.registerIndex != 0 && input.inputMode == Input.INPUT_MODE.NORMAL) { Console.ForegroundColor = BinaryCalc.COLOR_INACTIVE_FG; }
        Console.Write(" <LEFT>            ASL: ARITHMETIC SHIFT LEFT (N,Z,C)" + Environment.NewLine);
        Console.Write(" <RIGHT>           LSR: LOGICAL SHIFT RIGHT   (N,Z,C)" + Environment.NewLine);
        Console.Write(" " + (binaryCalc.settings["modKeyCtrl"].enabled ? "<CTRL> + <LEFT> " : "<SHIFT> + <LEFT>") + "  ROL: ROTATE LEFT           (N,Z,C)" + Environment.NewLine);
        Console.Write(" " + (binaryCalc.settings["modKeyCtrl"].enabled ? "<CTRL> + <RIGHT> " : "<SHIFT> + <RIGHT>") + " ROR: ROTATE RIGHT          (N,Z,C)" + Environment.NewLine);
        Console.Write(" <+>               ADC: ADD WITH CARRY        (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <->               SBC: SUBTRACT WITH CARRY   (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <&>               LOGICAL OPERATIONS         (N,Z)" + Environment.NewLine);
        if (processor.registerIndex != 0 && input.inputMode == Input.INPUT_MODE.NORMAL) { Console.ResetColor(); }
        Console.Write(" <S>               " + (processor.registerIndex == 0 ?
            "STA: STORE ACCUMULATOR" :
            "ST" + BinaryCalc.GetRegisterChar(processor.registerIndex) + ": STORE " + BinaryCalc.GetRegisterChar(processor.registerIndex) + " REGISTER")
            + Environment.NewLine);
        Console.Write(" <L>               " + (processor.registerIndex == 0 ?
            "LDA: LOAD ACCUMULATOR      (N,Z)" :
            "LD" + BinaryCalc.GetRegisterChar(processor.registerIndex) + ": LOAD " + BinaryCalc.GetRegisterChar(processor.registerIndex) + " REGISTER       (N,Z)")
            + Environment.NewLine);
        Console.Write(" <T>               " + (processor.registerIndex == 0 ?
            "TRANSFER ACCUMULATOR       (N,Z)" :
            "T" + BinaryCalc.GetRegisterChar(processor.registerIndex) + "A: TRANSFER " + BinaryCalc.GetRegisterChar(processor.registerIndex) + " TO A       (N,Z)")
            + Environment.NewLine);
        Console.Write(" <C>               TOGGLE CARRY FLAG" + Environment.NewLine);
        Console.Write(" <V>               CLEAR OVERFLOW FLAG" + Environment.NewLine);
        Console.Write(" <M>               ASSEMBLY LOG" + Environment.NewLine);
        Console.Write(" <P>               PREFERENCES" + Environment.NewLine);
        Console.Write(" <?>               HELP" + Environment.NewLine);
    }

    // Render status and keybinds for ADD substate
    private string RenderMainSubStateAdd()
    {
        Console.Write(" <D>               ENTER DECIMAL VALUE        (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <B>               ENTER BINARY VALUE         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <H>               ENTER HEXADECIMAL VALUE    (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <0>               ADC $00 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <1>               ADC $01 (" + BinaryCalc.ByteToString(binaryCalc.memory[1]) + ")         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <2>               ADC $02 (" + BinaryCalc.ByteToString(binaryCalc.memory[2]) + ")         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <3>               ADC $03 (" + BinaryCalc.ByteToString(binaryCalc.memory[3]) + ")         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <4>               ADC $04 (" + BinaryCalc.ByteToString(binaryCalc.memory[4]) + ")         (N,V,Z,C)" + Environment.NewLine);
        return "ADC: ADD WITH CARRY";
    }

    // Render status and keybinds for SUBTRACT substate
    private string RenderMainSubStateSubtract()
    {
        Console.Write(" <D>               ENTER DECIMAL VALUE        (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <B>               ENTER BINARY VALUE         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <H>               ENTER HEXADECIMAL VALUE    (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <0>               SBC $00 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <1>               SBC $01 (" + BinaryCalc.ByteToString(binaryCalc.memory[1]) + ")         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <2>               SBC $02 (" + BinaryCalc.ByteToString(binaryCalc.memory[2]) + ")         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <3>               SBC $03 (" + BinaryCalc.ByteToString(binaryCalc.memory[3]) + ")         (N,V,Z,C)" + Environment.NewLine);
        Console.Write(" <4>               SBC $04 (" + BinaryCalc.ByteToString(binaryCalc.memory[4]) + ")         (N,V,Z,C)" + Environment.NewLine);
        return "SBC: SUBTRACT WITH CARRY";
    }

    // Render status and keybinds for STORE substate
    private string RenderMainSubStateStore()
    {
        Console.Write(" <0>               ST" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $00 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")" + Environment.NewLine);
        Console.Write(" <1>               ST" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $01 (" + BinaryCalc.ByteToString(binaryCalc.memory[1]) + ")" + Environment.NewLine);
        Console.Write(" <2>               ST" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $02 (" + BinaryCalc.ByteToString(binaryCalc.memory[2]) + ")" + Environment.NewLine);
        Console.Write(" <3>               ST" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $03 (" + BinaryCalc.ByteToString(binaryCalc.memory[3]) + ")" + Environment.NewLine);
        Console.Write(" <4>               ST" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $04 (" + BinaryCalc.ByteToString(binaryCalc.memory[4]) + ")" + Environment.NewLine);
        return (processor.registerIndex == 0 ? "STA: STORE ACCUMULATOR" : "ST" + BinaryCalc.GetRegisterChar(processor.registerIndex) + ": LOAD " + BinaryCalc.GetRegisterChar(processor.registerIndex) + " REGISTER");
    }

    // Render status and keybinds for LOAD substate
    private string RenderMainSubStateLoad()
    {
        Console.Write(" <D>               ENTER DECIMAL VALUE        (N,Z)" + Environment.NewLine);
        Console.Write(" <B>               ENTER BINARY VALUE         (N,Z)" + Environment.NewLine);
        Console.Write(" <H>               ENTER HEXADECIMAL VALUE    (N,Z)" + Environment.NewLine);
        Console.Write(" <0>               LD" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $00 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <1>               LD" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $01 (" + BinaryCalc.ByteToString(binaryCalc.memory[1]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <2>               LD" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $02 (" + BinaryCalc.ByteToString(binaryCalc.memory[2]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <4>               LD" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $04 (" + BinaryCalc.ByteToString(binaryCalc.memory[4]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <3>               LD" + BinaryCalc.GetRegisterChar(processor.registerIndex) + " $03 (" + BinaryCalc.ByteToString(binaryCalc.memory[3]) + ")         (N,Z)" + Environment.NewLine);
        return (processor.registerIndex == 0 ? "LDA: LOAD ACCUMULATOR" : "LD" + BinaryCalc.GetRegisterChar(processor.registerIndex) + ": LOAD " + BinaryCalc.GetRegisterChar(processor.registerIndex) + " REGISTER");
    }

    // Render status and keybinds for LOGICAL substate
    private string RenderMainSubStateLogical()
    {
        Console.Write(" <A>               AND: LOGICAL AND                (N,Z)" + Environment.NewLine);
        Console.Write(" <E>               EOR: EXCLUSIVE OR               (N,Z)" + Environment.NewLine);
        Console.Write(" <O>               ORA: LOGICAL INCLUSIVE OR       (N,Z)" + Environment.NewLine);
        return "LOGICAL OPERATIONS";
    }

    // Render status and keybinds for AND substate
    private string RenderMainSubStateAnd()
    {
        Console.Write(" <D>               ENTER DECIMAL VALUE        (N,Z)" + Environment.NewLine);
        Console.Write(" <B>               ENTER BINARY VALUE         (N,Z)" + Environment.NewLine);
        Console.Write(" <H>               ENTER HEXADECIMAL VALUE    (N,Z)" + Environment.NewLine);
        Console.Write(" <0>               AND $00 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <1>               AND $01 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <2>               AND $02 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <3>               AND $03 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <4>               AND $04 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        return "AND: LOGICAL AND";
    }

    // Render status and keybinds for EOR substate
    private string RenderMainSubStateEor()
    {
        Console.Write(" <D>               ENTER DECIMAL VALUE" + Environment.NewLine);
        Console.Write(" <B>               ENTER BINARY VALUE" + Environment.NewLine);
        Console.Write(" <H>               ENTER HEXADECIMAL VALUE" + Environment.NewLine);
        Console.Write(" <0>               EOR $00 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <1>               EOR $01 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <2>               EOR $02 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <3>               EOR $03 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <4>               EOR $04 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        return "EOR: EXCLUSIVE OR";
    }

    // Render status and keybinds for ORA substate
    private string RenderMainSubStateOra()
    {
        Console.Write(" <D>               ENTER DECIMAL VALUE" + Environment.NewLine);
        Console.Write(" <B>               ENTER BINARY VALUE" + Environment.NewLine);
        Console.Write(" <H>               ENTER HEXADECIMAL VALUE" + Environment.NewLine);
        Console.Write(" <0>               ORA $00 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <1>               ORA $01 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <2>               ORA $02 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <3>               ORA $03 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        Console.Write(" <4>               ORA $04 (" + BinaryCalc.ByteToString(binaryCalc.memory[0]) + ")         (N,Z)" + Environment.NewLine);
        return "ORA: LOGICAL INCLUSIVE OR";
    }

    // Render status and keybinds for TRANSFER substate
    private string RenderMainSubStateTransfer()
    {
        if (processor.registerIndex == 0)
        {
            Console.Write(" <X>               TAX                    (N,Z)" + Environment.NewLine);
            Console.Write(" <Y>               TAY                    (N,Z)" + Environment.NewLine);
        }
        else if (processor.registerIndex == 1)
        {
            Console.Write(" <A>               TXA                    (N,Z)" + Environment.NewLine);
        }
        else if (processor.registerIndex == 2)
        {
            Console.Write(" <A>               TYA                    (N,Z)" + Environment.NewLine);
        }
        return "TRANSFER " + (processor.registerIndex == 0 ? "ACCUMULATOR" : BinaryCalc.GetRegisterChar(processor.registerIndex) + " REGISTER");
    }

    // Render status and keybinds for all MAIN substates
    private void RenderMainSubState()
    {
        string footerTitle = "";

        // Grey out text if in COMMAND or NUMERICAL input mode
        if (input.inputMode != Input.INPUT_MODE.NORMAL) { Console.ForegroundColor = BinaryCalc.COLOR_INACTIVE_FG; }

        // Check substate and call appropriate render method
        switch (binaryCalc.mainSubState)
        {
            case BinaryCalc.MAIN_SUB_STATE.DEFAULT or BinaryCalc.MAIN_SUB_STATE.QUIT:
                RenderMainSubStateDefault();
                break;
            case BinaryCalc.MAIN_SUB_STATE.ADD:
                footerTitle = RenderMainSubStateAdd();
                break;
            case BinaryCalc.MAIN_SUB_STATE.SUBTRACT:
                footerTitle = RenderMainSubStateSubtract();
                break;
            case BinaryCalc.MAIN_SUB_STATE.STORE:
                footerTitle = RenderMainSubStateStore();
                break;
            case BinaryCalc.MAIN_SUB_STATE.LOAD:
                footerTitle = RenderMainSubStateLoad();
                break;
            case BinaryCalc.MAIN_SUB_STATE.TRANSFER:
                footerTitle = RenderMainSubStateTransfer();
                break;
            case BinaryCalc.MAIN_SUB_STATE.LOGICAL:
                footerTitle = RenderMainSubStateLogical();
                break;
            case BinaryCalc.MAIN_SUB_STATE.AND:
                footerTitle = RenderMainSubStateAnd();
                break;
            case BinaryCalc.MAIN_SUB_STATE.EOR:
                footerTitle = RenderMainSubStateEor();
                break;
            case BinaryCalc.MAIN_SUB_STATE.ORA:
                footerTitle = RenderMainSubStateOra();
                break;
        }

        // Set text color back to normal
        if (input.inputMode != Input.INPUT_MODE.NORMAL) { Console.ResetColor(); }

        // Add empty lines to fill BinaryCalc.RENDER_HEIGHT
        for (int i = Console.CursorTop; i < BinaryCalc.RENDER_HEIGHT - 3; i++)
        {
            Console.Write(Environment.NewLine);
        }

        // Footer
        Console.Write(BinaryCalc.seperator[0] + Environment.NewLine);
        if (binaryCalc.mainSubState == BinaryCalc.MAIN_SUB_STATE.DEFAULT)
        {
            Console.Write(" PRESS <ESC> TO QUIT" + Environment.NewLine);
        }
        else if (binaryCalc.mainSubState == BinaryCalc.MAIN_SUB_STATE.QUIT)
        {
            Console.Write(" ");
            Console.ForegroundColor = BinaryCalc.COLOR_QUIT_PROMPT_FG;
            Console.BackgroundColor = BinaryCalc.COLOR_QUIT_PROMPT_BG;
            Console.Write(" ARE YOU SURE YOU WANT TO QUIT? Y/N " + Environment.NewLine);
            Console.ResetColor();
        }
        else
        {
            Console.Write(" ");
            Console.ForegroundColor = BinaryCalc.COLOR_SUB_STATE_FG;
            Console.BackgroundColor = BinaryCalc.COLOR_SUB_STATE_BG;
            Console.Write(" " + footerTitle + " ");
            Console.ResetColor();
            Console.Write(" PRESS <ESC> TO CANCEL".PadLeft(BinaryCalc.RENDER_WIDTH - 4 - footerTitle.Length, ' ') + Environment.NewLine);
        }

        if (input.inputMode != Input.INPUT_MODE.NORMAL)
        {
            input.RenderInputLine();
        }

    }

    // Render the MAIN screen
    private void RenderMain()
    {
        // Main title
        RenderMainTitlebar();

        // Register selector
        Console.Write(" ");
        for (byte i = 0; i < BinaryCalc.REGISTER_NUM; i++)
        {
            if (i == processor.registerIndex)
            {
                Console.ForegroundColor = BinaryCalc.COLOR_REGISTER_ACTIVE_FG;
                Console.BackgroundColor = BinaryCalc.COLOR_REGISTER_ACTIVE_BG;
            }
            else
            {
                Console.ForegroundColor = BinaryCalc.COLOR_REGISTER_INACTIVE_FG;
                Console.BackgroundColor = BinaryCalc.COLOR_REGISTER_INACTIVE_BG;
            }
            Console.Write("  " + BinaryCalc.GetRegisterChar(i) + "  ");
            Console.ResetColor();
            Console.Write("  ");
        }

        // Memory status
        for (byte i = 0; i < BinaryCalc.MEMORY_NUM; i++)
        {
            Console.Write(" $" + (i.ToString()).PadLeft(2, '0') + ":  ");
        }

        // End storage line
        Console.Write(Environment.NewLine);

        // Register selector (hex value)
        Console.Write(" ");
        for (byte i = 0; i < BinaryCalc.REGISTER_NUM; i++)
        {
            if (i != processor.registerIndex) { Console.ForegroundColor = BinaryCalc.COLOR_REGISTER_INACTIVE_BG; }
            Console.Write(" 0x" + BinaryCalc.HexToString(processor.registers[i]) + "  ");
            if (i != processor.registerIndex) { Console.ResetColor(); }
        }

        // Memory status (hex value)
        for (byte i = 0; i < BinaryCalc.MEMORY_NUM; i++)
        {
            Console.Write(" 0x" + BinaryCalc.HexToString(binaryCalc.memory[i]) + "  ");
        }

        // End storage line (hex value)
        Console.Write(Environment.NewLine);

        // Vertical spacer
        Console.Write(Environment.NewLine);

        // Binary value titlebar
        RenderTitlebar("BINARY VALUE:");

        // Vertical spacer
        Console.Write(Environment.NewLine);

        // Iterate through the x and y coords of the "pixels" and display the digits from the selected register
        string displayLine = BinaryCalc.ByteToString(processor.registers[processor.registerIndex]);
        for (int y = 0; y < BinaryCalc.CHAR_HEIGHT; y++)
        {
            Console.Write(" ");
            for (int x = 0; x < BinaryCalc.CHAR_WIDTH * displayLine.Length; x++)
            {
                // Find the current char from the input string
                char currentChar = displayLine[x / BinaryCalc.CHAR_WIDTH];

                // Convert the char to a index number for the DIGITS array
                int currentDigit = 0;
                if (Char.IsNumber(currentChar)) { currentDigit = (int)Char.GetNumericValue(currentChar); }

                // Check if the current bit was changed by last operation
                byte bitToCheck = (byte)(0b10000000 >> (byte)(x / BinaryCalc.CHAR_WIDTH));
                bool bitChanged = (processor.registers[processor.registerIndex] & bitToCheck) != (processor.registersPrev[processor.registerIndex] & bitToCheck);

                // Render the current "pixel"
                if (bitChanged && binaryCalc.settings["uiHlChangedBit"].enabled) { Console.ForegroundColor = BinaryCalc.COLOR_BIT_HL_FG; }
                Console.Write(digits[currentDigit, y, x % BinaryCalc.CHAR_WIDTH] ? BinaryCalc.FILLED : BinaryCalc.EMPTY);
                if (bitChanged && binaryCalc.settings["uiHlChangedBit"].enabled) { Console.ResetColor(); }
            }

            // End the current line
            Console.Write(Environment.NewLine);
        }

        // Vertical spacer
        Console.Write(Environment.NewLine);

        // Decimal values titlebar
        RenderTitlebar("DECIMAL VALUE:");

        // Decimal values
        Console.Write(" UNSIGNED VALUE:     " + (processor.registers[processor.registerIndex].ToString()).PadLeft(3, ' ') +
            "        SIGNED VALUE:      " + (((int)((processor.registers[processor.registerIndex] & 0b01111111) - (processor.registers[processor.registerIndex] & 0b10000000))).ToString()).PadLeft(4, ' ')
            + Environment.NewLine);

        // Flags titlebar
        RenderTitlebar("FLAGS:");

        // Carry flag
        Console.Write(" (C) CARRY FLAG:       ");
        if (processor.carryFlag) { Console.ForegroundColor = BinaryCalc.COLOR_FLAG_SET_FG; Console.Write("1"); }
        else { Console.ForegroundColor = BinaryCalc.COLOR_FLAG_UNSET_FG; Console.Write("0"); }
        Console.ResetColor();

        // Whitespace between flags
        Console.Write("       ");

        // Zero flag
        Console.Write(" (Z) ZERO FLAG:        ");
        if (processor.zeroFlag) { Console.ForegroundColor = BinaryCalc.COLOR_FLAG_SET_FG; Console.Write("1"); }
        else { Console.ForegroundColor = BinaryCalc.COLOR_FLAG_UNSET_FG; Console.Write("0"); }
        Console.ResetColor();

        // End the current line
        Console.Write(Environment.NewLine);

        // Negative flag
        Console.Write(" (N) NEGATIVE FLAG:    ");
        if (processor.negativeFlag) { Console.ForegroundColor = BinaryCalc.COLOR_FLAG_SET_FG; Console.Write("1"); }
        else { Console.ForegroundColor = BinaryCalc.COLOR_FLAG_UNSET_FG; Console.Write("0"); }
        Console.ResetColor();

        // Whitespace between flags
        Console.Write("       ");

        // Overflow flag
        Console.Write(" (V) OVERFLOW FLAG:    ");
        if (processor.overflowFlag) { Console.ForegroundColor = BinaryCalc.COLOR_FLAG_SET_FG; Console.Write("1"); }
        else { Console.ForegroundColor = BinaryCalc.COLOR_FLAG_UNSET_FG; Console.Write("0"); }
        Console.ResetColor();

        // End the current line
        Console.Write(Environment.NewLine);

        // Message log titlebar
        int messageIndexMax = Math.Max(logger.messages.Count - BinaryCalc.MESSAGES_NUM, 0);
        int messageIndexNumDigits = Math.Max(messageIndexMax.ToString().Length, 2);
        RenderTitlebar("MESSAGE LOG:" +
            ((binaryCalc.settings["modKeyCtrl"].enabled ? "<CTRL> + <UP> / <DOWN> (" : "<SHIFT> + <UP> / <DOWN> (") +
            logger.messageIndex.ToString().PadLeft(messageIndexNumDigits, '0') +
            "/" +
            messageIndexMax.ToString().PadLeft(messageIndexNumDigits, '0') +
            ")").PadLeft(BinaryCalc.RENDER_WIDTH - 14, ' '));

        // Render message log
        RenderMessages();

        // Keybinds titlebar
        RenderTitlebar("KEY:              ACTION:                    FLAGS:");

        // Render status, available keybinds and footer for the current substate
        RenderMainSubState();
    }

    // Render the HELP screen
    private void RenderHelp()
    {

        // Main title
        RenderMainTitlebar();

        // Render the HELP container
        binaryCalc.containerScroll[BinaryCalc.STATE.HELP].Render();
    }

    // Render the SETTINGS screen
    private void RenderSettings()
    {

        // Main title
        RenderMainTitlebar();

        // Render the SETTINGS container
        binaryCalc.containerToggle[BinaryCalc.STATE.SETTINGS].Render();
    }

    // Render the ASSEMBLY screen
    private void RenderAssembly()
    {

        // Main title
        RenderMainTitlebar();

        // Render the ASSEMBLY container
        binaryCalc.containerScroll[BinaryCalc.STATE.ASSEMBLY].Render();
    }

    // Render the ERROR screen
    private void RenderError()
    {
        // Clear the console
        Console.Clear();

        // Show error message
        Console.Write("ERROR!" + Environment.NewLine);

        // Show error message if window size is too small
        if (binaryCalc.appError["windowSize"])
        {
            Console.Write("WINDOW SIZE IS TOO SMALL!" + Environment.NewLine);
            Console.Write("PLEASE RESIZE THE WINDOW" + Environment.NewLine);
            Console.Write("AND PRESS <ENTER>" + Environment.NewLine);
            Console.Write("OR PRESS <ESC> TO QUIT" + Environment.NewLine);
        }
    }

    // Render the result in console
    public void RenderConsole()
    {
        // Clear the console
        Console.SetCursorPosition(0, 0);
        for (int i = 0; i < BinaryCalc.RENDER_HEIGHT; i++)
        {
            Console.Write(blank + Environment.NewLine);
        }
        Console.SetCursorPosition(0, 0);

        // Call the correct Render function for the current application STATE
        switch (binaryCalc.appState)
        {
            case BinaryCalc.STATE.MAIN:
                RenderMain();
                break;
            case BinaryCalc.STATE.HELP:
                RenderHelp();
                break;
            case BinaryCalc.STATE.SETTINGS:
                RenderSettings();
                break;
            case BinaryCalc.STATE.ASSEMBLY:
                RenderAssembly();
                break;
            case BinaryCalc.STATE.ERROR:
                RenderError();
                break;
        }
    }

    // Constructor
    public Render(BinaryCalc binaryCalc, Logger logger, Processor processor, Input input) 
    {
        SetupDigits();
        this.binaryCalc = binaryCalc;
        this.logger = logger;
        this.processor = processor;
        this.input = input;
    }
}
