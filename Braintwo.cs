using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamBot_
{
    public class Braintwo
    {

        public class Command
        {
            public string command_;
            public Action action;
        }

        public char[] ListSyntax = new char[] {
            '_', ';',
            ':', '=',
            '+', '-',
            '>', '<',
            ']', '*',
            '/', '^',
            'R',
            '.', '#',
            '[', 'P',
            '?', '0',
            '~', '@',
            'C','X',
            'Y','&',
            '1','2',
            '3','4',
            '5','6',
            '7','8',
            '9','\'',
            '!','|',
            '$',')',
            '(','x',
            ' ','ç',
            'i','v',
            '\\','\n',
            '\r','s',
            '{','}',
            'p','m',
            '%'

        };

        private static Dictionary<string, Command> Commands = new Dictionary<string, Command>();

        public double currentMemory = 0;
        public double[] Memory = new double[2];
        public int pointerMemory = 0;
        public string CryptoHex;
        public string KeyRSAPublic = "";
        public string KeyRSAPrivate = "";
        // public RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();


        public string Output;

        static void CreateCommand(string command, Action action_)
        {
            try
            {
                Commands.Add(command, new Command() { command_ = command, action = action_ });
            }
            catch { }
        }

        static void ExecuteCommand(string command)
        {
            try
            {
                Commands[command].action();
            }
            catch { }
        }

        public void ExecuteCode(char Char)
        {
            if (this.ListSyntax.Contains(Char))
            {
                ExecuteCommand(Char.ToString());
            }
            else
            {
                throw new Exception("Error to execute code " + Char + " dont exists.");
            }
        }
        int pointerMemory_param = 0;

        public void Interpreter(string code, params string[] vars_c)
        {

            bool Execute = false;
            string c_param = String.Empty;
            string c_memory = String.Empty;
            string c_value = String.Empty;

            for (int i = 0; i < code.Length; i++)
            {

                if (code[i] == '$')
                {
                    if (i > 0)
                    {
                        if (code[i - 1] == '(')
                        {
                            try
                            {
                                if (code[i - 2] == ']')
                                {
                                    Memory[pointerMemory] += 1;
                                }
                            }
                            catch { }
                            if (Memory[pointerMemory] == 0)
                            {
                                pointerMemory_param = int.Parse(code[i + 1].ToString());
                                int a = vars_c[pointerMemory_param].Length;
                                c_param = code.Replace($"(${pointerMemory_param}x", vars_c[pointerMemory_param]);
                                string bytesend = String.Empty;
                                for (int x = 0; x < a; x++)
                                {
                                    bytesend += c_param[x].ToString();
                                }
                                c_memory = c_memory + bytesend;
                                Execute = true;
                            }
                        }
                        if (code[i - 1] != '(')
                        {
                            //c_param = String.Empty;
                            //c_memory = String.Empty;
                            //c_value = String.Empty;
                            pointerMemory_param = int.Parse(code[i + 1].ToString());
                            c_param = code.Replace($"${pointerMemory_param}x", vars_c[pointerMemory_param]);
                            c_memory = c_memory + c_param;
                            Execute = true;
                        }


                    }
                    else
                    {
                        pointerMemory_param = int.Parse(code[i + 1].ToString());
                        c_param = code.Replace($"${pointerMemory_param}x", vars_c[pointerMemory_param]);
                        c_memory = c_memory + c_param;
                        Execute = true;
                    }
                }
            }


            if (Execute)
            {
                string[] xSplit = c_memory.Split('x');

                for (int i = 0; i < xSplit.Length; i++)
                {

                    try
                    {
                        if (xSplit[i][0] != 'x' || xSplit[i][0] != '(')
                        {
                            c_value += xSplit[i];
                        }
                    }
                    catch { }
                }

                string Ok = String.Empty;

                string senderX = c_value.Replace("(", String.Empty);
                for (int i = 0; i < senderX.Length; i++)
                {
                    if (senderX[i] != 'x' && senderX[i] != '(')
                    {
                        Ok += senderX[i];
                    }
                }


                string SenderZ = Ok.Replace("$0", String.Empty);
                string SenderA = SenderZ.Replace("$1", String.Empty);
                string SenderB = SenderA.Replace("$2", String.Empty);
                string SenderC = SenderB.Replace("$3", String.Empty);
                string SenderD = SenderC.Replace("$4", String.Empty);
                string SenderE = SenderD.Replace("$5", String.Empty);
                string SenderF = SenderE.Replace("$6", String.Empty);
                string SenderG = SenderF.Replace("$7", String.Empty);
                string SenderH = SenderG.Replace("$8", String.Empty);
                string SenderI = SenderH.Replace("$9", String.Empty);




                bool FOUND = false;

                for (int i = 0; i < SenderI.Length; i++)
                {
                    if (SenderI[i] == '-' && SenderI[i + 1] == '*' && SenderI[i + 1 + 1] == '/')
                    {
                        FOUND = true;
                    }
                    else
                    {
                        if (!FOUND)
                        {
                            ExecuteCode(SenderI[i]);
                        }
                    }
                    if (FOUND)
                    {
                        try
                        {
                            i += 1;
                            if (SenderI[i] == ';')
                            {
                                FOUND = false;
                            }
                        }
                        catch { }

                    }
                }

                if (SenderI == String.Empty && code.Length > 0)
                {
                    for (int i = 0; i < code.Length; i++)
                    {
                        ExecuteCode(code[i]);
                    }
                }
                Execute = false;
            }

        }

        public string IncludeExecute = String.Empty;

        public void include(params string[] includes)
        {
            try
            {
                for (int i = 0; i < includes.Length; i++)
                {
                    using (StreamReader sr = new StreamReader(includes[i]))
                    {
                        IncludeExecute = sr.ReadToEnd();
                        for (int a = 0; a < IncludeExecute.Length; a++)
                        {
                            this.ExecuteCode(IncludeExecute[a]);
                        }
                    }
                }

            }
            catch { }
        }
        public Braintwo()
        {
            CreateCommand("m", new Action(delegate ()
            {
                Memory[pointerMemory] = Math.Sqrt(currentMemory);
            }));
            CreateCommand("p", new Action(delegate ()
            {
                Memory[pointerMemory] = Math.Pow(Memory[0], Memory[1]);
            }));
            CreateCommand("=", new Action(delegate ()
            {
                currentMemory = currentMemory * 2;
            }));
            CreateCommand("+", new Action(delegate ()
            {
                currentMemory += 1;
            }));
            CreateCommand("-", new Action(delegate ()
            {
                currentMemory -= 1;
            }));
            CreateCommand(";", new Action(delegate ()
            {
                double backupMemory = 0;
                backupMemory = Memory[0];
                Memory[0] = Memory[1];
                Memory[1] = backupMemory;
                backupMemory = 0;
            }));
            CreateCommand(":", new Action(delegate ()
            {
                Memory[pointerMemory] = Memory[0] - Memory[1];
            }));
            CreateCommand("_", new Action(delegate ()
            {
                Thread.Sleep(100);
                currentMemory = new Random().Next(0, 248);
            }));
            CreateCommand(">", new Action(delegate ()
            {
                pointerMemory += 1;
            }));
            CreateCommand("<", new Action(delegate ()
            {
                pointerMemory -= 1;
            }));
            CreateCommand("]", new Action(delegate ()
            {
                try
                {
                    Memory[pointerMemory] = currentMemory;
                    currentMemory = 0;
                }
                catch { }
            }));
            CreateCommand("*", new Action(delegate ()
            {
                currentMemory = Memory[0] * Memory[1];
            }));
            CreateCommand("/", new Action(delegate ()
            {
                currentMemory = Memory[0] / Memory[1];
            }));
            CreateCommand("^", new Action(delegate ()
            {
                currentMemory = Math.Pow(Memory[0], Memory[1]);
            }));
            CreateCommand("R", new Action(delegate ()
            {
                Memory[0] = 0;
                Memory[1] = 0;
                pointerMemory = 0;
            }));
            CreateCommand(".", new Action(delegate ()
            {
                Output = Output + ((char)currentMemory);
            }));
            CreateCommand("#", new Action(delegate ()
            {
                Program.sayBrainFuck("[cMemory]: " + currentMemory + "\n[Memory]: [" + Memory[0] + "] [" + Memory[1] + "]" + "\n[pointerMemory]: " + pointerMemory + " \n\n");
            }));
            CreateCommand("[", new Action(delegate ()
            {
                currentMemory = Memory[pointerMemory];
            }));
            CreateCommand("P", new Action(delegate ()
            {
                Program.sayBrainFuck(Output);
            }));
            CreateCommand("?", new Action(delegate ()
            {
                if (currentMemory == 0)
                {
                    currentMemory = 1;
                }
                else
                {
                    currentMemory = 0;
                }
            }));
            CreateCommand("0", new Action(delegate ()
            {
                currentMemory = 0;
            }));
            CreateCommand("~", new Action(delegate ()
            {
                currentMemory = currentMemory * Memory[pointerMemory];
            }));
            CreateCommand("@", new Action(delegate ()
            {
                Memory[pointerMemory] = 0;
            }));
            CreateCommand("C", new Action(delegate ()
            {
                currentMemory = Memory[0] + Memory[1];
            }));
            CreateCommand("X", new Action(delegate ()
            {
                currentMemory = currentMemory + 10;
            }));
            CreateCommand("Y", new Action(delegate ()
            {
                currentMemory = currentMemory - 10;
            }));
            CreateCommand("&", new Action(delegate ()
            {
                Program.sayBrainFuck(currentMemory.ToString());
            }));
            CreateCommand("1", new Action(delegate ()
            {
                Memory[pointerMemory] += 1;
            }));
            CreateCommand("2", new Action(delegate ()
            {
                Memory[pointerMemory] += 2;
            }));
            CreateCommand("3", new Action(delegate ()
            {
                Memory[pointerMemory] += 3;
            }));
            CreateCommand("4", new Action(delegate ()
            {
                Memory[pointerMemory] += 4;
            }));
            CreateCommand("5", new Action(delegate ()
            {
                Memory[pointerMemory] += 5;
            }));
            CreateCommand("6", new Action(delegate ()
            {
                Memory[pointerMemory] += 6;
            }));
            CreateCommand("7", new Action(delegate ()
            {
                Memory[pointerMemory] += 7;
            }));
            CreateCommand("8", new Action(delegate ()
            {
                Memory[pointerMemory] += 8;
            }));
            CreateCommand("9", new Action(delegate ()
            {
                Memory[pointerMemory] += 9;
            }));
            CreateCommand("'", new Action(delegate ()
            {
                double a = Memory[pointerMemory] * 2;
                Memory[pointerMemory] = Memory[pointerMemory] - a;
            }));
            CreateCommand("\\", new Action(delegate ()
            {
                Memory[pointerMemory] = (Memory[0] * Memory[1]);
            }));
            CreateCommand("s", new Action(delegate ()
            {
                string a = $"0,{Memory[pointerMemory]}";
                Memory[pointerMemory] = double.Parse(a);
            }));
            CreateCommand("!", new Action(delegate ()
            {
                Memory[pointerMemory] = Memory[pointerMemory] * 2;
            }));
            CreateCommand("|", new Action(delegate ()
            {
                Memory[pointerMemory] = Memory[0] + Memory[1];
            }));
            CreateCommand("$", new Action(delegate ()
            {
                //Ignore.
            }));
            CreateCommand(")", new Action(delegate ()
            {
                double readValue = double.Parse(Console.ReadLine());
                currentMemory = readValue;
            }));
            CreateCommand("(", new Action(delegate ()
            {
                //Ignore
            }));
            CreateCommand("x", new Action(delegate ()
            {
                //Ignore
            }));
            CreateCommand(" ", new Action(delegate ()
            {
                //Ignore
            }));



            CreateCommand("}", new Action(delegate ()
            {
                // RSAParameters RSAKeyInfo = RSA.ExportParameters(false);
                var Values = Encoding.UTF8.GetBytes(Output);
                Output = String.Empty;

                using (var rsa = new RSACryptoServiceProvider(128))
                {
                    try
                    {
                        // rsa.ImportParameters(RSAKeyInfo);
                        rsa.FromXmlString(KeyRSAPublic);
                        var encryptedData = rsa.Encrypt(Values, true);

                        var Converted = Convert.ToBase64String(encryptedData);//new StringBuilder();
                                                                              /* foreach (var x in encryptedData)
                                                                                   Converted.Append(x.ToString("X2"));*/
                        CryptoHex = Converted.ToString();

                    }
                    finally { rsa.PersistKeyInCsp = false; }

                }
            }));
            CreateCommand("{", new Action(delegate ()
            {
                /*//RSAParameters RSAKeyInfo = RSA.ExportParameters(false);
                using (var rsa = new RSACryptoServiceProvider(128))
                {
                    try
                    {
                        var base64Encrypted = CryptoHex;
                        // rsa.ImportParameters(RSAKeyInfo);
                        rsa.FromXmlString(KeyRSAPrivate);

                        var resultBytes = Convert.FromBase64String(base64Encrypted);
                        var decryptedBytes = rsa.Decrypt(resultBytes,false);
                        var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                        Output = decryptedData.ToString();
                    }
                    catch (Exception ex) { Program.sayBrainFuck(ex.ToString()); }
                    finally
                    {
                        rsa.PersistKeyInCsp = false;
                    }
                }*/
                var testData = Encoding.UTF8.GetBytes(CryptoHex);

                using (var rsa = new RSACryptoServiceProvider(1024))
                {
                    try
                    {
                        var base64Encrypted = CryptoHex;
                        rsa.FromXmlString(KeyRSAPrivate);

                        var resultBytes = Convert.FromBase64String(base64Encrypted);
                        var decryptedBytes = rsa.Decrypt(resultBytes, true);
                        var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                        Output = decryptedData.ToString();
                    }
                    finally
                    {
                        rsa.PersistKeyInCsp = false;
                    }
                }

            }));
            CreateCommand("i", new Action(delegate ()
            {
                currentMemory = Memory[pointerMemory] + currentMemory;
            }));
            CreateCommand("ç", new Action(delegate ()
            {
                double backup = 0.0;
                backup = currentMemory;
                currentMemory = Memory[pointerMemory];
                Memory[pointerMemory] = backup;
            }));
            CreateCommand("v", new Action(delegate ()
            {
                Memory[pointerMemory] = Memory[0] - Memory[1];
            }));
            CreateCommand("\n", new Action(delegate ()
            {
                //Ignore
            }));
            CreateCommand("\r", new Action(delegate ()
            {
                //Ignore
            }));
            CreateCommand("%", new Action(delegate ()
            {
                currentMemory = Memory[0] % Memory[1];
            }));
        }

        public void Reset()
        {
            Memory[0] = 0;
            Memory[1] = 0;
            pointerMemory = 0;
            Output = String.Empty;
            currentMemory = 0;
        }
    }

}
