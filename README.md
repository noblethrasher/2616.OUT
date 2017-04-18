# 2616.OUT
2616.out (formally known as ArcRx and ArcReaction) is an experimental web application library that recapitulates the principles of REST as articulated by [RFC 2616](https://tools.ietf.org/html/rfc2616) and Roy Fielding's dissertation.

It builds upon the lessons learned from the design and implementation of [ArcReaction](https://github.com/noblethrasher/OkExample/tree/master/Projects/ArcReaction) (which was based only on Fielding's dissertation).

What distinquishes the project from other web "framworks" is that it is designed in such a way that the RFC2616 Specification will serve as the best user manual. Thus, its princple classes correpond to the principle objects in RFC2616 andd have names like `MediaType`, `Representation`, `Method` (e.g. `GET`, `POST`, etc.), and `AppState`.

The express purpose of goal of this project is to have a simple-to-use architectural framework that will last for 100 years. This is not vainity or hubris, because the web, as a spec, is only at version 1.1 even through approaching it's 20th birthday.



