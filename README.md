# 2616.OUT
2616.out (formally known as ArcRx and ArcReaction) is a library for building web applications in a *pure* and *strict* RESTful style as articulated by [RFC 2616](https://tools.ietf.org/html/rfc2616) and [Roy Fielding's dissertation](https://www.ics.uci.edu/~fielding/pubs/dissertation/fielding_dissertation.pdf).

It builds upon the lessons learned from the design and implementation of [ArcReaction](https://github.com/noblethrasher/OkExample/tree/master/Projects/ArcReaction) (which was based soley on Fielding's dissertation).

What distinquishes the project from other web "frameworks" is that it is designed in such a way that the [RFC2616 specification](https://www.ietf.org/rfc/rfc2616.txt) will serve as the best user manual. Hence, its princple classes correpond to the principle objects in RFC2616 and have names like `MediaType`, `Representation`, `Method` (e.g. `GET`, `POST`, etc.), and `AppState`.

The express goal of this project is to realize a simple-to-use architectural framework that will last for [100 years](http://www.paulgraham.com/hundred.html).

This is not mere hubris for a couple of reasons:

1. The design is already done and articulated in the form of RFC2616 ***and***
2. RFC2616 is a manifestly successful specification, yet has not changed much (it's only at version 1.1), even as it approaches its 20th anniversary.

The main idea is that the explosion of web applicaton frameworks just might be strange and unnecessary since the fundementals of the web have changed very little in two decades. It would be nice





