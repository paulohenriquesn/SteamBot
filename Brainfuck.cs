//
// Edit by Paulo Henrique
// Credits: https://github.com/james1345-1/Brainfuck
// 01/14/2018

//Edited to Run Command @brainfuck in SteamBot: https://github.com/paulohenriquesn/SteamBot

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SteamBot_
{
    class Brainfuck
    {
        private char[] mem;
        private int mp;
        private char[] com;
        private int ip = 0;
        private int EOF;
        public Brainfuck(string s)
        {

            mem = new char[30000];
            mp = 0;
            try
            {
                com = s.ToCharArray();
            }
            catch { Program.sayBrainFuck("Invalid Command Brainfuck"); }
            EOF = com.Length;

        }      
        public void run()
        {
            while (ip < EOF)
            {
                char c = com[ip];
                switch (c)
                {
                    case '>': mp++; break;
                    case '<': mp--; break;
                    case '+': mem[mp]++; break;
                    case '-': mem[mp]--; break;
                    case '.': Program.sayBrainFuck(mem[mp].ToString()); break;
                    case ',':
                        try
                        {
                            mem[mp] = (char)Console.Read();
                        }
                        catch (Exception e)
                        {
                            Debug.Write(e.StackTrace);
                        }
                        break;
                    case '[':
                        if (mem[mp] == 0)
                        {
                            while (com[ip] != ']') ip++;
                        }
                        break;

                    case ']':
                        if (mem[mp] != 0)
                        {
                            while (com[ip] != '[') ip--;
                        }
                        break;
                }
                ip++;
            }
        }
        public static int EXIT_SUCCESS = 1;
        public static int EXIT_FAILURE = -1;
        public void RunCommand(string command_)
        {
            (new Brainfuck(command_)).run();
        }  
    }
}