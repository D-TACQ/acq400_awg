using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;

namespace acq400_awg
{
    class SiteClient
    {
        TcpClient sk;
        string uut;
        int site;
        BinaryWriter writer;
        BinaryReader reader;
        string model;

        string ReadString()
        {
            string response = "";
            bool done = false;
            while (!done)
            {
                char[] lastchar = reader.ReadChars(1);
                if (lastchar[0] != '\n')
                {
                    response += lastchar[0];
                }
                else
                {
                    done = true;
                }
            }
            return response;
        }
        public SiteClient(string _uut, int _site)
        {
            uut = _uut;
            site = _site;
            sk = new TcpClient(uut, 4220+site);
            writer = new BinaryWriter(sk.GetStream());
            reader = new BinaryReader(sk.GetStream());
            model = GetKnob("MODEL");
        }
        public void SetKnob(string knob, string value)
        {
            writer.Write(knob + "=" + value + "\n");
            Console.WriteLine(ReadString());
        }
        public string GetKnob(string knob)
        {
            writer.Write(knob + "\n");
            string rs = ReadString();
            Console.WriteLine(rs);
            return rs;
        }
    }

    class AWG_Controller
    {
        SiteClient s0;
        SiteClient s1;

     
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
                    Console.WriteLine(uut + " rep " + reps + " load " + fnames[fn]);
                }
        }
        string uut;
        AWG_Controller(string _uut)
        {
            uut = _uut;
            s0 = new SiteClient(uut, 0);
            s1 = new SiteClient(uut, 1);
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
                AWG_Controller ctrl = new AWG_Controller(args[0]);
                ctrl.RunLoop(Convert.ToInt32(args[1]), new List<string>(args).GetRange(2, args.Length-2).ToArray());
            }
        }
    }
}
