using NForza.Cyrus.Abstractions;

namespace TracksDemo.Tracks.Query;

[Query]
public record struct SearchTracksQuery(string SearchTerms, int Page = 1, int PageSize = 10)
{
}
