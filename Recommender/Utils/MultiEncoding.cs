using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Utils;

public static class MultiEncoding
{
    private static readonly Dictionary<string, int> _encodingMap 
        = new Dictionary<string, int>()
        {
            {"(no genres listed)",  0},
            {"Action",              1},
            {"Adventure",           2},
            {"Animation",           3},
            {"Children",            4},
            {"Comedy",              5},
            {"Crime",               6},
            {"Documentary",         7},
            {"Drama",               8},
            {"Fantasy",             9},
            {"Film-Noir",           10},
            {"Horror",              11},
            {"IMAX",                12},
            {"Musical",             13},
            {"Mystery",             14},
            {"Romance",             15},
            {"Sci-Fi",              16},
            {"Thriller",            17},
            {"War",                 18},
            {"Western",             19},
        };
    public static BitArray GetBits(long coding)
    {
        return new BitArray(BitConverter.GetBytes(coding));
    }

    public static long GetMultiEncoding(string genres)
    {
        long coding = 0;
        string[] strings = genres.Split('|');
        foreach (string s in strings)
        {
            if (_encodingMap.ContainsKey(s))
            {
                coding |= 1L << _encodingMap[s];
            }
            else
            {
                coding |= 1L << 63;
                // 未列出的项目置最高位, 为后期添加分类预留空间
            }
        }
        return coding;
    }

    public static List<string> GetGenres(long coding)
    {
        var genres = new List<string>();
        BitArray bits = GetBits(coding);
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i])
            {
                var genre = _encodingMap.FirstOrDefault(e => e.Value == i);
                if (genre.Equals(default))
                {
                    continue;
                }
                else
                {
                    genres.Add(genre.Key);
                }                
            }
        }
        return genres;
    }

    public static long GetPerferCoding(Dictionary<string, int> count)
    {
        var perferStrings = count.OrderByDescending(e => e.Value)
            .ThenByDescending(e => e.Key)
            .Select(e => e.Key)
            .Take(5)
            .ToList();
        long coding = 0;
        foreach (var perfer in perferStrings)
        {
            if (_encodingMap.ContainsKey(perfer))
            {
                coding |= 1L << _encodingMap[perfer];
            }
            else
            {
                coding |= 1L << 64;
            }
        }
        return coding;
    }

    public static int Count(long coding)
    {
        int count = 0;
        BitArray bits = GetBits(coding);
        foreach (bool bit in bits)
        {
            if (bit)
            {
                count++;
            }
        }
        return count;
    }

    public static int Multiply(long coding1, long coding2)
    {
        long newCoding = coding1 & coding2;
        // 按位与保留共同属性
        return Count(newCoding);
    }
}
