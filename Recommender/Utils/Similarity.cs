﻿using Recommender.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Utils;

public class Similarity
{
    /// <summary>
    /// 电影特征的余弦相似度
    /// </summary>
    /// <param name="movieA">电影A</param>
    /// <param name="movieB">电影B</param>
    /// <returns>余弦相似度</returns>
    public static double MovieFeatureCosine(MovieFeature movieA, MovieFeature movieB)
    {
        double squareA = movieA.SquareSum();
        double squareB = movieB.SquareSum();
        double multiply = 0;
        multiply += movieA.AverageRating * movieB.AverageRating;
        multiply += movieA.RatingRate1 * movieB.RatingRate1;
        multiply += movieA.RatingRate2 * movieB.RatingRate2;
        multiply += movieA.RatingRate3 * movieB.RatingRate3;
        multiply += movieA.RatingRate4 * movieB.RatingRate4;
        multiply += movieA.RatingRate5 * movieB.RatingRate5;
        multiply += MultiHotEncoding.Multiply(movieA.Genres, movieB.Genres);
        return multiply / (Math.Sqrt(squareA) * Math.Sqrt(squareB) + 1e-18);
        // 防止除零, 分母加1e-18
    }

    /// <summary>
    /// 用户特征的余弦相似度
    /// </summary>
    /// <param name="userA">用户A</param>
    /// <param name="userB">用户B</param>
    /// <returns>余弦相似度</returns>
    public static double UserFeatureCosine(UserFeature userA, UserFeature userB)
    {
        double squareA = userA.SquareSum();
        double squareB = userB.SquareSum();
        double multiply = 0;
        multiply += userA.AverageRating * userB.AverageRating;
        multiply += userA.RatingRate1 * userB.RatingRate1;
        multiply += userA.RatingRate2 * userB.RatingRate2;
        multiply += userA.RatingRate3 * userB.RatingRate3;
        multiply += userA.RatingRate4 * userB.RatingRate4;
        multiply += userA.RatingRate5 * userB.RatingRate5;
        multiply += MultiHotEncoding.Multiply(userA.Perfer, userB.Perfer);
        return multiply / (Math.Sqrt(squareA) * Math.Sqrt(squareB) + 1e-18);
        // 防止除零, 分母加1e-18
    }

    private static double Covariance(UserFeature userA, UserFeature userB)
    {
        double avgA = userA.Average();
        double avgB = userB.Average();

        double cov = 0;

        cov += (userA.AverageRating / 5 - avgA) * (userB.AverageRating / 5 - avgB);
        cov += (userA.RatingRate1 - avgA) * (userB.RatingRate1 - avgB);
        cov += (userA.RatingRate2 - avgA) * (userB.RatingRate2 - avgB);
        cov += (userA.RatingRate3 - avgA) * (userB.RatingRate3 - avgB);
        cov += (userA.RatingRate4 - avgA) * (userB.RatingRate4 - avgB);
        cov += (userA.RatingRate5 - avgA) * (userB.RatingRate5 - avgB);

        cov /= 5;

        return cov;
    }

    /// <summary>
    /// 用户特征的皮尔逊相似度
    /// </summary>
    /// <param name="userA">用户A</param>
    /// <param name="userB">用户B</param>
    /// <returns>皮尔逊相似度</returns>
    public static double UserFeaturePearson(UserFeature userA, UserFeature userB)
    {
        double cov = Covariance(userA, userB);

        double varA = userA.Variance();
        double varB = userB.Variance();

        return cov / (Math.Sqrt(varA) * Math.Sqrt(varB) + 1e-18);
    }

    /// <summary>
    /// 用户Embedding特征的余弦相似度
    /// </summary>
    /// <param name="userA">用户A</param>
    /// <param name="userB">用户B</param>
    /// <returns>余弦相似度</returns>
    public static double EmbeddingUserCosine(UserFeature userA, UserFeature userB)
    {
        double numerator = 0;
        double denominatorA = 0;
        double denominatorB = 0;
        for (int i = 0; i < userA.FeaturesEncoded.Count(); i++)
        {
            // 不转double可能会产生Inf
            numerator += (double)userA.FeaturesEncoded[i] * userB.FeaturesEncoded[i];
            denominatorA += Math.Pow(userA.FeaturesEncoded[i], 2);
            denominatorB += Math.Pow(userB.FeaturesEncoded[i], 2);
        }
        return numerator / (Math.Sqrt(denominatorA) * Math.Sqrt(denominatorB) + 1e-18);
    }

    /// <summary>
    /// 加权的用户相似度
    /// </summary>
    /// <param name="userA">用户A</param>
    /// <param name="userB">用户B</param>
    /// <returns>用户相似度</returns>
    public static double UserSimilarity(UserFeature userA, UserFeature userB)
    {
        return 0.1 * UserFeaturePearson(userA, userB) + 0.45 * UserFeatureCosine(userA, userB) + 0.45 * EmbeddingUserCosine(userA, userB);
    }
}