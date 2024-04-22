public static class Lines 
{
    public static string blank = "";
    public static string[] seperator = new string[2];
        
    // Make seperator and blank line
    public static void Init(int width)
    {
        blank        = new String(' ', width);
        seperator[0] = new String('-', width);
        seperator[1] = new String('/', width);
    }
}
