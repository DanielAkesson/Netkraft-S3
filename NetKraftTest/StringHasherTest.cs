using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using NetKraft.Messaging;
using System.Text;

namespace NetKraftTest
{
    [TestClass]
    public class StringHasherTest
    {
        [TestMethod]
        public void PrintMisses16Bit()
        {
            Random r = new Random();
            int totalMisses = 0;
            int bestCase = 0;
            for(int i=0;i<5;i++)
            {
                ushort seed = (ushort)r.Next(0, ushort.MaxValue);
                int amount = CountBeforeCollison(seed, true);
                bestCase = amount > bestCase ? amount : bestCase;
                Console.WriteLine($"Collided after {amount} strings for the seed {seed}");
                totalMisses += amount;
            }
            Console.WriteLine($"The 32bit hash function had a collision at an average of {totalMisses / 5} and best case of {bestCase} strings");
        }
        [TestMethod]
        public void PrintMisses32Bit()
        {
            Random r = new Random();
            int totalMisses = 0;
            int bestCase = 0;
            for (int i = 0; i < 5; i++)
            {
                uint seed = (uint)r.Next(0, int.MaxValue);
                int amount = CountBeforeCollison(seed, false);
                bestCase = amount > bestCase ? amount : bestCase;
                Console.WriteLine($"Collided after {amount} strings for the seed {seed}");
                totalMisses += amount;
            }
            Console.WriteLine($"The 32bit hash function had a collision at an average of {totalMisses / 5} and best case of {bestCase} strings");
        }

        private int CountBeforeCollison(uint seed, bool size_16bit)
        {
            Random r = new Random();
            Dictionary<uint, string> currentValues = new Dictionary<uint, string>();
            bool collison = false;
            int misses = 0;
            while (!collison)
            {
                string name = RandomString(r.Next(10,40));
                uint hash = size_16bit ? StringHasher.HashStringTo16Bit((ushort)seed, name) : StringHasher.HashStringTo32Bit(seed, name);

                //If this name has already been used it does not count as a collision!
                if (currentValues.ContainsKey(hash) && currentValues[hash] == name){continue;}
                //If the hash does not exist in the map, add it to check against later.
                if (!currentValues.ContainsKey(hash)){currentValues.Add(hash, name); misses++; }
                //We collided
                else {
                    Console.WriteLine($"Found collision for string '{name}' and '{currentValues[hash]}' both resulting in hash {hash} with the seed {seed}");
                    collison = true;
                }
            }
            return misses;
        }
        private string RandomString(int length)
        {
            StringBuilder sb = new StringBuilder("", 50);
            Random r = new Random();
            for(int i=0;i<length;i++)
            {
                sb.Append((char)r.Next(65, 85));
            }
            return sb.ToString();
        }
    }
}
