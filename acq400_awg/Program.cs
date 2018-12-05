using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace acq400_awg
{

    class Program
    {
        void SetMaxLen(int len)
        {
            Console.WriteLine("SetMaxLen:" + len);
        }
        void OpenFiles(string [] fnames)
        {

        }
        void RunAwg(string fn)
        {

        }
        void RunLoop(int reps, string[] fnames)
        {
            OpenFiles(fnames);

            for (int ii = 0; ii < reps; ++ii)
                for (int fn = 0; fn < fnames.Length; ++fn)
                {
                    Console.WriteLine("rep " + reps + " load " + fnames[fn]);
                }
        }
        string uut;
        Program(string _uut)
        {
            uut = _uut;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("acq400_awg " + string.Join(" ", args));
            if (args.Length < 3)
            {
                Console.WriteLine("acq400_awg uut reps [file1]  [file2..]");
            } 
            else
            {
                Program p = new Program(args[0]);
                p.RunLoop(Convert.ToInt32(args[1]), new List<string>(args).GetRange(2, args.Length-2).ToArray());
            }
        }
    }
}
