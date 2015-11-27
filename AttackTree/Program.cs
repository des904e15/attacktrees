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
                    validator.Add(x => File.Exists(x), "That file doesn't exist!");
                    validator.Add(x => Path.GetExtension(x) == ".exe", "That's not an executable!");
                    validator.Add(x => Path.GetFileName(x) == "dot.exe", "That's not the dot.exe executable!");

                    c = ColorConsole.ReadLine<string>("The dot.exe path: ", validator: validator);

                    config["dot.path"] = c;
                }
                return c;
            }
        }

        static string dott =
        #region input
        @"digraph finite_state_machine {
  graph [bb=""0,0,798.5,326""];
  node [label=""\N"",
    shape=none
  ];
  root   [height=0.52778,
    label=""Catch cheating\nbastard!!!"",
    pos=""433,307"",
    shape=AND,
    width=1.375];
  access   [height=0.5,
    pos=""228,234"",
    width=0.75];
  root -> access   [pos=""e,255.18,244.41 383.38,288.81 346.86,276.17 298.03,259.25 264.92,247.79""];
  surveil  [height=0.5,
    pos=""433,234"",
    width=0.75];
  root -> surveil  [pos=""e,433,252 433,287.72 433,279.97 433,270.79 433,262.27""];
  revenge  [height=0.5,
    pos=""605,234"",
    width=0.81944];
  root -> revenge  [pos=""e,575.31,247.26 476.85,287.9 504.33,276.56 539.44,262.06 565.95,251.12""];
  deviceschedule   [height=0.5,
    pos=""544,90"",
    width=1.4028];
  installECM   [height=0.5,
    pos=""39,162"",
    width=1.0833];
  access -> installECM   [pos=""e,78.188,177.51 200.92,222.97 171.53,212.09 123.99,194.48 88.006,181.15""];
  compromisePhablet  [height=0.5,
    pos=""160,162"",
    width=1.7778];
  access -> compromisePhablet  [pos=""e,176.62,180.1 211.19,215.7 202.87,207.14 192.69,196.66 183.61,187.3""];
  compromiseSM   [height=0.5,
    pos=""296,162"",
    width=1.4861];
  access -> compromiseSM   [pos=""e,279.38,180.1 244.81,215.7 253.13,207.14 263.31,196.66 272.39,187.3""];
  athome   [height=0.5,
    pos=""396,162"",
    width=0.79167];
  surveil -> athome  [pos=""e,405.04,180.1 423.85,215.7 419.6,207.64 414.44,197.89 409.73,188.98""];
  doing  [height=0.5,
    pos=""470,162"",
    width=0.75];
  surveil -> doing   [pos=""e,460.96,180.1 442.15,215.7 446.4,207.64 451.56,197.89 456.27,188.98""];
  devicestatus   [height=0.5,
    pos=""433,90"",
    width=1.1806];
  athome -> devicestatus   [pos=""e,423.96,108.1 405.15,143.7 409.4,135.64 414.56,125.89 419.27,116.98""];
  lookwindow   [height=0.5,
    pos=""329,90"",
    width=1.1944];
  athome -> lookwindow   [pos=""e,345.37,108.1 379.44,143.7 371.32,135.22 361.41,124.86 352.53,115.58""];
  doing -> deviceschedule  [pos=""e,525.92,108.1 488.29,143.7 497.43,135.05 508.64,124.45 518.6,115.03""];
  doing -> devicestatus  [pos=""e,442.04,108.1 460.85,143.7 456.6,135.64 451.44,125.89 446.73,116.98""];
  detectsignature  [height=0.5,
    pos=""433,18"",
    width=1.3889];
  devicestatus -> detectsignature  [pos=""e,433,36.104 433,71.697 433,63.983 433,54.712 433,46.112""];
  shutOnOff  [height=0.5,
    pos=""554,162"",
    width=1.0694];
  revenge -> shutOnOff   [pos=""e,566.46,180.1 592.39,215.7 586.4,207.47 579.12,197.48 572.52,188.42""];
  modSchedule  [height=0.5,
    pos=""657,162"",
    width=1.2917];
  revenge -> modSchedule   [pos=""e,644.29,180.1 617.85,215.7 624.03,207.39 631.54,197.28 638.32,188.14""];
  destroyitall   [height=0.5,
    pos=""760,162"",
    width=1.0694];
  revenge -> destroyitall  [pos=""e,722.35,180 634.52,219.67 656.74,209.64 687.63,195.69 713.2,184.14""];
}
";
        #endregion

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
