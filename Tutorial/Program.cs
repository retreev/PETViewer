namespace Tutorial
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (Window window = new Window(800, 600, "YAY!"))
            {
                window.Run(60.0);
            }
        }
    }
}
