# 2616.OUT

## Introduction

The annoying thing about web frameworks is that they make it easy to get started on a project, but after a certain point they stop scaling. By "scaling", I'm not talking about the number of concurrent users, but rather about the difficulty of absorbing discrete changes after a certain point. This is too bad, because HTTP was designed for building applications that not only scale with respect to the number of clients, but also vis-à-vis edits to a codebase. In fact, HTTP doesn't need much abstraction because it turns out that it is already a well thought-out framework for building adaptive, distrubted, scalable, and secure applications.

2616.out (formally known as ArcRx and ArcReaction) is a library for building web applications in a *pure* and *strict* RESTful style as articulated by [RFC 2616](https://tools.ietf.org/html/rfc2616) and [Roy Fielding's dissertation](https://www.ics.uci.edu/~fielding/pubs/dissertation/fielding_dissertation.pdf).

It builds upon the lessons learned from the design and implementation of [ArcReaction](https://github.com/noblethrasher/OkExample/tree/master/Projects/ArcReaction) (which was based soley on Fielding's dissertation).

What distinquishes the project from other web "frameworks" is that it is designed in such a way that the [RFC2616 specification](https://www.ietf.org/rfc/rfc2616.txt) will serve as the best user manual. Hence, its princple classes correpond to the principle objects in RFC2616 and have names like `MediaType`, `Representation`, `Method` (e.g. `GET`, `POST`, etc.), and `AppState`.

The express goal of this project is to realize a simple-to-use architectural framework that will last for [100 years](http://www.paulgraham.com/hundred.html).

This is not mere hubris for a couple of reasons:

1. The design is already done and articulated in the form of RFC2616 ***and***
2. RFC2616 is a manifestly successful specification, yet has not changed much (it's only at version 1.1), even as it approaches its 20th anniversary.

The main idea is that the explosion of web applicaton frameworks just might be strange and unnecessary since the fundementals of the web have changed very little in two decades.


## How REST makes programming scalable

Most of the literature on REST and HTTP focuses on its scalability story with respect to concurrent clients, so I won't bother to relitigate that here. Suffice to say, RFC2616.OUT inherits all of the scalability features of HTTP/REST, to the degree that it faithfully recapitulates the spec. But, what I want to talk about are the ways in which the REST architectural style make it easy to write programs in genreral.

### Editing Code

For experienced programmers, throwing out big chunks of code is one of the most satisfying parts of writing programs. REST is the only architectural style of which I'm aware that makes it simple and easy to delete code. This is because REST enshrines the attributes of client/server and cacheability, which means that we can enshrine deprecation of of an API directly in the API itself. To acchieve this, all we need to do is set an exires header on a resource (which is a representation of application state, and hence is an object in the OOP sense). This means that all APIs come with a sunset clause that the application developer is free to renew, or allow to expire. Moreover, we can always have an idea of how many clients are referencing an API endpoint just by looking at the last **n** times that  it was accessed in a certain window of time. From there, we can compute the number of clients that may be using an API resoource before we expire it. Of course, the same mechanism also makes it simple to add new features because in REST, API discovery is an empirical process from the client's point of view: Accessing a representation of application state (i.e. a resource) also means getting advice about the next available states.

### Shipping Code

REST is also the only architectural style that makes process migration simple. This is because the constraints of layering and code-on-demand means that we can ship an entire layer so that it is closer to the client. For example, in the case of a web application, we can ship all or part of a database to the client in the form of JavaScript and IndexDB.


### Organizing Code

As Alan Kay [put it in his dissertation](http://www.chilton-computing.org.uk/inf/pdfs/kay.htm#c1), a computer \[program\] is an "abstraction of a well-defined universe which may resemble other well-known universes to any necessary degree". So, while we can make programs that simulate just about any process, the most useful ones tend to be those that represent some aspect of our universe. But, most programs do this poorly because they are written by humans who, without special training, will recapitulate their wrong ideas about reality into code. People learning to draw do the same thing, especially with faces. This is because our brains seem to have optimized hardware for recognizing face-like things (which is why it doesn't take much to suggest a face -- e.g. the front of a car) and one of the first things an artist learns to do is to "see" — which is to draw what's actually on thier retina rather than what's in thier brain. Simiarly, as great apes, we humans tend to see heirarchy everywhere, which is what made it hard come up with accurate models of natural phenomena. It tooks us over 350 years to go from accepting that the earth was not the center of the universe to knowing that the notion of a center is nonsense in the first place. This realization is also key to building distributed systems, and the corresponding benefits of the REST style are well-known. But, RESTful designs are also  anti-hierarchical. This is because, like all computer systems, RESTful systems are state machines. However, any state can be a start state thanks to the constraints of statelessness and uniformity of interface. This is probably the biggest hurdle to understanding REST, because most programmers are trained to organize code heirachically; this is especially (and uneccessarily) true of OOP.
