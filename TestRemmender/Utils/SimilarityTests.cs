using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recommender.Model;
using Recommender.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Utils.Tests;

[TestClass()]
public class SimilarityTests
{
    [TestMethod()]
    public void MovieFeatureCosineTest()
    {
        MovieFeature movieA = new MovieFeature()
        {
            RatingCount1 = 0,
            RatingCount2 = 0,
            RatingCount3 = 0,
            RatingCount4 = 0,
            RatingCount5 = 0,
            TotalRating = 0,
            Genres = 0,
        };
        MovieFeature movieB = new MovieFeature()
        {
            RatingCount1 = 0,
            RatingCount2 = 0,
            RatingCount3 = 0,
            RatingCount4 = 0,
            RatingCount5 = 0,
            TotalRating = 0,
            Genres = 0,
        };
        Assert.IsTrue(Math.Abs(0 - Similarity.MovieFeatureCosine(movieA, movieB)) < 1e-3);
        movieA = new MovieFeature()
        {
            RatingCount1 = 1,
            RatingCount2 = 0,
            RatingCount3 = 3,
            RatingCount4 = 0,
            RatingCount5 = 6,
            TotalRating = 38,
            Genres = 0x0000_0000_0000_00ffLU,
        };
        movieB = new MovieFeature()
        {
            RatingCount1 = 0,
            RatingCount2 = 2,
            RatingCount3 = 0,
            RatingCount4 = 3,
            RatingCount5 = 4,
            TotalRating = 34,
            Genres = 0x0000_0000_0000_06d8LU
        };
        Assert.IsTrue(Math.Abs(0.85678 - Similarity.MovieFeatureCosine(movieA, movieB)) < 1e-3);
    }

    [TestMethod()]
    public void EmbeddingUserCosineTest()
    {
        UserFeature userA = new UserFeature()
        {
            FeaturesEncoded = new float[] { float.MaxValue }
        };
        UserFeature userB = new UserFeature()
        {
            FeaturesEncoded = new float[] { float.MaxValue }
        };
        Assert.IsTrue(Math.Abs(1 - Similarity.EmbeddingUserCosine(userA, userB)) < 1e-3);
        userA = new UserFeature()
        {
            FeaturesEncoded = new float[] { 1.2f, -1.6f, 7f, 5f }
        };
        userB = new UserFeature()
        {
            FeaturesEncoded = new float[] { 0f, 1.3f, 0.5f, 1.1f }
        };
        Assert.IsTrue(Math.Abs(0.44147 - Similarity.EmbeddingUserCosine(userA, userB)) < 1e-3);
    }

    [TestMethod()]
    public void UserFeatureCosineTest()
    {
        UserFeature userA = new UserFeature()
        {
            RatingCount1 = 0,
            RatingCount2 = 0,
            RatingCount3 = 0,
            RatingCount4 = 0,
            RatingCount5 = 0,
            TotalRating = 0,
            Perfer = 0,
        };
        UserFeature userB = new UserFeature()
        {
            RatingCount1 = 0,
            RatingCount2 = 0,
            RatingCount3 = 0,
            RatingCount4 = 0,
            RatingCount5 = 0,
            TotalRating = 0,
            Perfer = 0,
        };
        Assert.IsTrue(Math.Abs(0 - Similarity.UserFeatureCosine(userA, userB)) < 1e-3);
        userA = new UserFeature()
        {
            RatingCount1 = 1,
            RatingCount2 = 0,
            RatingCount3 = 3,
            RatingCount4 = 0,
            RatingCount5 = 6,
            TotalRating = 38,
            Perfer = 0x0000_0000_0000_00ffLU,
        };
        userB = new UserFeature()
        {
            RatingCount1 = 0,
            RatingCount2 = 2,
            RatingCount3 = 0,
            RatingCount4 = 3,
            RatingCount5 = 4,
            TotalRating = 34,
            Perfer = 0x0000_0000_0000_06d8LU
        };
        Assert.IsTrue(Math.Abs(0.85678 - Similarity.UserFeatureCosine(userA, userB)) < 1e-3);
    }

    [TestMethod()]
    public void UserFeaturePearsonTest()
    {
        UserFeature userA = new UserFeature()
        {
            RatingCount1 = 0,
            RatingCount2 = 0,
            RatingCount3 = 0,
            RatingCount4 = 0,
            RatingCount5 = 0,
            TotalRating = 0
        };
        UserFeature userB = new UserFeature()
        {
            RatingCount1 = 0,
            RatingCount2 = 0,
            RatingCount3 = 0,
            RatingCount4 = 0,
            RatingCount5 = 0,
            TotalRating = 0
        };
        Assert.IsTrue(Math.Abs(0 - Similarity.UserFeaturePearson(userA, userB)) < 1e-3);
        userA = new UserFeature()
        {
            RatingCount1 = 1,
            RatingCount2 = 0,
            RatingCount3 = 3,
            RatingCount4 = 0,
            RatingCount5 = 6,
            TotalRating = 32.5
        };
        userB = new UserFeature()
        {
            RatingCount1 = 1,
            RatingCount2 = 0,
            RatingCount3 = 3,
            RatingCount4 = 0,
            RatingCount5 = 6,
            TotalRating = 32
        };
        Assert.IsTrue(Math.Abs(0.19999 - Similarity.UserFeaturePearson(userA, userB)) < 1e-3);
    }
}