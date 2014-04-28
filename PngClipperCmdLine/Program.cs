using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using PngClipperCore;

namespace PngClipperCmdLine
{
    class Program
    {
        class CommonConverter:PngClipperCore.NameConverter
        {
            public CommonConverter()
            {
                regex_ = new Regex(Pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            public String GetPattern()
            {
                return Pattern;
            }

            public String FormatName(Match match, String outPath, int fileNo)
            {
                int no = Int32.Parse(match.Groups[1].ToString());
                return String.Format("{0}\\@_{1}{2}",outPath, fileNo, match.Groups[2]);
            }

            public bool IsMatched(String name, out Match match, out int fileNo)
            {
                Match ans = regex_.Match(name);
                if (ans.Success)
                {
                    match = ans;
                    fileNo = int.Parse(ans.Groups[1].ToString());
                    return true;
                }

                fileNo = -1;
                match = Match.Empty;
                return false;
            }

            public bool IsMatched(String name)
            {
                Match match;
                int fileNo;
                return IsMatched(name, out match, out fileNo); 
            }

            private readonly Regex regex_;
            private const String Pattern = @"^[a-z_]*(\d+)(\.png)$";
        }


        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Not enough parameters");
                return;
            }

            String td = args[0];
            if (!Directory.Exists(td))
            {
                Console.WriteLine("Bye");
                return;
            }

            PngCutter pc = new PngCutter(new CommonConverter());
            pc.ProcessTarget(td, td, "dti.txt");
            pc.RemoveFilesByPattern(td);
        }
    }
}
