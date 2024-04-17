// TODO:
// Add and display tracking of execution time
// Implement logging of results and add display of log
// Limit input to valid input
// Add support for negative numbers
// Add support for multiple operations of same type at the same time
// Add toggle: Use built-in or bit shift multiplication / division
// Add toggle: Float/Int/16.16 fixed-point mode
// Limit results: Round results to fit within max chars limitation
// Limit results: Limit large numbers in result within max chars limitation
// Add support for parenthesis
// Add support for multiple operations of different types at the same time

class Calc
{
    // Set global constants
    private enum DIGIT 
    {
        DOT = 10,
        PARENTHESIS_OPEN,
        PARENTHESIS_CLOSE,
        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE,
        E,
        R
    }
    private const char EMPTY = ' ';
    private const char FILLED = '#';
    private const int CHAR_SPACING = 2;
    private const int CHAR_HEIGHT = 7;
    private const int CHAR_WIDTH = 5 + CHAR_SPACING;
    private const int CHAR_LIMIT = 16;
    private bool[,,] DIGITS = new bool[19,CHAR_HEIGHT,CHAR_WIDTH];
    private char[] VALID_CHARS = {
        '0',
        '1',
        '2',
        '3',
        '4',
        '5',
        '6',
        '7',
        '8',
        '9',
        '.',
        //'(',
        //')',
        '+',
        '-',
        '*',
        '/'
    };

    // Set global variables
    private bool running = true;
    private bool error = false;
    private string errorMessage = "";
    private List<string> messageLog = new List<string>();
    private string inputExpressions = "";
    private string[] display = new string[CHAR_HEIGHT];
    private string seperatorLine = "";
    private int countInput = 0;
    private long ticksInit = 0;
    private long ticksStart = 0;
    private long ticksEnd = 0;

