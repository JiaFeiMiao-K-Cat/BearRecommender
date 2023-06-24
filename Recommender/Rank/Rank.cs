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

    public List<Movie>? RecommendMovies(int userId, int k)
    {
        var user = _context.UserFeatures.FirstOrDefault(x => x.UserId == userId);
        var list = _filter.RecommendMovies(userId, k);
        var movieFeatures  = new List<MovieFeature>();
        foreach (var movie in list)
        {
            if (user!.MovieIds.Contains(movie))
            {
                continue;
            }// 用户已经看过
            movieFeatures.Add(_context.MovieFeatures.FirstOrDefault(e => e.MovieId == movie));
        }
        var movieIds = movieFeatures.OrderByDescending(e => e.AverageRating)
            .ThenByDescending(e => MultiEncoding.Multiply(e.Genres, user!.Perfer))
            .ThenByDescending(e => _filter.PredictUserRating(userId, e.MovieId))
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
