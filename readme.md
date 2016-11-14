## Ambition

- content cache query
- lucene query
- simple, yet flexible syntax

## Current state

Can currently do this:

    news

translates to

    contentCache.GetContentByXPath("//news")

and

    latest news

translates to

    contentCache
        .GetContentByXPath("//news")
        .OrderByDescending(c => c.CreateDate)

and

    latest 5 news

translates to

    contentCache
        .GetContentByXPath("//news")
        .OrderByDescending(c => c.CreateDate)
        .Take(5)


