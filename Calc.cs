//namespace JobLoopProject;


// message queue
// bit shift multiplication / division
// float/int toggle

class Calc
{
    private enum DIGIT 
    {
        DOT = 10,
        PARENTHESIS_OPEN,
        PARENTHESIS_CLOSE,
        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE
    }
    private const char EMPTY = ' ';
    private const char FILLED = '#';
    private const char UNFILLED = '-';
    private const int CHAR_SPACING = 2;
    private const int CHAR_HEIGHT = 7;
    private const int CHAR_WIDTH = 5 + CHAR_SPACING;
    private const int CHAR_LIMIT = 24;
    private bool[,,] DIGITS = new bool[17,CHAR_HEIGHT,CHAR_WIDTH];
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

    private bool running = true;
    private string inputCalculations = "";
    private string[] display = new string[CHAR_HEIGHT];

    private void Init() 
    {
        // ZERO DIGIT
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

        // ONE DIGIT
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
        
        // TWO DIGIT
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
        
        // THREE DIGIT
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

        // FOUR DIGIT
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
        
        // FIVE DIGIT
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

        // SIX DIGIT
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

        // SEVEN DIGIT
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

        // EIGHT DIGIT
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

        // NINE DIGIT
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

        // DOT DIGIT
        DIGITS[(int)DIGIT.DOT,6,2] = true;

        // PARETHESIS_OPEN DIGIT
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,0,2] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,1,1] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,2,0] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,3,0] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,4,0] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,5,1] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_OPEN,6,2] = true;

        // PARETHESIS_CLOSE DIGIT
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,0,2] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,1,3] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,2,4] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,3,4] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,4,4] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,5,3] = true;
        DIGITS[(int)DIGIT.PARENTHESIS_CLOSE,6,2] = true;
        
        // PLUS DIGIT
        DIGITS[(int)DIGIT.PLUS,1,2] = true;
        DIGITS[(int)DIGIT.PLUS,2,2] = true;
        DIGITS[(int)DIGIT.PLUS,3,0] = true;
        DIGITS[(int)DIGIT.PLUS,3,1] = true;
        DIGITS[(int)DIGIT.PLUS,3,2] = true;
        DIGITS[(int)DIGIT.PLUS,3,3] = true;
        DIGITS[(int)DIGIT.PLUS,3,4] = true;
        DIGITS[(int)DIGIT.PLUS,4,2] = true;
        DIGITS[(int)DIGIT.PLUS,5,2] = true;
        
        // MINUS DIGIT
        DIGITS[(int)DIGIT.MINUS,3,0] = true;
        DIGITS[(int)DIGIT.MINUS,3,1] = true;
        DIGITS[(int)DIGIT.MINUS,3,2] = true;
        DIGITS[(int)DIGIT.MINUS,3,3] = true;
        DIGITS[(int)DIGIT.MINUS,3,4] = true;
        
        // MULTIPLY DIGIT
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
        
        // DIVIDE DIGIT
        DIGITS[(int)DIGIT.DIVIDE,5,0] = true;
        DIGITS[(int)DIGIT.DIVIDE,4,1] = true;
        DIGITS[(int)DIGIT.DIVIDE,3,2] = true;
        DIGITS[(int)DIGIT.DIVIDE,2,3] = true;
        DIGITS[(int)DIGIT.DIVIDE,1,4] = true;
    }
    
    private void AddNewChar(char input)
    {
        if (inputCalculations.Length < CHAR_LIMIT) {
            inputCalculations += input;
        }
    }

    private void DeleteLastChar()
    {
        if (inputCalculations.Length > 0)
        {
            inputCalculations = inputCalculations.Remove(inputCalculations.Length - 1);
        }
    }
    private void Calculate(){
    // PEMDAS stands for Parentheses, Exponents, Multiplication and Division (same level), 
    // and Addition and Subtraction (same level).
    }

    private void GetInput() 
    {
        ConsoleKeyInfo key = Console.ReadKey(true);
        
        if (VALID_CHARS.Contains(key.KeyChar))
        {
                AddNewChar(key.KeyChar);
        }
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

    private void Render()
    {
        string displayLine = inputCalculations.Length == 0 ? "0" : inputCalculations;
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine("INPUT: " + '"'+inputCalculations+'"');
        for (int y = 0; y < CHAR_HEIGHT; y++)
        {
            display[y] = "";
            for (int x = 0; x < CHAR_WIDTH * displayLine.Length; x++)
            {
                char currentChar = displayLine[x / CHAR_WIDTH];
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
                    }
                }
                display[y] += DIGITS[currentDigit,y,x % CHAR_WIDTH] ? FILLED : EMPTY;
            }
            Console.WriteLine(display[y]);
        }
    }
    
    private void Run()
    {
        while (running == true)
        {

            Render();
            GetInput();
        }
        Console.Clear();
    }
    
    public Calc()
    {
        Init();
        Run();
    }
}