    // Initialize values
    private void Init() 
    {
        // Set start time
        ticksInit = DateTime.Now.Ticks;

        // Format the message string
        messageLog.Add(" WELCOME TO CALCULATOR!");

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
        
        // Two digit
        DIGITS[2,1,0] = true;
        DIGITS[2,0,1] = true;
        DIGITS[2,0,2] = true;
        DIGITS[2,0,3] = true;
        DIGITS[2,1,4] = true;
        DIGITS[2,2,4] = true;
        DIGITS[2,3,3] = true;
        DIGITS[2,4,2] = true;
        DIGITS[2,5,1] = true;
        DIGITS[2,6,0] = true;
        DIGITS[2,6,1] = true;
        DIGITS[2,6,2] = true;
        DIGITS[2,6,3] = true;
        DIGITS[2,6,4] = true;
        
        // Three digit
        DIGITS[3,1,0] = true;
        DIGITS[3,0,1] = true;
        DIGITS[3,0,2] = true;
        DIGITS[3,0,3] = true;
        DIGITS[3,1,4] = true;
        DIGITS[3,2,4] = true;
        DIGITS[3,3,3] = true;
        DIGITS[3,4,4] = true;
        DIGITS[3,5,4] = true;
        DIGITS[3,6,3] = true;
        DIGITS[3,6,2] = true;
        DIGITS[3,6,1] = true;
        DIGITS[3,5,0] = true;

        // Four digit
        DIGITS[4,1,3] = true;
        DIGITS[4,2,2] = true;
        DIGITS[4,3,1] = true;
        DIGITS[4,4,0] = true;
        DIGITS[4,4,0] = true;
        DIGITS[4,4,1] = true;
        DIGITS[4,4,2] = true;
        DIGITS[4,4,3] = true;
        DIGITS[4,4,4] = true;
        DIGITS[4,3,4] = true;
        DIGITS[4,2,4] = true;
        DIGITS[4,1,4] = true;
        DIGITS[4,0,4] = true;
        DIGITS[4,5,4] = true;
        DIGITS[4,6,4] = true;
        
        // Five digit
        DIGITS[5,0,4] = true;
        DIGITS[5,0,3] = true;
        DIGITS[5,0,2] = true;
        DIGITS[5,0,1] = true;
        DIGITS[5,0,0] = true;
        DIGITS[5,1,0] = true;
        DIGITS[5,2,0] = true;
        DIGITS[5,3,1] = true;
        DIGITS[5,3,2] = true;
        DIGITS[5,3,3] = true;
        DIGITS[5,4,4] = true;
        DIGITS[5,5,4] = true;
        DIGITS[5,6,3] = true;
        DIGITS[5,6,2] = true;
        DIGITS[5,6,1] = true;
        DIGITS[5,5,0] = true;

        // Six digit
        DIGITS[6,0,1] = true;
        DIGITS[6,0,2] = true;
        DIGITS[6,1,0] = true;
        DIGITS[6,2,0] = true;
        DIGITS[6,3,0] = true;
        DIGITS[6,3,1] = true;
        DIGITS[6,3,2] = true;
        DIGITS[6,3,3] = true;
        DIGITS[6,4,4] = true;
        DIGITS[6,5,4] = true;
        DIGITS[6,6,3] = true;
        DIGITS[6,6,2] = true;
        DIGITS[6,6,1] = true;
        DIGITS[6,5,0] = true;
        DIGITS[6,4,0] = true;

        // Seven digit
        DIGITS[7,0,0] = true;
        DIGITS[7,0,1] = true;
        DIGITS[7,0,2] = true;
        DIGITS[7,0,3] = true;
        DIGITS[7,0,4] = true;
        DIGITS[7,6,0] = true;
        DIGITS[7,5,1] = true;
        DIGITS[7,4,2] = true;
        DIGITS[7,3,3] = true;
        DIGITS[7,2,4] = true;
        DIGITS[7,1,4] = true;

        // Eight digit
        DIGITS[8,1,0] = true;
        DIGITS[8,2,0] = true;
        DIGITS[8,4,0] = true;
        DIGITS[8,5,0] = true;
        DIGITS[8,0,1] = true;
        DIGITS[8,3,1] = true;
        DIGITS[8,6,1] = true;
        DIGITS[8,0,2] = true;
        DIGITS[8,3,2] = true;
        DIGITS[8,6,2] = true;
        DIGITS[8,0,3] = true;
        DIGITS[8,3,3] = true;
        DIGITS[8,6,3] = true;
        DIGITS[8,1,4] = true;
        DIGITS[8,2,4] = true;
        DIGITS[8,4,4] = true;
        DIGITS[8,5,4] = true;

        // Nine digit
        DIGITS[9,1,0] = true;
        DIGITS[9,2,0] = true;
        DIGITS[9,0,1] = true;
        DIGITS[9,3,1] = true;
        DIGITS[9,6,1] = true;
        DIGITS[9,0,2] = true;
        DIGITS[9,3,2] = true;
        DIGITS[9,6,2] = true;
        DIGITS[9,0,3] = true;
        DIGITS[9,3,3] = true;
        DIGITS[9,6,3] = true;
        DIGITS[9,1,4] = true;
        DIGITS[9,2,4] = true;
        DIGITS[9,3,4] = true;
        DIGITS[9,4,4] = true;
        DIGITS[9,5,4] = true;

        // Dot sign
        DIGITS[(int)DIGIT.DOT,6,2] = true;

        // Open parenthesis sign
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,0,2] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,1,1] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,2,0] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,3,0] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,4,0] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,5,1] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,6,2] = true;

        // Close parenthesis sign
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,0,2] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,1,3] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,2,4] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,3,4] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,4,4] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,5,3] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,6,2] = true;
        
        // Plus sign
        DIGITS[(int)DIGIT.PLUS,1,2] = true;
        DIGITS[(int)DIGIT.PLUS,2,2] = true;
        DIGITS[(int)DIGIT.PLUS,3,0] = true;
        DIGITS[(int)DIGIT.PLUS,3,1] = true;
        DIGITS[(int)DIGIT.PLUS,3,2] = true;
        DIGITS[(int)DIGIT.PLUS,3,3] = true;
        DIGITS[(int)DIGIT.PLUS,3,4] = true;
        DIGITS[(int)DIGIT.PLUS,4,2] = true;
        DIGITS[(int)DIGIT.PLUS,5,2] = true;
        
        // Minus sign
        DIGITS[(int)DIGIT.MINUS,3,0] = true;
        DIGITS[(int)DIGIT.MINUS,3,1] = true;
        DIGITS[(int)DIGIT.MINUS,3,2] = true;
        DIGITS[(int)DIGIT.MINUS,3,3] = true;
        DIGITS[(int)DIGIT.MINUS,3,4] = true;
        
        // Multiply sign
        DIGITS[(int)DIGIT.MULTIPLY,1,0] = true;
        DIGITS[(int)DIGIT.MULTIPLY,2,1] = true;
        DIGITS[(int)DIGIT.MULTIPLY,1,4] = true;
        DIGITS[(int)DIGIT.MULTIPLY,2,3] = true;
        DIGITS[(int)DIGIT.MULTIPLY,3,0] = true;
        DIGITS[(int)DIGIT.MULTIPLY,3,1] = true;
        DIGITS[(int)DIGIT.MULTIPLY,3,2] = true;
        DIGITS[(int)DIGIT.MULTIPLY,3,3] = true;
        DIGITS[(int)DIGIT.MULTIPLY,3,4] = true;
        DIGITS[(int)DIGIT.MULTIPLY,4,1] = true;
        DIGITS[(int)DIGIT.MULTIPLY,5,0] = true;
        DIGITS[(int)DIGIT.MULTIPLY,4,3] = true;
        DIGITS[(int)DIGIT.MULTIPLY,5,4] = true;
        
        // Divide sign
        DIGITS[(int)DIGIT.DIVIDE,5,0] = true;
        DIGITS[(int)DIGIT.DIVIDE,4,1] = true;
        DIGITS[(int)DIGIT.DIVIDE,3,2] = true;
        DIGITS[(int)DIGIT.DIVIDE,2,3] = true;
        DIGITS[(int)DIGIT.DIVIDE,1,4] = true;
        
        // Letter E
        DIGITS[(int)DIGIT.E,0,0] = true;
        DIGITS[(int)DIGIT.E,0,1] = true;
        DIGITS[(int)DIGIT.E,0,2] = true;
        DIGITS[(int)DIGIT.E,0,3] = true;
        DIGITS[(int)DIGIT.E,0,4] = true;
        DIGITS[(int)DIGIT.E,1,0] = true;
        DIGITS[(int)DIGIT.E,2,0] = true;
        DIGITS[(int)DIGIT.E,3,0] = true;
        DIGITS[(int)DIGIT.E,3,1] = true;
        DIGITS[(int)DIGIT.E,3,2] = true;
        DIGITS[(int)DIGIT.E,4,0] = true;
        DIGITS[(int)DIGIT.E,5,0] = true;
        DIGITS[(int)DIGIT.E,6,0] = true;
        DIGITS[(int)DIGIT.E,6,1] = true;
        DIGITS[(int)DIGIT.E,6,2] = true;
        DIGITS[(int)DIGIT.E,6,3] = true;
        DIGITS[(int)DIGIT.E,6,4] = true;
        
        // Letter R
        DIGITS[(int)DIGIT.R,0,0] = true;
        DIGITS[(int)DIGIT.R,0,1] = true;
        DIGITS[(int)DIGIT.R,0,2] = true;
        DIGITS[(int)DIGIT.R,0,3] = true;
        DIGITS[(int)DIGIT.R,1,4] = true;
        DIGITS[(int)DIGIT.R,2,4] = true;
        DIGITS[(int)DIGIT.R,3,3] = true;
        DIGITS[(int)DIGIT.R,3,2] = true;
        DIGITS[(int)DIGIT.R,3,1] = true;
        DIGITS[(int)DIGIT.R,4,2] = true;
        DIGITS[(int)DIGIT.R,5,3] = true;
        DIGITS[(int)DIGIT.R,6,4] = true;
        DIGITS[(int)DIGIT.R,1,0] = true;
        DIGITS[(int)DIGIT.R,2,0] = true;
        DIGITS[(int)DIGIT.R,3,0] = true;
        DIGITS[(int)DIGIT.R,4,0] = true;
        DIGITS[(int)DIGIT.R,5,0] = true;
        DIGITS[(int)DIGIT.R,6,0] = true;
    }
    
    // Set error
    private void SetError(string message = "Something not good:(")
    {
        error = true;
        messageLog.Add("ERROR! " + message.ToUpper());
    }

    // Clear the error
    private void ClearError()
    {
        error = false;
    }

    // Try to add new character to the input string
    private void AddNewChar(char input)
    {
        // Check within character limit
        if (inputExpressions.Length < CHAR_LIMIT) {
            ClearError();
            inputExpressions += input;
            countInput++;
        }
    }

    // Try to delete the last character in the input string
    private void DeleteLastChar()
    {
        // Check if there is content in string
        if (inputExpressions.Length > 0)
        {
            inputExpressions = inputExpressions.Remove(inputExpressions.Length - 1);
        }
    }

    // Process expression from user and display result
    private void Calculate()
    {
        // Check if there is content in the string
        if (inputExpressions.Length > 0)
        {

            // Set ticks for start of execution
            ticksStart = DateTime.Now.Ticks;

            // Iterate through expressions character by character and check content
            int numMul = 0;
            int numDiv = 0;
            int numAdd = 0;
            int numSub = 0;
            int index = 0;
            int subIndex = 0;
            char prevC = ' ';
            foreach (char c in inputExpressions)
            {
                bool operatorFound = false;

                // Count number of operators
                if (c == '*') { numMul++; operatorFound = true; }
                if (c == '/') { numDiv++; operatorFound = true; }
                if (c == '+') { numAdd++; operatorFound = true; }
                if (c == '-') { numSub++; operatorFound = true; }

                // Error checking
                if (c == '.' || operatorFound == true)
                {
                    if (subIndex == 0) 
                    {
                        SetError("Operator or dot can't be first char.");
                    }
                    else if (index == inputExpressions.Length || prevC == '.') 
                    {
                        SetError("Operator or dot can't be last sign.");
                    }
                }

                // Zero subIndex after each operator
                if (operatorFound)
                {
                    subIndex = 0;
                }
                else { subIndex++; }
                
                prevC = c;
                index++;

            }

            int numOperators = numMul + numDiv + numAdd + numSub;

            // Check if more than one operator was found
            if (!error && numOperators > 1) 
            { 
                SetError("Only 1 operator supported per calculation."); 
            }

            // Start processing operations in expressions if there are 
            // no errors and at least one operator was found
            if (!error && numOperators > 0)
            {
                double result = 0;

                // Process multiplication operations
                if (numMul > 0)
                {
                    string[] inputSplit = inputExpressions.Split('*');
                    result = Convert.ToDouble(inputSplit[0]);
                    foreach (string splitSegment in inputSplit.Skip(1))
                    {
                        result *= Convert.ToDouble(splitSegment);
                    }
                }

                // Process division operations
                if (numDiv > 0)
                {
                    string[] inputSplit = inputExpressions.Split('/');
                    result = Convert.ToDouble(inputSplit[0]);
                    foreach (string splitSegment in inputSplit.Skip(1))
                    {
                        double foundNum = Convert.ToDouble(splitSegment);
                        if (foundNum == 0) { SetError("Division by zero."); }
                        else { result /= foundNum; }
                    }
                }

                // Process addition operations
                if (numAdd > 0)
                {
                    string[] inputSplit = inputExpressions.Split('+');
                    result = Convert.ToDouble(inputSplit[0]);
                    foreach (string splitSegment in inputSplit.Skip(1))
                    {
                        result += Convert.ToDouble(splitSegment);
                    }
                }

                // Process subtraction operations
                if (numSub > 0)
                {
                    string[] inputSplit = inputExpressions.Split('-');
                    result = Convert.ToDouble(inputSplit[0]);
                    foreach (string splitSegment in inputSplit.Skip(1))
                    {
                        result -= Convert.ToDouble(splitSegment);
                    }
                }

                // Set the result as input (clamped to over 0)
                inputExpressions = result > 0 ? result.ToString() : "0";
            }

            // Set the result to empty on error
            if (error) { inputExpressions = ""; }
            
            // Set ticks for end of execution and check execution time
            ticksEnd = DateTime.Now.Ticks;
            long timeToExecute = ticksEnd - ticksStart;
            if (!error && numOperators > 0) 
            {
                messageLog.Add(" EXPRESSION(S) EXECUTED IN " + timeToExecute.ToString() + " TICKS.");
            }
        }
    }

    // Get input from user
    private void GetInput() 
    {
        ConsoleKeyInfo key = Console.ReadKey(true);
        
        // Check if pressed key is one of the valid input chars
        if (VALID_CHARS.Contains(key.KeyChar))
        {
                AddNewChar(key.KeyChar);
        }

        // Check if the pressed key is one of the special keys
        else
        {
            switch (key.Key)
                {
                case ConsoleKey.Backspace:
                    DeleteLastChar();
                    break;
                case ConsoleKey.Enter:
                    Calculate();
                    break;
                case ConsoleKey.Escape:
                    running = false;
                    break;
                }
        }
    }

    // Render the result on screen
    private void Render()
    {
        // Format the display string
        string displayLine = 
            error ? "ERR" : 
            inputExpressions.Length == 0 ? "0" : inputExpressions;
        
        // Clear console
        Console.CursorVisible = false;
        Console.Clear();

        // Text above the display
        Console.WriteLine();
        //Console.WriteLine("DEBUG: INPUT " + '"'+inputExpressions+'"');
        Console.WriteLine(messageLog.Last());
        Console.WriteLine(seperatorLine);

        // Iterate through the x and y coords of the "pixels" 
        // and display the character from the input string
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
                else {
                    switch(currentChar)
                    {
                        case '.':
                            currentDigit = (int)DIGIT.DOT;
                            break;
                        case '(':
                            currentDigit = (int)DIGIT.PARENTHESIS_OPEN;
                            break;
                        case ')':
                            currentDigit = (int)DIGIT.PARENTHESIS_CLOSE;
                            break;
                        case '+':
                            currentDigit = (int)DIGIT.PLUS;
                            break;
                        case '-':
                            currentDigit = (int)DIGIT.MINUS;
                            break;
                        case '*':
                            currentDigit = (int)DIGIT.MULTIPLY;
                            break;
                        case '/':
                            currentDigit = (int)DIGIT.DIVIDE;
                            break;
                        case 'E':
                            currentDigit = (int)DIGIT.E;
                            break;
                        case 'R':
                            currentDigit = (int)DIGIT.R;
                            break;
                    }
                }

                // Set the value of the current "pixel"
                display[y] += DIGITS[currentDigit,y,x % CHAR_WIDTH] ? FILLED : EMPTY;
            }

            // Render the results of the current row
            Console.WriteLine(display[y]);
        }

        // Text under the display
        Console.WriteLine(seperatorLine);
        Console.WriteLine(" PRESS [ESC] TO QUIT");
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
    public Calc()
    {
        Init();
        Run();
    }
}

