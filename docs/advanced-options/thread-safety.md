---
layout: default
title: "Thread-safety "
nav_order: 9
parent: "Advanced options"
---

All interfaces and their implementations **are thread-safe** (or at least they should be), including asynchronous and normal mappers, and factories created from them.

Concurrent collections and Interlocked operations are used, so performance shouldn't be much of an issue.

Custom mappers and user maps should be concurrent or not, depending on the user necessities.