public static class Lines 
{
    public static string blank = "";
    public static string[] seperator = new string[2];
        
    // Make seperator and blank line
    public static void Init(int width)
    {
        blank        = "".PadLeft(width,' ');
        seperator[0] = "".PadLeft(width,'-');
        seperator[1] = "".PadLeft(width,'*');
    }
}
