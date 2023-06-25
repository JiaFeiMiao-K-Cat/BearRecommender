using Microsoft.EntityFrameworkCore;
using Recommender.Matching;
using Recommender.Model;
using Recommender.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recommender.Rank;

public class Rank
{
    private Context _context { get; set; }

    private CollaborativeFiltering _filter { get; set; }

    public Rank(Context context)
    {
        _context = context;
        _filter = new CollaborativeFiltering(context);
    }

    /// <summary>
    /// 推荐电影
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="k">电影数量, 实际返回数量为[0, k]</param>
    /// <returns>电影列表</returns>
    public List<Movie>? RecommendMovies(int userId, int k)
    {
        var user = _context.UserFeatures.FirstOrDefault(x => x.UserId == userId);
        var list = _filter.RecommendMovies(userId, k);
        if (user == null)
        {
            return null;
        } // 用户不存在
        var movieFeatures  = new List<MovieFeature>();
        foreach (var movie in list!)
        {
            if (user!.MovieIds.Contains(movie))
            {
                continue;
            }// 用户已经看过
            var faeture = _context.MovieFeatures.FirstOrDefault(e => e.MovieId == movie);
            if (faeture == null)
            {
                continue;
            }// 电影不存在
            movieFeatures.Add(faeture);
        }
        var movieIds = movieFeatures
            .OrderByDescending(e => MultiHotEncoding.Multiply(e.Genres, user!.Perfer)) // 先按与用户偏好的重合度降序
            .ThenByDescending(e => e.AverageRating) // 再按电影平均得分降序
            .ThenByDescending(e => _filter.PredictUserRating(userId, e.MovieId)) // 最后按预测得分降序
            .Take(k)
            .Select(e => e.MovieId);
        var recommendList = new List<Movie>();
        foreach (var movie in movieIds)
        {
            recommendList.Add(_context.Movies.FirstOrDefault(e => e.MovieId == movie));
        }
        return recommendList;
    }
}
