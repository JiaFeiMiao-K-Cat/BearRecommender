using Microsoft.EntityFrameworkCore;
using Recommender.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Model;

public class Context : DbContext
{
    private readonly string _connectionString;

    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieFeature> MovieFeatures { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<UserFeature> UserFeatures { get; set; }

    public Context(string connectionString)
    {
        _connectionString = connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }

    /// <summary>
    /// 增加评分记录
    /// </summary>
    /// <param name="rating">评分记录</param>
    public void AddRating(Rating rating)
    {
        if (Ratings.Any(e => e.UserId == rating.UserId 
                && e.MovieId == rating.MovieId 
                && e.Timestamp == rating.Timestamp))
        {
            return;
        } // 已存在, 不更新评分
        else
        {
            Ratings.Add(rating);

            #region update UserFeature
            var userFeature = UserFeatures.FirstOrDefault(e => e.UserId == rating.UserId);
            if (userFeature == null)
            {
                userFeature = new UserFeature();
                userFeature.UserId = rating.UserId;
                UserFeatures.Add(userFeature);
            }
            userFeature.TotalRating += rating.UserRating;
            switch ((int)double.Ceiling(rating.UserRating)) 
            {
                case 1: { userFeature.RatingCount1++; break; }
                case 2: { userFeature.RatingCount2++; break; }
                case 3: { userFeature.RatingCount3++; break; }
                case 4: { userFeature.RatingCount4++; break; }
                case 5: { userFeature.RatingCount5++; break; }
            }
            if (rating.UserRating >= 3.5)
            {
                userFeature.MovieIds.Add(rating.MovieId); // 更新高分电影列表, 已经按时间升序了, 所以直接添加即可
            }
            userFeature.NewRecordsCount++; // 更新计数器, 重新计算Embedding留到离线处理进行, 提高在线服务性能
            UserFeatures.Update(userFeature);
            #endregion

            #region update MovieFeature
            var movieFeature = MovieFeatures.FirstOrDefault(e => e.MovieId == rating.MovieId);
            if (movieFeature != null)
            {
                movieFeature.TotalRating += rating.UserRating;
                switch ((int)double.Ceiling(rating.UserRating))
                {
                    case 1: { movieFeature.RatingCount1++; break; }
                    case 2: { movieFeature.RatingCount2++; break; }
                    case 3: { movieFeature.RatingCount3++; break; }
                    case 4: { movieFeature.RatingCount4++; break; }
                    case 5: { movieFeature.RatingCount5++; break; }
                }
                MovieFeatures.Update(movieFeature);
            }
            #endregion
            SaveChanges();
        }
    }

    /// <summary>
    /// 新增用户
    /// </summary>
    /// <param name="perfer">用户偏好</param>
    /// <returns>用户ID</returns>
    public int AddNewUser(string perfer)
    {
        UserFeature user = new UserFeature();
        user.Perfer = MultiHotEncoding.GetMultiEncoding(perfer);
        UserFeatures.Add(user);
        SaveChanges();
        return user.UserId;
    }
}
