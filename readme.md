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

## Current BNF

    <query>             ::= [<order>] [<limit>] <contentSelector>
    <contentSelector>   ::= <documentType>
    <order>             ::= "latest"
    <limit>             ::= <number>

## Target BNF

    <query> ::=             ["I want"]
                            [["the"] <order>]
                            [<limit>]
                            <contentSelector> 
                            ["from" <route>]
                            ["where" <member> <op> <expression>]

    <order> ::=             "latest" | "earliest" | "first" | "last" | "...?"

    <limit> ::=             <number>

    <contentSelector> ::=   <documentType> | <hierarchySelector> |
                            <hierarchySelector> <documentType>

    <hierarchySelector> ::= "everything" | "children" | "descendants" | "ancestors"

    <route> ::=             <contentCacheRoute>

    <member> ::=            <propertyValue> | <builtinPropertyValue>
    <op> ::=                # the useful ones
    <expression> ::=        # to be evolved
