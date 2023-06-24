using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recommender.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Utils.Tests;

[TestClass()]
public class MultiHotEncodingTests
{
    [TestMethod()]
    public void GetMultiEncodingTest()
    {
        Assert.AreEqual(0x0000_0000_0000_0000LU, MultiHotEncoding.GetMultiEncoding(""));
        Assert.AreEqual(0x0000_0000_0000_000fLU, MultiHotEncoding.GetMultiEncoding("|(no genres listed)||Animation|Action|Adventure"));
        Assert.AreEqual(0x0000_0000_0000_000fLU, MultiHotEncoding.GetMultiEncoding("(no genres listed)|Animation|Action|Adventure"));
        Assert.AreEqual(0x0000_0000_000f_ffffLU,
            MultiHotEncoding.GetMultiEncoding("(no genres listed)|Action|Adventure|Animation|Children|Comedy|Crime|Documentary|" +
                "Drama|Fantasy|Film-Noir|Horror|IMAX|Musical|Mystery|Romance|Sci-Fi|Thriller|War|Western"));

        Assert.AreEqual(0x8000_0000_0000_0000LU, MultiHotEncoding.GetMultiEncoding("abc"));
        Assert.AreEqual(0x8000_0000_0000_000fLU, MultiHotEncoding.GetMultiEncoding("abc|(no genres listed)|Animation|Action|Adventure"));
        Assert.AreEqual(0x8000_0000_000f_ffffLU,
            MultiHotEncoding.GetMultiEncoding("abc|(no genres listed)|Action|Adventure|Animation|Children|Comedy|Crime|Documentary|" +
                "Drama|Fantasy|Film-Noir|Horror|IMAX|Musical|Mystery|Romance|Sci-Fi|Thriller|War|Western"));
    }

    [TestMethod()]
    public void GetGenresTest()
    {
        CollectionAssert.AreEqual(new[] { "(no genres listed)", "Action", "Adventure", "Animation" }, MultiHotEncoding.GetGenres(0x0000_0000_0000_000fLU));
        CollectionAssert.AreEqual(new[] { "(no genres listed)", "Action", "Adventure", "Animation" }, MultiHotEncoding.GetGenres(0x8000_0000_0000_000fLU));
        CollectionAssert.AreEqual(new[] { "(no genres listed)", "Action", "Adventure", "Animation" }, MultiHotEncoding.GetGenres(0x80a0_0000_0000_000fLU));
    }

    [TestMethod()]
    public void CountTest()
    {
        Assert.AreEqual(5, MultiHotEncoding.Count(0x0000_0000_0008_000fLU));
        Assert.AreEqual(0, MultiHotEncoding.Count(0x0000_0000_0000_0000LU));
        Assert.AreEqual(64, MultiHotEncoding.Count(0xffff_ffff_ffff_ffffLU));
    }

    [TestMethod()]
    public void GetPerferCodingTest()
    {
        Assert.AreEqual(0x0000_0000_0000_0000LU, MultiHotEncoding.GetPerferCoding(new Dictionary<string, int>()));

        Dictionary<string, int> map = new Dictionary<string, int>()
        {
            {"Action", 1}, {"Animation", 2}, {"Western", 4}
        };
        Assert.AreEqual(0x0000_0000_0008_000aLU, MultiHotEncoding.GetPerferCoding(map));

        map.Add("abc", 1);
        Assert.AreEqual(0x8000_0000_0008_000aLU, MultiHotEncoding.GetPerferCoding(map));

        map.Add("(no genres listed)", 2); map.Add("Adventure", 3); map.Add("Children", 1); map.Add("IMAX", 1);
        Assert.AreEqual(0x8000_0000_0008_000dLU, MultiHotEncoding.GetPerferCoding(map));

        map.Remove("abc");
        Assert.AreEqual(0x0000_0000_0008_000fLU, MultiHotEncoding.GetPerferCoding(map));
    }

    [TestMethod()]
    public void MultiplyTest()
    {
        Assert.AreEqual(0, MultiHotEncoding.Multiply(0, 0));
        Assert.AreEqual(64, MultiHotEncoding.Multiply(0xffff_ffff_ffff_ffffLU, 0xffff_ffff_ffff_ffffLU));
        Assert.AreEqual(3, MultiHotEncoding.Multiply(0x8000_0000_0008_000aLU, 0x0000_0000_0008_000fLU));
        Assert.AreEqual(1, MultiHotEncoding.Multiply(0x8000_0000_0008_000aLU, 0x0000_0000_0008_0005LU));
    }
}