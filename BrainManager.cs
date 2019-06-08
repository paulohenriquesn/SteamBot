using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SteamBot_
{
   public class BrainManager
    {
        public Dictionary<string, string> brainData = new Dictionary<string, string>();
        private bool xd = false;
        public string getResponse(string value)
        {
            string result = String.Empty;
            if (value != "" || !String.IsNullOrWhiteSpace(value))
            {
                string[] haha = File.ReadAllLines("memoria.data");
              
                if (brainData.ContainsKey(value))
                    result = brainData[value];

                if (result == "" || result == String.Empty)
                {
                    using (StreamWriter sw = new StreamWriter("memoria.data"))
                    {
                        foreach (string line in haha)
                        {
                            sw.WriteLine(line);
                        }
                        if (xd == true)
                        {
                            sw.WriteLine("#" + value);
                            xd = false;
                        }
                        else
                        {

                            sw.WriteLine("-" + value);
                            xd = true;
                        }
                    }
                    string[] kk = File.ReadAllLines("memoria.data");
                    result = kk[new Random().Next(0, kk.Length)].Replace("#", String.Empty).Replace("-", String.Empty);
                }

                
            }
            return result;
        }
    }
}
