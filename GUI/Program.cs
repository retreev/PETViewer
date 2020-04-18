namespace GUI
{
    public static class Program
    {
        private static void Main()
        {
            // TODO filepicker/drag'n'drop
            using (var window = new Window(800, 600, 60.0, "PETViewer"))
            {
                window.Run();
            }
        }
    }
}
