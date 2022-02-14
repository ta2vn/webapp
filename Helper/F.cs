/*
    MIT license
    Ping tatadoan@outlook.com for Info | Bug | More Feature
    I'd be happy to have a response
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Tlang
{
    public static class F
    {
        static Random rand = new Random();

        public static int Random()
        {
            return rand.Next(int.MaxValue);
        }

        public static int Random(int number)
        {
            return rand.Next(number);
        }


        public static object OpenUrl(string http)
        {
            if (http.StartsWith("http:") || http.StartsWith("https:"))
            {
                ThreadStart th = new ThreadStart(delegate() { System.Diagnostics.Process.Start(http); });
                th.Invoke();
                return null;
            }
            return null;
        }

        public static int IndexOf(byte[] content, byte[] sp, int start)
        {
            int i = start;
            if (content.Length < sp.Length) return -1;
            while (i < content.Length)
            {
                while (i < content.Length && content[i] != sp[0])
                {
                    i++;
                }
                if (i < content.Length && content[i] == sp[0])
                {
                    if (Equal(content, i, sp))
                    {
                        return i;
                    }
                }
                i++;
            }
            return -1;
        }

        public static int IndexOf(byte[] content, byte sp, int start)
        {
            int i = start;

            while (i < content.Length && content[i] != sp)
            {
                i++;
            }
            if (i < content.Length && content[i] == sp)
            {
                return i;
            }

            return -1;
        }

        public static bool Equal(byte[] data, int start, byte[] b)
        {
            int i = start;
            int end = i + b.Length;
            int j = 0;

            if (end > data.Length) return false;
            for (; i < end; i++)
            {
                if (data[i] != b[j])
                {
                    return false;
                }
                j++;
            }
            return true;
        }

        public static byte[] SubBytes(byte[] data, int start, int length)
        {
            byte[] d = new byte[length];
            int end = start + length;
            int j = 0;
            for (int i = start; i < end; i++)
            {
                d[j] = data[i];
                j++;
            }
            return d;
        }

        public static byte[] SubBytes(byte[] data, int start)
        {
            byte[] d = new byte[data.Length - start];
            int j = 0;
            for (int i = start; i < data.Length; i++)
            {
                d[j] = data[i];
                j++;
            }
            return d;
        }

        public static byte[] SubRightBytes(byte[] data, byte[] sp)
        {
            int index = IndexOf(data, sp, 0);
            if (index == -1) return new byte[] { };
            return SubBytes(data, 0, index);
        }
    }
}
