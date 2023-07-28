using System;
using System.Text;
using System.Collections;
using System.Security.Cryptography;

namespace DFACut 
{
    internal class Program
    {     
        static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                string filenameDFA1 = args[0];
                string filenameDFA2 = args[1];
                string filenameOutputDFA = args[2];
                if (File.Exists(filenameDFA1) && File.Exists(filenameDFA2))
                {
                    File.Delete(filenameOutputDFA);
                    if (!File.Exists(filenameOutputDFA))
                    {
                        DFA dfa1;
                        DFA dfa2;
                        DFA dfaOutput;
                        using (FileStream dfaFile = File.OpenRead(filenameDFA1))
                        {
                            using (StreamReader sr = new StreamReader(dfaFile))
                            {
                                dfa1 = DFA.parseDFA(sr.ReadToEnd());
                            }
                        }

                        using (FileStream dfaFile = File.OpenRead(filenameDFA2))
                        {
                            using (StreamReader sr = new StreamReader(dfaFile))
                            {
                                dfa2 = DFA.parseDFA(sr.ReadToEnd());
                            }
                        }

                        dfaOutput = DFA.Intersect(dfa1, dfa2);
                        dfaOutput.Optimize();

                        using (StreamWriter dfaFile = File.CreateText(filenameOutputDFA))
                        {
                            dfaFile.Write(dfaOutput.printDFA());
                        }
                    }
                    else
                    {
                        Console.WriteLine("ResultDFA exists.");
                    }
                }
                else
                {
                    Console.WriteLine("DFA1 or DFA2 not found.");
                }
            }
            else
            {
                Console.WriteLine("Usage: <DFA1> <DFA2> <ResultDFA>");
            }
        }

       


      

     
    }
}