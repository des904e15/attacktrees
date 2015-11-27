using CommandLineParsing;
using System.Diagnostics;
using System.IO;

namespace AttackTree
{
    class Program : Command
    {
        private Configuration config;
        private string dotPath
        {
            get
            {
                var c = config["dot.path"];
                if (c == null)
                {
                    Validator<string> validator = new Validator<string>();
                    validator.Add(x => x != null && x != string.Empty, "That's not a filepath!");
                    validator.Add(x => File.Exists(x), x => "The file \"" + x + "\" doesn't exist!");
                    validator.Add(x => Path.GetExtension(x) == ".exe", "That's not an executable!");
                    validator.Add(x => Path.GetFileName(x) == "dot.exe", "That's not the dot.exe executable!");

                    c = ColorConsole.ReadLine<string>("The dot.exe path: ", validator: validator);

                    config["dot.path"] = c;
                }
                return c;
            }
        }

        [NoName]
        private readonly Parameter<string[]> file = null;
        [Name("--open", "-o")]
        private readonly FlagParameter open;

        public Program()
        {
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var configPath = Path.Combine(dir, "config");
            this.config = new Configuration(configPath);

            file.Validator.Add(x => x.Length > 0, "A .dot file must be specified.");
            file.Validator.Add(x => x.Length == 1, "Only one .dot file can be specified.");
            file.Validator.AddForeach(FileExists);
        }

        public Message FileExists(string filepath)
        {
            string file = Path.Combine(Directory.GetCurrentDirectory(), filepath);
            if (!File.Exists(file))
                return "The specified .dot file does not exist.";
            else
                return Message.NoError;
        }

        private string GenerateDot(string path)
        {
            ProcessStartInfo psi = new ProcessStartInfo(dotPath, $@"-Tdot ""{path}""")
            {
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            string generated;

            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
                generated = process.StandardOutput.ReadToEnd();
            }

            return generated;
        }

        protected override void Execute()
        {
            string filepath = Path.Combine(Directory.GetCurrentDirectory(), file.Value[0]);
            string pdfpath = Path.ChangeExtension(filepath, ".pdf");

            GraphBuilder.Build(GenerateDot(filepath), pdfpath, open.IsSet);
        }

        static void Main(string[] args)
        {
            var msg = new Program().ParseAndExecute(args);

            if (msg.IsError)
            {
                ColorConsole.WriteLine(msg.GetMessage());
                System.Console.ReadKey(true);
            }
        }
    }

    //public class And : PDFGroup
    //{
    //    private Arc left, right;

    //    public And(RectangleF rect)
    //        : base(true, rect)
    //    {
    //        this.left = new Arc()
    //        this.right = right;
    //    }

    //    protected override IEnumerable<PDFObject> GetPDFObjects()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
