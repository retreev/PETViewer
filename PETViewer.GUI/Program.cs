using System;
using System.IO;
using System.Linq;

namespace PETViewer.GUI
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Console.Out.WriteLine($"Starting PETViewer: [{string.Join(", ", args)}]");

            string modelPath = null;
            if (args.Length > 0)
            {
                modelPath = args[0];
            }
            else if (InitModelFileExists(out var initModelPath))
            {
                modelPath = initModelPath;
            }

            if (modelPath == null)
            {
                Console.Out.WriteLine("No model to display");
                return;
            }

            using (var window = new Window(800, 600, 60.0, "PETViewer"))
            {
                window.Run(modelPath);
            }
        }

        private static bool InitModelFileExists(out string initModelPath)
        {
            initModelPath = null;

            FileInfo initModelsFile = new FileInfo("init_models");
            bool exists = initModelsFile.Exists;
            if (exists)
            {
                initModelPath = File.ReadLines(initModelsFile.FullName).First();
            }

            return exists;
        }
    }
}
