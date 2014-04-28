using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace PngClipperCore
{
    public interface NameConverter
    {
        String GetPattern();
        String FormatName(Match match, String outPath, int fileNo);
        bool IsMatched(String name, out Match match, out int fileNo);
        bool IsMatched(String name);
    }

    public class PngCutter
    {
        public PngCutter(NameConverter nc)
        {
            nc_ = nc;
        }

        private bool isColorNone(Color color)
        {
            return color.R == 0 && color.G == 0 && color.B == 0 && color.A == 0;
        }

        private Rectangle GetMinimumAreaForImage(Bitmap bm)
        {
            //~ Step 1: Search for starting x
            int leftBound = 0;
            for (int x = 0; x < bm.Width; ++x)
            {
                bool hit = false;
                for (int y = 0; y < bm.Height; ++y)
                {
                    Color color = bm.GetPixel(x, y);
                    if (!isColorNone(color))
                    {
                        leftBound = x;
                        hit = true;
                        break;
                    }
                }
                if (hit)
                {
                    break;
                }
            }


            //~
            int rightBound = bm.Width - 1;
            for (int x = bm.Width - 1; x >= 0; --x)
            {
                bool hit = false;
                for (int y = 0; y < bm.Height; ++y)
                {
                    Color color = bm.GetPixel(x, y);
                    if (!isColorNone(color))
                    {
                        rightBound = x;
                        hit = true;
                        break;
                    }
                }
                if (hit)
                {
                    break;
                }
            }

            int topBound = 0;
            for (int y = 0; y < bm.Height; ++y)
            {
                bool hit = false;
                for (int x = 0; x < bm.Width; ++x)
                {
                    Color color = bm.GetPixel(x, y);
                    if (!isColorNone(color))
                    {
                        topBound = y;
                        hit = true;
                        break;
                    }
                }
                if (hit)
                {
                    break;
                }
            }

            int bottomBound = 0;
            for (int y = bm.Height - 1; y >= 0; --y)
            {
                bool hit = false;
                for (int x = 0; x < bm.Width; ++x)
                {
                    Color color = bm.GetPixel(x, y);
                    if (!isColorNone(color))
                    {
                        bottomBound = y;
                        hit = true;
                        break;
                    }
                }
                if (hit)
                {
                    break;
                }
            }

            //~ Fix
            int width = rightBound - leftBound;
            int height = bottomBound - topBound;
            if (width % 2 != 0 && rightBound < bm.Width - 1 )
            {
                rightBound += 1;
            }
            if (height % 2 != 0 && bottomBound < bm.Height - 1)
            {
                bottomBound += 1;
            }

            return new Rectangle(leftBound, topBound, rightBound - leftBound, bottomBound - topBound);
        }

        private void SaveAreaToFile(Bitmap bm, Rectangle rc, String fileName)
        {
            //Format32bppArgb
            Bitmap newBm = new Bitmap(rc.Size.Width, rc.Size.Height);

            for (int r = 0; r < rc.Size.Height; ++r)
            {
                int y = r + rc.Location.Y;
                for (int c = 0; c < rc.Size.Width; ++c)
                {
                    int x = c + rc.Location.X;
                    newBm.SetPixel(c, r, bm.GetPixel(x, y));
                }
            }
            newBm.Save(fileName, ImageFormat.Png);
        }

        public static String StripFileName(String file)
        {
            String name;
            int pos = file.LastIndexOf('\\');
            if (pos >= 0)
            {
                name = file.Substring(pos + 1);
            }
            else
            {
                name = file;
            }
            return name;
        }

        public static String StripPath(String path)
        {
            String pre;
            int pos = path.LastIndexOf('\\');
            if (pos >= 0)
            {
                pre = path.Substring(0, pos);
            }
            else
            {
                pre = path;
            }
            return pre;
        }

        public static String AppendPath(String pre, String name)
        {
            if (!pre.EndsWith("\\") && !name.StartsWith("\\"))
            {
                return pre + '\\' + name;
            }
            return pre + name;
        }

        public Rectangle ProcessSingle(String path, String outPath)
        {
            Console.WriteLine("Converting \"{0}\" => \"{1}\"", path, outPath);
            Rectangle rc;
            using (Image image = Image.FromFile(path))
            {
                Bitmap bm = new Bitmap(image);
                rc = GetMinimumAreaForImage(bm);
                SaveAreaToFile(bm, rc, outPath);
            }

            return rc;
        }

        private PointF CalcCenter(Rectangle rc)
        {
            return new PointF(rc.Location.X + rc.Size.Width * 0.5f,
                rc.Location.Y + rc.Size.Height * 0.5f);
        }
   
        public Dictionary<String, String> GetNameList(String path, String outPath)
        {
            Dictionary<String, String> convertDc = new Dictionary<String, String>();

            List<String> okFiles = new List<String>();
            String[] files = Directory.GetFiles(path);

            //~ Sort them !
            List<int> numbers = new List<int>();
            foreach (String file in files)
            {
                String name = StripFileName(file);
                Match match;
                int fileNo;
                if (nc_.IsMatched(name, out match, out fileNo))
                {
                    if (fileNo < 0)
                    {
                        throw new System.Exception("file no is less than zero");
                    }
                    numbers.Add(fileNo);
                }
            }

            numbers.Sort(); //~ From less to upper
            Dictionary<int, int> fileNoMap = new Dictionary<int, int>();
            int opNow = 0;
            foreach (int v in numbers)
            {
                fileNoMap.Add(v, opNow);
                ++opNow;
            }

            //~ trim to new name
            foreach (String file in files)
            {
                String name = StripFileName(file);
                Match match;
                int fileNo;
                if (nc_.IsMatched(name,out match, out fileNo))
                {
                    int fixedFileNo = fileNoMap[fileNo];
                    String outFull = nc_.FormatName(match, outPath, fixedFileNo);
                    convertDc.Add(file, outFull);
                }
            }
            return convertDc;
        }

        public void RemoveFilesByPattern(String path)
        {
            String[] files = Directory.GetFiles(path);
            foreach (String file in files)
            {
                if( nc_.IsMatched(StripFileName(file)))
                {
                    Console.WriteLine("Deleting {0} ... ", file);
                    File.Delete(file);
                }
            }
        }

        public Dictionary<String, String> ProcessTargetWithDC(Dictionary<String, String> dc, String dtInfoFull)
        {
            String[] okFiles = dc.Keys.ToArray<String>();
            if (okFiles.Length == 0)
            {
                return dc;
            }

            Array.Sort(okFiles, String.CompareOrdinal);

            bool first = true;
            PointF preCenter = new PointF();

            StreamWriter sw = null;
            try
            {
                sw = File.CreateText(dtInfoFull);

                foreach (String full in okFiles)
                {
                    String pre = StripPath(full);
                    String name = StripFileName(full);
                    String outFull = dc[full];
                    Rectangle rect = ProcessSingle(full, outFull);
                    if (first)
                    {
                        preCenter = CalcCenter(rect);
                        first = false;
                    }

                    PointF centerNow = CalcCenter(rect);
                    PointF delta = new PointF(centerNow.X - preCenter.X, centerNow.Y - preCenter.Y);
                    //Console.WriteLine("{0} => {1}\n", name, delta);
                    sw.WriteLine("{0} {1}", delta.X, delta.Y);
                }
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }
            return dc;
        }

        public Dictionary<String,String> ProcessTarget(String path, String outPath, String dtInfoName)
        {
            Dictionary<String, String> rv = GetNameList(path, outPath);
            return ProcessTargetWithDC(rv, AppendPath(outPath,dtInfoName));
        }

        private readonly NameConverter nc_;
    }
}
