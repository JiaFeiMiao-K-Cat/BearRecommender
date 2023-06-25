using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Utils;

public static class MultiHotEncoding
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
    private static BitArray GetBits(ulong coding)
    {
        return new BitArray(BitConverter.GetBytes(coding));
    }

    /// <summary>
    /// 获取分类集对应的多热编码
    /// </summary>
    /// <param name="genres">分类集</param>
    /// <returns>多热编码</returns>
    public static ulong GetMultiEncoding(string genres)
    {
        ulong coding = 0;
        string[] strings = genres.Split('|');
        foreach (string s in strings)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                continue;
            }
            if (_encodingMap.ContainsKey(s))
            {
                coding |= 1LU << _encodingMap[s];
            }
            else
            {
                coding |= 1LU << 63;
                // 未列出的项目置最高位, 为后期添加分类预留空间
            }
        }
        return coding;
    }

    /// <summary>
    /// 获取分类集
    /// </summary>
    /// <param name="coding">多热编码</param>
    /// <returns>分类列表</returns>
    public static List<string> GetGenres(ulong coding)
    {
        var genres = new List<string>();
        BitArray bits = GetBits(coding);
        int max = _encodingMap.MaxBy(e => e.Value).Value;
        // 超过最大值显然无法对应, 无需处理
        for (int i = 0; i <= max; i++)
        {
            if (bits[i])
            {
                var genre = _encodingMap.First(e => e.Value == i);
                // 必定有对应键值对, 直接添加即可
                genres.Add(genre.Key);          
            }
        }
        return genres;
    }

    /// <summary>
    /// 获取用户偏好(最多五项)的多热编码
    /// </summary>
    /// <param name="count">用户观看过的电影分类统计</param>
    /// <returns>用户偏好</returns>
    public static ulong GetPerferCoding(Dictionary<string, int> count)
    {
        var perferStrings = count.OrderByDescending(e => e.Value)
            .ThenBy(e => e.Key)
            .Select(e => e.Key)
            .Take(5)
            .ToList();
        ulong coding = 0;
        foreach (var perfer in perferStrings)
        {
            if (_encodingMap.ContainsKey(perfer))
            {
                coding |= 1LU << _encodingMap[perfer];
            }
            else
            {
                coding |= 1LU << 63;
            }
        }
        return coding;
    }

    /// <summary>
    /// 计算多热编码中1的个数
    /// </summary>
    /// <param name="coding">多热编码</param>
    /// <returns>1的个数</returns>
    public static int Count(ulong coding)
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

    /// <summary>
    /// 计算两个多热编码的乘积, 即共同的1的个数
    /// </summary>
    /// <param name="coding1">编码1</param>
    /// <param name="coding2">编码2</param>
    /// <returns>乘积</returns>
    public static int Multiply(ulong coding1, ulong coding2)
    {
        ulong newCoding = coding1 & coding2;
        // 按位与保留共同属性
        return Count(newCoding);
    }
}
