# Silk! V1.6 -- The rewrite

Over the months, Silk! has become a much larger project than I ever could've imagined, with plenty more planned, but sadly it's hard to add those features when the code is unweildy and somewhat hard to manage. On top of this, the code shouldn't only be maintainable, but understandable at a glance. 

And thus this page exists. Outlining what should be changed.

<sub>btw this is all subject to change!</sub>

# Contributing:
To contribute, please [fork the repo](https://github.com/VelvetThePanda/Silk) from the *rewrite/\<project\>* branch, where <project> is whatever is being worked on at the time. If you'd like to tackle something that's listed below, but doesn't exist, feel free to fork *rewrite/core*, and push to a branch named *rewrite/<project>*, where project is the suffix of whatever project the branch focuses on. e.g. Silk.Extensions -> *rewrite/extensions*.

This document is subject to change! Check back regularly for the up-to-date list of outlined changes, and what's been completed.

## Silk.Core.Data
Changes to the data project (where all data access is handled, via MediatR):

 * DTOs! Data Transfer Objects (DTOs): As it would turn out, passing/handling raw models isn't great. DTOs can be passed between layers, and prevent changing data that should be immutable. 

## Silk.Core

 Changes to the what's refered to as the 'core' project(s):

* ~~Silk.Core.Logic may be removed all together~~
* /Utilities should be sifted through and have unused classes tossed, and applicable classes either merged closer to where they're used, or put in Silk.Shared
* /Notifications and /EventHandlers should be merged into a single folder, and possible have their types merged into a singular files, as they're used in one location.
* Complex services (e.g. InfractionService) need to be overhauled. 
* 

