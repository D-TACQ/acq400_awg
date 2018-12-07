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
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public void SetKnob(string knob, string value)
        {
            string cmd = knob + "=" + value + "\n";
            //Console.WriteLine(cmd);
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
        public string fname;
        public byte[] raw;
        public int nbytes;
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
            return "FileBuffer: " + fname + " " + nbytes + " " + raw.Length;
        }

    }
    class AWG_Controller
    {
        SiteClient s0;
        SiteClient s1;

        int SAMPLE_SIZE = 16;

        void SetMaxLen(int len)
        {
            Console.WriteLine("SetMaxLen:" + len);
            s1.SetKnob("playloop_maxlen", (len/SAMPLE_SIZE).ToString());
        }

        bool WaitFor(string knob, string value)
        {
            bool shot_complete = false;

            while (!shot_complete)
            {
                System.Threading.Thread.Sleep(500);
                shot_complete = s1.GetKnob(knob).Split(' ')[1] == value;
            }
            return true;
        }
        bool WaitShotComplete()
        {
            return WaitFor("AWG:SHOT_COMPLETE", "1");
        }

        bool WaitAwgNotActive()
        {
            return WaitFor("AWG:ACTIVE", "0");
        }

        IList<FileBuffer> fbs = new List<FileBuffer>();
        void OpenFiles(string [] fnames)
        {
            foreach (string fn in fnames)
            {
                fbs.Add(new FileBuffer(fn));
            }
        }

        void LoadAwg(FileBuffer fb)
        {
            SetMaxLen(fb.nbytes);
            TcpClient sk = new TcpClient(uut, 54203);
            BinaryWriter writer = new BinaryWriter(sk.GetStream());
            writer.Write(fb.raw);
            writer.Close();
            sk.Close();
            Console.WriteLine(uut + " wait2 ");
        }
        void RunAwg(FileBuffer fb)
        {
            WaitAwgNotActive();
            Console.WriteLine(uut + " load " + fb);
            LoadAwg(fb);
            WaitShotComplete();
            Console.WriteLine(uut + " done ");
        }
        void RunLoop(int reps, string[] fnames)
        {
            OpenFiles(fnames);

            for (int ii = 0; ii < reps; ++ii)
                foreach (FileBuffer fb in fbs)
                {
                    Console.WriteLine("\nrep:" + ii + " buffer:" + fb);
                    RunAwg(fb);
                }
        }
        string uut;
        AWG_Controller(string _uut)
        {
            string ss = System.Environment.GetEnvironmentVariable("SAMPLE_SIZE");
            if (ss != null)
            {
                SAMPLE_SIZE = Int32.Parse(ss);
            }
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
