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
            Console.WriteLine("01\n");
            model = GetKnob("MODEL");
            Console.WriteLine("02");
        }
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public void SetKnob(string knob, string value)
        {
            string cmd = knob + "=" + value + "\n";
            foreach (byte b in cmd)
            {
                writer.Write(b);
            }
            
            Console.WriteLine(ReadString());
        }
        public string GetKnob(string knob)
        {
            string cmd = knob + "\n";
            foreach (byte b in cmd)
            {
                writer.Write(b);
            }
            string rs = ReadString();
            Console.WriteLine(rs);
            return rs;
        }
    }

    class FileBuffer
    {
        string fname;
        byte[] raw;
        int nbytes;
        public FileBuffer(string _fname)
        {
            fname = _fname;
     
            using (FileStream fs = File.OpenRead(fname))
            {
                nbytes = (int)fs.Length;
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    raw = binaryReader.ReadBytes((int)fs.Length);
                }
            }

        }
        public override string ToString()
        {
            return "FileBuffer: " + fname + " " + nbytes;
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

    

        IList<FileBuffer> fbs = new List<FileBuffer>();
        void OpenFiles(string [] fnames)
        {
            foreach (string fn in fnames)
            {
                fbs.Add(new FileBuffer(fn));
            }
        }
        void RunAwg(FileBuffer fb)
        {
            Console.WriteLine(uut + " load " + fb);
        }
        void RunLoop(int reps, string[] fnames)
        {
            OpenFiles(fnames);

            for (int ii = 0; ii < reps; ++ii)
                foreach (FileBuffer fp in fbs)
                {
                    
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
