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
        public static bool Trace = false;
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
        public string SetKnob(string knob, string value)
        {
            string cmd = knob + "=" + value + "\n";
            if (Trace)
            {
                Console.WriteLine(cmd);
            }

            foreach (byte b in cmd)
            {
                writer.Write(b);
            }
            string rs = ReadString();
            if (Trace)
            {
                Console.WriteLine(rs);
            }
            return rs;
        }
        public string GetKnob(string knob)
        {
            string cmd = knob + "\n";
            foreach (byte b in cmd)
            {
                writer.Write(b);
            }
            string rs = ReadString();
            if (Trace)
            {
                Console.WriteLine(rs);
            }
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

        int Shot;

        int SAMPLE_SIZE = 16;
        public static int awg_port = 54203;
        public static bool auto_soft_trigger = false;



        int SetMaxLen(int len)
        {
            string max_len = (len / SAMPLE_SIZE).ToString();
            s1.SetKnob("playloop_maxlen", max_len);
            return Convert.ToInt32(max_len);
        }

        bool WaitFor(string knob, string value)
        {
            bool shot_complete = false;
            int poll_count = 0;

            while (!shot_complete)
            {
                System.Threading.Thread.Sleep(500);
                shot_complete = s1.GetKnob(knob) == value;
                if (((++poll_count)&0xf) == 0)
                {
                    Console.WriteLine("polling:" + knob + " for " + value);
                }
            }
            return true;
        }
        bool WaitShotComplete()
        {
            return WaitFor("task_active", "0");
        }

        bool WaitAwgNotActive()
        {
            return WaitFor("task_active", "0");
        }

        bool WaitLoadComplete(int loadlen)
        {
            bool load_complete = false;
            int poll_count = 0;
            string knob = "playloop_length";

            while (!load_complete)
            {
                if (((++poll_count) & 0xf) == 0)
                {
                    Console.WriteLine("polling:" + knob + " for " + loadlen);
                }
                System.Threading.Thread.Sleep(500);
                int pll = Convert.ToInt32(s1.GetKnob(knob).Split(' ')[0]);
                load_complete = pll >= loadlen;
            }
            return true;
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
            int max_len = SetMaxLen(fb.nbytes);
            TcpClient sk;
            Console.WriteLine("awg_port:" + awg_port);
            if (awg_port == 6666)
            {
                sk = new TcpClient("localhost", awg_port);
            }
            else
            {
                sk = new TcpClient(uut, awg_port);
            }
                      
            BinaryWriter writer = new BinaryWriter(sk.GetStream());
            writer.Write(fb.raw);
            sk.Client.Shutdown(SocketShutdown.Send);
            WaitLoadComplete(max_len);
            sk.Client.Shutdown(SocketShutdown.Both);
            writer.Close();
            sk.Close();
        }
        void RunAwg(FileBuffer fb)
        {
            WaitAwgNotActive();
            Console.WriteLine(uut + " shot:" + Shot + " load " + fb + s1.GetKnob("shot"));
            LoadAwg(fb);
            if (auto_soft_trigger)
            {
                s0.SetKnob("soft_trigger", "1");
            }
            WaitShotComplete();
            Console.WriteLine(uut + " shot:" + Shot + " done ");
            ++Shot;
        }
        void RunLoop(int reps, string[] fnames)
        {
            OpenFiles(fnames);

            s1.SetKnob("shot", "0");
            
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
            Shot = 0;
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
                int ii = 0;
                for (ii = 0; ii < args.Length; ++ii)
                {
                    if (args[ii].StartsWith("--"))
                    {
                        string [] key_val = args[ii].Split('=');
                        if (String.Compare(key_val[0], "") == 0)
                        {
                            SiteClient.Trace = Int32.Parse(key_val[1]) != 0;
                        } else if (String.Compare(key_val[0], "--awg_port") == 0)
                        {
                            AWG_Controller.awg_port = Int32.Parse(key_val[1]);
                            Console.WriteLine("awg_port set:" + AWG_Controller.awg_port);
                        }
                        else if (String.Compare(key_val[0], "--auto_soft_trigger") == 0)
                        {
                            AWG_Controller.auto_soft_trigger = Int32.Parse(key_val[1]) != 0;
                            Console.WriteLine("auto_soft_trigger set:" + AWG_Controller.auto_soft_trigger);
                        }
                        else if (String.Compare(key_val[0], "--overlap_load") == 0)
                        {
                            if (Int32.Parse(key_val[1]) == 0)
                            {
                                AWG_Controller.awg_port = 54201;
                                Console.WriteLine("awg_port set:" + AWG_Controller.awg_port);
                            }                            
                        }
                        else
                        {
                            Console.WriteLine("ERROR unknown switch:" + args[ii]);
                            //System.Environment.Exit(1);
                        }

                    } else
                    {
                        break;
                    }
                }
                string uut = args[ii];
                string reps = args[ii + 1];
                int ii1 = ii + 2;
               
                AWG_Controller ctrl = new AWG_Controller(uut);
                ctrl.RunLoop(Convert.ToInt32(reps), new List<string>(args).GetRange(ii1, args.Length-ii1).ToArray());
            }
        }
    }
}
